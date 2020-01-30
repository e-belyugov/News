﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using News.Core.Models;
using News.Core.Services.Web;
using News.Core.Services.Logging;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;
using News.Core.Helpers;

namespace News.Core.Services.Parsing
{
    /// <summary>
    /// Kuzpress parser
    /// </summary>
    public class KuzpressParser : IParser
    {
        // Parsed articles 
        private readonly IList<Article> _articles = new List<Article>();

        // Web service
        private readonly IWebService _webService;

        // Logger
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public KuzpressParser(IWebService webService, ILogger logger)
        {
            _webService = webService;
            _logger = logger;
        }

        /// <summary>
        /// Cleaning html
        /// </summary>
        private string GetArticleText(Article article, ParserData parserData, string html, bool clean)
        {
            string cleaned = html;
            try
            {
                string oldArticle = cleaned.SubstringBetweenSubstrings("</h1>", "<div class=\"newsLine\">");

                // Filter by article text
                bool skip = oldArticle.Contains("youtube") || oldArticle.Contains("<table");

                if (!skip)
                {
                    if (clean)
                    {
                        // Cleaning html 

                        string newArticle = oldArticle;

                        newArticle = newArticle.Replace("<p></h5></p>", "</h5");

                        newArticle = newArticle.ReplaceTags("<strong", "</strong>", "h");
                        newArticle = newArticle.ReplaceTags("<span", "</span>", "*");
                        newArticle = newArticle.ReplaceTags("<div", "</div>", "*");
                        newArticle = newArticle.ReplaceTags("<h2", "</h2>", "h");
                        newArticle = newArticle.ReplaceTags("<h3", "</h3>", "h"); 
                        newArticle = newArticle.ReplaceTags("<h4", "</h4>", "h");
                        newArticle = newArticle.ReplaceTags("<h5", "</h5>", "h");
                        newArticle = newArticle.ReplaceTags("<h6", "</h6>", "h");
                        newArticle = newArticle.ReplaceTags("<h7", "</h7>", "h");

                        if (Regex.Matches(newArticle, "<p").Count == 1) newArticle = newArticle.Replace("</p>", "</p><p>") + "</p>";

                        cleaned = cleaned.Replace(oldArticle, newArticle);
                    }
                    else
                    {
                        oldArticle = oldArticle.Replace("h4", "h3");
                        oldArticle = oldArticle.Replace("h5", "h3");
                        oldArticle = oldArticle.Replace("h6", "h3");
                        oldArticle = oldArticle.Replace("src=\"/i", "src=\"" + parserData.SourceMainLink + "/i");
                        oldArticle = oldArticle.Replace("src=", "width=\"500\" src=");

                        /*
                        oldArticle = oldArticle.ReplaceSubstringBetweenSubstrings("<a", ">", "");
                        oldArticle = oldArticle.Replace("<a>", "<font style=\"text-decoration:underline\">");
                        oldArticle = oldArticle.Replace("</a>", "</font>");
                        */
                        //oldArticle = oldArticle.Replace("<a", "<a target=\"_blank\"");

                        oldArticle = oldArticle + "<p>Ссылка на статью: <a href=\"" + article.SourceLink + "\">" + parserData.SourceTitle + "</a></p>";

                        oldArticle = "<head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head>"
                            + "<font style=\"font-family:segoe ui; font-size:16px\">" + oldArticle + "</font>";

                        cleaned = oldArticle;
                    }
                }
                else
                {
                    cleaned = "Skip";
                }

                return cleaned;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return html;
            }
        }

        /// <summary>
        /// Parsing source document
        /// </summary>
        public async Task<IList<Article>> Parse(ParserData parserData, IList<Article> existingArticles)
        {
            try
            {
                // Preparing clean result list
                _articles.Clear();

                // Loading web data
                string html = await _webService.GetDataAsync(parserData.SourceParseLink,
                    Encoding.GetEncoding(parserData.SourceEncoding));

                // Saving last parsing time 
                parserData.LastTimeStamp = DateTime.Now;

                // Parsing main page 
                HtmlNode.ElementsFlags.Remove("link");
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                var articleItems = doc.DocumentNode.SelectNodes("//item");

                if (articleItems != null)
                {
                    int articleCount = 0;
                    foreach (HtmlNode item in articleItems)
                    {
                        articleCount++;

                        var node = item.ChildNodes["title"];

                        if (node != null)
                        {
                            // Article title
                            var title = node.InnerText;
                            title = title.RemoveSpecialTags();

                            // Checking new data
                            if (articleCount == 1)
                            {
                                if (parserData.LastData == title)
                                {
                                    // No new data
                                    break;
                                }

                                // New data
                                parserData.LastData = title;
                                foreach (var article in existingArticles.Where(x =>
                                    x.SourceMainLink == parserData.SourceMainLink))
                                    article.New = false;
                            }

                            // Checking if article exists already
                            var existingArticle = existingArticles.SingleOrDefault(x =>
                                x.SourceMainLink == parserData.SourceMainLink && x.Title == title);
                            if (existingArticle != null)
                            {
                                existingArticle.New = true;
                                continue;
                                //break;
                            }

                            // Article link
                            node = item.ChildNodes["link"];
                            string link = "";
                            if (node != null) link = node.InnerText;

                            // Article intro text
                            node = item.ChildNodes["description"];
                            string introText = "";
                            if (node != null)
                            {
                                introText = node.InnerText;
                                introText = introText.RemoveSpecialTags();
                            }

                            // Article time stamp
                            node = item.ChildNodes["pubDate"];
                            DateTime timeStamp = DateTime.MinValue;
                            if (node != null)
                            {
                                var timeString = node.InnerText.Replace(" +0300","");

                                if (!DateTime.TryParseExact(timeString, "ddd, dd MMM yyyy HH:mm:ss", 
                                    CultureInfo.InvariantCulture, DateTimeStyles.None, out timeStamp)) timeStamp = DateTime.MinValue;

                                timeStamp = timeStamp.AddHours(4);
                            }

                            // Creating article
                            if (
                                title != null
                                && link != null
                                && introText != null
                                && timeStamp != DateTime.MinValue
                                )
                            {
                                Article article = new Article
                                {
                                    Id = 0,
                                    SourceMainLink = parserData.SourceMainLink,
                                    SourceTitle = parserData.SourceTitle,
                                    SourceLink = link,
                                    Title = title,
                                    IntroText = introText,
                                    TimeStamp = timeStamp,
                                    New = true
                                };

                                // Parsing article
                                var result = await ParseArticle(parserData, link, article);

                                // Adding article to list
                                if (result) _articles.Add(article);
                            }
                        }
                    }
                }

                return _articles;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                _articles.Clear();
                return _articles;
            }
        }

        /// <summary>
        /// Parsing article 
        /// </summary>
        private async Task<bool> ParseArticle(ParserData parserData, string link, Article article)
        {
            try
            {
                // Loading article text from web
                string html = await _webService.GetDataAsync(link, Encoding.GetEncoding(parserData.SourceEncoding));

                if (html != "")
                {
                    // Article text
                    string text = GetArticleText(article, parserData, html, false);
                    if (text.Contains("Skip")) return false; // Skipping article

                    // Article image
                    byte[] image = null;
                    var articleDoc = new HtmlDocument();
                    articleDoc.LoadHtml(html);
                    var articlesHeaders = articleDoc.DocumentNode.SelectNodes("//div[@class='outer-info']");
                    if (articlesHeaders != null && articlesHeaders.Count > 0)
                    {
                        var articleItem = articlesHeaders[0];

                        foreach (HtmlNode articleNode in articleItem.ChildNodes)
                        {
                            if (articleNode.Name == "a" && image == null)
                            {
                                var imgNode = articleNode.ChildNodes["img"];
                                var attribute = imgNode?.Attributes["src"];
                                if (attribute != null)
                                {
                                    var imageLink = parserData.SourceMainLink + attribute.Value;
                                    if (Path.HasExtension(imageLink)) image = await _webService.GetImageAsync(imageLink);
                                }
                            }
                        }
                    }

                    // Logo if no image
                    //var resourceHelper = new ResourceHelper(_logger);
                    //if (image == null) image = resourceHelper.GetParserLogo(parserData.SourceTitle);

                    // Saving article fields
                    article.Text = text;
                    article.Image = image;
                    article.HasImage = image != null;
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return false;
            }
        }
    }
}

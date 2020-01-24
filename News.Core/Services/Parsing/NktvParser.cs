﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using News.Core.Models;
using News.Core.Services.Logging;
using News.Core.Services.Web;

namespace News.Core.Services.Parsing
{
    /// <summary>
    /// Nktv parser
    /// </summary>
    public class NktvParser : IParser
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
        public NktvParser(IWebService webService, ILogger logger)
        {
            _webService = webService;
            _logger = logger;
        }

        /// <summary>
        /// Cleaning html
        /// </summary>
        private string GetArticleText(Article article, ParserData parserData, string html)
        {
            string cleaned = html;
            try
            {
                cleaned = cleaned.SubstringBetweenSubstrings("<p><strong>", "</div><!-- END Post Content. -->");
                cleaned = "<p><strong>" + cleaned;

                cleaned = cleaned + "<p>Ссылка на статью: <a href=\"" + article.SourceLink + "\">" + parserData.SourceTitle + "</a></p>";

                cleaned = "<head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head>"
                             + "<font style=\"font-family:segoe ui; font-size:16px\">" + cleaned + "</font>";

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
                                introText = node.InnerHtml;
                                introText = introText.Replace("<![CDATA[", "").Replace("&#8230;]]>","...");
                                introText = introText.RemoveSpecialTags();
                            }

                            // Article time stamp
                            node = item.ChildNodes["pubDate"];
                            DateTime timeStamp = DateTime.MinValue;
                            if (node != null)
                            {
                                //var timeString = node.InnerText.Replace(" +0300", "");
                                var timeString = node.InnerText.Replace(" +0000", "");

                                if (!DateTime.TryParseExact(timeString, "ddd, dd MMM yyyy HH:mm:ss",
                                    CultureInfo.InvariantCulture, DateTimeStyles.None, out timeStamp)) timeStamp = DateTime.MinValue;

                                //timeStamp = timeStamp.AddHours(4);
                                timeStamp = timeStamp.AddHours(7);
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

                                //// Adding article to list
                                if (result) _articles.Add(article);

                                //break;
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
                    string text = GetArticleText(article, parserData, html);
                    if (text.Contains("Skip")) return false; // Skipping article

                    // Article image
                    byte[] image = null;
                    var articleDoc = new HtmlDocument();
                    articleDoc.LoadHtml(html);
                    var articlesHeaders = articleDoc.DocumentNode.SelectNodes("//div[@class='bdaia-post-featured-image']");
                    if (articlesHeaders != null && articlesHeaders.Count > 0)
                    {
                        var articleItem = articlesHeaders[0];

                        foreach (HtmlNode articleNode in articleItem.ChildNodes)
                        {
                            if (articleNode.Name == "img")
                            {
                                var attribute = articleNode.Attributes["src"];
                                if (attribute != null)
                                {
                                    //var imageLink = parserData.SourceMainLink + attribute.Value;
                                    var imageLink = attribute.Value;
                                    if (Path.HasExtension(imageLink)) image = await _webService.GetImageAsync(imageLink);
                                }

                                break;
                            }
                        }
                    }

                    // Saving article fields
                    article.Text = text;
                    article.Image = image;
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

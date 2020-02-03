using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
    [SuppressMessage("ReSharper", "StringIndexOfIsCultureSpecific.1")]
    public class VashgorodParser : IParser
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
        public VashgorodParser(IWebService webService, ILogger logger)
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
                if (cleaned.Contains("На правах рекламы")) return "Skip";

                // Article timestamp
                var timeString =
                    cleaned.SubstringBetweenSubstrings("<meta itemprop=\"datePublished\" content=\"", "+07:00");
                if (!DateTime.TryParseExact(timeString, "yyyy-MM-ddTHH:mm:ss",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var timeStamp))
                    timeStamp = DateTime.MinValue;
                article.TimeStamp = timeStamp;

                // Article full text
                cleaned = cleaned.SubstringBetweenSubstrings("<div id=\"beacon-article-start\"></div>",
                    "<div id=\"beacon-article-end\"></div>");
                cleaned = cleaned.Replace(" dir=\"ltr\"", "").Trim();
                cleaned = cleaned.Replace(" align=\"JUSTIFY\"", "").Trim();

                // Article large image link
                article.LargeImageLink = cleaned.GetImgHref();
                article.HasLargeImage = article.LargeImageLink != "";

                // Removing images
                cleaned = cleaned.RemoveTagWithContent("img");
                cleaned = cleaned.Replace("<p><strong></strong></p>", "");

                if (
                    cleaned.Contains("youtube")
                    || cleaned.Contains("<table")
                    || cleaned.Contains("vk.com/video")
                    || cleaned.Contains("360tv")
                    ) return "Skip";

                // Article intro text
                var introText = cleaned.SubstringBetweenSubstrings("<p><strong>", "</strong></p>").RemoveSpecialTags().Trim();
                if (introText != "" && !introText.Contains("img"))
                {
                    for (var i = 0; i <= 1; i++)
                    {
                        var aString = introText.SubstringBetweenSubstrings("<a", ">");
                        if (aString != "")
                        {
                            introText = introText.Replace(aString, "");
                            introText = introText.Replace("<a>", "").Replace("</a>", "");
                        }
                    }
                    article.IntroText = introText;
                }

                cleaned = cleaned + "<p>Ссылка на сайт: <a href=\"" + parserData.SourceMainLink + "\">" + parserData.SourceTitle + "</a></p>";

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
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                var articleItems = doc.DocumentNode.SelectNodes("//article[contains(@class,'vg-item')]");

                if (articleItems != null)
                {
                    int articleCount = 0;
                    foreach (HtmlNode item in articleItems)
                    {
                        articleCount++;

                        var articleNode = item.ChildNodes["a"];

                        if (articleNode != null)
                        {
                            // Article link
                            var linkAttribute = articleNode.Attributes["href"];
                            string link = null;
                            if (linkAttribute != null) link = parserData.SourceMainLink + linkAttribute.Value;

                            // Article title
                            string title = null;
                            var divNode = articleNode.ChildNodes["div"];
                            var headerNode = divNode?.ChildNodes["header"];
                            if (headerNode != null)
                            {
                                title = headerNode.InnerText;
                                title = title.RemoveSpecialTags().Trim();
                            }

                            // Checking data validity
                            if (title == null || link == null) continue;

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
                                foreach (var articleItem in existingArticles.Where(x =>
                                    x.SourceMainLink == parserData.SourceMainLink))
                                    articleItem.New = false;
                            }

                            // Article image
                            byte[] smallImage = null;
                            var figureNode = articleNode.ChildNodes["figure"];
                            var styleAttribute = figureNode?.Attributes["style"];
                            if (styleAttribute != null)
                            {
                                var imageLink = styleAttribute.Value;
                                imageLink = imageLink.Replace("background-image: url(", "").Replace(")", "");
                                smallImage = await _webService.GetImageAsync(imageLink);
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

                            // Creating article
                            Article article = new Article
                            {
                                Id = 0,
                                SourceMainLink = parserData.SourceMainLink,
                                SourceTitle = parserData.SourceTitle,
                                SourceLink = link,
                                Title = title,
                                SmallImage = smallImage,
                                HasSmallImage = smallImage != null,
                                New = true
                            };

                            // Parsing article
                            var result = await ParseArticle(parserData, link, article);

                            // Adding article to list
                            if (result) _articles.Add(article);
                        }

                        //break;
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

                    // Saving article fields
                    article.Text = text;
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

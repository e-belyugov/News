using System;
using System.Collections.Generic;
using System.Text;
using News.Core.Models;
using News.Core.Services.Web;
using News.Core.Services.Logging;
using System.Diagnostics;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;

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
        public string CleanHtml(string html)
        {
            string cleaned = html;
            try
            {
                string oldArticle = cleaned.SubstringBetweenSubstrings("</h1>", "<div class=\"newsLine\">");

                // Filter by article text
                bool skip = false;
                if (
                    oldArticle.Contains("youtube")
                    || oldArticle.Contains("<table")
                    )
                    skip = true;

                // Replacing tags
                if (!skip)
                {
                    string newArticle = oldArticle;

                    newArticle = newArticle.Replace("<p></h5></p>","</h5");

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
        /// Parsing html
        /// </summary>
        public async Task<IList<Article>> Parse(ParserData parserData, IList<Article> existingArticles)
        {
            try
            {
                // Preparing clean result list
                _articles.Clear();

                // Loading web data
                string html = await _webService.GetDataAsync(parserData.SourceMainLink, 
                    Encoding.GetEncoding(parserData.SourceEncoding));

                // Saving last parsing time 
                parserData.LastTimeStamp = DateTime.Now;

                // Parsing main page 
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                var headers = doc.DocumentNode.SelectNodes("//div[@class='outer-info inList']");

                if (headers != null)
                {
                    int articleCount = 0;
                    foreach (HtmlNode item in headers)
                    {
                        articleCount++;
                        string title = null;
                        string link = null;
                        string introText = null;
                        string text = null;
                        byte[] image = null;
                        DateTime timeStamp = DateTime.MinValue;

                        // a node
                        var node = item.ChildNodes["a"];
                        if (node != null)
                        {
                            var attribute = node.Attributes["href"];
                            if (attribute != null)
                            {
                                // Article title
                                node = node.ChildNodes["h2"];
                                if (node != null) title = node.InnerText;

                                // ----------------------------------------------------------------------------------

                                // Checking new data
                                if (articleCount == 1)
                                {
                                    if (parserData.LastData == title) 
                                    {
                                        // No new data
                                        break;
                                    }
                                    else
                                    {
                                        // New data
                                        parserData.LastData = title;
                                        foreach (var article in existingArticles.Where(x => x.SourceMainLink == parserData.SourceMainLink)) 
                                            article.New = false;
                                    }
                                }

                                // Filtering exisiting articles
                                var existingArticle = existingArticles.Where(x => x.Title == title).FirstOrDefault();
                                if (existingArticle != null)
                                {
                                    existingArticle.New = true;
                                    continue;
                                }

                                // ----------------------------------------------------------------------------------

                                // Article link
                                link = parserData.SourceMainLink + attribute.Value;

                                // Loading article text from web
                                html = await _webService.GetDataAsync(link, Encoding.GetEncoding(parserData.SourceEncoding));

                                if (html != "")
                                {
                                    HtmlDocument articleDoc = new HtmlDocument();

                                    html = CleanHtml(html); // Cleaning html
                                    if (html.Contains("Skip")) continue; // Skipping article

                                    articleDoc.LoadHtml(html);

                                    var articlesHeaders = articleDoc.DocumentNode.SelectNodes("//div[@class='outer-info']");

                                    if (articlesHeaders != null && articlesHeaders.Count > 0)
                                    {
                                        text = "";
                                        var articleItem = articlesHeaders[0];

                                        foreach (HtmlNode articleNode in articleItem.ChildNodes)
                                        {
                                            if (
                                                articleNode.Name == "p"
                                                )
                                            {
                                                var innerText = articleNode.InnerText;
                                                innerText = innerText.Trim();

                                                if (innerText != "")
                                                {
                                                    innerText = innerText.Replace("&quot;", "\"");
                                                    innerText = innerText.Replace("&nbsp;", "");
                                                    innerText = innerText.Replace("&hellip;", "...");
                                                    innerText = innerText.Replace("&mdash;", "-");
                                                    innerText = innerText.Replace("&laquo;", "\"");
                                                    innerText = innerText.Replace("&raquo;", "\"");
                                                    text += innerText + Environment.NewLine;
                                                }
                                            }

                                            // Article image
                                            if (articleNode.Name == "a" && image == null)
                                            {
                                                var imgNode = articleNode.ChildNodes["img"];
                                                if (imgNode != null)
                                                {
                                                    attribute = imgNode.Attributes["src"];
                                                    if (attribute != null)
                                                    {
                                                        var imageLink = parserData.SourceMainLink + attribute.Value;
                                                        if (Path.HasExtension(imageLink)) image = await _webService.GetImageAsync(imageLink);
                                                    }
                                                }
                                            }
                                        }
                                        text = text.Replace("<br /><br />", Environment.NewLine);
                                        text = text.TrimEnd('\r', '\n', ' ');
                                        text = text.Replace("^h", "<Bold>");
                                        text = text.Replace("h^", "</Bold>");
                                        text = text.Replace("^*", "<Italic>");
                                        text = text.Replace("*^", "</Italic>");
                                    }

                                    // Article date
                                    articlesHeaders = articleDoc.DocumentNode.SelectNodes("//div[@class='newsLineTime']");
                                    if (articlesHeaders != null && articlesHeaders.Count > 0)
                                    {
                                        var articleItem = articlesHeaders[0];
                                        if (articleItem != null)
                                        {
                                            var dateString = articleItem.InnerText;
                                            if (!DateTime.TryParse(dateString, out timeStamp)) timeStamp = DateTime.MinValue;
                                        }
                                    }
                                }
                            }
                        }

                        // p node
                        node = item.ChildNodes["p"];
                        if (node != null)
                        {
                            introText = Regex.Unescape(node.InnerText).Trim();
                            introText = introText.Replace(@"\", "");
                        }

                        // img node
                        node = item.ChildNodes["img"];
                        if (node != null)
                        {
                            var attribute = node.Attributes["src"];
                            if (attribute != null)
                            {
                                var imageLink = parserData.SourceMainLink + attribute.Value;
                            }
                        }

                        // Adding article to list
                        if (
                            title != null
                            && link != null
                            && introText != null
                            && text != null
                            //&& image != null
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
                                Text = text,
                                Image = image,
                                TimeStamp = timeStamp,
                                SerialTimeStamp = parserData.LastTimeStamp.AddSeconds(-articleCount),
                                New = true
                            };
                            _articles.Add(article);
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
    }
}

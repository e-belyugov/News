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
        /// Parsing html
        /// </summary>
        public async Task<IList<Article>> Parse(ParserData parserData)
        {
            try
            {
                _articles.Clear();

                string html = await _webService.GetDataAsync(parserData.SourceMainLink, 
                    Encoding.GetEncoding(parserData.SourceEncoding));

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                var headers = doc.DocumentNode.SelectNodes("//div[@class='outer-info inList']");

                foreach (HtmlNode item in headers)
                {
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
                            node = node.ChildNodes["h2"];
                            if (node != null) title = node.InnerText;
                            link = parserData.SourceMainLink + attribute.Value;

                            // Filtering atricles
                            if (title != null && title.Contains("видео")) continue;

                            // Article text
                            html = await _webService.GetDataAsync(link, Encoding.GetEncoding(parserData.SourceEncoding));
                            if (html != "")
                            {
                                HtmlDocument articleDoc = new HtmlDocument();
                                articleDoc.LoadHtml(html);
                                var articlesHeaders = articleDoc.DocumentNode.SelectNodes("//div[@class='outer-info']");

                                if (articlesHeaders.Count > 0)
                                {
                                    text = "";
                                    var articleItem = articlesHeaders[0];
                                    foreach (HtmlNode articleNode in articleItem.ChildNodes)
                                    {
                                        if (articleNode.Name == "p" || articleNode.Name == "div")
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
                                                    image = await _webService.GetImageAsync(imageLink);
                                                }
                                            }
                                        }
                                    }
                                    text = text.Replace("<br /><br />", Environment.NewLine);
                                    text = text.TrimEnd('\r', '\n', ' ');
                                }

                                // Article date
                                articlesHeaders = articleDoc.DocumentNode.SelectNodes("//div[@class='newsLineTime']");
                                if (articlesHeaders.Count > 0)
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
                        introText = introText.Replace(@"\","");
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
                        && image != null
                        && timeStamp != DateTime.MinValue
                        )
                    {
                        Article article = new Article
                        {
                            SourceMainLink = parserData.SourceMainLink,
                            SourceTitle = parserData.SourceTitle,
                            SourceLink = link,
                            Title = title,
                            IntroText = introText,
                            Text = text,
                            Image = image
                        };
                        _articles.Add(article);
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

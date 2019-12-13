using System;
using System.Collections.Generic;
using System.Text;
using News.Core.Models;
using News.Core.Services.Web;
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
        // Source data
        private readonly string _sourceMainLink = "http://kuzpress.ru";
        private readonly string _sourceTitle = "kuzpress.ru";

        // Parsed articles
        private readonly IList<Article> _articles = new List<Article>();

        // Web service
        private readonly IWebService _webService;

        /// <summary>
        /// Constructor
        /// </summary>
        public KuzpressParser(IWebService webService)
        {
            _webService = webService;
        }

        /// <summary>
        /// Parsing html
        /// </summary>
        public async Task<IList<Article>> Parse(ParserData parserData)
        {
            try
            {
                //byte[] image = await _webService.GetImageAsync("http://kuzpress.ru/i/info/600x600/35/35921.jpg");
                //byte[] image = await _webService.GetImageAsync("http://kuzpress.ru");

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

                    // a node
                    var node = item.ChildNodes["a"];
                    if (node != null)
                    {
                        var attribute = node.Attributes["href"];
                        if (attribute != null)
                        {
                            node = node.ChildNodes["h2"];
                            if (node != null) title = node.InnerText;
                            link = _sourceMainLink + attribute.Value;

                            // Filtering atricles
                            if (title != null && title.Contains("видео")) continue;

                            // Parsing article text
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
                                        //if (articleNode.Name == "p")
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
                                                text += innerText + Environment.NewLine;
                                            }
                                        }
                                    }
                                    text = text.Replace("<br /><br />", Environment.NewLine);
                                    text = text.TrimEnd('\r', '\n', ' ');
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

                    // Adding article to list
                    if (title != null && link != null && introText != null && text != null)
                    {
                        Article article = new Article
                        {
                            SourceMainLink = _sourceMainLink,
                            SourceTitle = _sourceTitle,
                            SourceLink = link,
                            Title = title,
                            IntroText = introText,
                            Text = text
                        };
                        _articles.Add(article);
                    }
                }

                return _articles;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\tERROR {0}", ex.Message);
                _articles.Clear();
                return _articles;
            }
        }
    }
}

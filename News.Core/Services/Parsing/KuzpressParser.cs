using System;
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
using MvvmCross.Binding.Binders;
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

        // Image links dictionary
        private readonly Dictionary<string, string> _imageLinks = new Dictionary<string, string>();

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
        private string GetArticleText(Article article, ParserData parserData, string html)
        {
            string cleaned = html;
            try
            {
                cleaned = cleaned.SubstringBetweenSubstrings("</h1>", "<div class=\"newsLine\">");

                // Filter by article text
                //bool skip = cleaned.Contains("youtube") 
                //            || cleaned.Contains("<table")
                //            || cleaned.Contains("vk.com/video")
                //            || cleaned.Contains("360tv");
                bool skip = false;

                if (!skip)
                {
                    // Replacing small headers
                    cleaned = cleaned.Replace("h4", "h3");
                    cleaned = cleaned.Replace("h5", "h3");
                    cleaned = cleaned.Replace("h6", "h3");
                    cleaned = cleaned.Replace("src=\"/i", "src=\"" + parserData.SourceMainLink + "/i");
                    cleaned = cleaned.Replace("src=", "width=\"500\" src=");
                    cleaned = cleaned.RemoveTagWithContent("iframe");

                    cleaned = cleaned + "<p>Ссылка на статью: <a href=\"" + article.SourceLink + "\">" + parserData.SourceTitle + "</a></p>";

                    cleaned = "<head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head>"
                        + "<font style=\"font-family:segoe ui; font-size:16px\">" + cleaned + "</font>";
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
                //_logger.Info("KUZPRESS : Begin main source REQUEST");
                string html = await _webService.GetDataAsync(parserData.SourceParseLink,
                    Encoding.GetEncoding(parserData.SourceEncoding));
                //_logger.Info("KUZPRESS : End main source REQUEST");

                // Saving last parsing time 
                parserData.LastTimeStamp = DateTime.Now;

                // Parsing main page 
                //_logger.Info("KUZPRESS : Begin main source document LOADING");
                HtmlNode.ElementsFlags.Remove("link");
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                //_logger.Info("KUZPRESS : End main source document LOADING");
                //_logger.Info("KUZPRESS : Begin main source items PARSING");
                var articleItems = doc.DocumentNode.SelectNodes("//item");
                //_logger.Info("KUZPRESS : End main source items PARSING");

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
                                //_logger.Info("KUZPRESS : Begin new article flag UNSETTING");
                                parserData.LastData = title;
                                foreach (var article in existingArticles.Where(x =>
                                    x.SourceMainLink == parserData.SourceMainLink))
                                    article.New = false;
                                //_logger.Info("KUZPRESS : End new article flag UNSETTING");

                                // Loading and parsing main source html
                                string mainHtml = await _webService.GetDataAsync(parserData.SourceMainLink,
                                    Encoding.GetEncoding(parserData.SourceEncoding));
                                _imageLinks.Clear();
                                var mainDoc = new HtmlDocument();
                                mainDoc.LoadHtml(mainHtml);
                                var mainDocHeaders = mainDoc.DocumentNode.SelectNodes("//img[contains(@class,'anons')]");
                                foreach (var mainDocNode in mainDocHeaders)
                                {
                                    var imageLink = mainDocNode.Attributes["src"]?.Value;
                                    if (imageLink != null)
                                    {
                                        imageLink = parserData.SourceMainLink + imageLink;
                                        var articleLink = mainDocNode.ParentNode?.Attributes["href"]?.Value;
                                        if (articleLink != null && !_imageLinks.ContainsKey(articleLink))
                                        {
                                            articleLink = parserData.SourceMainLink + articleLink;
                                            _imageLinks.Add(articleLink, imageLink);
                                        }
                                    }
                                }
                            }

                            // Checking if article exists already
                            //_logger.Info("KUZPRESS : Begin checking EXISTING article");
                            var existingArticle = existingArticles.FirstOrDefault(x =>
                                x.SourceMainLink == parserData.SourceMainLink && x.Title == title);
                            //_logger.Info("KUZPRESS : End checking EXISTING article");
                            if (existingArticle != null)
                            {
                                existingArticle.New = true;
                                continue;
                                //break;
                            }

                            //_logger.Info("KUZPRESS : Begin parsing ARTICLE");

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

                            //_logger.Info("KUZPRESS : End parsing ARTICLE");

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
                                    New = true,
                                    Loaded = false
                                };

                                /*
                                // Parsing article
                                _logger.Info("KUZPRESS : Begin article REQUEST");
                                var result = await ParseArticle(parserData, link, article);
                                _logger.Info("KUZPRESS : End article REQUEST");

                                // Adding article to list
                                if (result) _articles.Add(article);
                                */

                                // Article small image
                                _imageLinks.TryGetValue(link, out var imageLink);
                                article.HasSmallImage = false;
                                if (imageLink != null)
                                {
                                    article.SmallImage = await _webService.GetImageAsync(imageLink);
                                    article.HasSmallImage = true;
                                }

                                // Adding article to list
                                _articles.Add(article);
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
        public async Task<bool> ParseArticle(ParserData parserData, string link, Article article)
        {
            try
            {
                // Loading article text from web
                string html = await _webService.GetDataAsync(link, Encoding.GetEncoding(parserData.SourceEncoding));

                if (html != "")
                {
                    // Article reserved large image link
                    string reservedImageLink = "";
                    if (!article.HasSmallImage)
                    {
                        var mainDoc = new HtmlDocument();
                        mainDoc.LoadHtml(html);
                        var mainDocHeaders = mainDoc.DocumentNode.SelectNodes("//img[contains(@class,'anons')]");
                        if (mainDocHeaders != null)
                        {
                            foreach (var mainDocNode in mainDocHeaders)
                            {
                                reservedImageLink = mainDocNode.Attributes["src"]?.Value;
                                if (reservedImageLink != null) reservedImageLink = parserData.SourceMainLink + reservedImageLink;
                                break;
                            }
                        }
                    }

                    // Article text
                    string text = GetArticleText(article, parserData, html);
                    if (text.Contains("Skip")) return false; // Skipping article

                    // Article large image link
                    article.LargeImageLink = text.GetImgHref();
                    article.HasLargeImage = article.LargeImageLink != "";
                    if (!article.HasLargeImage)
                    {
                        if (article.HasSmallImage)
                        {
                            // Small image instead of large
                            _imageLinks.TryGetValue(link, out var imageLink);
                            article.LargeImageLink = imageLink;
                            article.HasLargeImage = true;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(reservedImageLink))
                            {
                                article.LargeImageLink = reservedImageLink;
                                article.HasLargeImage = true;
                            }
                        }

                    }

                    // Removing images
                    text = text.RemoveTagWithContent("img");

                    /*
                    // Article image
                    byte[] smallImage = null;
                    var articleDoc = new HtmlDocument();
                    articleDoc.LoadHtml(html);
                    var articlesHeaders = articleDoc.DocumentNode.SelectNodes("//div[@class='outer-info']");
                    if (articlesHeaders != null && articlesHeaders.Count > 0)
                    {
                        var articleItem = articlesHeaders[0];

                        foreach (HtmlNode articleNode in articleItem.ChildNodes)
                        {
                            if (articleNode.Name == "a" && smallImage == null)
                            {
                                var imgNode = articleNode.ChildNodes["img"];
                                var attribute = imgNode?.Attributes["src"];
                                if (attribute != null)
                                {
                                    var imageLink = parserData.SourceMainLink + attribute.Value;
                                    if (Path.HasExtension(imageLink))
                                    {
                                        //_logger.Info("KUZPRESS : Begin article image REQUEST");
                                        smallImage = await _webService.GetImageAsync(imageLink);
                                        //_logger.Info("KUZPRESS : End article image REQUEST");
                                        if (!article.HasLargeImage)
                                        {
                                            // Small image instead of large
                                            article.LargeImageLink = imageLink;
                                            article.HasLargeImage = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    */

                    // Logo if no image
                    //var resourceHelper = new ResourceHelper(_logger);
                    //if (image == null) image = resourceHelper.GetParserLogo(parserData.SourceTitle);

                    // Saving article fields
                    article.Text = text;
                    /*
                    article.SmallImage = smallImage;
                    article.HasSmallImage = smallImage != null;
                    */
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using News.Core.Models;
using News.Core.Services.Parsing;
using News.Core.Services.Database;
using News.Core.Services.Logging;
using System.Diagnostics;
using System.Linq;

namespace News.Core.Services
{
    /// <summary>
    /// Article service
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ArticleService : IArticleService
    {
        // Parser list
        private readonly IParserList _parsers;

        // Article database
        private readonly IArticleDatabase _database;
        private bool _databasePrepared = false;

        // Logger
        private readonly ILogger _logger;

        /// <summary>
        /// Last error description
        /// </summary>
        public string LastError => _logger.LastError;

        /// <summary>
        /// Constructor
        /// </summary>
        public ArticleService(IParserList parsers, IArticleDatabase database, ILogger logger)
        {
            _parsers = parsers;
            _database = database;
            _logger = logger;
        }

        /// <summary>
        /// Getting all articles
        /// </summary>
        public async Task<IEnumerable<Article>> GetArticlesAsync(IEnumerable<Article> localArticles)
        {
            try
            {
                // Converted articles
                var articles = (IList<Article>) localArticles;

                // Loading parser list
                IList<ParserData> parserDataList = await _database.GetParserDataAsync();

                // Checking for new articles (if remotely)
                bool gotNewArticles = false;
                foreach (var parser in _parsers.Parsers)
                {
                    string parserType = parser.ToString(); // Parser type

                    var parserData = parserDataList.FirstOrDefault(x => parserType.Contains(x.TypeName) && x.Enabled);
                    if (
                        parserData != null
                        && DateTime.Now >= parserData.LastTimeStamp.AddSeconds(parserData.Period) // Checking parsing period
                        )
                    {
                        var newArticles = await parser.Parse(parserData, articles);
                        if (newArticles.Any())
                        {
                            foreach (var newArticle in newArticles) articles.Add(newArticle);
                            if (!gotNewArticles) gotNewArticles = true;
                        }
                    }
                }

                // Saving parser data
                await _database.SaveParserDataAsync(parserDataList);

                // Processing articles (if got new)
                if (gotNewArticles)
                {
                    await _database.SaveArticlesAsync(articles);
                    await _database.DeleteOldArticlesAsync();
                }

                // Sorting and filtering articles
                articles = articles.OrderByDescending(x => x.TimeStamp).Where(x => x.New).ToList();

                return articles;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return new List<Article>();
            }
        }


        /// <summary>
        /// Getting article
        /// </summary>
        public async Task<Article> GetArticleAsync(Article article)
        {
            try
            {
                // Loading parser data
                IList<ParserData> parserDataList = await _database.GetParserDataAsync();
                var parserData = parserDataList.FirstOrDefault(x => article.SourceMainLink.Contains(x.SourceMainLink) && x.Enabled);

                // Parsing article
                if (parserData != null)
                {
                    foreach (var parser in _parsers.Parsers)
                    {
                        string parserType = parser.ToString(); // Parser type
                        if (parserType.Contains(parserData.TypeName))
                        {
                            var result = await parser.ParseArticle(parserData, article.SourceLink, article);
                            if (result)
                            {
                                // Saving article
                                article.Loaded = true;
                                await _database.SaveArticleAsync(article);
                            }
                            break;
                        }
                    }
                }

                return article;
            }
            catch (Exception e)
            {
                article.Loaded = false;
                _logger.Error(e);
                return article;
            }
        }

        /// <summary>
        /// Getting local articles
        /// </summary>
        public async Task<IEnumerable<Article>> GetLocalArticlesAsync()
        {
            try
            {
                // Preparing database
                if (!_databasePrepared)
                {
                    //_logger.Info("DATABASE : begin PREPARING");
                    await _database.PrepareAsync();
                    _databasePrepared = true;
                    //_logger.Info("DATABASE : end PREPARING");
                }

                // Loading articles from database
                //_logger.Info("DATABASE : begin LOADING");
                var articles = await _database.GetArticlesAsync();
                //_logger.Info("DATABASE : end LOADING");

                // Sorting and filtering articles
                articles = articles.OrderByDescending(x => x.TimeStamp).Where(x => x.New).ToList();

                return articles;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return new List<Article>();
            }
        }

        /*
        /// <summary>
        /// Getting string data from web page
        /// </summary>
        public async Task<IEnumerable<Article>> GetArticlesAsync(bool remotely)
        {
            try
            {
                // Preparing database
                if (!_databasePrepared)
                {
                    await _database.PrepareAsync();
                    _databasePrepared = true;
                }

                // Loading articles from database
                var articles = await _database.GetArticlesAsync();

                // Loading parser list
                IList<ParserData> parserDataList = await _database.GetParserDataAsync();

                // Checking for new articles (if remotely)
                bool gotNewArticles = false;
                if (remotely)
                {
                    foreach (var parser in _parsers.Parsers)
                    {
                        string parserType = parser.ToString(); // Parser type

                        var parserData = parserDataList.FirstOrDefault(x => parserType.Contains(x.TypeName) && x.Enabled);
                        if (
                            parserData != null
                            && DateTime.Now >= parserData.LastTimeStamp.AddSeconds(parserData.Period) // Checking parsing period
                            )
                        {
                            var newArticles = await parser.Parse(parserData, articles);
                            if (newArticles.Any())
                            {
                                foreach (var newArticle in newArticles) articles.Add(newArticle);
                                if (!gotNewArticles) gotNewArticles = true;
                            }
                        }
                    }
                }

                // Saving parser data
                await _database.SaveParserDataAsync(parserDataList);

                // Processing articles (if got new)
                if (gotNewArticles)
                {
                    await _database.SaveArticlesAsync(articles);
                    await _database.DeleteOldArticlesAsync();
                }

                // Sorting and filtering articles
                //articles = articles.OrderByDescending(x => x.TimeStamp).Where(x => x.New).Take(25).ToList();
                articles = articles.OrderByDescending(x => x.TimeStamp).Where(x => x.New).ToList();

                return articles;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return new List<Article>();
            }
        }
        */
    }
}

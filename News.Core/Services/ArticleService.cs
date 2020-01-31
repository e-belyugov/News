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
        /// Getting local articles
        /// </summary>
        public async Task<IEnumerable<Article>> GetLocalArticlesAsync()
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

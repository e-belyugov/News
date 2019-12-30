using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using News.Core.Models;
using News.Core.Services.Parsing;
using News.Core.Services.Database;
using News.Core.Services.Logging;
using System.Diagnostics;

namespace News.Core.Services
{
    /// <summary>
    /// Article service
    /// </summary>
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
        public string LastError { get => _logger.LastError; }

        /// <summary>
        /// Getting string data from web page
        /// </summary>
        public ArticleService(IParserList parsers, IArticleDatabase database, ILogger logger)
        {
            _parsers = parsers;
            _database = database;
            _logger = logger;
        }

        /// <summary>
        /// Getting string data from web page
        /// </summary>
        public async Task<IList<Article>> GetArticlesAsync()
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

                // Checking for new articles
                var newArticles = await _parsers.Parsers[0].Parse(parserDataList[0], articles);

                // Saving parser data
                var result = await _database.SaveParserDataAsync(parserDataList);

                //var result = await _database.SaveArticlesAsync(articles);

                return articles;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return new List<Article>();
            }
        }
    }
}

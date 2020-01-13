using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using News.Core.Models;
using SQLite;
using System.IO;
using News.Core.Services.Logging;

namespace News.Core.Services.Database
{
    /// <summary>
    /// Article database
    /// </summary>
    public class ArticleDatabase : IArticleDatabase
    {
        // User version
        private readonly int _userVersion = 68;

        // Logger
        private readonly ILogger _logger;

        // Connection
        private SQLiteAsyncConnection _connection;

        /// <summary>
        /// Constructor
        /// </summary>
        public ArticleDatabase(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Preparing database
        /// </summary>
        public async Task<bool> PrepareAsync()
        {
            try
            {
                // Setting connection
                string dbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NewsRadar.db3");
                _connection = new SQLiteAsyncConnection(dbPath, storeDateTimeAsTicks:false);

                // Checking database version
                int oldUserVersion = await _connection.ExecuteScalarAsync<int>("pragma user_version;");
                if (oldUserVersion != _userVersion)
                {
                    await _connection.ExecuteAsync($"pragma user_version = {_userVersion};");

                    // Recreating tables for new version
                    await _connection.DropTableAsync<ParserData>();
                    await _connection.CreateTableAsync<ParserData>();
                    await _connection.DropTableAsync<Article>();
                    await _connection.CreateTableAsync<Article>();
                }

                return true;

            }
            catch (Exception e)
            {
                _logger.Error(e);
                return false;
            }
        }

        /// <summary>
        /// Loading parser data from database
        /// </summary>
        public async Task<IList<ParserData>> GetParserDataAsync()
        {
            List<ParserData> parserDataList = await _connection.Table<ParserData>().ToListAsync();

            if (parserDataList.Count == 0)
            {
                parserDataList = new List<ParserData>() {
                    new ParserData() 
                    { 
                        Id = 0, 
                        Period = 10,
                        TypeName = "KuzpressParser",
                        SourceMainLink = "http://kuzpress.ru",
                        SourceParseLink = "http://kuzpress.ru/rss/rss.xml", 
                        SourceTitle = "kuzpress.ru", 
                        SourceEncoding = "windows-1251"} 
                };
            }

            return parserDataList;
        }

        /// <summary>
        /// Saving parser data to database
        /// </summary>
        public async Task<bool> SaveParserDataAsync(IList<ParserData> parsers)
        {
            try
            {
                foreach (var parser in parsers)
                {
                    if (parser.Id == 0)
                    {
                        var result = await _connection.InsertAsync(parser);
                    }
                    else
                    {
                        var result = await _connection.UpdateAsync(parser);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return false;
            }
        }

        /// <summary>
        /// Loading articles from database
        /// </summary>
        public async Task<IList<Article>> GetArticlesAsync()
        {
            try
            {
                List<Article> articleList = await _connection.Table<Article>().ToListAsync();
                return articleList;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return new List<Article>();
            }
        }

        /// <summary>
        /// Saving articles to database
        /// </summary>
        public async Task<bool> SaveArticlesAsync(IList<Article> articles)
        {
            try
            {
                foreach (var article in articles)
                {
                    if (article.Id == 0)
                    {
                        var result = await _connection.InsertAsync(article);
                    }
                    else
                    {
                        var result = await _connection.UpdateAsync(article);
                    }
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

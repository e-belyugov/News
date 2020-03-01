using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using News.Core.Models;
using SQLite;
using System.IO;
using System.Linq;
using System.Net;
using News.Core.Services.Logging;

namespace News.Core.Services.Database
{
    /// <summary>
    /// Article database
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ArticleDatabase : IArticleDatabase
    {
        // User version
        private readonly int _userVersion = 216;

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

                    new ParserData
                    {
                        Id = 0,
                        Period = 10,
                        TypeName = "KuzpressParser",
                        SourceMainLink = "http://kuzpress.ru",
                        SourceParseLink = "http://kuzpress.ru/rss/rss.xml",
                        SourceTitle = "kuzpress.ru",
                        SourceEncoding = "windows-1251",
                        Enabled = true
                    },

                    new ParserData
                    {
                        Id = 0,
                        Period = 10,
                        TypeName = "NktvParser",
                        SourceMainLink = "https://nk-tv.com",
                        SourceParseLink = "https://nk-tv.com/feed",
                        SourceTitle = "nk-tv.com",
                        SourceEncoding = "utf-8",
                        Enabled = true
                    },

                    new ParserData
                    {
                        Id = 0,
                        Period = 20,
                        TypeName = "VashgorodParser",
                        SourceMainLink = "https://vashgorod.ru",
                        SourceParseLink = "https://vashgorod.ru/novokuznetsk",
                        SourceTitle = "vashgorod.ru",
                        SourceEncoding = "utf-8",
                        Enabled = true
                    }
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
                var articleList = await _connection.Table<Article>().ToListAsync();
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
                // Existing articles
                var existingArticles = await _connection.Table<Article>().ToListAsync();

                foreach (var article in articles)
                {
                    if (article.Id == 0)
                    {
                        // Checking if article exists (before insert)
                        var existingArticle = existingArticles.SingleOrDefault(x => x.Title == article.Title);
                        if (existingArticle != null) continue;

                        // Insert
                        await _connection.InsertAsync(article);
                    }
                    else
                    {
                        // Update
                        await _connection.UpdateAsync(article);
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
        /// Saving article to database
        /// </summary>
        public async Task<bool> SaveArticleAsync(Article article)
        {
            try
            {
                await _connection.UpdateAsync(article);
                return true;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return false;
            }
        }

        /// <summary>
        /// Deleting old articles
        /// </summary>
        public async Task<bool> DeleteOldArticlesAsync()
        {
            try
            {
                var existingArticles = await _connection.Table<Article>().ToListAsync();
                foreach (var article in existingArticles.Where(x => !x.New))
                {
                    await _connection.DeleteAsync(article);
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

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
        // Logger
        private readonly ILogger _logger;

        // Logger
        readonly SQLiteAsyncConnection _database;

        /// <summary>
        /// Constructor
        /// </summary>
        public ArticleDatabase(ILogger logger)
        {
            _logger = logger;

            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NewsRadar.db3");
            
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<ParserData>().Wait();
        }

        /// <summary>
        /// Loading parser data from database
        /// </summary>
        public async Task<List<ParserData>> GetParserDataAsync()
        {
            List<ParserData> list = await _database.Table<ParserData>().ToListAsync();

            if (list.Count == 0)
            {
                list = new List<ParserData>() {
                    new ParserData() { Id = 0, 
                        SourceMainLink = "http://kuzpress.ru", 
                        SourceTitle = "kuzpress.ru", 
                        SourceEncoding = "windows-1251"} };
            }

            var result = await SaveParserDataAsync(list);

            return list;
        }

        /// <summary>
        /// Saving parser data to database
        /// </summary>
        public async Task<bool> SaveParserDataAsync(List<ParserData> parsers)
        {
            try
            {
                foreach (var parser in parsers)
                {
                    if (parser.Id == 0)
                    {
                        var result = await _database.InsertAsync(parser);
                    }
                    else
                    {
                        var result = await _database.UpdateAsync(parser);
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

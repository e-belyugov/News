using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using News.Core.Models;
using News.Core.Services.Parsing;
using News.Core.Services.Database;
using System.Diagnostics;

namespace News.Core.Services
{
    /// <summary>
    /// Article service
    /// </summary>
    public class ArticleService : IArticleService
    {
        // Parser list
        readonly IParserList _parsers;

        // Article database
        readonly IArticleDatabase _database;

        /// <summary>
        /// Getting string data from web page
        /// </summary>
        public ArticleService(IParserList parsers, IArticleDatabase database)
        {
            _parsers = parsers;
            _database = database;
        }

        /// <summary>
        /// Getting string data from web page
        /// </summary>
        public async Task<IList<Article>> GetArticlesAsync()
        {
            try
            {
                IList<ParserData> parserDataList = _database.GetParserDataAsync();
                return await _parsers.Parsers[0].Parse(parserDataList[0]);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\tERROR {0}", ex.Message);
                return null;
            }
        }
    }
}

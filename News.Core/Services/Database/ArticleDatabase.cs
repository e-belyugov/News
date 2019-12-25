using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using News.Core.Models;
using SQLite;
using System.IO;

namespace News.Core.Services.Database
{
    /// <summary>
    /// Article database
    /// </summary>
    public class ArticleDatabase : IArticleDatabase
    {
        readonly SQLiteAsyncConnection _database;

        /// <summary>
        /// Constructor
        /// </summary>
        public ArticleDatabase()
        {
            string dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "News.db3");
            
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<ParserData>().Wait();
        }

        /// <summary>
        /// Loading ParserData from database
        /// </summary>
        public List<ParserData> GetParserDataAsync()
        {
            List<ParserData> list = new List<ParserData>() {
                    new ParserData() { SourceMainLink = "http://kuzpress.ru", SourceTitle = "kuzpress.ru", SourceEncoding = "windows-1251"} };

            return list;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using News.Core.Models;

namespace News.Core.Services.Database
{
    /// <summary>
    /// Article database
    /// </summary>
    public class ArticleDatabase : IArticleDatabase
    {
        /// <summary>
        /// Loading ParserData from database
        /// </summary>
        public List<ParserData> GetParserDataAsync()
        {
            List<ParserData> list = new List<ParserData>() {
                    new ParserData() { SourceMainLink = "http://kuzpress.ru", SourceEncoding = "windows-1251"} };

            return list;
        }
    }
}

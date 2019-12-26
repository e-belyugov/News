using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using News.Core.Models;

namespace News.Core.Services.Database
{
    /// <summary>
    /// Article database interface
    /// </summary>
    public interface IArticleDatabase
    {
        /// <summary>
        /// Loading ParserData from database
        /// </summary>
        Task<List<ParserData>> GetParserDataAsync();
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using News.Core.Models;
using News.Core.Services.Parsing;
using News.Core.Services.Database;

namespace News.Core.Services
{
    /// <summary>
    /// Article service interface
    /// </summary>
    public interface IArticleService
    {
        /// <summary>
        /// Getting string data from web page
        /// </summary>
        Task<IEnumerable<Article>> GetArticlesAsync(bool remotely);

        /// <summary>
        /// Last error description
        /// </summary>
        string LastError { get; }
    }
}

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
        /// Getting all articles
        /// </summary>
        Task<IEnumerable<Article>> GetArticlesAsync(IEnumerable<Article> localArticles);

        /// <summary>
        /// Getting article
        /// </summary>
        Task<Article> GetArticleAsync(Article article);

        /// <summary>
        /// Getting local articles
        /// </summary>
        Task<IEnumerable<Article>> GetLocalArticlesAsync();

        /// <summary>
        /// Last error description
        /// </summary>
        string LastError { get; }
    }
}

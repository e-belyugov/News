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
        /// Preparing database
        /// </summary>
        Task<bool> PrepareAsync();

        /// <summary>
        /// Loading ParserData from database
        /// </summary>
        Task<IList<ParserData>> GetParserDataAsync();

        /// <summary>
        /// Saving parser data to database
        /// </summary>
        Task<bool> SaveParserDataAsync(IList<ParserData> parserList);

        /// <summary>
        /// Loading articles from database
        /// </summary>
        Task<IList<Article>> GetArticlesAsync();

        /// <summary>
        /// Saving articles to database
        /// </summary>
        Task<bool> SaveArticlesAsync(IList<Article> articles);

        /// <summary>
        /// Saving article to database
        /// </summary>
        Task<bool> SaveArticleAsync(Article article);

        /// <summary>
        /// Deleting old articles
        /// </summary>
        Task<bool> DeleteOldArticlesAsync();
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using News.Core.Models;
using System.Threading.Tasks;

namespace News.Core.Services.Parsing
{
    /// <summary>
    /// Parser interface
    /// </summary>
    public interface IParser
    {
        /// <summary>
        /// Parsing articles
        /// </summary>
        Task<IList<Article>> Parse(ParserData parserData, IList<Article> existingArticles);

        /// <summary>
        /// Parsing article
        /// </summary>
        Task<bool> ParseArticle(ParserData parserData, string link, Article article);
    }
}

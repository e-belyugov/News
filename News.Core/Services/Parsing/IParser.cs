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
        /// Parsing html
        /// </summary>
        Task<IList<Article>> Parse(ParserData parserData);
    }
}

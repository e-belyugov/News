using System;
using System.Collections.Generic;
using System.Text;

namespace News.Core.Services.Parsing
{
    /// <summary>
    /// Parser list interface
    /// </summary>
    public interface IParserList
    {
        /// <summary>
        /// Parser list
        /// </summary>
        IList<IParser> Parsers { get; }
    }
}

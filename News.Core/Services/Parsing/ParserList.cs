using System;
using System.Collections.Generic;
using System.Text;
using News.Core.Services.Web;

namespace News.Core.Services.Parsing
{
    /// <summary>
    /// Parser list
    /// </summary>
    public class ParserList : IParserList
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ParserList(IWebService webService)
        {
            IParser kuzpressParser = new KuzpressParser(webService);
            Parsers.Add(kuzpressParser);
        }

        /// <summary>
        /// Parser list
        /// </summary>
        public IList<IParser> Parsers { get; } = new List<IParser>();
    }
}

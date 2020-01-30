using System;
using System.Collections.Generic;
using System.Text;
using News.Core.Services.Web;
using News.Core.Services.Logging;

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
        public ParserList(IWebService webService, ILogger logger)
        {
            IParser kuzpressParser = new KuzpressParser(webService, logger);
            Parsers.Add(kuzpressParser);

            IParser nktvParser = new NktvParser(webService, logger);
            Parsers.Add(nktvParser);

            IParser vashgorodParser = new VashgorodParser(webService, logger);
            Parsers.Add(vashgorodParser);
        }

        /// <summary>
        /// Parser list
        /// </summary>
        public IList<IParser> Parsers { get; } = new List<IParser>();
    }
}

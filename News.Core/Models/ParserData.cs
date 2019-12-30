using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace News.Core.Models
{
    /// <summary>
    /// Parser data
    /// </summary>
    public class ParserData
    {
        /// <summary>
        /// Parser identifier
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Parser type name
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Parser period (in seconds)
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Parser last timestamp
        /// </summary>
        public int LastTimeStamp { get; set; }

        /// <summary>
        /// Source main link
        /// </summary>
        public string SourceMainLink { get; set; }

        /// <summary>
        /// Source title
        /// </summary>
        public string SourceTitle { get; set; }

        /// <summary>
        /// Source encoding
        /// </summary>
        public string SourceEncoding { get; set; }

        /// <summary>
        /// Parser last data
        /// </summary>
        public string LastData { get; set; }
    }
}

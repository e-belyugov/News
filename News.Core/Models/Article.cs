using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace News.Core.Models
{
    /// <summary>
    /// Article
    /// </summary>
    public class Article
    {
        /// <summary>
        /// Article identifier
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Article timestamp
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Article title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Article intro text
        /// </summary>
        public string IntroText { get; set; }

        /// <summary>
        /// Article text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Article source title
        /// </summary>
        public string SourceTitle { get; set; }

        /// <summary>
        /// Article source main link
        /// </summary>
        public string SourceMainLink { get; set; }

        /// <summary>
        /// Article source link
        /// </summary>
        public string SourceLink { get; set; }

        /// <summary>
        /// Article small image
        /// </summary>
        public byte[] SmallImage { get; set; }

        /// <summary>
        /// Article small image flag
        /// </summary>
        public bool HasSmallImage { get; set; }

        /// <summary>
        /// Article large image link
        /// </summary>
        public string LargeImageLink { get; set; }

        /// <summary>
        /// Article large image flag
        /// </summary>
        public bool HasLargeImage { get; set; }
        /// <summary>
        /// New article flag
        /// </summary>
        public bool New { get; set; }
    }
}

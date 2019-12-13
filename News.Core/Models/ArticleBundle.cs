using System;
using System.Collections.Generic;
using System.Text;

namespace News.Core.Models
{
    /// <summary>
    /// Article bundle
    /// </summary>
    public class ArticleBundle
    {
        /// <summary>
        /// Article list
        /// </summary>
        public IList<Article> ArticleList { get; set; }

        /// <summary>
        /// Selected article
        /// </summary>
        public Article SelectedArticle { get; set; }
    }
}

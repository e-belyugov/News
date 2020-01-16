using System;
using MvvmCross.Forms.Presenters.Attributes;
using Xamarin.Forms;
using MvvmCross.Forms.Views;
using News.Core.ViewModels;

namespace News.Forms.UI.Pages
{
    /// <summary>
    /// Article view
    /// </summary>
    [MvxMasterDetailPagePresentation(WrapInNavigationPage = true)]
    public partial class ArticleView : MvxContentPage<ArticleViewModel>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ArticleView()
        {
            InitializeComponent();
        }
    }
}
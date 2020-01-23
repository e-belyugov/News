using System;
using System.Diagnostics;
using System.Windows.Input;
using MvvmCross.Forms.Presenters.Attributes;
using Xamarin.Forms;
using MvvmCross.Forms.Views;
using News.Core.ViewModels;
using Xamarin.Essentials;

namespace News.Forms.UI.Pages
{
    /// <summary>
    /// Article view
    /// </summary>
    //[MvxMasterDetailPagePresentation(WrapInNavigationPage = true)]
    public partial class ArticleView : MvxContentPage<ArticleViewModel>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ArticleView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// OnAppearing event
        /// </summary>
        protected override void OnAppearing()
        {
        }

        /// <summary>
        /// Article view navigating event
        /// </summary>
        private void ArticleWebView_OnNavigating(object sender, WebNavigatingEventArgs e)
        {
            if (e.Url.Contains("http"))
            {
                try
                {
                    var webView = sender as WebView;
                    webView?.GoBack();
                    Launcher.OpenAsync(new Uri(e.Url));
                }
                catch (Exception)
                {
                    e.Cancel = true;
                }
                e.Cancel = true;
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
        }
    }
}
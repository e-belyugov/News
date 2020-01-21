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
        public ICommand TapCommand => new Command<string>(OpenBrowser);

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
            //ArticleWebView
        }

        private void ArticleWebView_OnNavigating(object sender, WebNavigatingEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Url));
        }

        void OpenBrowser(string url)
        {
            //Device.OpenUri(new Uri(url));
            Launcher.OpenAsync(new Uri(url));
        }
    }
}
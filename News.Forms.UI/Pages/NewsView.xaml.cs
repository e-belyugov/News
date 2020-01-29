using System.Collections;
using System.Collections.Generic;
using MvvmCross.Forms.Presenters.Attributes;
using Xamarin.Forms;
using MvvmCross.Forms.Views;
using News.Core.ViewModels;
using Plugin.Toast;
using Plugin.Toast.Abstractions;
using Xamarin.Essentials;

namespace News.Forms.UI.Pages
{
    /// <summary>
    /// News view
    /// </summary>
    //[MvxMasterDetailPagePresentation(MasterDetailPosition.Detail, WrapInNavigationPage = true, NoHistory = true)]
    public partial class NewsView : MvxContentPage<NewsViewModel>
    {
        // First run flag
        private bool _firstRun = true;

        // ViewModel
        private NewsViewModel _newsViewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public NewsView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// OnAppearing event
        /// </summary>
        protected override void OnAppearing()
        {
            if (_firstRun)
            {
                _firstRun = false;
                RefreshButton_Clicked(null, null);
            }
        }

        /// <summary>
        /// Refresh button click
        /// </summary>
        private void RefreshButton_Clicked(object sender, System.EventArgs e)
        {
            _newsViewModel = DataContext as NewsViewModel;
            if (_newsViewModel == null) return;

            var current = Connectivity.NetworkAccess;
            bool remotely = current == NetworkAccess.Internet;

            if (!remotely)
                CrossToastPopUp.Current.ShowToastWarning("Интернет соединение отсутствует. Будут загружены статьи из памяти устройства", 
                    ToastLength.Long);

            _newsViewModel?.RefreshArticlesCommand.Execute(remotely);

            ScrollToTop();
        }

        /// <summary>
        /// Scrolling listvew to top
        /// </summary>
        private void ScrollToTop()
        {
            var itemsSource = (IList)NewsListView.ItemsSource;
            if (itemsSource != null && itemsSource.Count != 0) 
                NewsListView.ScrollTo(itemsSource[0], ScrollToPosition.Start, false);
        }

        /// <summary>
        /// Home button click
        /// </summary>
        private void HomeButton_Clicked(object sender, System.EventArgs e)
        {
            ScrollToTop();
        }

        /// <summary>
        /// OnSizeAllocated event
        /// </summary>
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            ButtonStackLayout.HeightRequest = width > height ? 200 : 85;
        }
    }
}
using MvvmCross.Forms.Presenters.Attributes;
using Xamarin.Forms;
using MvvmCross.Forms.Views;
using News.Core.ViewModels;

namespace News.Forms.UI.Pages
{
    /// <summary>
    /// News view
    /// </summary>
    //[MvxMasterDetailPagePresentation(MasterDetailPosition.Detail, WrapInNavigationPage = true, NoHistory = true)]
    public partial class NewsView : MvxContentPage<NewsViewModel>
    {
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
        }

        /// <summary>
        /// Refresh button click
        /// </summary>
        private void RefreshButton_Clicked(object sender, System.EventArgs e)
        {
            //if (_newsViewModel != null) _newsViewModel.RefreshDataCommand.Execute();
        }

        /// <summary>
        /// News ListView Binding context changed event
        /// </summary>
        private void NewsCollectionView_BindingContextChanged(object sender, System.EventArgs e)
        {
            _newsViewModel = DataContext as NewsViewModel;
        }

        /// <summary>
        /// Home button click
        /// </summary>
        private void HomeButton_Clicked(object sender, System.EventArgs e)
        {
            //NewsCollectionView.ScrollTo(0);
        }

        /// <summary>
        /// News CollectionView selection changed event
        /// </summary>
        private void NewsCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (_newsViewModel != null) _newsViewModel.NavigateCommand.Execute();
        }

        /// <summary>
        /// News CollectionView size changed event
        /// </summary>
        private void NewsCollectionView_SizeChanged(object sender, System.EventArgs e)
        {
            //NewsCollectionView.IsVisible = true;
            //if (_newsViewModel != null && _newsViewModel.SelectedArticle != null)
            //{
            //    NewsCollectionView.ScrollTo(_newsViewModel.SelectedArticle);
            //}
        }
    }
}
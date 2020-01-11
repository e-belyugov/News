using Xamarin.Forms;
using MvvmCross.Forms.Views;
using News.Core.ViewModels;

namespace News.Forms.UI.Pages
{
    /// <summary>
    /// News view
    /// </summary>
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
        /// Refresh button click
        /// </summary>
        private void RefreshButton_Clicked(object sender, System.EventArgs e)
        {
            if (_newsViewModel != null) _newsViewModel.RefreshDataCommand.Execute();
        }

        /// <summary>
        /// News ListView Binding context changed event
        /// </summary>
        private void NewsCollectionView_BindingContextChanged(object sender, System.EventArgs e)
        {
            _newsViewModel = DataContext as NewsViewModel;
        }
    }
}
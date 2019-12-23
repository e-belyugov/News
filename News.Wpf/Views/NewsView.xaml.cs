using MvvmCross.Platforms.Wpf.Views;
using News.Core.ViewModels;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using System.Windows.Controls;
using News.Core.Models;

namespace News.WPF.Views
{
    /// <summary>
    /// News view
    /// </summary>
    [MvxContentPresentation(WindowIdentifier = nameof(MainWindow), StackNavigation = true)]
    public partial class NewsView : MvxWpfView
    {
        private NewsViewModel _newsViewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public NewsView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// View loaded event
        /// </summary>
        private void NewsListView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _newsViewModel = DataContext as NewsViewModel;

            // Some article selected
            if (_newsViewModel != null 
                && _newsViewModel.SelectedArticle != null)
                NewsListView.ScrollIntoView(_newsViewModel.SelectedArticle);
        }

        /// <summary>
        /// Listview selection changed event
        /// </summary>
        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!NewsListView.IsLoaded) return;
            if (_newsViewModel != null) _newsViewModel.NavigateCommand.Execute();
        }

        /// <summary>
        /// Refresh button click
        /// </summary>
        private void RefreshButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_newsViewModel != null) _newsViewModel.RefreshDataCommand.Execute();
        }

        /// <summary>
        /// Listview preview mouse down event
        /// </summary>
        private void NewsListView_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!NewsListView.IsLoaded) return;
            if (_newsViewModel != null) _newsViewModel.NavigateCommand.Execute();
        }
    }
}

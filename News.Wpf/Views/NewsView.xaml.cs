using MvvmCross.Platforms.Wpf.Views;
using News.Core.ViewModels;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using System.Windows.Controls;
using News.Core.Models;
using System.Windows;
using System.Windows.Media;

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

            // Scroll to selected article
            if (_newsViewModel != null
                && _newsViewModel.SelectedArticle != null)
            {
                NewsListView.ScrollIntoView(_newsViewModel.SelectedArticle);
            }
        }

        /// <summary>
        /// Refresh button click
        /// </summary>
        private void RefreshButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _newsViewModel?.RefreshArticlesCommand.Execute();
        }

        /// <summary>
        /// Listview preview mouse down event
        /// </summary>
        private void NewsListView_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!NewsListView.IsLoaded) return;

            // Detecting listview item
            DependencyObject listViewItem = (DependencyObject)e.OriginalSource;
            while (listViewItem != null && !(listViewItem is ListViewItem))
                listViewItem = VisualTreeHelper.GetParent(listViewItem);

            // Selecting listview item
            if (listViewItem != null)
            {
                NewsListView.SelectedItem = ((ListViewItem)listViewItem).Content;
                _newsViewModel?.NavigateToArticleCommand.Execute();
            }
        }
    }
}

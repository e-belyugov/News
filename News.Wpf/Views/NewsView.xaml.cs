using MvvmCross.Platforms.Wpf.Views;
using News.Core.ViewModels;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using System.Windows;

namespace News.WPF.Views
{
    [MvxContentPresentation(WindowIdentifier = nameof(MainWindow), StackNavigation = true)]
    public partial class NewsView : MvxWpfView
    {
        public NewsView()
        {
            InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!NewsListView.IsLoaded) return;

            NewsViewModel newsViewModel = DataContext as NewsViewModel;
            newsViewModel.NavigateCommand.Execute();
        }

        private void RefreshButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NewsViewModel newsViewModel = DataContext as NewsViewModel;
            newsViewModel.RefreshDataCommand.Execute();
        }
    }
}

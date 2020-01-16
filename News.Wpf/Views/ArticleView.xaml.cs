using System.Windows;
using MvvmCross.Platforms.Wpf.Views;
using News.Core.ViewModels;
using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using System.Diagnostics;
using System.Windows.Navigation;

namespace News.Wpf.Views
{
    /// <summary>
    /// Interaction logic for ArticleView.xaml
    /// </summary>
    [MvxContentPresentation]
    public partial class ArticleView : MvxWpfView
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ArticleView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Back button click
        /// </summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ArticleViewModel articleViewModel = DataContext as ArticleViewModel;
            articleViewModel?.NavigateToNewsCommand.Execute();
        }

        /// <summary>
        /// Navigate to web page
        /// </summary>
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}

using System.Windows;
using MvvmCross.Platforms.Wpf.Views;
using News.Core.ViewModels;

namespace News.Wpf.Views
{
    /// <summary>
    /// Interaction logic for ArticleView.xaml
    /// </summary>
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
            articleViewModel.NavigateCommand.Execute();
        }
    }
}

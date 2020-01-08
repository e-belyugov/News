using Xamarin.Forms;
using MvvmCross.Forms.Views;
using News.Core.ViewModels;

namespace News.Forms.UI.Pages
{
    public partial class NewsView : MvxContentPage<NewsViewModel>
    {
        public NewsView()
        {
            InitializeComponent();
        }
    }
}
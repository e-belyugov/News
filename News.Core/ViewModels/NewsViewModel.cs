using System;
using MvvmCross.ViewModels;
using MvvmCross.Navigation;
using MvvmCross.Commands;
using System.Threading.Tasks;
using News.Core.Services;
using News.Core.Models;
using System.Linq;

namespace News.Core.ViewModels
{
    /// <summary>
    /// News ViewModel
    /// </summary>
    public class NewsViewModel : MvxViewModel
    {
        // Article service
        readonly IArticleService _articleService;

        // Navigation service
        readonly IMvxNavigationService _navigationService;

        // -----------------------------------------------
        /// <summary>
        /// Article observable collection
        /// </summary>
        private MvxObservableCollection<Article> _articles;
        public MvxObservableCollection<Article> Articles
        {
            get => _articles;
            set
            {
                _articles = value;
                RaisePropertyChanged(() => Articles);
            }
        }

        // -----------------------------------------------
        /// <summary>
        /// Article selected command
        /// </summary>
        public IMvxCommand<Article> ArticleSelectedCommand { get; private set; }

        // -----------------------------------------------
        /// <summary>
        /// Selected article
        /// </summary>
        public Article SelectedArticle { get; set; }

        // -----------------------------------------------
        /// <summary>
        /// Refresh articles command
        /// </summary>
        //public IMvxCommand RefreshArticlesCommand { get; private set; }
        public IMvxCommand<bool> RefreshArticlesCommand { get; private set; }

        // -----------------------------------------------
        /// <summary>
        /// Navigate to article command
        /// </summary>
        private IMvxAsyncCommand _navigateToArticleCommand;
        public IMvxAsyncCommand NavigateToArticleCommand
        {
            get
            {
                _navigateToArticleCommand = _navigateToArticleCommand ?? new MvxAsyncCommand(() =>
                                       _navigationService.Navigate<ArticleViewModel, Article>(SelectedArticle));
                return _navigateToArticleCommand;
            }
        }

        // -----------------------------------------------
        /// <summary>
        /// Busy flag
        /// </summary>
        private bool _isBusy;
        public bool IsBusy {
            get => _isBusy;
            set
            {
                _isBusy = value;
                RaisePropertyChanged(() => IsBusy);
            }
        }

        // -----------------------------------------------
        /// <summary>
        /// Load articles task
        /// </summary>
        private MvxNotifyTask LoadArticlesTask { get; set; }

        // -----------------------------------------------
        /// <summary> 
        /// Last error string
        /// </summary>
        private string _lastError = "";
        public string LastError 
        { 
            get => _lastError;
            set
            {
                _lastError = value;
                RaisePropertyChanged(() => LastError);
            }
        }

        // -----------------------------------------------
        // -----------------------------------------------
        // -----------------------------------------------

        /// <summary>
        /// Constructor
        /// </summary>
        public NewsViewModel(IArticleService articleService, IMvxNavigationService navigationService)
        {
            _articleService = articleService;
            _navigationService = navigationService;

            Articles = new MvxObservableCollection<Article>();

            ArticleSelectedCommand = new MvxAsyncCommand<Article>(ArticleSelected);
            RefreshArticlesCommand = new MvxCommand<bool>(RefreshArticles);

            IsBusy = false;
        }

        // -----------------------------------------------
        /// <summary>
        /// Initializing ViewModel
        /// </summary>
        public override Task Initialize()
        {
            //LoadArticlesTask = MvxNotifyTask.Create(LoadArticles);

            return base.Initialize();
        }

        // -----------------------------------------------
        /// <summary>
        /// Loading articles
        /// </summary>
        public async Task LoadArticles(bool remotely)
        {
            try
            {
                _lastError = "";
                IsBusy = true;

                var result = await _articleService.GetArticlesAsync(remotely);
                Articles.Clear();
                foreach (var article in result) Articles.Add(article);

                _lastError = _articleService.LastError;
                await RaisePropertyChanged(nameof(LastError));

                IsBusy = false;
            }
            catch (Exception)
            { 
                IsBusy = false;
            }
        }

        // -----------------------------------------------
        /// <summary>
        /// Selecting article
        /// </summary>
        private async Task ArticleSelected(Article selectedArticle)
        {
            await _navigationService.Navigate<ArticleViewModel, Article>(selectedArticle);
        }

        // -----------------------------------------------
        /// <summary>
        /// Refreshing articles
        /// </summary>
        private void RefreshArticles(bool remotely)
        {
            LoadArticlesTask = MvxNotifyTask.Create(LoadArticles(remotely));
            RaisePropertyChanged(() => LoadArticlesTask);
        }
    }
}

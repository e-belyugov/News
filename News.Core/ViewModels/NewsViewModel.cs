using System;
using System.Collections.Generic;
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
    // ReSharper disable once ClassNeverInstantiated.Global
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
        public IMvxCommand<Article> ArticleSelectedCommand { get; }

        // -----------------------------------------------
        /// <summary>
        /// Selected article
        /// </summary>
        public Article SelectedArticle { get; set; }

        // -----------------------------------------------
        /// <summary>
        /// Load articles command
        /// </summary>
        public IMvxCommand<bool> LoadArticlesCommand { get; }

        // -----------------------------------------------
        /// <summary>
        /// Refresh articles command
        /// </summary>
        public IMvxCommand<bool> RefreshArticlesCommand { get; }

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
            LoadArticlesCommand = new MvxCommand<bool>(LoadArticles);

            IsBusy = false;
        }

        // -----------------------------------------------
        /// <summary>
        /// Loading articles task
        /// </summary>
        private async Task LoadArticles(bool firstRun, bool remotely)
        {
            try
            {
                //_lastError = "";
                //await RaisePropertyChanged(nameof(LastError));
                LastError = "";
                IsBusy = true;

                //var result = await _articleService.GetArticlesAsync(remotely);
                //Articles.Clear();
                //foreach (var article in result) Articles.Add(article);

                // Result
                IEnumerable<Article> result;

                // First run
                if (firstRun)
                {
                    // Local articles
                    result = await _articleService.GetLocalArticlesAsync();
                    Articles.Clear();
                    foreach (var article in result) Articles.Add(article);
                }

                // Getting remote data
                if (remotely)
                {
                    // Old data
                    var oldCount = Articles.Count;
                    var oldTitle = Articles.Any() ? Articles[0].Title : "";

                    // All articles
                    result = await _articleService.GetArticlesAsync(localArticles: Articles);

                    // New data
                    var newCount = Articles.Count;
                    var newTitle = Articles.Any() ? Articles[0].Title : "";

                    // Checking for new data
                    if (newCount != oldCount || oldTitle != newTitle)
                    {
                        Articles.Clear();
                        foreach (var article in result) Articles.Add(article);
                    }
                }

                //_lastError = _articleService.LastError;
                //await RaisePropertyChanged(nameof(LastError));
                IsBusy = false;
                LastError = _articleService.LastError;
            }
            finally
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
            try
            {
                LastError = "";
                IsBusy = true;

                if (!selectedArticle.Loaded || String.IsNullOrEmpty(selectedArticle.Text))
                {
                    await _articleService.GetArticleAsync(selectedArticle);
                    var article = Articles.FirstOrDefault(x => x.Id == selectedArticle.Id);
                    if (article != null)
                    {
                        article.IntroText = selectedArticle.IntroText;
                        await RaisePropertyChanged(nameof(Articles));
                    }
                }

                await _navigationService.Navigate<ArticleViewModel, Article>(selectedArticle);

                IsBusy = false;
                LastError = _articleService.LastError;
            }
            finally
            {
                IsBusy = false;
            }
        }

        // -----------------------------------------------
        /// <summary>
        /// Refreshing articles
        /// </summary>
        private void LoadArticles(bool remotely)
        {
            LoadArticlesTask = MvxNotifyTask.Create(LoadArticles(true, remotely));
            RaisePropertyChanged(() => LoadArticlesTask);
        }

        // -----------------------------------------------
        /// <summary>
        /// Refreshing articles
        /// </summary>
        private void RefreshArticles(bool remotely)
        {
            LoadArticlesTask = MvxNotifyTask.Create(LoadArticles(false, remotely));
            RaisePropertyChanged(() => LoadArticlesTask);
        }
    }
}

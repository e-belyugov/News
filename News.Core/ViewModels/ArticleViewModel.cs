using System;
using System.Collections.Generic;
using System.Text;
using MvvmCross.ViewModels;
using News.Core.Models;
using MvvmCross.Commands;
using MvvmCross.Navigation;

namespace News.Core.ViewModels
{
    /// <summary>
    /// Article ViewModel
    /// </summary>
    public class ArticleViewModel : MvxViewModel<ArticleBundle>
    {
        // Article list
        private IList<Article> _articles = new List<Article>();

        // Navigation service
        readonly IMvxNavigationService _navigationService;

        /// <summary>
        /// Navigate command
        /// </summary>
        private IMvxAsyncCommand _navigateCommand;
        public IMvxAsyncCommand NavigateCommand
        {
            get
            {
                var articleBundle = new ArticleBundle() { ArticleList = _articles, SelectedArticle = _selectedArticle };

                _navigateCommand = _navigateCommand ?? new MvxAsyncCommand(() =>
                    _navigationService.Navigate<NewsViewModel, ArticleBundle>(articleBundle));
                return _navigateCommand;
            }
        }

        /// <summary>
        /// Selected article (backend field)
        /// </summary>
        public Article _selectedArticle;

        /// <summary>
        /// Selected article
        /// </summary>
        public Article SelectedArticle { get { return _selectedArticle; } set { _selectedArticle = value; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public ArticleViewModel(IMvxNavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        /// <summary>
        /// Preparing view model
        /// </summary>
        public override void Prepare(ArticleBundle articleBundle)
        {
            _articles = articleBundle.ArticleList;
            _selectedArticle = articleBundle.SelectedArticle;
        }
    }
}

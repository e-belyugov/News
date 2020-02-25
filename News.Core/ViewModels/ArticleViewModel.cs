using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MvvmCross.ViewModels;
using News.Core.Models;
using MvvmCross.Commands;
using MvvmCross.Navigation;

namespace News.Core.ViewModels
{
    /// <summary>
    /// Article ViewModel
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ArticleViewModel : MvxViewModel<Article>
    {
        // Navigation service
        readonly IMvxNavigationService _navigationService;

        /// <summary>
        /// Active article
        /// </summary>
        private Article _article;
        public Article Article
        {
            get => _article;
            set
            {
                _article = value;
                RaisePropertyChanged(() => Article);
            }
        }

        /// <summary>
        /// Navigate command
        /// </summary>
        private IMvxAsyncCommand _navigateToNewsCommand;
        public IMvxAsyncCommand NavigateToNewsCommand
        {
            get
            {
                _navigateToNewsCommand = _navigateToNewsCommand ?? new MvxAsyncCommand(() => _navigationService.Navigate<NewsViewModel>());
                return _navigateToNewsCommand;
            }
        }

        // -----------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        public ArticleViewModel(IMvxNavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        // -----------------------------------------------
        /// <summary>
        /// Initializing ViewModel
        /// </summary>
        public override Task Initialize()
        {
            return Task.FromResult(0);
        }

        // -----------------------------------------------
        /// <summary>
        /// Preparing ViewModel
        /// </summary>
        public override void Prepare(Article parameter)
        {
            Article = parameter;
        }
    }
}

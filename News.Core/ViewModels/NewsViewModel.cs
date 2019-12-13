﻿using System;
using System.Collections.Generic;
using System.Text;
using MvvmCross.ViewModels;
using MvvmCross.Navigation;
using MvvmCross.Commands;
using System.Threading.Tasks;
using News.Core.Services;
using News.Core.Models;
using System.Diagnostics;
using System.Collections.ObjectModel;
using MvvmCross.Base;
using MvvmCross;

namespace News.Core.ViewModels
{
    /// <summary>
    /// News ViewModel
    /// </summary>
    public class NewsViewModel : MvxViewModel<ArticleBundle>
    {
        // Article service
        readonly IArticleService _articleService;

        // Navigation service
        readonly IMvxNavigationService _navigationService;

        // Article list
        private IList<Article> _articles = new List<Article>();

        /// <summary>
        /// Article observable collection
        /// </summary>
        public ObservableCollection<Article> Articles { get { return new ObservableCollection<Article>(_articles); } set { _articles = value; } }

        /// <summary>
        /// Selected article
        /// </summary>
        public Article SelectedArticle { get; set; }

        /// <summary>
        /// Busy flag
        /// </summary>
        private bool _isBusy;
        public bool IsBusy {
            get => _isBusy; 
            set => SetProperty(ref _isBusy, value); 
        }

        /// <summary>
        /// Navigate command
        /// </summary>
        private IMvxAsyncCommand _navigateCommand;
        public IMvxAsyncCommand NavigateCommand
        {
            get
            {
                var articleBundle = new ArticleBundle() { ArticleList = _articles, SelectedArticle = SelectedArticle };

                _navigateCommand = _navigateCommand ?? new MvxAsyncCommand(() => 
                    _navigationService.Navigate<ArticleViewModel, ArticleBundle>(articleBundle));
                return _navigateCommand;
            }
        }

        /// <summary>
        /// Refresh data command
        /// </summary>
        private MvxNotifyTask _myTaskNotifier;
        public IMvxCommand RefreshDataCommand { get; private set; }
        public MvxNotifyTask MyTaskNotifier
        {
            get => _myTaskNotifier;
            private set => SetProperty(ref _myTaskNotifier, value);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public NewsViewModel(IArticleService articleService, IMvxNavigationService navigationService)
        {
            _articleService = articleService;
            _navigationService = navigationService;
            IsBusy = false;

            RefreshDataCommand = new MvxCommand(() => MyTaskNotifier = MvxNotifyTask.Create(() => RefreshDataAsync(), 
                onException: ex => OnException(ex)));
        }

        /// <summary>
        /// Initialization
        /// </summary>
        public override async Task Initialize()
        {
            await base.Initialize();

            //if (_firstRun)
            //{
            //    _firstRun = false;
            //    RefreshDataCommand.Execute();
            //}
        }

        /// <summary>
        /// Refresh data
        /// </summary>
        public async Task RefreshDataAsync()
        {
            try
            {
                IsBusy = true;

                _articles = await _articleService.GetArticlesAsync();
                await RaisePropertyChanged(nameof(Articles));

                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                Debug.WriteLine("\tERROR {0}", ex.Message);
            }
        }

        /// <summary>
        /// Exception handler
        /// </summary>
        private void OnException(Exception exception)
        {
            // log the handled exception!
        }

        /// <summary>
        /// Preparing view model
        /// </summary>
        public override void Prepare(ArticleBundle articleBundle)
        {
            _articles = articleBundle.ArticleList;
            SelectedArticle = articleBundle.SelectedArticle;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using MvvmCross;
using MvvmCross.ViewModels;
using News.Core.ViewModels;
using News.Core.Services;
using News.Core.Services.Parsing;
using News.Core.Services.Web;
using News.Core.Services.Database;
using MvvmCross.Plugin.Json;
using MvvmCross.Base;

namespace News.Core
{
    /// <summary>
    /// Article database
    /// </summary>
    public class App : MvxApplication
    {
        /// <summary>        
        /// Initialization
        /// </summary>
        public override void Initialize()
        {
            //Mvx.IoCProvider.RegisterType<IWebService, MockWebService>();
            Mvx.IoCProvider.RegisterType<IWebService, WebService>();
            Mvx.IoCProvider.RegisterType<IParserList, ParserList>();
            Mvx.IoCProvider.RegisterType<IArticleDatabase, ArticleDatabase>();
            Mvx.IoCProvider.RegisterType<IArticleService, ArticleService>();
            Mvx.IoCProvider.RegisterType<IMvxJsonConverter, MvxJsonConverter>();

            RegisterAppStart<NewsViewModel>();
        }
    }
}

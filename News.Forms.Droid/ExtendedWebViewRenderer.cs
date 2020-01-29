using System;
using Android.Graphics;
using News.Forms.Droid;
using News.Forms.UI.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using WebView = Android.Webkit.WebView;

#pragma warning disable 612
[assembly: ExportRenderer(typeof(ExtendedWebView), typeof(ExtendedWebViewRenderer))]
#pragma warning restore 612
namespace News.Forms.Droid
{
    /// <summary>
    /// Extended web view renderer
    /// </summary>
    [System.Obsolete]
    public class ExtendedWebViewRenderer : WebViewRenderer
    {
        static ExtendedWebView _xwebView = null;
        WebView _webView;

        class ExtendedWebViewClient : Android.Webkit.WebViewClient
        {
            readonly ExtendedWebViewRenderer _renderer;

            public ExtendedWebViewClient(ExtendedWebViewRenderer renderer)
            {
                _renderer = renderer ?? throw new ArgumentNullException("renderer");
            }

            public override void OnPageStarted(Android.Webkit.WebView view, string url, Bitmap favicon)
            {
                base.OnPageStarted(view, url, favicon);

                var args = new WebNavigatingEventArgs(WebNavigationEvent.NewPage, new UrlWebViewSource { Url = url }, url);
                _renderer.ElementController.SendNavigating(args);

                if (args.Cancel)
                {
                    //_renderer.Control.StopLoading();
                    view.GoBack();
                }
                else
                {
                    //base.OnPageStarted(view, url, favicon);
                }
            }

            public override async void OnPageFinished(WebView view, string url)
            {
                if (_xwebView != null)
                {
                    int i = 10;
                    while (view.ContentHeight == 0 && i-- > 0) // wait here till content is rendered
                        await System.Threading.Tasks.Task.Delay(100);
                    _xwebView.HeightRequest = view.ContentHeight;
                }
                base.OnPageFinished(view, url);
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.WebView> e)
        {
            base.OnElementChanged(e);
            _xwebView = e.NewElement as ExtendedWebView;
            _webView = Control;

            if (e.OldElement == null)
            {
                _webView.SetWebViewClient(new ExtendedWebViewClient(this));
            }

        }
    }
}
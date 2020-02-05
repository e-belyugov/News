using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using News.Forms.Droid;
using News.Forms.UI.Helpers;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidDevice))]
namespace News.Forms.Droid
{
    /// <summary>
    /// Android device info
    /// </summary>
    public class AndroidDevice : IDevice
    {
        public string GetIdentifier()
        {
            return Settings.Secure.GetString(Android.App.Application.Context.ContentResolver, Settings.Secure.AndroidId);
        }
    }
}
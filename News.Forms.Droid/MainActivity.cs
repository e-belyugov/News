using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Content.PM;
using Android.Widget;
using MvvmCross.Forms.Platforms.Android.Core;
using MvvmCross.Forms.Platforms.Android.Views;
using News.Core;
using News.Forms.UI;

namespace News.Forms.Droid
{
    [Activity(
        Label = "Новости НК",
        Icon = "@mipmap/ic_launcher",
        Theme = "@style/AppTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation,
        LaunchMode = LaunchMode.SingleTask)]
    public class MainActivity : MvxFormsAppCompatActivity<MvxFormsAndroidSetup<App, FormsApp>, App, FormsApp>
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(bundle);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
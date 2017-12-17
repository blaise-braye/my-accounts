using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using OxyPlot;

namespace MyAccounts.Classification.MobileUi.Droid
{
    [Activity(Label = "MyAccounts.Classification.MobileUi.Android", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);
            var pv = new OxyPlot.Xamarin.Forms.PlotView()
            {
                Model = new PlotModel()
            };

            Xamarin.Forms.Forms.Init(this, bundle);
            OxyPlot.Xamarin.Forms.Platform.Android.PlotViewRenderer.Init();
            LoadApplication(new Mobile.Shared.App());
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
        }
    }
}

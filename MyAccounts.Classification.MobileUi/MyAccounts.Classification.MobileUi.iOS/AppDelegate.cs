using Foundation;
using UIKit;

namespace MyAccounts.Classification.MobileUi.iOS
{
    using Xamarin.Forms.Platform.iOS;

    [Register("AppDelegate")]
    public partial class AppDelegate : FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            ////OxyPlot.Xamarin.Forms.Platform.iOS.PlotViewRenderer.Init();
            this.LoadApplication(new Mobile.Shared.App());

            return base.FinishedLaunching(app, options);
        }
    }
}

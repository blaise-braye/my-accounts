namespace MyAccounts.Classification.MobileUi.iOS
{
    using Foundation;
    using UIKit;

    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            this.LoadApplication(new Mobile.Shared.App());

            return base.FinishedLaunching(app, options);
        }
    }
}

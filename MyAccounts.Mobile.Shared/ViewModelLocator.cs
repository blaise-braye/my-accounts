using GalaSoft.MvvmLight.Ioc;
using MyAccounts.Mobile.Shared.ViewModels;

namespace MyAccounts.Mobile.Shared
{
    /// <summary>
    ///     This class contains static references to all the view models in the
    ///     application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            //if (ViewModelBase.IsInDesignModeStatic)
            //{
            //    // Create design time view services and models
            //    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            //}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}
            SimpleIoc.Default.Register<AccountsPageViewModel>();
            SimpleIoc.Default.Register<AboutPageViewModel>();
        }

        public AccountsPageViewModel AccountsPage => SimpleIoc.Default.GetInstance<AccountsPageViewModel>();

        public AboutPageViewModel AboutPage => SimpleIoc.Default.GetInstance<AboutPageViewModel>();

        public AccountDetailPageViewModel ItemDetailsPage => null;

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
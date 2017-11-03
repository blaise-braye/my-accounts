using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using MyAccounts.Classification.MobileUi.ViewModels;

namespace MyAccounts.Classification.MobileUi
{
    /// <summary>
    ///     This class contains static references to all the view models in the
    ///     application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

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

            SimpleIoc.Default.Register<AccountsViewModel>();
            SimpleIoc.Default.Register<AboutViewModel>();
        }

        public AccountsViewModel AccountsPage => ServiceLocator.Current.GetInstance<AccountsViewModel>();

        public AboutViewModel AboutPage => ServiceLocator.Current.GetInstance<AboutViewModel>();

        public AccountDetailViewModel ItemDetailsPage => null;

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
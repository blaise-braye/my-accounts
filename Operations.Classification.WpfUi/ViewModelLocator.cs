/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:Operations.Classification.WpfUi"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using AutoMapper;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using Operations.Classification.WpfUi.Data;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Managers.Integration.GererMesComptes;
using QifApi.Transactions;

namespace Operations.Classification.WpfUi
{
    /// <summary>
    ///     This class contains static references to all the view models in the
    ///     application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        ///     Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            Mapper.Initialize(
                cfg =>
                {
                    cfg.CreateMap<AccountViewModel, AccountEntity>();
                    cfg.CreateMap<AccountEntity, AccountViewModel>();
                    cfg.CreateMap<BasicTransaction, BasicTransactionModel>();
                });

            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<MainViewModel>();
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
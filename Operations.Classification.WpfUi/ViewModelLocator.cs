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
using MyAccounts.Business.AccountOperations;
using MyAccounts.Business.AccountOperations.Unified;
using MyAccounts.Business.Managers.Accounts;
using MyAccounts.Business.Managers.Imports;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Managers.Imports;
using Operations.Classification.WpfUi.Managers.Integration.GererMesComptes;
using Operations.Classification.WpfUi.Managers.Settings;
using Operations.Classification.WpfUi.Managers.Transactions;
using QifApi.Transactions;

namespace Operations.Classification.WpfUi
{
    /// <summary>
    ///     This class contains static references to all the view models in the
    ///     application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            Mapper.Initialize(
                cfg =>
                {
                    cfg.CreateMap<AccountViewModel, AccountEntity>();
                    cfg.CreateMap<AccountEntity, AccountViewModel>();
                    cfg.CreateMap<BasicTransaction, BasicTransactionModel>();
                    cfg.CreateMap<UnifiedAccountOperation, UnifiedAccountOperationModel>();
                    cfg.CreateMap<Properties.Settings, SettingsModel>();
                    cfg.CreateMap<SettingsModel, Properties.Settings>();

                    cfg.CreateMap<ImportCommand, ImportCommandGridModel>();
                    cfg.CreateMap<ImportCommand, ImportEditorViewModel>();
                    cfg.CreateMap<FileStructureMetadata, ImportEditorViewModel>();

                    cfg.CreateMap<ImportExecutionImpact, ImportCommandGridModel>()
                        .ForMember(s => s.LastExecution, opt => opt.MapFrom(s => s.CreationDate))
                        .ForMember(s => s.Success, opt => opt.MapFrom(s => s.Success))
                        .ForMember(s => s.NewOperations, opt => opt.MapFrom(s => s.NewOperations))
                        .ForMember(s => s.AlreadyKnown, opt => opt.MapFrom(s => s.AlreadyKnown))
                        .ForMember(s => s.NotCompliant, opt => opt.MapFrom(s => s.NotCompliant))
                        .ForAllOtherMembers(m => m.Ignore());
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
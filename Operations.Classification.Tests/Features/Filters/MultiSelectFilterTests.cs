using System.Collections.Generic;
using System.Linq;
using MyAccounts.Business.GererMesComptes;
using NUnit.Framework;
using Operations.Classification.WpfUi.Managers.Accounts.Models;
using Operations.Classification.WpfUi.Managers.Integration.GererMesComptes;
using Operations.Classification.WpfUi.Managers.Reports;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.NUnit3;

namespace Operations.Classification.Tests.Features.Filters
{
    [TestFixture]
    public class MultiSelectFilterTests
    {
        [Test]
        [AutoData]
        public void WhenFilteringGmcDeltasByActionThenDataIsFiltered(IFixture fixture)
        {
            var filter = new GmcManagerFilterViewModel();

            var allItem = filter.DeltaFilter.GetAllItem();
            var toRemoveFilter = filter.DeltaFilter.DataItems.First(d=>  ((DeltaAction[])d.CommandParameter)[0] == DeltaAction.Remove);
            var deltaBuilder = fixture.Build<TransactionDelta>();
            var toRemoveDelta = deltaBuilder.With(d => d.Action, DeltaAction.Remove).CreateMany().ToList();
            var toUpdateMemoDelta = deltaBuilder.With(d => d.Action, DeltaAction.UpdateMemo).CreateMany();
            var delta = toRemoveDelta.Concat(toUpdateMemoDelta).ToList();

            Assert.That(filter.IsActive(), Is.False, "should not be active when initialized");

            allItem.Uncheck();
            Assert.That(filter.IsActive(), Is.True, "should be active when nothing is checked");

            var filteredDelta = filter.DeltaFilter.Apply(delta, d => d.Action);
            Assert.That(filteredDelta, Is.EquivalentTo(Enumerable.Empty<AccountViewModel>()), "nothing should be filtered when nothing is selected");

            toRemoveFilter.Check();
            Assert.That(filter.IsActive(), Is.True, "should be active when one item is check");

            filteredDelta = filter.DeltaFilter.Apply(delta, d => d.Action);
            Assert.That(filteredDelta, Is.EquivalentTo(toRemoveDelta), "only selected filter should be taken into account when applying the filter");
        }

        [Test]
        [AutoData]
        public void WhenFilteringDashboardByAccountThenDataIsFiltered(List<AccountViewModel> accounts)
        {
            var filter = new DashboardFilterViewModel();
            filter.AccountsFilter.Initialize(accounts, account => account.Name);
            var allItem = filter.AccountsFilter.GetAllItem();
            var item = filter.AccountsFilter.DataItems.First();

            Assert.That(filter.IsActive(), Is.False, "should not be active when initialized");

            allItem.Uncheck();
            Assert.That(filter.IsActive(), Is.True, "should be active when nothing is checked");

            var filteredAccounts = filter.AccountsFilter.Apply(accounts);
            Assert.That(Enumerable.Empty<AccountViewModel>(), Is.EquivalentTo(filteredAccounts), "nothing should be filtered when nothing is selected");
            
            item.Check();
            Assert.That(filter.IsActive(), Is.True, "should be active when one item is check");

            var filteredAccount = filter.AccountsFilter.Apply(accounts).Single();
            Assert.That(item.CommandParameter, Is.EqualTo(filteredAccount), "only selected filter should be taken into account when applying the filter");
        }
    }
}

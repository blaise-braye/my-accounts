using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using MyAccounts.Business.GererMesComptes;
using NUnit.Framework;

namespace MyAccounts.Tests.Gmc.Features
{
    [TestFixture(Category = "UnitTest")]
    public class WhenIParseQifOperations
    {
        private CultureInfo _cultureBackup;

        [SetUp]
        public void Setup()
        {
            _cultureBackup = CultureInfo.CurrentCulture;
        }

        [TearDown]
        public void TearDown()
        {
            CultureInfo.CurrentCulture = _cultureBackup;
        }

        [Test]
        public void ThenAmountParsingIsDoneWithGmcCulture()
        {
            var qifData = @"
!Type:Bank
T0.01
^
";
            // Given I use a culture different than es-US (which is the one to use for serialization of GMC data)
            CultureInfo.CurrentCulture = new CultureInfo("fr-BE");

            // When I parse the decimal data
            var qifDom = QifMapper.ParseQifDom(qifData);

            // Then decimal parsing happened as expected
            qifDom.BankTransactions.First().Amount.Should().Be((decimal)0.01);
        }

        [Test]
        public void ThenDateParsingIsDoneWithGmcCulture()
        {
            var qifData = @"
!Type:Bank
D09/28/2016
^
";
            // When I parse the decimal data
            var qifDom = QifMapper.ParseQifDom(qifData);

            // Then decimal parsing happened as expected
            qifDom.BankTransactions.First().Date.Should().Be(new DateTime(2016, 09, 28));
        }
    }
}
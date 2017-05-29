using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Operations.Classification.GererMesComptes;

namespace Operations.Classification.Tests.Features.GererMesComptes
{
    [TestFixture]
    public class When_I_parse_qif_operations
    {
        private CultureInfo _cultureBackup;
        
        [Test]
        public void ThenDateParsingIsDoneWithGmgCulture()
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

        [Test]
        public void ThenAmountParsingIsDoneWithGmgCulture()
        {
            var qifData = @"
!Type:Bank
T0.01
^
";
            // Given I use a culture different than es-US (which is the one to use for serialization of GMG data)
            CultureInfo.CurrentCulture = new CultureInfo("fr-BE");

            // When I parse the decimal data
            var qifDom = QifMapper.ParseQifDom(qifData);

            // Then decimal parsing happened as expected
            qifDom.BankTransactions.First().Amount.Should().Be((decimal)0.01);
        }

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
    }
}
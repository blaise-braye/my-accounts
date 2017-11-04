using System;
using System.Reflection;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;

namespace MyAccounts.Tests.AutoFixtures.Builders
{
    public class DateBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is PropertyInfo pi && pi.PropertyType == typeof(DateTime) && pi.Name.EndsWith("Date"))
            {
                return context.Create<DateTime>().Date;
            }

            return new NoSpecimen();
        }
    }
}
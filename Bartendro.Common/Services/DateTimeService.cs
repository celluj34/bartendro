using System;
using System.Diagnostics.CodeAnalysis;

namespace Bartendro.Common.Services
{
    public interface IDateTimeService
    {
        DateTime Now();
    }

    internal class DateTimeService : IDateTimeService
    {
        [ExcludeFromCodeCoverage]
        public DateTime Now()
        {
            return DateTime.UtcNow;
        }
    }
}
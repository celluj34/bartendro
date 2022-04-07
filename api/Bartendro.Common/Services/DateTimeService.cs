using System;
using System.Diagnostics.CodeAnalysis;

namespace Bartendro.Common.Services
{
    public interface IDateTimeService
    {
        DateTimeOffset Now();
    }

    internal class DateTimeService : IDateTimeService
    {
        [ExcludeFromCodeCoverage]
        public DateTimeOffset Now()
        {
            return DateTimeOffset.UtcNow;
        }
    }
}
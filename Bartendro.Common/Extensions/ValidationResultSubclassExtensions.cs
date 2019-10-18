using System;
using FluentValidation.Results;

namespace Bartendro.Common.Extensions
{
    public static class ValidationResultSubclassExtensions
    {
        public static string GetFullMessage<T>(this T result, string delimited) where T : ValidationResult
        {
            return ValidationResultExtensions.GetFullMessage(result, delimited);
        }

        public static T AddError<T>(this T result, Exception exception) where T : ValidationResult
        {
            ValidationResultExtensions.AddError(result, exception);

            return result;
        }

        public static T AddError<T>(this T result, string error) where T : ValidationResult
        {
            ValidationResultExtensions.AddError(result, error);

            return result;
        }

        public static T AddError<T>(this T result, string propertyName, string error) where T : ValidationResult
        {
            ValidationResultExtensions.AddError(result, propertyName, error);

            return result;
        }

        public static T Merge<T, K>(this T result, K otherResult) where T : ValidationResult where K : ValidationResult
        {
            ValidationResultExtensions.Merge(result, otherResult);

            return result;
        }
    }
}
using System;
using System.Linq;
using FluentValidation.Results;

namespace Bartendro.Common.Extensions
{
    public static class ValidationResultExtensions
    {
        public static string GetFullMessage<T>(this T result, string delimited) where T : ValidationResult
        {
            return result.Errors.Select(x => x.ErrorMessage).Join(delimited);
        }

        public static ValidationResult AddError(this ValidationResult result, Exception exception)
        {
            return result.AddError(exception.GetFullMessage());
        }

        public static ValidationResult AddError(this ValidationResult result, string errorMessage)
        {
            return result.AddError(string.Empty, errorMessage);
        }

        public static ValidationResult AddError(this ValidationResult result, string propertyName, string errorMessage)
        {
            if(result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            result.Errors.Add(new ValidationFailure(propertyName, errorMessage));

            return result;
        }

        public static ValidationResult Merge(this ValidationResult result, ValidationResult otherResult)
        {
            if(result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if(otherResult == null || otherResult.IsValid)
            {
                return result;
            }

            foreach(var error in otherResult.Errors)
            {
                result.Errors.Add(error);
            }

            return result;
        }
    }
}
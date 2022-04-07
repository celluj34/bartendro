using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace Bartendro.Database.Validators
{
    /// <summary>
    ///     Wrapper around FluentValidation.AbstractValidator to run DataAnnotations validations in addition to regular
    ///     FluentValidations
    /// </summary>
    internal abstract class DataAnnotationsValidator<T> : AbstractValidator<T>
    {
        public override Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = new())
        {
            var result = new ValidationResult();

            var dataAnnotationFailures = GetDataAnnotationsValidationFailures(context);
            foreach(var failure in dataAnnotationFailures)
            {
                result.Errors.Add(failure);
            }

            return Task.FromResult(result);
        }

        private IEnumerable<ValidationFailure> GetDataAnnotationsValidationFailures(ValidationContext<T> context)
        {
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            var validationContext = new ValidationContext(context.InstanceToValidate, null, null);
            Validator.TryValidateObject(context.InstanceToValidate, validationContext, validationResults);

            return validationResults.SelectMany(x => x.MemberNames, (x, y) => new ValidationFailure(y, x.ErrorMessage));
        }
    }
}
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Bartendro.Common.Extensions;
using FluentValidation;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace Bartendro.Database.Validators
{
    internal class AbstractValidator<T> : FluentValidation.AbstractValidator<T>
    {
        protected override bool PreValidate(ValidationContext<T> context, ValidationResult result)
        {
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            var validationContext = new ValidationContext(context.InstanceToValidate, null, null);
            var valid = Validator.TryValidateObject(context.InstanceToValidate, validationContext, validationResults);

            var allResults = validationResults.SelectMany(x => x.MemberNames,
                (x, y) => new
                {
                    PropertyName = y,
                    x.ErrorMessage
                });

            foreach(var validationResult in allResults)
            {
                result.AddError(validationResult.PropertyName, validationResult.ErrorMessage);
            }

            return valid && base.PreValidate(context, result);
        }
    }
}
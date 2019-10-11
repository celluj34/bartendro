using FluentValidation.Results;

namespace Bartendro.Database.Models
{
    public class EntityResult<T> : ValidationResult
    {
        public T Entity {get;set;}
    }
}
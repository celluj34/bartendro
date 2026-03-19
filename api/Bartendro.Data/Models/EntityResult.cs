using FluentValidation.Results;

namespace Bartendro.Data.Models
{
    public class EntityResult<T> : ValidationResult
    {
        public EntityResult() {}

        public EntityResult(T entity) : this()
        {
            Entity = entity;
        }

        public T? Entity
        {
            get => IsValid ? field : default;
        }
    }
}
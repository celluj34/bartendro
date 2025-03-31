using FluentValidation.Results;

namespace Bartendro.Data.Models
{
    public class EntityResult<T> : ValidationResult
    {
        private readonly T _entity;

        public EntityResult() {}

        public EntityResult(T entity) : this()
        {
            _entity = entity;
        }

        public T Entity => IsValid ? _entity : default;
    }
}
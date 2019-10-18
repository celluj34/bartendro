using FluentValidation.Results;

namespace Bartendro.Database.Models
{
    public class EntityResult<T> : ValidationResult
    {
        public EntityResult() {}

        public EntityResult(T entityModel)
        {
            EntityModel = entityModel;
        }

        public T EntityModel {get;}
    }
}
using System.Threading.Tasks;
using Bartendro.Common.Services;
using Bartendro.Database.Entities;
using Bartendro.Database.Services;
using FluentValidation;
using FluentValidation.Results;

namespace Bartendro.Database.Commands
{
    internal class CreateCommand<T> : Command<T> where T : Entity, new()
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IValidator<T> _validator;

        public CreateCommand(IDatabaseContext databaseContext, IValidator<T> validator, IDateTimeService dateTimeService) : base(databaseContext)
        {
            _validator = validator;
            _dateTimeService = dateTimeService;
        }

        protected override Task<T> Get(IDatabaseContext databaseContext)
        {
            return Task.FromResult(new T());
        }

        protected override Task<ValidationResult> Validate(T entity)
        {
            return _validator.ValidateAsync(entity);
        }

        protected override async Task Save(IDatabaseContext databaseContext, T entity)
        {
            entity.DateCreated = _dateTimeService.Now();

            await databaseContext.Set<T>().AddAsync(entity);
        }
    }
}
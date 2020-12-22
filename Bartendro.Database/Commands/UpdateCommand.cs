using System;
using System.Threading.Tasks;
using Bartendro.Common.Services;
using Bartendro.Database.Entities;
using Bartendro.Database.Services;
using FluentValidation;
using FluentValidation.Results;

namespace Bartendro.Database.Commands
{
    internal class UpdateCommand<T> : Command<T> where T : Entity, new()
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IValidator<T> _validator;
        private Guid _id;
        private byte[] _version;

        public UpdateCommand(IDatabaseContext databaseContext, IValidator<T> validator, IDateTimeService dateTimeService) : base(databaseContext)
        {
            _validator = validator;
            _dateTimeService = dateTimeService;
        }

        protected override Task<T> Get(IDatabaseContext databaseContext)
        {
            return databaseContext.FindByIdAndVersionAsync<T>(_id, _version);
        }

        protected override Task<ValidationResult> Validate(T entity)
        {
            return _validator.ValidateAsync(entity);
        }

        protected override Task Save(IDatabaseContext databaseContext, T entity)
        {
            entity.DateModified = _dateTimeService.Now();

            databaseContext.Set<T>().Update(entity);

            return Task.CompletedTask;
        }

        public void Initialize(Guid id, byte[] version)
        {
            _id = id;
            _version = version;
        }
    }
}
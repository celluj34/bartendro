using System;
using System.Threading.Tasks;
using Bartendro.Common.Services;
using Bartendro.Data.Entities;
using Bartendro.Data.Services;
using FluentValidation.Results;

namespace Bartendro.Data.Commands
{
    internal class DeleteCommand<T> : Command<T> where T : Entity, new()
    {
        private readonly IDateTimeService _dateTimeService;
        private Guid _id;
        private byte[] _version;

        public DeleteCommand(IDatabaseContext databaseContext, IDateTimeService dateTimeService) : base(databaseContext)
        {
            _dateTimeService = dateTimeService;
        }

        protected override Task<T> Get(IDatabaseContext databaseContext)
        {
            return databaseContext.FindByIdAndVersionAsync<T>(_id, _version);
        }

        protected override Task<ValidationResult> Validate(T entity)
        {
            return Task.FromResult(new ValidationResult());
        }

        protected override Task Save(IDatabaseContext databaseContext, T entity)
        {
            entity.DateModified = _dateTimeService.Now();
            entity.Deleted = true;

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
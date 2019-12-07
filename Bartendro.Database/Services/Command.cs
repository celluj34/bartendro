using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bartendro.Common.Extensions;
using Bartendro.Common.Services;
using Bartendro.Database.Entities;
using Bartendro.Database.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Bartendro.Database.Services
{
    public interface ICommand<out T> where T : Entity, new()
    {
        ICommand<T> Run(Action<T> action);
        ICommand<T> Run(Func<T, Task> action);
        Task<DatabaseResult> SaveChanges();
    }

    internal class Command<T> : ICommand<T> where T : Entity, new()
    {
        private const string DocumentConflictError = "This document has already been updated. Refresh and try again.";
        private readonly List<Func<T, Task>> _actions;
        private readonly IDatabaseContext _databaseContext;
        private readonly IDateTimeService _dateTimeService;
        private readonly AbstractValidator<T> _validator;
        private Func<Task<T>> _getAction;
        private Action<T> _saveAction;
        private bool _validate;

        public Command(IDatabaseContext databaseContext, AbstractValidator<T> validator, IDateTimeService dateTimeService)
        {
            _databaseContext = databaseContext;
            _validator = validator;
            _dateTimeService = dateTimeService;
            _actions = new List<Func<T, Task>>();
        }

        public ICommand<T> Run(Action<T> action)
        {
            if(action != null)
            {
                _actions.Add(x =>
                {
                    action(x);

                    return Task.CompletedTask;
                });
            }

            return this;
        }

        public ICommand<T> Run(Func<T, Task> action)
        {
            if(action != null)
            {
                _actions.Add(action);
            }

            return this;
        }

        public async Task<DatabaseResult> SaveChanges()
        {
            var (entity, result) = await GetEntity();

            if(!result.IsValid)
            {
                return result;
            }

            result = await ApplyActions(entity);

            if(!result.IsValid)
            {
                return result;
            }

            result = ValidateEntity(entity);

            if(!result.IsValid)
            {
                return result;
            }

            result = await SaveEntity(entity);

            return result;
        }

        private async Task<(T entity, DatabaseResult result)> GetEntity()
        {
            try
            {
                var entity = await _getAction();

                return (entity, new DatabaseResult());
            }
            catch(DbUpdateConcurrencyException)
            {
                var result = new DatabaseResult().AddError(DocumentConflictError);

                return (default, result);
            }
            catch(Exception ex)
            {
                var result = new DatabaseResult().AddError(ex);

                return (default, result);
            }
        }

        private async Task<DatabaseResult> ApplyActions(T entity)
        {
            foreach(var action in _actions)
            {
                try
                {
                    await action.Invoke(entity);
                }
                catch(Exception ex)
                {
                    return new DatabaseResult().AddError(ex);
                }
            }

            return new DatabaseResult();
        }

        private DatabaseResult ValidateEntity(T entity)
        {
            if(!_validate)
            {
                return new DatabaseResult();
            }

            var validationResult = _validator.Validate(entity);

            return new DatabaseResult().Merge(validationResult);
        }

        private async Task<DatabaseResult> SaveEntity(T entity)
        {
            try
            {
                _saveAction(entity);

                await _databaseContext.SaveChangesAsync();

                return new DatabaseResult(new EntityModel
                {
                    Id = entity.Id,
                    Version = entity.Version
                });
            }
            catch(DbUpdateConcurrencyException)
            {
                return new DatabaseResult().AddError(DocumentConflictError);
            }
            catch(Exception ex)
            {
                return new DatabaseResult().AddError(ex);
            }
        }

        internal ICommand<T> Create()
        {
            _getAction = () => Task.FromResult(new T());

            _validate = true;
            _saveAction = entity =>
            {
                entity.DateCreated = _dateTimeService.Now();

                _databaseContext.Set<T>().Add(entity);
            };

            return this;
        }

        internal ICommand<T> Update(Guid id, byte[] version)
        {
            _getAction = async () =>
            {
                var entity = await _databaseContext.Set<T>().FindAsync(id);

                if(entity.Version != version)
                {
                    throw new DbUpdateConcurrencyException();
                }

                return entity;
            };

            _validate = true;

            _saveAction = entity => _databaseContext.Set<T>().Update(entity);

            return this;
        }

        internal ICommand<T> Delete(Guid id, byte[] version)
        {
            _getAction = async () =>
            {
                var entity = await _databaseContext.Set<T>().FindAsync(id);

                if(entity.Version != version)
                {
                    throw new DbUpdateConcurrencyException();
                }

                return entity;
            };

            _validate = false;

            _saveAction = entity =>
            {
                entity.DateModified = _dateTimeService.Now();
                entity.Deleted = true;

                _databaseContext.Set<T>().Update(entity);
            };

            return this;
        }
    }
}
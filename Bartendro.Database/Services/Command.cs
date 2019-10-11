using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Arsenal.Helpers.Common.Extensions;
using Bartendro.Database.Entities;
using Bartendro.Database.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

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
        private readonly AbstractValidator<T> _validator;
        private Func<Task<T>> _getAction;
        private Action<T> _saveAction;
        private bool _validate;

        public Command(IDatabaseContext databaseContext, AbstractValidator<T> validator)
        {
            _databaseContext = databaseContext;
            _validator = validator;
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

                _databaseContext.SaveChangesAsync();

                return new DatabaseResult
                {
                    Entity = entity.Id.ToString()
                };
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
            _saveAction = entity => _databaseContext.Set<T>().Add(entity);

            return this;
        }

        internal ICommand<T> Update(Guid id)
        {
            _getAction = async () =>
            {
                var entity = await _databaseContext.Set<T>().FindAsync(id);

                //if(entity.Version == version)
                {
                    return entity;
                }

                //throw new DbUpdateConcurrencyException();
            };

            _validate = true;

            _saveAction = entity => _databaseContext.Set<T>().Update(entity);

            return this;
        }

        internal ICommand<T> Delete(Guid id)
        {
            _getAction = async () =>
            {
                var entity = await _databaseContext.Set<T>().FindAsync(id);

                //if(entity.Version == version)
                {
                    return entity;
                }

                //throw new DbUpdateConcurrencyException();
            };

            _validate = false;
            _saveAction = entity => _databaseContext.Set<T>().Remove(entity);

            return this;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bartendro.Common.Extensions;
using Bartendro.Data.Entities;
using Bartendro.Data.Models;
using Bartendro.Data.Services;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Bartendro.Data.Commands
{
    public interface ICommand<out T> where T : Entity, new()
    {
        ICommand<T> Run(Action<T> action);
        ICommand<T> Run(Func<T, Task> action);
        Task<DatabaseResult> SaveChangesAsync();
    }

    internal abstract class Command<T> : ICommand<T> where T : Entity, new()
    {
        private const string DocumentConflictError = "This document has already been updated. Refresh and try again.";
        private readonly IDatabaseContext _databaseContext;
        private readonly List<Func<T, Task>> _updateActions = new();

        protected Command(IDatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public ICommand<T> Run(Action<T> action)
        {
            if(action != null)
            {
                _updateActions.Add(x =>
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
                _updateActions.Add(action);
            }

            return this;
        }

        public async Task<DatabaseResult> SaveChangesAsync()
        {
            var (entity, result) = await GetEntityAsync();
            if(!result.IsValid)
            {
                return new DatabaseResult().Merge(result);
            }

            result = await ApplyActionsAsync(entity);
            if(!result.IsValid)
            {
                return new DatabaseResult().Merge(result);
            }

            result = await ValidateEntity(entity);
            if(!result.IsValid)
            {
                return new DatabaseResult().Merge(result);
            }

            result = await SaveEntityAsync(entity);
            if(!result.IsValid)
            {
                return new DatabaseResult().Merge(result);
            }

            return new DatabaseResult(new EntityModel
            {
                Id = result.Entity.Id,
                Version = result.Entity.Version
            });
        }

        protected abstract Task<T> Get(IDatabaseContext databaseContext);
        protected abstract Task<ValidationResult> Validate(T entity);
        protected abstract Task Save(IDatabaseContext databaseContext, T entity);

        private async Task<(T, DatabaseResult)> GetEntityAsync()
        {
            try
            {
                var entity = await Get(_databaseContext);

                return (entity, new DatabaseResult());
            }
            catch(DbUpdateConcurrencyException)
            {
                return (default, new DatabaseResult().AddError(DocumentConflictError));
            }
            catch(Exception ex)
            {
                return (default, new DatabaseResult().AddError(ex));
            }
        }

        private async Task<DatabaseResult> ApplyActionsAsync(T entity)
        {
            foreach(var updateAction in _updateActions)
            {
                try
                {
                    await updateAction(entity);
                }
                catch(Exception ex)
                {
                    return new DatabaseResult().AddError(ex);
                }
            }

            return new DatabaseResult();
        }

        private async Task<DatabaseResult> ValidateEntity(T entity)
        {
            try
            {
                var result = await Validate(entity);

                return new DatabaseResult().Merge(result);
            }
            catch(Exception ex)
            {
                return new DatabaseResult().AddError(ex);
            }
        }

        private async Task<DatabaseResult> SaveEntityAsync(T entity)
        {
            try
            {
                await Save(_databaseContext, entity);

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
    }
}
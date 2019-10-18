using System;
using System.Threading.Tasks;
using Bartendro.Database.Entities;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Bartendro.Database.Services
{
    public interface ICommandFactory
    {
        ICommand<T> Create<T>() where T : Entity, new();
        ICommand<T> Update<T>(Guid id, byte[] version) where T : Entity, new();
        Task<ValidationResult> Delete<T>(Guid id, byte[] version) where T : Entity, new();
    }

    internal class CommandFactory : ICommandFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ICommand<T> Create<T>() where T : Entity, new()
        {
            return GetCommand<T>().Create();
        }

        public ICommand<T> Update<T>(Guid id, byte[] version) where T : Entity, new()
        {
            return GetCommand<T>().Update(id, version);
        }

        public async Task<ValidationResult> Delete<T>(Guid id, byte[] version) where T : Entity, new()
        {
            return await GetCommand<T>().Delete(id, version).SaveChanges();
        }

        private Command<T> GetCommand<T>() where T : Entity, new()
        {
            return _serviceProvider.GetRequiredService<Command<T>>();
        }
    }
}
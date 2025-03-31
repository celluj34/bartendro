using System;
using System.Threading.Tasks;
using Bartendro.Data.Commands;
using Bartendro.Data.Entities;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Bartendro.Data.Services
{
    public interface ICommandFactory
    {
        ICommand<T> Create<T>() where T : Entity, new();
        ICommand<T> Update<T>(Guid id, byte[] version) where T : Entity, new();
        Task<ValidationResult> DeleteAsync<T>(Guid id, byte[] version) where T : Entity, new();
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
            var command = _serviceProvider.GetRequiredService<CreateCommand<T>>();

            return command;
        }

        public ICommand<T> Update<T>(Guid id, byte[] version) where T : Entity, new()
        {
            var command = _serviceProvider.GetRequiredService<UpdateCommand<T>>();
            command.Initialize(id, version);

            return command;
        }

        public async Task<ValidationResult> DeleteAsync<T>(Guid id, byte[] version) where T : Entity, new()
        {
            var command = _serviceProvider.GetRequiredService<DeleteCommand<T>>();
            command.Initialize(id, version);

            return await command.SaveChangesAsync();
        }
    }
}
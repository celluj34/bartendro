using System.Threading.Tasks;
using Bartendro.Database.Entities;
using Bartendro.Database.Models;
using Bartendro.Database.Services;
using Blazor.Extensions.Storage.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Bartendro.Web.Pages.Recipes
{
    public partial class Create
    {
        private readonly Recipe _recipe;
        private DatabaseResult _result;

        public Create()
        {
            _recipe = new Recipe();
        }

        [Inject]
        private ICommandFactory CommandFactory { get; set; }

        [Inject]
        private ILocalStorage LocalStorage { get; set; }

        private async Task HandleValidSubmit()
        {
            _result = await CommandFactory.Create<Recipe>().Run(x => x.Title = _recipe.Title).SaveChangesAsync();
        }
    }
}
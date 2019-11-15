using System.Threading.Tasks;
using Bartendro.Database.Entities;
using Bartendro.Database.Services;
using Blazor.Extensions.Storage;
using Microsoft.AspNetCore.Components;

namespace Bartendro.Web.Pages.Recipes
{
    public partial class Create
    {
        private readonly Recipe _recipe;

        public Create()
        {
            _recipe = new Recipe();
        }

        [Inject]
        private ICommandFactory CommandFactory {get;set;}

        [Inject]
        private LocalStorage LocalStorage {get;set;}

        private async Task HandleValidSubmit()
        {
            var result = await CommandFactory.Create<Recipe>().Run(x => x.Title = _recipe.Title).SaveChanges();
        }
    }
}
using System.Threading.Tasks;
using Bartendro.Database.Entities;
using Bartendro.Database.Services;
using Bartendro.Web.Models.Recipes;
using Microsoft.AspNetCore.Components;

namespace Bartendro.Web.Pages.Recipes
{
    public class CreateRecipe : ComponentBase
    {
        protected readonly RecipeCreateModel Recipe = new RecipeCreateModel();

        [Inject]
        private ICommandFactory CommandFactory {get;set;}

        protected async Task HandleValidSubmit()
        {
            var result = await CommandFactory.Create<Recipe>().Run(x => x.Title = Recipe.Title).SaveChanges();
        }
    }
}
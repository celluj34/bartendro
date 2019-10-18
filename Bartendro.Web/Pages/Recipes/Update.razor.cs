using System;
using System.Linq;
using System.Threading.Tasks;
using Bartendro.Database.Entities;
using Bartendro.Database.Services;
using Bartendro.Web.Models.Recipes;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Bartendro.Web.Pages.Recipes
{
    public class UpdateRecipe : ComponentBase
    {
        protected RecipeUpdateModel Recipe;

        [Inject]
        private IReader Reader {get;set;}

        [Inject]
        private ICommandFactory CommandFactory {get;set;}

        [Parameter]
        public Guid Id {get;set;}

        protected override async Task OnParametersSetAsync()
        {
            Recipe = await Reader.Query<Recipe>()
                                 .Where(x => x.Id == Id)
                                 .Select(x => new RecipeUpdateModel
                                 {
                                     Id = x.Id,
                                     Version = x.Version,
                                     Title = x.Title
                                 })
                                 .SingleOrDefaultAsync();
        }

        protected async Task HandleValidSubmit()
        {
            var result = await CommandFactory.Update<Recipe>(Recipe.Id, Recipe.Version).Run(x => x.Title = Recipe.Title).SaveChanges();
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bartendro.Database.Entities;
using Bartendro.Database.Services;
using Bartendro.Web.Models.Recipes;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Bartendro.Web.Pages.Recipes
{
    public class ListRecipes : ComponentBase
    {
        protected bool Loading = true;
        protected IEnumerable<RecipeListModel> Recipes;

        [Inject]
        private IReader Reader {get;set;}

        protected override async Task OnInitializedAsync()
        {
            Recipes = await GetAllAsync();

            Loading = false;
        }

        protected async Task Refresh()
        {
            Loading = true;

            Recipes = await GetAllAsync();

            Loading = false;
        }

        private async Task<List<RecipeListModel>> GetAllAsync()
        {
            return await Reader.Query<Recipe>()
                               .Select(x => new RecipeListModel
                               {
                                   Id = x.Id,
                                   Title = x.Title
                               })
                               .ToListAsync();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bartendro.Data.Entities;
using Bartendro.Data.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Bartendro.Web.Pages.Recipes
{
    public partial class List
    {
        private bool _loading = true;
        private IEnumerable<RecipeListModel> _recipes;

        [Inject]
        private IReader Reader { get; set; }

        protected override async Task OnInitializedAsync()
        {
            _recipes = await GetAllAsync();

            _loading = false;
        }

        private async Task Refresh()
        {
            _loading = true;

            _recipes = await GetAllAsync();

            await Task.Delay(1000);

            _loading = false;
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

        private class RecipeListModel
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
        }
    }
}
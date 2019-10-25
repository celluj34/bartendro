using System;
using System.Linq;
using System.Threading.Tasks;
using Bartendro.Database.Entities;
using Bartendro.Database.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Bartendro.Web.Pages.Recipes
{
    public partial class Update
    {
        private RecipeUpdateModel _recipe;

        [Inject]
        private IReader Reader {get;set;}

        [Inject]
        private ICommandFactory CommandFactory {get;set;}

        [Parameter]
        public Guid Id {get;set;}

        protected override async Task OnParametersSetAsync()
        {
            _recipe = await Reader.Query<Recipe>()
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
            var result = await CommandFactory.Update<Recipe>(_recipe.Id, _recipe.Version).Run(x => x.Title = _recipe.Title).SaveChanges();
        }

        private class RecipeUpdateModel
        {
            public Guid Id {get;set;}
            public byte[] Version {get;set;}
            public string Title {get;set;}
        }
    }
}
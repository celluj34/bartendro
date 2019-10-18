using System;

namespace Bartendro.Application.Models.Recipes
{
    public class RecipeUpdateModel
    {
        public Guid Id {get;set;}
        public byte[] Version {get;set;}
        public string Title {get;set;}
    }
}
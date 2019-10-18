using System;

namespace Bartendro.Web.Models.Recipes
{
    public class RecipeUpdateModel
    {
        public Guid Id {get;set;}
        public byte[] Version {get;set;}
        public string Title {get;set;}
    }
}
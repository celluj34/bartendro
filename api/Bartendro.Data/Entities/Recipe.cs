using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bartendro.Data.Entities
{
    [Table("Recipes")]
    public class Recipe : Entity
    {
        [Required]
        [MaxLength(128)]
        public string Title { get; set; }

        public List<Ingredient> Ingredients { get; set; }
    }
}
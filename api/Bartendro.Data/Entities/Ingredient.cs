using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bartendro.Data.Entities
{
    [Table("Ingredients")]
    public class Ingredient : Entity
    {
        [Required]
        [MaxLength(128)]
        public string Title { get; set; }

        [Required]
        [MaxLength(128)]
        public string Ounces { get; set; }
    }
}
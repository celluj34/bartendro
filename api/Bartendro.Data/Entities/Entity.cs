using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bartendro.Data.Entities
{
    public abstract class Entity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }

        public bool Deleted { get; set; }

        [Required]
        public DateTimeOffset DateCreated { get; set; }

        public DateTimeOffset? DateModified { get; set; }
    }
}
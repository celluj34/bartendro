using System;
using System.ComponentModel.DataAnnotations;

namespace Bartendro.Database.Entities
{
    public abstract class Entity
    {
        [Key]
        public Guid Id {get;set;}

        [Timestamp]
        public byte[] Version {get;set;}

        [Required]
        public DateTimeOffset DateCreated {get;set;}

        [Required]
        public DateTimeOffset DateModified {get;set;}
    }
}
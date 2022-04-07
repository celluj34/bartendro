using System;

namespace Bartendro.Database.Models
{
    public class EntityModel
    {
        public Guid Id { get; set; }
        public byte[] Version { get; set; }
    }
}
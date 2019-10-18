namespace Bartendro.Database.Models
{
    public class DatabaseResult : EntityResult<EntityModel>
    {
        public DatabaseResult() {}

        public DatabaseResult(EntityModel entityModel) : base(entityModel) {}
    }
}
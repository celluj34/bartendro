namespace Bartendro.Data.Models
{
    public class DatabaseResult : EntityResult<EntityModel>
    {
        public DatabaseResult() {}

        public DatabaseResult(EntityModel entityModel) : base(entityModel) {}
    }
}
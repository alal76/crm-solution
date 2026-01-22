using System;

namespace CRM.Core.Entities
{
    public class EntityTag : BaseEntity
    {
        public string? EntityType { get; set; }
        public int EntityId { get; set; }
        public int TagId { get; set; }
        public string? Tag { get; set; }
    }
}

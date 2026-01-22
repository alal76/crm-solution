using System;

namespace CRM.Core.Entities
{
    public class CustomField : BaseEntity
    {
        public string? EntityType { get; set; }
        public int EntityId { get; set; }
        public string? Key { get; set; }
        public string? Value { get; set; }
    }
}

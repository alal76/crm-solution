using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.WorkflowEngine;

/// <summary>
/// Key-value store for workflow instance variables
/// </summary>
public class WorkflowContextVariable : BaseEntity
{
    public int WorkflowInstanceId { get; set; }
    
    /// <summary>
    /// Variable key/name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Variable value (serialized as JSON for complex types)
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? Value { get; set; }
    
    /// <summary>
    /// Value type: String, Number, Boolean, Object, Array
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string ValueType { get; set; } = "String";
    
    /// <summary>
    /// Step key that last set this variable
    /// </summary>
    [MaxLength(100)]
    public string? SetByStepKey { get; set; }
    
    /// <summary>
    /// Whether this variable is encrypted
    /// </summary>
    public bool IsEncrypted { get; set; } = false;
    
    /// <summary>
    /// Whether this is a system variable (not user-modifiable)
    /// </summary>
    public bool IsSystemVariable { get; set; } = false;
    
    // Navigation properties
    public virtual WorkflowInstance WorkflowInstance { get; set; } = null!;
}

/// <summary>
/// Variable value types
/// </summary>
public static class WorkflowVariableTypes
{
    public const string String = "String";
    public const string Number = "Number";
    public const string Boolean = "Boolean";
    public const string DateTime = "DateTime";
    public const string Object = "Object";
    public const string Array = "Array";
}

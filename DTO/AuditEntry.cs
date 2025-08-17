using System.ComponentModel.DataAnnotations;

namespace AuditService.DTO
{

    public enum AuditAction
    {
        Created,
        Updated,
        Deleted
    }

    public class AuditEntry
    {
        public int Id { get; set; }

        [Required]
        public string EntityName { get; set; } = string.Empty;

        [Required]
        public string EntityId { get; set; } = string.Empty;

        [Required]
        public AuditAction Action { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? Changes { get; set; }

        public string? AdditionalMetadata { get; set; }

        // Navigation property for changes
        public List<AuditFieldChange> FieldChanges { get; set; } = new();
    }

}

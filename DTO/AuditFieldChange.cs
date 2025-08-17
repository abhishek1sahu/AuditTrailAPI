using System.ComponentModel.DataAnnotations;

namespace AuditService.DTO
{
    public class AuditFieldChange
    {
        public int Id { get; set; }

        public int AuditEntryId { get; set; }

        [Required]
        public string FieldName { get; set; } = string.Empty;

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public string? FieldType { get; set; }

        // Navigation property
        public AuditEntry AuditEntry { get; set; } = null!;
    }

}

using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace AuditService.DTO
{
    public class AuditRequest
    {
        [Required]
        public string EntityName { get; set; } = string.Empty;

        [Required]
        public string EntityId { get; set; } = string.Empty;

        [Required]
        public AuditAction Action { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public JsonElement? ObjectBefore { get; set; } 

        public JsonElement? ObjectAfter { get; set; }

        public Dictionary<string, object>? AdditionalMetadata { get; set; }
    }

}

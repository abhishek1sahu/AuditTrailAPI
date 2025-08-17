namespace AuditService.DTO
{
    public class AuditResponse
    {
        public int AuditId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public AuditAction Action { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public List<FieldChangeDto> Changes { get; set; } = new();
        public Dictionary<string, object>? AdditionalMetadata { get; set; }
    }

}

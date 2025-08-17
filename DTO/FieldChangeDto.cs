namespace AuditService.DTO
{
    public class FieldChangeDto
    {
        public string FieldName { get; set; } = string.Empty;
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
        public string? FieldType { get; set; }
    }

}

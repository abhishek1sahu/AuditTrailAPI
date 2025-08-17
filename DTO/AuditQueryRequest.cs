namespace AuditService.DTO
{
    public class AuditQueryRequest
    {
        public string? EntityName { get; set; }
        public string? EntityId { get; set; }
        public string? UserId { get; set; }
        public AuditAction? Action { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}

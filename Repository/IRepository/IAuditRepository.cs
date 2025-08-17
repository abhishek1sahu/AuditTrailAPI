using AuditService.DTO;

namespace AuditService.Repository.IRepository
{
    public interface IAuditRepository
    {
        Task<AuditEntry> CreateAuditEntryAsync(AuditEntry entry);
        Task<PagedResult<AuditEntry>> GetAuditEntriesAsync(AuditQueryRequest query);
        Task<AuditEntry?> GetAuditEntryByIdAsync(int id);
        Task<List<AuditEntry>> GetEntityHistoryAsync(string entityName, string entityId);
    }
}

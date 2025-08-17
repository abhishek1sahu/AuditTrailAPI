using AuditService.DTO;

namespace AuditService.Service.IService
{
    public interface IAuditService
    {
        Task<AuditResponse> CreateAuditEntryAsync(AuditRequest request);
        Task<PagedResult<AuditResponse>> GetAuditEntriesAsync(AuditQueryRequest query);
        Task<AuditResponse?> GetAuditEntryByIdAsync(int id);
        Task<List<AuditResponse>> GetEntityHistoryAsync(string entityName, string entityId);

    }
}

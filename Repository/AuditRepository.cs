using AuditService.Data;
using AuditService.DTO;
using AuditService.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace AuditService.Repository
{
    public class AuditRepository : IAuditRepository
    {
        private readonly AuditDbContext _context;

        public AuditRepository(AuditDbContext context)
        {
            _context = context;
        }

        public async Task<AuditEntry> CreateAuditEntryAsync(AuditEntry entry)
        {
            _context.AuditEntries.Add(entry);
            await _context.SaveChangesAsync();
            return entry;
        }

        public async Task<PagedResult<AuditEntry>> GetAuditEntriesAsync(AuditQueryRequest query)
        {
            var queryable = _context.AuditEntries
                .Include(a => a.FieldChanges)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(query.EntityName))
                queryable = queryable.Where(a => a.EntityName == query.EntityName);

            if (!string.IsNullOrEmpty(query.EntityId))
                queryable = queryable.Where(a => a.EntityId == query.EntityId);

            if (!string.IsNullOrEmpty(query.UserId))
                queryable = queryable.Where(a => a.UserId == query.UserId);

            if (query.Action.HasValue)
                queryable = queryable.Where(a => a.Action == query.Action.Value);

            if (query.FromDate.HasValue)
                queryable = queryable.Where(a => a.Timestamp >= query.FromDate.Value);

            if (query.ToDate.HasValue)
                queryable = queryable.Where(a => a.Timestamp <= query.ToDate.Value);

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .OrderByDescending(a => a.Timestamp)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<AuditEntry>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<AuditEntry?> GetAuditEntryByIdAsync(int id)
        {
            return await _context.AuditEntries
                .Include(a => a.FieldChanges)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<List<AuditEntry>> GetEntityHistoryAsync(string entityName, string entityId)
        {
            return await _context.AuditEntries
                .Include(a => a.FieldChanges)
                .Where(a => a.EntityName == entityName && a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
    }

}

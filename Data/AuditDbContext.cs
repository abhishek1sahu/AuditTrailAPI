using AuditService.DTO;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AuditService.Data
{
    public class AuditDbContext : DbContext
    {
        public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

        public DbSet<AuditEntry> AuditEntries { get; set; }
        public DbSet<AuditFieldChange> AuditFieldChanges { get; set; }
    }
}

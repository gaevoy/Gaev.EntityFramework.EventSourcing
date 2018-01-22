using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Gaev.EntityFramework.EventSourcing
{
    public abstract class DbContextBase : DbContext
    {
        private readonly IChangesBroadcaster _broadcaster;
        
        public DbSet<EntityEvent> Events { get; set; }

        protected DbContextBase(DbContextOptions options, IChangesBroadcaster broadcaster) : base(options)
        {
            _broadcaster = broadcaster;
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChanges, CancellationToken cancellation = new CancellationToken())
        {
            return _broadcaster.WrapSaveAsync(this, () => base.SaveChangesAsync(acceptAllChanges, cancellation));
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            return _broadcaster.WrapSave(this, () => base.SaveChanges(acceptAllChangesOnSuccess));
        }
    }
}
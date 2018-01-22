using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Gaev.EntityFramework.EventSourcing
{
    public abstract class DbContextBase : DbContext
    {
        private readonly IChangeDataCapture _wrapper;
        
        public DbSet<EntityChange> Events { get; set; }

        protected DbContextBase(DbContextOptions options, IChangeDataCapture wrapper) : base(options)
        {
            _wrapper = wrapper;
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChanges, CancellationToken cancellation = new CancellationToken())
        {
            return _wrapper.SaveChangesAsync(this, () => base.SaveChangesAsync(acceptAllChanges, cancellation));
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            return _wrapper.SaveChanges(this, () => base.SaveChanges(acceptAllChangesOnSuccess));
        }
    }
}
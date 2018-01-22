using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Gaev.EntityFramework.EventSourcing.Tests
{
    public static class Subscriptions
    {
        private static readonly Func<DbContextBase, long, int, AsyncEnumerable<EntityEvent>> GetEventsQuery
            = EF.CompileAsyncQuery((DbContextBase db, long checkpoint, int take)
                => db.Events.OrderBy(e => e.Id).Where(e => e.Id > checkpoint).Take(take));

        public static async Task CatchUp(Func<object, Task> handle, TestDbContext context,
            CancellationToken cancellation)
        {
            var checkpointId = "main";
            var checkpoint = await context.Checkpoints.FirstOrDefaultAsync(e => e.Id == checkpointId);
            if (checkpoint == null)
            {
                checkpoint = new Checkpoint {Id = checkpointId};
                context.Checkpoints.Add(checkpoint);
            }

            while (!cancellation.IsCancellationRequested)
            {
                var events = await GetEventsQuery(context, checkpoint.EventId, 1000).ToListAsync();
                foreach (var evt in events)
                {
                    await handle(Mappings.Deserialize(evt));
                    checkpoint.EventId = evt.Id;
                    await context.SaveChangesAsync();
                }

                try
                {
                    await Task.Delay(100, cancellation);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
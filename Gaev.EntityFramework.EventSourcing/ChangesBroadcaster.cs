using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using static Microsoft.EntityFrameworkCore.EntityState;

namespace Gaev.EntityFramework.EventSourcing
{
    public class ChangesBroadcaster : IChangesBroadcaster
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None
        };
        private readonly IEntityMapper _mapper;

        public ChangesBroadcaster(IEntityMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<int> WrapSaveAsync(DbContext context, Func<Task<int>> save)
        {
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                var changes = GetChanges(context);
                var result = await save();
                var events = ToEvents(changes);
                AddEvents(context, events);
                await save();
                transaction.Commit();
                return result;
            }
        }

        public int WrapSave(DbContext context, Func<int> save)
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                var changes = GetChanges(context);
                var result = save();
                var events = ToEvents(changes);
                AddEvents(context, events);
                save();
                transaction.Commit();
                return result;
            }
        }

        private static List<Change> GetChanges(DbContext context)
        {
            return (
                from change in context.ChangeTracker.Entries()
                where change.State == Added || change.State == Modified || change.State == Deleted
                select new Change(change)).ToList();
        }

        private IEnumerable<object> ToEvents(List<Change> changes)
        {
            return changes.Select(e => _mapper.ToEvent(e.State, e.Entity)).Where(e => e != null);
        }

        private static void AddEvents(DbContext context, IEnumerable<object> events)
        {
            var changes = events.Select(evt => new EntityEvent
            {
                Type = evt.GetType().FullName,
                Payload = JsonConvert.SerializeObject(evt, JsonSerializerSettings)
            });
            context.Set<EntityEvent>().AddRange(changes);
        }

        private struct Change
        {
            public Change(EntityEntry entity)
            {
                State = entity.State;
                Entity = entity.Entity;
            }

            public EntityState State { get; }
            public object Entity { get; }
        }
    }
}
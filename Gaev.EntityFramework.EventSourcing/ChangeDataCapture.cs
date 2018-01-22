using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.EntityState;

namespace Gaev.EntityFramework.EventSourcing
{
    public class ChangeDataCapture : IChangeDataCapture
    {
        private readonly IEventMapper _mapper;
        private readonly IEventStorage _storage;

        public ChangeDataCapture(IEventMapper mapper, IEventStorage storage = null)
        {
            _mapper = mapper;
            _storage = storage ?? new DefaultEventStorage();
        }

        public async Task<int> SaveChangesAsync(DbContext context, Func<Task<int>> save)
        {
            var changes = GetChanges(context);
            var result = await save();
            var events = ToEvents(changes);
            await _storage.SaveAsync(events, context, save);
            return result;
        }

        public int SaveChanges(DbContext context, Func<int> save)
        {
            var changes = GetChanges(context);
            var result = save();
            var events = ToEvents(changes);
            _storage.Save(events, context, save);
            return result;
        }

        private IEnumerable<object> ToEvents(List<Change> changes)
        {
            return changes.Select(e => _mapper.ToEvent(e.State, e.Entity)).Where(e => e != null);
        }

        private static List<Change> GetChanges(DbContext context)
        {
            return (
                from change in context.ChangeTracker.Entries()
                where change.State == Added || change.State == Modified || change.State == Deleted
                select new Change(change.State, change.Entity)).ToList();
        }

        private struct Change
        {
            public Change(EntityState state, object entity)
            {
                State = state;
                Entity = entity;
            }

            public EntityState State { get; }
            public object Entity { get; }
        }
    }
}
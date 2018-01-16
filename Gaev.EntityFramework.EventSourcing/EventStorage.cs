using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Gaev.EntityFramework.EventSourcing
{
    public class DefaultEventStorage : IEventStorage
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.None
        };

        public Task SaveAsync(IEnumerable<object> events, DbContext context, Func<Task<int>> save)
        {
            AddEvents(context, events);
            return save();
        }

        public void Save(IEnumerable<object> events, DbContext context, Func<int> save)
        {
            AddEvents(context, events);
            save();
        }

        private static void AddEvents(DbContext context, IEnumerable<object> events)
        {
            var now = DateTimeOffset.UtcNow;
            var changes = events.OfType<EntityChange>().Select(evt => new EntityChange
            {
                Timestamp = now,
                Type = evt.GetType().Name,
                Payload = JsonConvert.SerializeObject(evt, JsonSerializerSettings)
            });
            context.Set<EntityChange>().AddRange(changes);
        }
    }
}
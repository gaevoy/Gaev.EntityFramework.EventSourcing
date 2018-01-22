using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Gaev.EntityFramework.EventSourcing.Tests
{
    public static class DbContextBaseExt
    {
        private static readonly Func<DbContextBase, long, int, IEnumerable<EntityEvent>> GetEventsQuery
            = EF.CompileQuery((DbContextBase db, long checkpoint, int take)
                => db.Events.OrderBy(e => e.Id).Where(e => e.Id > checkpoint).Take(take));
        
        public static IEnumerable<EntityEvent> GetEvents(this DbContextBase context, long? checkpoint, int take = 1000)
        {
            return GetEventsQuery(context, checkpoint ?? 0, take);
        }
    }
}
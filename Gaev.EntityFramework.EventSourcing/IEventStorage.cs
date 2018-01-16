using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Gaev.EntityFramework.EventSourcing
{
    public interface IEventStorage
    {
        Task SaveAsync(IEnumerable<object> events, DbContext context, Func<Task<int>> save);
        void Save(IEnumerable<object> events, DbContext context, Func<int> save);
    }
}
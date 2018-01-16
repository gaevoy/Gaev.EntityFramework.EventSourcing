using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Gaev.EntityFramework.EventSourcing
{
    public interface IChangeDataCapture
    {
        Task<int> SaveChangesAsync(DbContext context, Func<Task<int>> save);
        int SaveChanges(DbContext context, Func<int> save);
    }
}
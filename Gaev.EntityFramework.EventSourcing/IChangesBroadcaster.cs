using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Gaev.EntityFramework.EventSourcing
{
    public interface IChangesBroadcaster
    {
        Task<int> WrapSaveAsync(DbContext context, Func<Task<int>> save);
        int WrapSave(DbContext context, Func<int> save);
    }
}
using Microsoft.EntityFrameworkCore;

namespace Gaev.EntityFramework.EventSourcing
{
    public interface IEventMapper
    {
        object ToEvent(EntityState state, object entity);
    }
}
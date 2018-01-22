using Microsoft.EntityFrameworkCore;

namespace Gaev.EntityFramework.EventSourcing
{
    public interface IEntityMapper
    {
        object ToEvent(EntityState state, object entity);
    }
}
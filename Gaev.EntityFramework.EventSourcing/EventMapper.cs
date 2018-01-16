using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Gaev.EntityFramework.EventSourcing
{
    public class EventMapper : IEventMapper
    {
        private readonly Dictionary<Type, Func<object, EntityState, object>> _mapping =
            new Dictionary<Type, Func<object, EntityState, object>>();

        private static readonly Func<object, EntityState, object> ReturnNull = (entity, state) => null;

        public object ToEvent(EntityState state, object entity)
        {
            if (_mapping.TryGetValue(entity.GetType(), out var mapping))
                return mapping(entity, state);
            throw new ApplicationException($"{entity.GetType().Name} is not mapped in {GetType().Name}");
        }

        public EventMapper Map<TEntity>(Func<TEntity, EntityState, object> mapping)
        {
            _mapping[typeof(TEntity)] = (entity, state) => mapping((TEntity) entity, state);
            return this;
        }

        public EventMapper Ignore<TEntity>()
        {
            _mapping[typeof(TEntity)] = ReturnNull;
            return this;
        }
    }
}
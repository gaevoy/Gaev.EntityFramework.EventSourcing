using System;

namespace Gaev.EntityFramework.EventSourcing
{
    public class EntityChange
    {
        public long Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }
    }
}
namespace Gaev.EntityFramework.EventSourcing
{
    public class EntityEvent
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }
    }
}
namespace Gaev.EntityFramework.EventSourcing
{
    public class EntityChange
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }
    }
}
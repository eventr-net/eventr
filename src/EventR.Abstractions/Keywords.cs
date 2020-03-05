namespace EventR.Abstractions
{
    using System.Diagnostics.Tracing;

    public static class Keywords
    {
        public const EventKeywords Session = (EventKeywords)(1 << 1);
        public const EventKeywords Persistence = (EventKeywords)(1 << 2);
        public const EventKeywords Serialization = (EventKeywords)(1 << 3);
        public const EventKeywords TransactionEnlistement = (EventKeywords)(1 << 4);
    }
}

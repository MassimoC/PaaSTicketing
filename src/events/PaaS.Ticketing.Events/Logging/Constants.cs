namespace PaaS.Ticketing.Events.Logging
{
    public static class Constants
    {
        // Template for consistent logging accross components: 
        // EventDescription is a short description of the Event being logged. 
        // EntityType: Business Entity Type being processed: e.g. Order, Shipment, etc.
        // EntityId: Id of the Business Entity being processed: e.g. Order Number, Shipment Id, etc. 
        // Status: Status of the Log Event, e.g. Succeeded, Failed, Discarded.
        // CorrelationId: Unique identifier of the message that can be processed by more than one component. 
        // CheckPoint: To classify and be able to correlate tracking events.
        // Description: A detailed description of the log event. 
        public const string Template = "{ShortDescription}, {EntityType}, {EntityId}, {Status}, {CorrelationId}, {CheckPoint}, {LongDescription}";

        public enum EntityType
        {
            Order,
            User,
            Concert
        }
        public enum CheckPoint
        {
            Publisher,
            Subscriber
        }
        public enum EventId
        {
            Processing = 1000,
            Retry = 2000,
            Failure = 3000
        }

        public enum Status
        {
            Pending,
            Paid,
            Delivered,
            Invoiced,
            Completed,
            Canceled
        }
    }
}

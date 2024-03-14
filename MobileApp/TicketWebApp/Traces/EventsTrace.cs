using System.Diagnostics;

namespace MyTraces
{
    public static class GetAllEventsTrace
    {
        public static readonly string EventsTraceServiceName = "my-first-trace";
        public static readonly ActivitySource MyActivitySource = new(EventsTraceServiceName);
    }

    public static class GetOneEventTrace
    {
        public static readonly string OneEventTraceServiceName = "get-single-event";
        public static readonly ActivitySource GetSingleEventActivitySource = new(OneEventTraceServiceName);
    }
}

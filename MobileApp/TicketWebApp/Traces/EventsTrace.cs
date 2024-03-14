using System.Diagnostics;

namespace MyTraces
{
    public static class GetAllEventsTrace
    {
        public static readonly string EventsTraceServiceName = "my-first-trace";
        public static readonly ActivitySource MyActivitySource = new(EventsTraceServiceName);
    }
}

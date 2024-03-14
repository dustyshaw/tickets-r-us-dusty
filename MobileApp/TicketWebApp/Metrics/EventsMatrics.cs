

using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.VisualBasic;

namespace MyMetrics;

public static class EventsMetic
{
    public static readonly string MyMeterName = "Events.Metering.GetSingleEvent";
    public static Meter EventsMeter = new Meter(MyMeterName, "0.0.1");
    public static Counter<int> NumOfCalls = EventsMeter.CreateCounter<int>("alleventscallcount", description: "Counts the number of calls to the Get all events API endpoint");

    // public static ActivitySource EventsActivitySource = new ActivitySource(MyMeterName);
}
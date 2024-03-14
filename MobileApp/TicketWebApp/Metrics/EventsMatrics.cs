

using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.VisualBasic;

namespace MyMetrics;

public static class EventsMetic
{
    public static Meter EventsMeter = new Meter("Events.Metering.GetSingleEvent", "0.0.1");
    public static Counter<int> NumOfCalls = EventsMeter.CreateCounter<int>("alleventscallcount", description: "Counts the number of calls to the Get all events API endpoint");
}

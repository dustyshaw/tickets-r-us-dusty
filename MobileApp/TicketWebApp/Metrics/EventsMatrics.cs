

using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.VisualBasic;

namespace MyMetrics;

public static class EventsMetic
{
    public static Meter EventsMeter = new Meter("Events.Metering.GetSingleEvent", "0.0.1");
    public static Counter<int> NumOfCalls = EventsMeter.CreateCounter<int>("alleventscallcount", description: "Counts the number of calls to the Get all events API endpoint");

    public static Meter UpDownMeter = new Meter("UpAndDownMeter", "0.0.1");
    public static UpDownCounter<int> NumberOfUpDownClicks = UpDownMeter.CreateUpDownCounter<int>("UpAndDownCounter", description: "Counts the number of up and down clicks");

    //public static Meter ObservableMetric = new Meter("ObservableThingy", "0.0.1");
    //public static ObservableCounter<int> NumberObservations = ObservableMetric.CreateObservableCounter<int>("ObservingThingy", description: "Observs things");


}

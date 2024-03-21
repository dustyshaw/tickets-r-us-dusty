

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualBasic;

namespace MyMetrics;

public static class EventsMetic
{
    public static Meter EventsMeter = new Meter("Events.Metering.GetSingleEvent", "0.0.1");
    public static Counter<int> NumOfCalls = EventsMeter.CreateCounter<int>(
        "alleventscallcount",
        description: "Counts the number of calls to the Get all events API endpoint");


    public static UpDownCounter<int> NumberOfUpDownClicks = EventsMeter.CreateUpDownCounter<int>(
        "UpAndDownCounter",
         description: "Counts the number of up and down clicks");

    
    public static int NumberOfCallsForCreateTicket = 0;
    public static ObservableCounter<int> NumberObservations = EventsMeter.CreateObservableCounter<int>("CreateTicketObserver", () => NumberOfCallsForCreateTicket );


    public static int NumberOfUnscannedTickets = 0;
    public static ObservableUpDownCounter<int> NumberUpDownObservations = EventsMeter.CreateObservableUpDownCounter<int>("NumberOfUnscannedTickets", () => NumberOfUnscannedTickets);


    public static int NumberOfActiveusersNumber = 0;
    public static ObservableGauge<int> ActiveUsersGauge = EventsMeter.CreateObservableGauge<int>("Gauge", () => NumberOfActiveusersNumber);


    public static Histogram<int> histogram = EventsMeter.CreateHistogram<int>("MyHistogram");

}

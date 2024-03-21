using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using MyMetrics;
using MyTraces;
using OpenTelemetry.Trace;
using TicketClassLib.Data;
using TicketClassLib.Services;
using TicketWebApp.Components.Pages;
using TicketWebApp.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TicketWebApp.Services;



public partial class ApiEventService : IEventService
{
    private readonly ILogger<ApiEventService> logger;
    private readonly IDbContextFactory<PostgresContext> dbFactory;

    public ApiEventService(ILogger<ApiEventService> logger, IDbContextFactory<PostgresContext> dbFactory)
    {
        this.dbFactory = dbFactory;
        this.logger = logger;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Dusty - ApiEventService: Get All Events.")]
#pragma warning disable SYSLIB1015 // Argument is not referenced from the logging message
    public static partial void LogAddEventServiceCall(ILogger logger, string description);
#pragma warning restore SYSLIB1015 // Argument is not referenced from the logging message


    [LoggerMessage(Level = LogLevel.Critical, Message = "Dusty - ApiEventService: Get Single Event.")]
#pragma warning disable SYSLIB1015 // Argument is not referenced from the logging message
    public static partial void LogGetSingleEventServiceCall(ILogger logger, string description);
#pragma warning restore SYSLIB1015 // Argument is not referenced from the logging message

    public async Task<Event> AddEvent(string name, DateTime date)
    {
        EventsMetic.NumOfCalls.Add(1);
        using var context = await dbFactory.CreateDbContextAsync();
        Event newEvent = new Event()
        {
            Id = 0,
            Name = name,
            Eventdate = date
        };

        var value = await context.Events.AddAsync(newEvent);
        await context.SaveChangesAsync();

        return newEvent;
    }

    public async Task<Event> AddEvent(Event newEvent)
    {
        EventsMetic.NumOfCalls.Add(1);
        using var context = await dbFactory.CreateDbContextAsync();

        var value = await context.Events.AddAsync(newEvent);
        await context.SaveChangesAsync();

        return newEvent;
    }

    public async Task<List<Event>> GetAll()
    {
        LogAddEventServiceCall(logger, "Getting All Events");
        EventsMetic.NumOfCalls.Add(1);
        using var myActivity = GetAllEventsTrace.MyActivitySource.StartActivity("Events.GetAll");

        using var context = await dbFactory.CreateDbContextAsync();

        return await context.Events
            .Include(e => e.Tickets)
            .ToListAsync();
    }

    public async Task<Event?> GetEvent(int id)
    {
        LogGetSingleEventServiceCall(logger, $"Getting a Single Event with ID {id}");

        EventsMetic.NumOfCalls.Add(1);
        using var myActivity = GetOneEventTrace.GetSingleEventActivitySource.StartActivity("Events.GetOne");

        using var context = await dbFactory.CreateDbContextAsync();

        return await context.Events
            .Include(e => e.Tickets)
            .Where(e => e.Id == id)
            .FirstOrDefaultAsync();
    }
}

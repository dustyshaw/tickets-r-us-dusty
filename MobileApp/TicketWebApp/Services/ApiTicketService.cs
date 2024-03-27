using Microsoft.EntityFrameworkCore;
using TicketClassLib.Data;
using TicketClassLib.Enums;
using TicketClassLib.Exceptions;
using TicketClassLib.Requests;
using TicketClassLib.Services;
using TicketWebApp.Data;


namespace TicketWebApp.Services;

public partial class ApiTicketService : ITicketService
{
    private readonly ILogger<ApiTicketService> logger;
    private readonly IDbContextFactory<PostgresContext> dbFactory;

    public ApiTicketService(ILogger<ApiTicketService> logger, IDbContextFactory<PostgresContext> dbFactory)
    {
        this.dbFactory = dbFactory;
        this.logger = logger;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Dusty - ApiTicketService: Get All Tickets.")]
#pragma warning disable SYSLIB1015 // Argument is not referenced from the logging message
    public static partial void LogTicketServiceCall(ILogger logger, string description);
#pragma warning restore SYSLIB1015 // Argument is not referenced from the logging message

    [LoggerMessage(Level = LogLevel.Information, Message = "Dusty - ApiTicketService: Add Ticket.")]
#pragma warning disable SYSLIB1015 // Argument is not referenced from the logging message
    public static partial void LogCreateTicketServiceCall(ILogger logger, string description);
#pragma warning restore SYSLIB1015 // Argument is not referenced from the logging message

    public event EventHandler? TicketsHaveChanged;

    public async Task<Ticket> CreateNewTicket(AddTicketRequest newRequest)
    {
        MyMetrics.EventsMetic.NumberOfCallsForCreateTicket++;
        MyMetrics.EventsMetic.NumberOfUnscannedTickets++;

        LogCreateTicketServiceCall(logger, $"Adding ticket with event id {newRequest.EventId}");

        using var context = await dbFactory.CreateDbContextAsync();

        Guid guid = Guid.NewGuid();
        Ticket ticket = new Ticket()
        {
            Id = guid,
            Eventid = newRequest.EventId,
            Name = newRequest.UserName,
            Isscanned = false,
            Lastupdated = DateTime.UtcNow,
        };

        context.Tickets.Add(ticket);
        await context.SaveChangesAsync();

        TicketsHaveChanged?.Invoke(this, new EventArgs());
        return ticket;


    }

    public async Task<List<Ticket>> GetAll()
    {


        LogTicketServiceCall(logger, "Getting All Tickets");

        using var context = await dbFactory.CreateDbContextAsync();

        return await context.Tickets
            .Include(t => t.Event)
            .ToListAsync();
    }

    public async Task<TicketStatus> ScanTicket(Guid TicketId, int EventId)
    {
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();

        MyMetrics.EventsMetic.NumberOfUnscannedTickets--;
        using var context = await dbFactory.CreateDbContextAsync();

        Ticket? tuc = await context.Tickets.FirstOrDefaultAsync(x => x.Id == TicketId);

        if (tuc == null || tuc.Eventid != EventId)
        {
            return TicketStatus.Unrecognized;
        }
        else if (tuc.Isscanned == true)
        {
            return TicketStatus.Used;
        }

        tuc.Isscanned = true;
        tuc.Lastupdated = DateTime.UtcNow;
        context.Tickets.Update(tuc);
        await context.SaveChangesAsync();

        TicketsHaveChanged?.Invoke(this, new EventArgs());

        watch.Stop();
        MyMetrics.EventsMetic.histogram.Record(watch.ElapsedMilliseconds);

        return TicketStatus.Success;
    }

    public async Task<Ticket> AddTicket(Ticket ticket)
    {
        MyMetrics.EventsMetic.NumberOfCallsForCreateTicket++;

        using var context = await dbFactory.CreateDbContextAsync();

        await context.Tickets.AddAsync(ticket);
        await context.SaveChangesAsync();

        TicketsHaveChanged?.Invoke(this, new EventArgs());

        return ticket;
    }

    public async Task<Ticket> UpdateTicket(Ticket ticket)
    {
        using var context = await dbFactory.CreateDbContextAsync();

        var tuc = await context.Tickets.Where(x => x.Id == ticket.Id).FirstOrDefaultAsync();

        if (tuc is null || (ticket.Isscanned == true && tuc.Isscanned == true))
        {
            throw new AlreadyScannedTicketException("Ticket already scanned");
        }

        tuc.Lastupdated = ticket.Lastupdated.ToUniversalTime();
        tuc.Isscanned = ticket.Isscanned;

        context.Update(tuc);
        try
        {
            await context.SaveChangesAsync();

        }
        catch
        {
            throw new Exception();
        }

        return tuc;
    }
}

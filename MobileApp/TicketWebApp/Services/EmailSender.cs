using System.Net;
using System.Net.Mail;
using MailKit.Net.Smtp;
using QRCoder;
namespace TicketWebApp.Services;

partial class EmailSender
{
    private readonly ILogger<EmailSender> logger;
    private string secretSender { get; set; }
    private string fromEmail { get; set; }

    public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
    {
        //secretSender = config["DustySecret"] ?? throw new Exception("Missing dusty email config");
        //fromEmail = config["DustysEmail"] ?? throw new Exception("Missing dusty email password config");


        this.logger = logger;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Dusty - EmailSender: Sending an Email.")]
#pragma warning disable SYSLIB1015 // Argument is not referenced from the logging message
    public static partial void LogAttemptingToSendAnEmail(ILogger logger, string description);
#pragma warning restore SYSLIB1015 // Argument is not referenced from the logging message

    public string sendEmail(MailAddress ReceiverEmail,
                            Guid ticketId)
    {
        LogAttemptingToSendAnEmail(logger, $"Sending Email for ticket {ticketId}");
        try
        {
            var from = new MailAddress(fromEmail, "TicketsRUs");
            var to = ReceiverEmail;
            var message = new MailMessage(from, to);

            //qrcode generation
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(ticketId.ToString(), QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] image = qrCode.GetGraphic(10);//an image
            Stream stream = new MemoryStream(image);
            var qrAttachment = new Attachment(stream, "qrCode.png");



            string body = $"<img src=\"cid:{qrAttachment.ContentId}\" />";
            message.Attachments.Add(qrAttachment);


            message.Subject = "no-reply Event Ticket QR Code";
            message.IsBodyHtml = true;
            message.Body = @$"
                <html><body>
                <h1>Event Ticket</h1>
                <p>Bring your ticket to the event to scan in. Enjoy the show!</p>
                {body}
                </body></html>";


            using (var client = new System.Net.Mail.SmtpClient("smtp.gmail.com"))
            {
                client.Port = 587;
                client.EnableSsl = true;


                // Note: only needed if the SMTP server requires authentication
                client.Credentials = new NetworkCredential(fromEmail, secretSender);

                client.Send(message);
            }
            return "Email Sent";
        }
        catch
        {
            return "Bad Exception Happened";
        }
    }
}

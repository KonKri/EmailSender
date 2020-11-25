using EmailSender.Api.Extentions;
using EmailSender.Api.Models;
using Ical.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace EmailSender.Api.Functions
{
    internal class EmailSenderFunction
    {
        private readonly ApiAuthorization _apiAuth;

        public EmailSenderFunction(ApiAuthorization apiAuth)
        {
            _apiAuth = apiAuth;
        }

        [FunctionName("EmailSenderFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "SendEmail")] HttpRequest req,
            ILogger logger)
        {
            logger.LogInformation("Fuction Triggered");
            // check if api key is valid.
            if (req.Headers["X-API-Key"] != _apiAuth.ValidKey)
            {
                logger.LogWarning("Unauthorized request.");
                return new UnauthorizedResult();
            }

            logger.LogInformation("Authorized.");

            var config = req.Headers.GetConfiguration();

            // validate model. if there are errors, return bad request.
            if (!config.IsModelValid(out var validationResults))
            {
                logger.LogWarning("Validation Error.", validationResults);
                return new BadRequestObjectResult(validationResults);
            }

            using (var client = new SmtpClient()
            {
                Host = config.Host,
                Port = config.Port.Value,
                Timeout = 10000,
                EnableSsl = config.EnableSsl.Value,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(config.SenderEmail, config.Password)
            })
            {
                try
                {
                    var msg = new MailMessage();
                    msg.To.Add(new MailAddress(config.RecipientEmail));
                    msg.From = new MailAddress(config.SenderEmail);
                    msg.Subject = config.Subject;
                    msg.Body = await req.ReadAsStringAsync();
                    //msg.IsBodyHtml = false;

                    StringBuilder str = new StringBuilder();
                    str.AppendLine("BEGIN:VCALENDAR");
                    str.AppendLine("PRODID:-//Schedule a Meeting");
                    str.AppendLine("VERSION:2.0");
                    str.AppendLine("METHOD:REQUEST");
                    str.AppendLine("BEGIN:VEVENT");
                    str.AppendLine(string.Format("DTSTART:{0:yyyyMMddTHHmmssZ}", DateTime.Now.AddMinutes(+330)));
                    str.AppendLine(string.Format("DTSTAMP:{0:yyyyMMddTHHmmssZ}", DateTime.UtcNow));
                    str.AppendLine(string.Format("DTEND:{0:yyyyMMddTHHmmssZ}", DateTime.Now.AddMinutes(+660)));
                    str.AppendLine("LOCATION: " + "Homeee");
                    str.AppendLine(string.Format("UID:{0}", Guid.NewGuid()));
                    //str.AppendLine(string.Format("DESCRIPTION:{0}", msg.Body));
                    //str.AppendLine(string.Format("X-ALT-DESC;FMTTYPE=text/html:{0}", msg.Body));
                    str.AppendLine(string.Format("SUMMARY:{0}", msg.Subject));
                    str.AppendLine(string.Format("ORGANIZER:MAILTO:{0}", msg.From.Address));

                    str.AppendLine(string.Format("ATTENDEE;CN=\"{0}\";RSVP=TRUE:mailto:{1}", msg.To[0].DisplayName, msg.To[0].Address));

                    str.AppendLine("BEGIN:VALARM");
                    str.AppendLine("TRIGGER:-PT15M");
                    str.AppendLine("ACTION:DISPLAY");
                    str.AppendLine("DESCRIPTION:This is not a description.");
                    str.AppendLine("END:VALARM");
                    str.AppendLine("END:VEVENT");
                    str.AppendLine("END:VCALENDAR");

                    msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(msg.Body, new ContentType("text/html")));

                    ContentType contype = new ContentType("text/calendar");
                    contype.Parameters.Add("method", "REQUEST");
                    contype.Parameters.Add("name", "Meeting.ics");
                    AlternateView avCal = AlternateView.CreateAlternateViewFromString(str.ToString(), contype);
                    msg.AlternateViews.Add(avCal);
                    msg.Headers.Add("Content-class", "urn:content-classes:calendarmessage");

                    await client.SendMailAsync(msg);

                    var sucMessage = $"Email with subject {config.Subject} was sent to {config.RecipientEmail} from {config.SenderEmail} successfully probabably. Check {config.RecipientEmail} account for incoming mail. Good luck ;)";
                    logger.LogInformation(sucMessage);
                    return new OkObjectResult(sucMessage);
                }
                catch (Exception exc)
                {
                    var errMessage = $"Exception: {exc.Message} \nInnerException: {exc.InnerException}";
                    logger.LogError(errMessage);
                    return new ContentResult()
                    {
                        Content = errMessage,
                        StatusCode = StatusCodes.Status500InternalServerError
                    };
                }
            }
        }
    }
}

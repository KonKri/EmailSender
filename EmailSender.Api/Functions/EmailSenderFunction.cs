using EmailSender.Api.Extentions;
using EmailSender.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
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
                    msg.IsBodyHtml = true;
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

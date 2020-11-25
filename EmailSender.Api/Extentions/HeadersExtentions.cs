using EmailSender.Api.Models;
using Microsoft.AspNetCore.Http;

namespace EmailSender.Api.Extentions
{
    internal static class HeadersExtentions
    {
        /// <summary>
        /// Get configuration object from header values.
        /// </summary>
        /// <param name="headerDictionary">HTTP Request Headers.</param>
        /// <returns>Configuration Obj.</returns>
        internal static Configuration GetConfiguration(this IHeaderDictionary headerDictionary)
        {
            return new Configuration
            {
                EnableSsl = bool.TryParse(headerDictionary["X-SMTP-Ssl"], out bool enableSsl) ? enableSsl : (bool?)null,
                Host = headerDictionary["X-SMTP-Host"],
                Password = headerDictionary["X-SMTP-Password"],
                Port = int.TryParse(headerDictionary["X-SMTP-Port"], out int port) ? port : (int?)null,
                RecipientEmail = headerDictionary["X-SMTP-Recipient-Email"],
                SenderEmail = headerDictionary["X-SMTP-Sender-Email"],
                Subject = headerDictionary["X-SMTP-Subject"],
            };
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace EmailSender.Api.Models
{
    internal class Configuration
    {
        [Required]
        public string Host { get; set; }

        [Required]
        public int? Port { get; set; }

        [Required]
        public bool? EnableSsl { get; set; }

        [Required]
        public string SenderEmail { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string RecipientEmail { get; set; }
    }
}

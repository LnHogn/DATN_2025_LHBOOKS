
using System.Net;
using System.Net.Mail;

namespace LHBooksWeb.Services.Email

{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("honglam.27012003@gmail.com", "wtcgtrbweyctzquj")
            };

            return client.SendMailAsync(
                new MailMessage(from: "demologin979@gmail.com",
                to: email,
                subject: subject,
                message));
        }
    }
}

﻿
using System.Net;
using System.Net.Mail;

namespace LHBooksWeb.Services.Email

{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message, bool isHTML = true)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("honglam.27012003@gmail.com", "wtcgtrbweyctzquj")
            };

            var mailMessage = new MailMessage(
                from: "honglam.27012003@gmail.com",
                to: email,
                subject: subject,
                message
            );

            // Thiết lập định dạng HTML cho email
            mailMessage.IsBodyHtml = isHTML;

            return client.SendMailAsync(mailMessage);
        }
    }
}

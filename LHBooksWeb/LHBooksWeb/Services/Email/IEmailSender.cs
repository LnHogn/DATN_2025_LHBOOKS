﻿namespace LHBooksWeb.Services.Email
{
    public interface IEmailSender
    {
            Task SendEmailAsync(string email, string subject, string message, bool isHTML = true);

    }
}

using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;

namespace BookStore.Utility;

public class EmailSender: IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        //var emailToSend = new MimeMessage();

        //emailToSend.From.Add(MailboxAddress.Parse("test@gmail.com"));
        //emailToSend.To.Add(MailboxAddress.Parse(email));
        //emailToSend.Subject = subject;
        //emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };

        //using (var emailClient = new SmtpClient())
        //{
        //    emailClient.Connect("smpt.gmail.com",587,MailKit.Security.SecureSocketOptions.StartTls);
        //    emailClient.Authenticate("normaluse09@gmail.com", "huyb rlcq nkap otqz");
        //    emailClient.Send(emailToSend);
        //    emailClient.Disconnect(true);
        //}

        return Task.CompletedTask;
       
    }
}
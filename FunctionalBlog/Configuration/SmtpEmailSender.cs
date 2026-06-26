using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace FunctionalBlog.Configuration;

// An IEmailSender backed by MailKit. Reads SMTP settings from the live ConfigurationCache on
// each send, so changes made in the admin settings page take effect without a restart.
// Throws on a misconfigured server or a failed send; transactional callers (registration,
// password reset) catch and log so a mail hiccup never 500s the request, while the admin
// "send test email" surfaces the error.
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly ConfigurationCache _config;
    private readonly ILog _log;

    public SmtpEmailSender(ConfigurationCache config, ILog log)
    {
        _config = config;
        _log = log;
    }

    public async ValueTask Send(string to, string subject, string body)
    {
        var smtp = _config.Smtp;
        if (!smtp.IsConfigured)
        {
            throw new InvalidOperationException("SMTP ist nicht konfiguriert (Host und Absenderadresse fehlen).");
        }

        using var message = new MimeMessage();
        message.From.Add(new MailboxAddress(smtp.FromName, smtp.FromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        var security = smtp.UseSsl ? SecureSocketOptions.StartTlsWhenAvailable : SecureSocketOptions.None;
        await client.ConnectAsync(smtp.Host, smtp.Port, security);

        if (!string.IsNullOrWhiteSpace(smtp.Username))
        {
            await client.AuthenticateAsync(smtp.Username, smtp.Password);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(quit: true);
        _log.Info($"[E-Mail] gesendet an {to}: \"{subject}\"");
    }
}

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace important_game.infrastructure.Contexts.Newsletter.Email;

[ExcludeFromCodeCoverage]
public class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailOptions> options, ILogger<SmtpEmailService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<EmailSendResult> SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateSmtpClient();
            using var message = CreateMailMessage(toEmail, subject, htmlBody);

            await client.SendMailAsync(message, cancellationToken);

            _logger.LogDebug("Email sent successfully to {Email}", toEmail);
            return new EmailSendResult { Success = true, Email = toEmail };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            return new EmailSendResult { Success = false, Email = toEmail, ErrorMessage = ex.Message };
        }
    }

    public async Task<List<EmailSendResult>> SendBulkEmailAsync(
        IEnumerable<EmailRecipient> recipients,
        string subject,
        Func<EmailRecipient, string> htmlBodyGenerator,
        CancellationToken cancellationToken = default)
    {
        var results = new List<EmailSendResult>();
        var recipientList = recipients.ToList();
        var batchSize = _options.BatchSize;
        var delayMs = _options.DelayBetweenBatchesMs;

        _logger.LogInformation("Starting bulk email send to {Count} recipients in batches of {BatchSize}",
            recipientList.Count, batchSize);

        for (int i = 0; i < recipientList.Count; i += batchSize)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var batch = recipientList.Skip(i).Take(batchSize);
            var batchNumber = (i / batchSize) + 1;
            var totalBatches = (int)Math.Ceiling((double)recipientList.Count / batchSize);

            _logger.LogInformation("Processing batch {BatchNumber}/{TotalBatches}", batchNumber, totalBatches);

            foreach (var recipient in batch)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var htmlBody = htmlBodyGenerator(recipient);
                var result = await SendEmailAsync(recipient.Email, subject, htmlBody, cancellationToken);
                results.Add(result);
            }

            // Delay between batches to avoid rate limiting
            if (i + batchSize < recipientList.Count && delayMs > 0)
            {
                await Task.Delay(delayMs, cancellationToken);
            }
        }

        var successCount = results.Count(r => r.Success);
        var failureCount = results.Count(r => !r.Success);

        _logger.LogInformation("Bulk email send completed. Success: {Success}, Failed: {Failed}",
            successCount, failureCount);

        return results;
    }

    private SmtpClient CreateSmtpClient()
    {
        var client = new SmtpClient(_options.Smtp.Host, _options.Smtp.Port)
        {
            EnableSsl = _options.Smtp.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrEmpty(_options.Smtp.Username))
        {
            client.Credentials = new NetworkCredential(_options.Smtp.Username, _options.Smtp.Password);
        }

        return client;
    }

    private MailMessage CreateMailMessage(string toEmail, string subject, string htmlBody)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        message.To.Add(new MailAddress(toEmail));

        // Add List-Unsubscribe header for email clients
        message.Headers.Add("List-Unsubscribe", $"<{_options.BaseUrl}/unsubscribe>");
        message.Headers.Add("List-Unsubscribe-Post", "List-Unsubscribe=One-Click");

        return message;
    }
}

namespace important_game.infrastructure.Contexts.Newsletter.Email;

public interface IEmailService
{
    Task<EmailSendResult> SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
    Task<List<EmailSendResult>> SendBulkEmailAsync(IEnumerable<EmailRecipient> recipients, string subject, Func<EmailRecipient, string> htmlBodyGenerator, CancellationToken cancellationToken = default);
}

public class EmailRecipient
{
    public required string Email { get; set; }
    public required string UnsubscribeToken { get; set; }
}

public class EmailSendResult
{
    public bool Success { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

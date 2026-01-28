namespace important_game.infrastructure.Contexts.Newsletter.Email;

public class EmailOptions
{
    public bool Enabled { get; set; } = false;
    public string Provider { get; set; } = "SMTP"; // "SMTP" or "SendGrid"
    public string FromEmail { get; set; } = "newsletter@matchtowatch.net";
    public string FromName { get; set; } = "Match to Watch";
    public string BaseUrl { get; set; } = "https://www.matchtowatch.net";

    // SMTP Settings
    public SmtpSettings Smtp { get; set; } = new();

    // SendGrid Settings
    public SendGridSettings SendGrid { get; set; } = new();

    // Newsletter Schedule
    public int SendDayOfWeek { get; set; } = 0; // 0 = Sunday
    public int SendHourUtc { get; set; } = 22; // 22:00 UTC

    // Batch Settings
    public int BatchSize { get; set; } = 50;
    public int DelayBetweenBatchesMs { get; set; } = 1000;
}

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = true;
}

public class SendGridSettings
{
    public string ApiKey { get; set; } = string.Empty;
}

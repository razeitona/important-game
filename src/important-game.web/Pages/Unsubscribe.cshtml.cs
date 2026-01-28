using important_game.infrastructure.Contexts.Newsletter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages
{
    public class UnsubscribeModel : PageModel
    {
        private readonly INewsletterService _newsletterService;
        private readonly ILogger<UnsubscribeModel> _logger;

        public UnsubscribeModel(INewsletterService newsletterService, ILogger<UnsubscribeModel> logger)
        {
            _newsletterService = newsletterService ?? throw new ArgumentNullException(nameof(newsletterService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [BindProperty(SupportsGet = true)]
        public string? Token { get; set; }

        public bool IsSuccess { get; set; }
        public bool IsAlreadyUnsubscribed { get; set; }
        public bool IsInvalidToken { get; set; }
        public bool ShowConfirmation { get; set; }

        public void OnGet()
        {
            // If no token provided, show error
            if (string.IsNullOrWhiteSpace(Token))
            {
                IsInvalidToken = true;
                return;
            }

            // Show confirmation form
            ShowConfirmation = true;
        }

        public async Task<IActionResult> OnPostAsync([FromForm] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                IsInvalidToken = true;
                return Page();
            }

            Token = token;

            try
            {
                var result = await _newsletterService.UnsubscribeAsync(token);

                if (result)
                {
                    IsSuccess = true;
                    _logger.LogInformation("User successfully unsubscribed using token: {Token}", token[..Math.Min(8, token.Length)] + "...");
                }
                else
                {
                    // Could be invalid token or already unsubscribed
                    IsAlreadyUnsubscribed = true;
                    _logger.LogInformation("Unsubscribe attempt with invalid or already-used token");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing unsubscribe request");
                IsInvalidToken = true;
            }

            return Page();
        }
    }
}

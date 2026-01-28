using important_game.infrastructure.Contexts.Newsletter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace important_game.web.Pages.Api
{
    [IgnoreAntiforgeryToken]
    public class NewsletterSubscribeModel(INewsletterService newsletterService) : PageModel
    {
        private readonly INewsletterService _newsletterService = newsletterService;

        public async Task<IActionResult> OnPostAsync([FromBody] SubscribeRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { success = false, message = "Email is required." });
                }

                var result = await _newsletterService.SubscribeAsync(request.Email, cancellationToken);

                return new JsonResult(new
                {
                    success = result.Success,
                    message = result.Message,
                    alreadySubscribed = result.AlreadySubscribed
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in NewsletterSubscribe POST: {ex}");
                return StatusCode(500, new { success = false, message = "An error occurred. Please try again later." });
            }
        }

        public class SubscribeRequest
        {
            public string Email { get; set; } = string.Empty;
        }
    }
}

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

public class TermsAndConditionsModel(ILogger<TermsAndConditionsModel> _logger) : PageModel
{
    public async Task OnGet()
    {
        // Simple page, no data loading required
        await Task.CompletedTask;
    }
}

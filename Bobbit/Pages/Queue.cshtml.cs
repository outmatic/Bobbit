using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bobbit.Services;
using Bobbit.Services.Model;

namespace Bobbit.Pages
{
    public class QueueModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Vhost { get; set; }
        [BindProperty(SupportsGet = true)]
        public string Queue { get; set; }
        public List<Message> Messages { get; set; }
        [BindProperty]
        public List<ulong> Delete { get; set; }

        private readonly IRabbitAdminService _rabbitAdminService;

        public QueueModel(IRabbitAdminService rabbitAdminService)
            => _rabbitAdminService = rabbitAdminService;

        public IActionResult OnGet()
        {
            if (!HttpContext.Session.TryGetValue("ConnectOptions", out var connectOptions))
                return Redirect("/login");

            Messages = _rabbitAdminService.FetchMessages(JsonSerializer.Deserialize<ConnectOptions>(connectOptions), Vhost, Queue);

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!HttpContext.Session.TryGetValue("ConnectOptions", out var connectOptions))
                return Redirect("/login");

            Messages = _rabbitAdminService.FetchMessages(JsonSerializer.Deserialize<ConnectOptions>(connectOptions), Vhost, Queue, new HashSet<ulong>(Delete));

            return Page();
        }
    }
}

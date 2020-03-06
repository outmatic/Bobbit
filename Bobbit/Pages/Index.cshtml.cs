﻿using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Bobbit.Services;
using Bobbit.Services.Model;

namespace Bobbit.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IRabbitAdminService _rabbitAdminService;

        [BindProperty]
        public ConnectOptions ConnectOptions { get; set; }

        public List<VirtualHost> VirtualHosts { get; set; }
        public List<Queue> Queues { get; set; }

        public IndexModel(
            ILogger<IndexModel> logger,
            IRabbitAdminService rabbitAdminService)
        {
            _logger = logger;
            _rabbitAdminService = rabbitAdminService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!HttpContext.Session.TryGetValue("ConnectOptions", out var connectOptions))
            {
                return Redirect("/login");
            }

            Queues = await _rabbitAdminService.GetQueues(JsonSerializer.Deserialize<ConnectOptions>(connectOptions));

            return Page();
        }


        public IActionResult OnGetSignout()
        {
            HttpContext.Session.Clear();

            return Redirect("/login");
        }
    }
}

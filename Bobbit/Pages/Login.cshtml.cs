using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bobbit.Services;
using Bobbit.Services.Model;

namespace Bobbit.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public ConnectOptions ConnectOptions { get; set; }

        private readonly IRabbitAdminService _rabbitAdminService;

        public LoginModel(IRabbitAdminService rabbitAdminService)
            => _rabbitAdminService = rabbitAdminService;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var canConnect = await _rabbitAdminService.CanConnectAsync(ConnectOptions);

            if (canConnect)
            {                
                HttpContext.Session.SetString("ConnectOptions", JsonSerializer.Serialize(ConnectOptions));

                return RedirectToPage("/index");
            }

            return Page();
        }
    }
}

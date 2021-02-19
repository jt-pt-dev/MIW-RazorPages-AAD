using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebClient.Pages
{
	[AllowAnonymous]
	public class IndexModel : PageModel
	{
		public IndexModel() { }

		public void OnGet()
		{

		}
	}
}

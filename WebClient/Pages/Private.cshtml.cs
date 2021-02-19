using ApiServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace WebClient.Pages
{

	public class PrivateModel : PageModel
	{
		private readonly IApi01Service api01Service;
		private readonly IApi02Service api02Service;

		public PrivateModel(IApi01Service api01Service, IApi02Service api02Service)
		{
			this.api01Service = api01Service;
			this.api02Service = api02Service;
		}

		public string ApiResponse { get; set; }

		public void OnGet()
		{

		}

		public async Task<ActionResult> OnGetApi01PublicAsync()
		{
			ApiResponse = await api01Service.CallPublicEndPoint();

			return Page();
		}


		public async Task<ActionResult> OnGetApi01ProtectedAsync()
		{
			var response = await api01Service.CallProtectedEndPoint();

			ApiResponse = response.Value;

			return Page();
		}

		public async Task<ActionResult> OnGetMeAsync()
		{
			var response = await api01Service.GetMe();

			ApiResponse = response.Value;

			return Page();
		}

		public async Task<ActionResult> OnGetApi02PublicAsync()
		{
			ApiResponse = await api02Service.CallPublicEndPoint();

			return Page();
		}

		public async Task<ActionResult> OnGetApi02ProtectedAsync()
		{
			var response = await api02Service.CallProtectedEndPoint();

			ApiResponse = response.Value;

			return Page();
		}
	}
}

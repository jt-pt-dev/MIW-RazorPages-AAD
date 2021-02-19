using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Api02.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class TestController : ControllerBase
	{
		[HttpGet("GetPublic")]
		public string GetPublic()
		{
			return $"Response from PUBLIC method of Api02 test controller at {DateTime.UtcNow} (UTC)";
		}

		[Authorize]
		[HttpGet("GetProtected")]
		public ActionResult<string> GetProtected()
		{
			return Ok($"Response from PRIVATE method of Api02 test controller at {DateTime.UtcNow} (UTC)");
		}
	}
}

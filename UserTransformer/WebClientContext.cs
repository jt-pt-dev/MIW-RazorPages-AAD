using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityService
{
	public class WebClientContext : IdentityDbContext<WebClientUser>
	{
		public WebClientContext(DbContextOptions<WebClientContext> options)
			: base(options)
		{
		}
	}
}

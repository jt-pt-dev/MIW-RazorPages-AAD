using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static IdentityService.IdentityEnums;

namespace IdentityService
{
	public static class PrincipalTransformer
	{
		public static async Task Transform(TokenValidatedContext context)
		{
			//If we can't find an objectId or email in the claims bail out
			//as we can't create a WebClientUser (AspNetUser) without them.

			var userId = context.Principal.FindFirstValue(ClaimConstants.ObjectId);

			if (string.IsNullOrWhiteSpace(userId))
			{
				return;
			}

			var email = context.Principal.FindFirstValue(ClaimTypes.Email);

			if (string.IsNullOrWhiteSpace(email))
			{
				return;
			}

			var dbContext = context.HttpContext.RequestServices.GetRequiredService<WebClientContext>();

			var user = await dbContext.Users
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Id == userId);

			if (user == null)
			{
				user = new WebClientUser
				{
					Id = userId,
					FirstName = context.Principal.FindFirstValue(ClaimTypes.GivenName),
					Surname = context.Principal.FindFirstValue(ClaimTypes.Surname),
					Email = email,
					UserName = email,
					CompanyId = "Unassigned"
				};

				dbContext.Users.Add(user);
				await dbContext.SaveChangesAsync();
			}

			var claims = new List<Claim>();

			//Some custom claims for our future application

			if (user.IsSuperAdmin)
			{
				claims.Add(new Claim("Is_SuperAdmin", true.ToString().ToLower()));
			}

			//If a user doesn't belong to a company they are an employee of the
			//system owner so they have access to all apis

			if (user.CompanyId == null)
			{
				foreach (var item in Enum.GetNames(typeof(Apis)))
				{
					claims.Add(new Claim($"Can_Access_{item}", true.ToString().ToLower()));
				}
			}
			else
			{
				//Get claims based on the employee's company access
			}

			if (claims.Any())
			{
				var appIdentity = new ClaimsIdentity(claims);

				context.Principal.AddIdentity(appIdentity);
			}
		}
	}
}

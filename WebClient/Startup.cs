using ApiServices;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using IdentityService;

namespace WebClient
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<WebClientContext>(options =>
				options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

			services.AddDistributedSqlServerCache(options =>
			{
				options.ConnectionString = Configuration.GetConnectionString("DefaultConnection");
				options.SchemaName = "dbo";
				options.TableName = "TokenCache";
			});

			services.Configure<CookiePolicyOptions>(options =>
			{
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
				// Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
				options.HandleSameSiteCookieCompatibility();
			});

			services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
				.AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
				.EnableTokenAcquisitionToCallDownstreamApi()
				.AddDistributedTokenCaches();

			services.Configure<MicrosoftIdentityOptions>(options =>
			{
				var existingOnTokenValidatedHandler = options.Events.OnTokenValidated;

				options.Events.OnTokenValidated = async context =>
				{
					await existingOnTokenValidatedHandler(context);

					//Allows us to add extra claims specific to our application

					await PrincipalTransformer.Transform(context);
				};
			});

			services.AddHttpClient<IApi01Service, Api01Service>();
			services.AddHttpClient<IApi02Service, Api02Service>();

			//Allows us to use MicrosoftIdentityConsentAndConditionalAccessHandler in the Api services.
			//This in turn means we don't need AuthorizeForScope on each page and can trigger
			//a challenge in the service if the token can't be found or has expired.
			services.TryAddScoped<MicrosoftIdentityConsentAndConditionalAccessHandler>();

			services.AddControllersWithViews(options =>
			{
				var policy = new AuthorizationPolicyBuilder()
					.RequireAuthenticatedUser()
					.Build();
				options.Filters.Add(new AuthorizeFilter(policy));
			}).AddMicrosoftIdentityUI();

			services.AddRazorPages();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");

				endpoints.MapRazorPages();
			});
		}
	}
}

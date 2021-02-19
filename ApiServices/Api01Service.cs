using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ApiServices
{
	public interface IApi01Service
	{
		Task<string> CallPublicEndPoint();
		Task<ActionResult<string>> CallProtectedEndPoint();
		Task<ActionResult<string>> GetMe();
	}

	public class Api01Service : IApi01Service
	{
		private readonly HttpClient httpClient;
		private readonly ITokenAcquisition tokenAcquisition;
		private readonly MicrosoftIdentityConsentAndConditionalAccessHandler handler;
		private readonly string[] api1Scopes;

		public Api01Service(IConfiguration configuration, HttpClient httpClient,
			ITokenAcquisition tokenAcquisition,
			MicrosoftIdentityConsentAndConditionalAccessHandler handler)
		{
			this.httpClient = httpClient;
			this.tokenAcquisition = tokenAcquisition;
			this.handler = handler;

			httpClient.BaseAddress = new Uri(configuration["AzureAd:Api01BaseAddress"]);
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			api1Scopes = new[] { configuration["AzureAd:Api01Scope"] };
		}

		private async Task<string> GetAccessToken(string[] scopes)
		{
			try
			{
				return await tokenAcquisition.GetAccessTokenForUserAsync(scopes);
			}
			catch (Exception ex)
			{
				//Triggers a challenge if the token is not found or has expired.
				handler.HandleException(ex);
			}

			return string.Empty;
		}

		public async Task<ActionResult<string>> CallProtectedEndPoint()
		{
			var accessToken = await GetAccessToken(api1Scopes);

			if (string.IsNullOrWhiteSpace(accessToken))
			{
				return "Auth error... access token empty";
			}

			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			var response = await httpClient.GetAsync("Test/GetProtected");

			if (response.IsSuccessStatusCode)
			{
				return await response.Content.ReadAsStringAsync();
			}
			else
			{
				return response.StatusCode.ToString();
			}
		}

		//Just testing out Microsoft Graph
		public async Task<ActionResult<string>> GetMe()
		{
			httpClient.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/me");

			var accessToken = await GetAccessToken(new string[] {
				"https://graph.microsoft.com/User.Read"
			});

			if (string.IsNullOrWhiteSpace(accessToken))
			{
				return "Auth error... access token empty";
			}

			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

			var response = await httpClient.GetAsync("");

			if (response.IsSuccessStatusCode)
			{
				return await response.Content.ReadAsStringAsync();
			}
			else
			{
				return response.StatusCode.ToString();
			}
		}

		public async Task<string> CallPublicEndPoint()
		{
			return await httpClient.GetStringAsync("Test/GetPublic");
		}
	}
}

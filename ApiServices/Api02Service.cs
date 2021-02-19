using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ApiServices
{
	public interface IApi02Service
	{
		Task<string> CallPublicEndPoint();
		Task<ActionResult<string>> CallProtectedEndPoint();
	}

	public class Api02Service : IApi02Service
	{
		private readonly HttpClient httpClient;
		private readonly ITokenAcquisition tokenAcquisition;
		private readonly MicrosoftIdentityConsentAndConditionalAccessHandler handler;
		private readonly string[] api2Scopes;

		public Api02Service(IConfiguration configuration, HttpClient httpClient,
			ITokenAcquisition tokenAcquisition,
			MicrosoftIdentityConsentAndConditionalAccessHandler handler)
		{
			this.httpClient = httpClient;
			this.tokenAcquisition = tokenAcquisition;
			this.handler = handler;

			httpClient.BaseAddress = new Uri(configuration["AzureAd:Api02BaseAddress"]);
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			api2Scopes = new[] { configuration["AzureAd:Api02Scope"] };
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
			var accessToken = await GetAccessToken(api2Scopes);

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

		public async Task<string> CallPublicEndPoint()
		{
			return await httpClient.GetStringAsync("Test/GetPublic");
		}
	}
}

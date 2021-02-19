using Microsoft.AspNetCore.Identity;

namespace IdentityService
{
	public class WebClientUser : IdentityUser
	{
		//Database structure is the classic AspNet Identity collection of tables.
		//These are the extra fields added to the AspNetUsers table for our application.

		public string FirstName { get; set; } = null;
		public string Surname { get; set; } = null;
		public bool IsSuperAdmin { get; set; } = false;
		public bool IsArchived { get; set; } = false;
		public string CompanyId { get; set; } = null;
		public int PreferenceUtcOffset { get; set; } = 600;
		public string PreferenceLanguage { get; set; } = "en-AU";
	}
}

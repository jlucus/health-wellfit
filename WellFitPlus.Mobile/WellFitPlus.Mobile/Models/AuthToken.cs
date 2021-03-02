using System;
using Newtonsoft.Json;

namespace WellFitPlus.Mobile.Models
{
    public class AuthToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("userName")]
        public string Username { get; set; }

		[JsonProperty("registrationDate")]
		public string RegistrationDate { get; set; }

        [JsonProperty(".issued")]
        public string IssuedAt { get; set; }

        [JsonProperty(".expires")]
        public string ExpiresAt { get; set; }

        [JsonProperty("guid")]
        public string UserID { get; set; }

        [JsonProperty("companyName")]
        public string CompanyName { get; set; }
    }
}

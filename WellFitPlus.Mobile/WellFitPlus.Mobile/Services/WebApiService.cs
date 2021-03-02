using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Threading.Tasks;
using WellFitPlus.Mobile.Models;
using WellFitPlus.Mobile.Helpers;
using System.Text;
using System.IO;

namespace WellFitPlus.Mobile.Services
{
    public class WebApiService
    {
        #region Attributes
        private string _webApiAddress;
        private Configuration _config;
        #endregion

        #region Constructor
        public WebApiService(Configuration config)
        {
            _config = config;
            _webApiAddress = string.Format("{0}://{1}/{2}/{3}/",
                _config.IsSecure ? "https" : "http",
                Configuration.SERVER,
                Configuration.RESOURCE_URI,
                Configuration.API_PATH);

            Console.WriteLine(string.Format("WebAPI Path: {0}", _webApiAddress));
        }
        #endregion

        #region Helpers
        private HttpClient BuildHttpClient(string baseUri, bool authorized)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(baseUri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // NOTE:    currently, this class does not support client grants (since the WebAPI doesn't utilize those).
            //          authorization is thus considered either "authorized," or "anonymous"
            if (authorized)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SessionService.Instance.AuthToken);
            }

            return client;
        }
        #endregion

        #region Methods
        public async Task<AuthToken> Login(string username, string password)
        {
            var client = BuildHttpClient(
                string.Format("{0}://{1}", _config.IsSecure ? "https" : " http", Configuration.SERVER),
                false);

            var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("grant_type", "password"),
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password)
                });

            // NOTE:    response contains headers, status, and the embedded auth token (print out to see)
            var authPath = string.Format("/{0}/{1}", Configuration.RESOURCE_URI, Configuration.AUTH_PATH);
            var response = await client.PostAsync(authPath, content).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<AuthToken>(token);
            }

            throw new SimpleHttpResponseException(response.StatusCode, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        public async Task<HttpStatusCode> Logout()
        {
            var client = BuildHttpClient(_webApiAddress, true);

            var response = await client.PostAsync("account/logout", null).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                client.Dispose();
                response.Dispose();
                return HttpStatusCode.OK;
            }

            throw new SimpleHttpResponseException(response.StatusCode, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        // wrapping this method for now since it's an anonymous request... dunno, might be overkill. delete as desired.
        public async Task<HttpStatusCode> Register(UserRegistration model)
        {
            return await Post("Account/Register", model, false);
        }

        public async Task<T> Get<T>(string path, string @params = "")
        {
            var client = BuildHttpClient(_webApiAddress, true);
            client.Timeout = TimeSpan.FromSeconds(60); // 60 second timeout

            var response = await client.GetAsync(string.Format("{0}/{1}", path, @params)).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var obj = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<T>(obj);
                client.Dispose();
                response.Dispose();
                return result;
            }

            throw new SimpleHttpResponseException(response.StatusCode, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        public async Task<MemoryStream> GetStream(string path, object @params)
        {
            var client = BuildHttpClient(_webApiAddress, true);

            string json = JsonConvert.SerializeObject(@params);

            var response = await client.PostAsync(
                path,
                new StringContent(JsonConvert.SerializeObject(@params), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var obj = (MemoryStream)await response.Content.ReadAsStreamAsync();
                client.Dispose();
                response.Dispose();
                return obj;
            }

            throw new SimpleHttpResponseException(response.StatusCode, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        public async Task<HttpStatusCode> Post<T>(string path, T @params, bool authorized = true)
        {
            var client = BuildHttpClient(_webApiAddress, authorized);

            var response = await client.PostAsync(
                path,
                new StringContent(JsonConvert.SerializeObject(@params), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {

                client.Dispose();
                response.Dispose();
                return HttpStatusCode.OK;
            }

            throw new SimpleHttpResponseException(response.StatusCode, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        public async Task<T> PostRetrieve<T>(string path, T @params)
        {
            var client = BuildHttpClient(_webApiAddress, true);

            using (var response = await client.PostAsync(
                path,
                new StringContent(JsonConvert.SerializeObject(@params), Encoding.UTF8, "application/json")))
            {

                if (response.IsSuccessStatusCode)
                {
                    var obj = await response.Content.ReadAsStringAsync(); //.ConfigureAwait(false);
                    var result = JsonConvert.DeserializeObject<T>(obj);
                    client.Dispose();
                    response.Dispose();
                    return result;
                }

                throw new SimpleHttpResponseException(response.StatusCode,
                                                      await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
        }

        public async Task<U> PostRetrieve<T, U>(string path, T @params)
        {
            var client = BuildHttpClient(_webApiAddress, true);
            client.Timeout = TimeSpan.FromMinutes(30);
            using (var response = await client.PostAsync(
                path,
                new StringContent(JsonConvert.SerializeObject(@params), Encoding.UTF8, "application/json")))
            {
                if (response.IsSuccessStatusCode)
                {
                    var obj = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<U>(obj);
                    client.Dispose();
                    response.Dispose();
                    return result;
                }

                throw new SimpleHttpResponseException(response.StatusCode, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
        }

        #endregion
    }
}

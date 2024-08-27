using BaseLibrary.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLibrary.Helpers
{
	public class GetHttpClient(IHttpClientFactory httpClientFactory, LocalStorageService localStorageService)
	{
		private const string HeaderKey = "Authorization";

		//This method be used to create a HttpClient with Header is Authorization contain user's token
		public async Task<HttpClient> GetPrivateHttpClient()
		{
			//Create HttpClient with name is SystemApiClient
			var client = httpClientFactory.CreateClient("SystemApiClient");
			//Get token from local storage 
			var stringToken = await localStorageService.GetToken();
			//Check if token is null or empty, return client no need HeaderKey
			if (string.IsNullOrEmpty(stringToken)) return client ;

			//If token not null, Deserialize token string to C# object.
			var deserializeToken = Serializations.DeserializeJsonString<UserSession>(stringToken);
			//If deserialize not success, return httpclient no need HeaderKey.
			if(deserializeToken == null) return client ;

			//If deserialize success, add headerkey to httpclient with value is Bearer
			client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", deserializeToken.Token);
			return client ;
		}

		//This method will be return  a HttpClient that no need header 'Authorization'
		public HttpClient GetPublicHttpClient()
		{
			//Create httpclient with name is SystemApiClient
			var client = httpClientFactory.CreateClient("SystemApiClient");
			client.DefaultRequestHeaders.Remove(HeaderKey);
			return client ;
		}
	}
}

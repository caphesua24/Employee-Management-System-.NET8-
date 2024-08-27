using BaseLibrary.DTOs;
using ClientLibrary.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ClientLibrary.Helpers
{
    //Manage HTTP requests
    public class CustomHttpHandler(GetHttpClient getHttpClient, LocalStorageService localStorageService, IUserAccountService userAccountService) : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //If the request URL contains the strings "login", "register", or "refresh-token",
            //the request will be processed as usual and no token refresh is required.
            bool loginUrl = request.RequestUri!.AbsoluteUri.Contains("login");
            bool registerUrl = request.RequestUri!.AbsoluteUri.Contains("register");
            bool refreshTokenUrl = request.RequestUri!.AbsoluteUri.Contains("refresh-token");

            if(loginUrl || registerUrl || refreshTokenUrl) return await base.SendAsync(request, cancellationToken);

            //Make HTTP requests and receive responses
            var result = await base.SendAsync(request, cancellationToken);
            //If the response has a status code of Unauthorized (401),
            //the system will attempt to refresh the token.
            if (result.StatusCode == HttpStatusCode.Unauthorized)
            {
                //Get token from localStorage
                var stringToken = await localStorageService.GetToken();
                //If there is no token, the system returns an Unauthorized response.
                if (stringToken == null) return result;

                //check if the header containers token
                string token = string.Empty;
                try { token = request.Headers.Authorization!.Parameter!; }
                catch { }

                //convert token from string to UserSession object
                var deserializedToken = Serializations.DeserializeJsonString<UserSession>(token);
                if (deserializedToken is null) return result;

                if(string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", deserializedToken.Token);
                    return await base.SendAsync(request, cancellationToken);
                }

                //If the token is invalid, the system will call the GetRefreshToken function to refresh the token by sending the refreshToken to the server.
                var newJwtToken = await GetRefreshToken(deserializedToken.RefreshToken!);
                if (string.IsNullOrEmpty(newJwtToken)) return result;

                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newJwtToken);
                return await base.SendAsync(request, cancellationToken);
            }
            return result;
        }

        //This method is used to call userAccountService to refresh the token.
        private async Task<string> GetRefreshToken(string refreshToken)
        {
            var result = await userAccountService.RefreshTokenAsync(new RefreshToken() { Token = refreshToken });
            string serializedToken = Serializations.SerializeObj(new UserSession() { Token = result.Token, RefreshToken = result.RefreshToken });
            await localStorageService.SetToken(serializedToken);
            return result.Token;
        }
    }
}

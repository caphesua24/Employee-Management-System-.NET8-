using Blazored.LocalStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLibrary.Helpers
{
	// Interface ILocalStorageService
	public class LocalStorageService(ILocalStorageService localStorageService)
	{
		//StorageKey: This is a constant (const) used to store the name key in local storage. This key will be used to save, retrieve or delete the token.
		private const string StorageKey = "authentication-token";
		//GetToken method: this method return token value from local storage with key 'StorageKey'
		public async Task<string?> GetToken() => await localStorageService.GetItemAsStringAsync(StorageKey);
		//SetToken method: this method will be used to store a token string in local storage with key 'StorageKey'
		public async Task SetToken(string item) => await localStorageService.SetItemAsStringAsync(StorageKey, item);
		//RemoveToken method: this method will be used to delete token in local storage
		public async Task RemoveToken() => await localStorageService.RemoveItemAsync(StorageKey);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClientLibrary.Helpers
{
	//This class will be used to convert between C# object JSON strings, also support converting JSON strings to list C# object.
	public static class Serializations
	{
		//convert c# object to json string
		public static string SerializeObj<T>(T modelObject) => JsonSerializer.Serialize(modelObject);
		//Convert json string to C# object
		public static T DeserializeJsonString<T>(string jsonString) => JsonSerializer.Deserialize<T>(jsonString);
		public static IList<T> DeserializeJsonStringList<T>(string jsonString) => JsonSerializer.Deserialize<IList<T>>(jsonString);
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary.DTOs
{
	public class AccountBase
	{
		[DataType(DataType.EmailAddress)] // this attribute specifies that the data should be treated as an email address
		[EmailAddress] // Validation attribute: checks if the string is in the correct email address format.
		[Required] // string not null or empty
		public string? Email {  get; set; }

		[DataType(DataType.Password)] // this attribute specifies that the data should be treated as an password
		[Required] // string not null or empty
		public string? Password { get; set; }
	}
}

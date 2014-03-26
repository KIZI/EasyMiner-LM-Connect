using System;
using System.Runtime.Serialization;

namespace LMConnect.WebApi.API.Users
{
	public class UserChangeRequest
	{
		[DataMember(Name = "email_link")]
		public string Link { get; set; }

		[DataMember(Name = "new_email")]
		public string NewEmail { get; set; }

		[DataMember(Name = "new_username")]
		public string NewUsername { get; set; }

		[DataMember(Name = "new_password")]
		public string NewPassword { get; set; }

		[DataMember(Name = "email_from")]
		public string EmailFrom { get; set; }

		public LMConnect.Key.UserPendingUpdate GetPendingUpdate(LMConnect.Key.User user)
		{
			return new LMConnect.Key.UserPendingUpdate
			{
				Link = this.Link,
				NewEmail = this.NewEmail,
				NewPassword = this.NewPassword,
				NewUsername = this.NewUsername,
				User = user,
				RequestedTime = DateTime.Now
			};
		}
	}
}
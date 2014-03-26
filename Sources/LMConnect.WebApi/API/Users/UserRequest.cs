using System.Runtime.Serialization;

namespace LMConnect.WebApi.API.Users
{
	/// <summary>
	/// POST name=username1&password=pwd1&db_id=database1&db_password=uknown
	/// TODO: UserRequest as XML
	/// </summary>
	[DataContract]
	public class UserRequest
	{
		[DataMember(Name = "name")]
		public string name { get; set; }
		
		[DataMember(Name = "password")]
		public string Password { get; set; }

		[DataMember(Name = "email")]
		protected string Email { get; set; }

		[DataMember(Name = "new_name")]
		public string new_name { get; set; }

		[DataMember(Name = "new_password")]
		public string new_password { get; set; }

		[DataMember(Name = "db_id")]
		public string db_id { get; set; }

		[DataMember(Name = "db_password")]
		public string db_password { get; set; }

		public LMConnect.Key.Database GetDatabase(LMConnect.Key.User owner)
		{
			if (this.db_id == null)
			{
				return null;
			}

			return new LMConnect.Key.Database
				{
					Name = this.db_id,
					Password = this.db_password,
					Owner = owner
				};
		}

		public LMConnect.Key.User GetUser()
		{
			return new LMConnect.Key.User
				{
					Username = this.name,
					Password = this.Password,
					Email = this.Email
				};
		}
	}
}
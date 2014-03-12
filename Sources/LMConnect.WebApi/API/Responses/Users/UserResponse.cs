using System.Linq;
using System.Xml.Linq;

namespace LMConnect.WebApi.API.Responses.Users
{
	public class UserResponse : Response
	{
		private LMConnect.Key.User User { get; set; }

		public UserResponse(LMConnect.Key.User user)
		{
			User = user;
		}

		protected override XElement GetBody()
		{
			return FromUser(User);
		}

		internal static XElement FromUser(LMConnect.Key.User user)
		{
			return new XElement("user",
								new XElement("username", user.Username),
								new XElement("email", user.Email),
								new XElement("databases", user.Databases.Select(FromDatabase)),
								new XElement("miners", user.Miners.Select(FromMiner))
				);
		}

		private static object FromMiner(LMConnect.Key.Miner miner)
		{
			return new XElement("miner",
				new XAttribute("id", miner.MinerId));
		}

		private static object FromDatabase(LMConnect.Key.Database db)
		{
			return new XElement("database",
								new XAttribute("id", db.Name));
		}
	}
}
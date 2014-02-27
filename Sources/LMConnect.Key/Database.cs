using System;

namespace LMConnect.Key
{
	public class Database
	{
		public virtual Guid Id { get; set; }

		public virtual string Name { get; set; }

		public virtual string Password { get; set; }

		public virtual User Owner { get; set; }
	}
}

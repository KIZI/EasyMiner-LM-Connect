using System.Linq;
using System.Net;
using System.Web.Http;
using LMConnect.WebApi.API;
using LMConnect.WebApi.API.Users;

namespace LMConnect.WebApi.Controllers
{
	[Authorize]
	public class UsersController : ApiBaseController
	{
		/// <summary>
		/// Lists all user or specific user.
		/// </summary>
		/// <param name="username">User to describe.</param>
		/// <returns>List of user or specific user description.</returns>
		[Filters.NHibernateTransaction]
		public Response Get(string username)
		{
			if (string.IsNullOrEmpty(username) && this.User.IsInRole("admin"))
			{
				var users = this.Repository.FindAll<LMConnect.Key.User>();

				return new UsersResponse(users);
			}
			else if (this.User.Identity.Name == username || this.User.IsInRole("admin"))
			{
				var user = this.Repository.Query<LMConnect.Key.User>()
				               .FirstOrDefault(u => u.Username == username);

				if (user != null)
				{
					return new UserResponse(user);
				}

				return this.ThrowHttpReponseException<Response>(HttpStatusCode.NotFound);
			}

			return this.ThrowHttpReponseException<Response>(HttpStatusCode.Unauthorized);
		}

		/// <summary>
		/// Register user or database for existing user.
		/// </summary>
		/// <returns>Registered UserResponse.</returns>
		[Filters.NHibernateTransaction]
		[AllowAnonymous]
		public UserResponse Post(UserRequest request)
		{
			var user = this.Repository.Query<LMConnect.Key.User>()
				.FirstOrDefault(u => u.Username == request.name && u.Password == request.Password);

			if (user == null)
			{
				user = request.GetUser();

				this.Repository.Add(user);
			}

			var database = request.GetDatabase(user);

			if (database != null)
			{
				user.Databases.Add(database);
			}

			this.Repository.Save(user);

			return new UserResponse(user);
		}

		/// <summary>
		/// Update user or change user's password.
		/// </summary>
		/// <returns>UserResponse.</returns>
		[Filters.NHibernateTransaction]
		public UserResponse Put(UserRequest request)
		{
			var user = this.GetLMConnectUser();

			if (user.Username == request.name)
			{
				// updating himself
				if (!string.IsNullOrEmpty(request.new_name))
				{
					user.Username = request.new_name;
				}

				if (!string.IsNullOrEmpty(request.new_password))
				{
					user.Password = request.new_password;
				}

				this.Repository.Save(user);

				return new UserResponse(user);
			}
			else if (this.User.IsInRole("admin"))
			{
				// updating by admin
				LMConnect.Key.User modified = this.Repository.Query<LMConnect.Key.User>()
									.FirstOrDefault(u => u.Username == request.name);

				return this.ThrowHttpReponseException<UserResponse>(
					"This feature is not yet implemented",
					HttpStatusCode.NotImplemented);
			}

			return this.ThrowHttpReponseException<UserResponse>(
				string.Format("User \"{0}\" not found or you are not auhtorized to modify him.", request.name),
				HttpStatusCode.NotFound);
		}

		/// <summary>
		/// Deletes existing user.
		/// </summary>
		/// <param name="username">Username of user to delete.</param>
		/// <returns>Response with message.</returns>
		[Filters.NHibernateTransaction]
		public Response Delete(string username)
		{
			if (this.User.Identity.Name == username || this.User.IsInRole("admin"))
			{
				LMConnect.Key.User user = this.Repository.Query<LMConnect.Key.User>()
				                           .FirstOrDefault(u => u.Username == username);

				if (user != null)
				{
					this.Repository.Remove(user);

					return new Response(string.Format("User \"{0}\" removed.", user.Username));
				}

				return this.ThrowHttpReponseException(
					string.Format("User \"{0}\" not found.", username),
					HttpStatusCode.NotFound);
			}

			return this.ThrowHttpReponseException(
				string.Format("User \"{0}\" not found or you are not auhtorized to delete him.", username),
				HttpStatusCode.NotFound);
		}
	}
}

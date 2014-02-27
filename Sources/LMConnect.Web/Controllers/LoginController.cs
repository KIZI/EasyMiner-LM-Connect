using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using LMConnect.Key.Repositories;
using LMConnect.Web.Models;

namespace LMConnect.Web.Controllers
{
	[AllowAnonymous]
	public class LoginController : Controller
	{
		private IRepository _repository;

		protected virtual IRepository Repository
		{
			get { return _repository ?? (_repository = new NHibernateRepository(MvcApplication.SessionFactory.GetCurrentSession())); }
		}

		[HttpGet]
		public ActionResult Index(string returnUrl = null)
		{
			this.ViewBag.ReturnUrl = returnUrl;
			return View();
		}

		[HttpPost]
		[Filters.Mvc.NHibernateTransaction]
		public ActionResult Index(LogInViewModel model, string returnUrl)
		{
			if (this.ModelState.IsValid)
			{
				LMConnect.Key.User user = this.Repository.Query<LMConnect.Key.User>()
					.FirstOrDefault(u => u.Username == model.UserName && u.Password == model.Password);

				if (user != null && user.IsAdmin)
				{
					FormsAuthentication.SetAuthCookie(model.UserName, false);

					return this.Redirect(returnUrl);	
				}
			}

			this.ViewBag.ReturnUrl = returnUrl;
			this.ModelState.AddModelError("", "The user name or password provided is incorrect.");

			return this.View(model);
		}

		public ActionResult Logout()
		{
			FormsAuthentication.SignOut();
			return this.Redirect("~/");
		}
	}
}

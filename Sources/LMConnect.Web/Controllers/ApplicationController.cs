using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using LMConnect.Web.API;
using LMConnect.Web.Models;
using LMConnect.WebApi.API;

namespace LMConnect.Web.Controllers
{
	public class ApplicationController : Controller
	{
		private LMConnect.Key.Repositories.IRepository _repository;

		protected virtual LMConnect.Key.Repositories.IRepository Repository
		{
			get { return _repository ?? (_repository = new LMConnect.Key.Repositories.NHibernateRepository(MvcApplication.SessionFactory.GetCurrentSession())); }
		}

		protected const string PARAMS_GUID = "guid";

		private LISpMiner.LISpMiner _miner;

		protected LISpMiner.LISpMiner LISpMiner
		{
			get
			{
				if (this._miner == null)
				{
					if (this.HttpContext == null)
					{
						// we need a context
						throw new Exception("Ensure to call base.ProcessRequest");
					}

					var guid = this.RouteData.Values[PARAMS_GUID] as string;

					if (guid == null)
					{
						throw new Exception(String.Format("Not specified which LISpMiner to work with."));
					}

					if (!MvcApplication.Environment.Exists(guid))
					{
						throw new Exception(String.Format("Requested LISpMiner with ID {0}, does not exists", guid));
					}

					this._miner = MvcApplication.Environment.GetMiner(guid);
				}

				return this._miner;
			}
		}

		public ActionResult Index()
		{
			return View(SVNDataAttribute.AssemblySVNData);
		}

		public ActionResult Update()
		{
			string path;

			if (this.HttpContext.Request.Params[PARAMS_GUID] != null)
			{
				throw new NotSupportedException();

				// path = Path.GetFullPath(this.LISpMiner.LMPath);
			}
			else
			{
				path = Path.GetFullPath(MvcApplication.Environment.LMPath + "\\..");
			}

			var manager = new LMConnect.Manager(path);

			ViewBag.Output = manager.Update();

			ViewBag.Path = path;

			return View();
		}

		#region Miners

		[Authorize]
		[Filters.NHibernateTransaction]
		public ActionResult Miners()
		{
			var miners = new List<MinerViewModel>();

			foreach (var miner in MvcApplication.Environment.Miners)
			{
				string owner = "???";
				var dbMiner = this.Repository.Query<LMConnect.Key.Miner>()
					.FirstOrDefault(m => m.MinerId == miner.Id);

				if (dbMiner != null)
				{
					owner = dbMiner.Owner.Username;
				}

				miners.Add(new MinerViewModel(miner, owner));
			}

			return View(miners);
		}

		[Authorize]
		[Filters.NHibernateTransaction]
		public ActionResult Miner()
		{
			return View(this.LISpMiner);
		}

		[Authorize]
		[ErrorHandler]
		[Filters.NHibernateTransaction]
		public ActionResult Remove()
		{
			var miner = this.Repository.Query<LMConnect.Key.Miner>()
				.FirstOrDefault(m => m.MinerId == this.LISpMiner.Id);

			MvcApplication.Environment.Unregister(this.LISpMiner);

			if (miner != null)
			{
				miner.Owner.Miners.Remove(miner);

				this.Repository.Remove(miner);
			}

			return new XmlResult
			{
				Data = new Response { Status = Status.Success, Message = "LISpMiner removed." }
			};
		}

		#endregion
	}
}

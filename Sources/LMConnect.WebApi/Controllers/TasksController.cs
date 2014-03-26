using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using LMConnect.LISpMiner;
using LMConnect.WebApi.API;
using LMConnect.WebApi.API.Tasks;

namespace LMConnect.WebApi.Controllers
{
	[Authorize]
	public class TasksController : ApiBaseController
	{
		private Type GetTaskLauncher(string taskType)
		{
			switch (taskType)
			{
				case "proc":
					return typeof(LMProcPooler);
				case "grid":
					return typeof(LMGridPooler);
				case "task":
				default:
					return typeof(LMTaskPooler);
			}
		}

		private void CheckMinerOwnerShip()
		{
			var user = this.GetLMConnectUser();
			var miner = this.Repository.Query<LMConnect.Key.Miner>()
				.FirstOrDefault(m => m.MinerId == this.LISpMiner.Id);

			if ((miner != null && user.Username != miner.Owner.Username) && !this.User.IsInRole("admin"))
			{
				this.ThrowHttpReponseException("Authorized user is not allowed to use this miner.", HttpStatusCode.Forbidden);
			}
		}

		[Filters.NHibernateTransaction]
		public Response Get()
		{
			CheckMinerOwnerShip();

			// TODO: List of all Tasks for given miner
			return new Response
			{
				Message = this.LISpMiner.GetTasks().First()
			};
		}

		[Filters.NHibernateTransaction]
		public Response Get(string taskType, string taskName = null, string template = null, string alias = null)
		{
			CheckMinerOwnerShip();

			// when exporting we dont need to know what was task type
			var name = taskName ?? taskType;

			var definition = new ExportTaskDefinition(name, template, alias);

			var response = new TaskResponse
			{
				OutputFilePath = this.LISpMiner.Export(definition)
			};

			return response;
		}

		[Filters.NHibernateTransaction]
		public TaskResponse Post(TaskRequest request, string template = null, string alias = null, string taskType = "task")
		{
			CheckMinerOwnerShip();

			Type type = this.GetTaskLauncher(taskType);

			var definition = new RunTaskDefinition(request.Task, template, alias);

			return new TaskResponse
			{
				OutputFilePath = this.LISpMiner.Run(type, definition)
			}; 
		}

		[Filters.NHibernateTransaction]
		public Response Put(TaskUpdateRequest request, string taskType = null, string taskName = null)
		{
			CheckMinerOwnerShip();

			var hasTaskType = !string.IsNullOrEmpty(taskType);
			var hasTaskName = !string.IsNullOrEmpty(taskName);

			if (!request.IsCancelation)
			{
				throw new Exception("Unsupported task update request.");
			}

			if (!hasTaskType && !hasTaskName)
			{
				this.LISpMiner.CancelAll();

				return new Response
				{
					Message = "All tasks has been canceled."
				};
			}
			else if (hasTaskType && !hasTaskName)
			{
				Type type = this.GetTaskLauncher(taskType);

				this.LISpMiner.CancelAll(type);

				return new Response
				{
					Message = "All tasks has been canceled."
				};
			}
			else if (hasTaskType && hasTaskName)
			{
				var type = this.GetTaskLauncher(taskType);

				this.LISpMiner.Cancel(type, taskName);

				return new Response
				{
					Message = String.Format("Task {0} has been canceled.", taskName)
				};
			}
			else // if (!hasTaskType && hasTaskName)
			{
				throw new ArgumentException();
			}
		}
	}
}

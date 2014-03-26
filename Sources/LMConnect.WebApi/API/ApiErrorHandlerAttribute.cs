using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using log4net;

namespace LMConnect.WebApi.API
{
	public class ApiErrorHandlerAttribute : ExceptionFilterAttribute
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(ApiErrorHandlerAttribute));

		private HttpStatusCode _statusCode = HttpStatusCode.InternalServerError;

		public HttpStatusCode StatusCode
		{
			get
			{
				return this._statusCode;
			}

			set
			{
				this._statusCode = value;
			}
		}

		public override void OnException(HttpActionExecutedContext filterContext)
		{
			if (filterContext == null)
			{
				throw new ArgumentNullException("filterContext");
			}

			Exception exception = filterContext.Exception;
			var exceptionResponse = new ExceptionResponse(exception);

			Log.Error(exception);
			
			filterContext.Response = filterContext.Request.CreateResponse(this.StatusCode, exceptionResponse);
		}
	}
}

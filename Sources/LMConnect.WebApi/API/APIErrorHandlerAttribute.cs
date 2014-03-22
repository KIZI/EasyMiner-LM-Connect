using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using LMConnect.WebApi.API.Responses;
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
			
			var response = new HttpResponseMessage
				{
					StatusCode = this.StatusCode,
					Content = new StringContent(exceptionResponse.Write())
					// response.ContentType = "application/xml"
				};

			// TODO: test if works, otherwise use reposne above
			filterContext.Response = filterContext.Request.CreateResponse(this.StatusCode, exceptionResponse);
		}
	}
}
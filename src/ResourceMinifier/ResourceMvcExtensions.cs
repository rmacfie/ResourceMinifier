using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ResourceMinifier
{
	/// <summary>
	/// 	Utilities and extensions to use the ResourceMinifier with ASP.NET MVC.
	/// </summary>
	public static class ResourceMvcExtensions
	{
		private static string _urlFormat;
		private static bool _etagInPath = true;

		/// <summary>
		/// 	Configure the route pattern to be used with the Mvc helper extensions. The pattern should be a standard Mvc route pattern
		/// 	leading to the Action method that is set up to return ResourceMinifier packages. The pattern must contain the parameter {key}.
		/// 	The pattern may include the parameter {etag}. If not, the urls will be suffixed with "?etag=NNNNNN".
		/// </summary>
		/// <example>
		/// 	"ContentCache/{etag}/{key}"
		/// </example>
		/// <param name = "routePattern"></param>
		public static void ConfigureRouting(string routePattern)
		{
			if (!routePattern.Contains("{key}"))
			{
				throw new ArgumentException("The route pattern must contain the string '{key}'.", "routePattern");
			}

			_urlFormat = routePattern.Replace("{key}", "{0}");

			if (_urlFormat.Contains("{etag}"))
			{
				_urlFormat = _urlFormat.Replace("{etag}", "{1}");
				_etagInPath = true;
			}
			else
			{
				_etagInPath = false;
			}
		}

		/// <summary>
		/// 	Returns a script tag referencing to the given javascript package. The first time this is run a ResourcePackage will be created and cached,
		/// 	succedent calls will simply return the script tag reference.
		/// </summary>
		public static MvcHtmlString CombinedJavascript(this HtmlHelper htmlHelper, string key, params string[] virtualPaths)
		{
			return CombinedJavascript(htmlHelper, key, true, virtualPaths);
		}

		/// <summary>
		/// 	Returns a script tag referencing to the given javascript package. The first time this is run a ResourcePackage will be created and cached,
		/// 	succedent calls will simply return the script tag reference.
		/// </summary>
		public static MvcHtmlString CombinedJavascript(this HtmlHelper htmlHelper, string key, bool doMinify, params string[] virtualPaths)
		{
			var package = ResourceRegistry.LoadJavascript(key, doMinify, virtualPaths);
			return
				MvcHtmlString.Create("<script type=\"text/javascript\" src=\"" + GetUrl(htmlHelper.ViewContext.RequestContext, package) + "\"></script>");
		}

		/// <summary>
		/// 	Returns the URL to the given javascript package. The first time this is run a ResourcePackage will be created and cached,
		/// 	succedent calls will simply return the URL.
		/// </summary>
		public static string CombinedJavascriptUrl(this UrlHelper urlHelper, string key, params string[] virtualPaths)
		{
			return CombinedJavascriptUrl(urlHelper, key, true, virtualPaths);
		}

		/// <summary>
		/// 	Returns the URL to the given javascript package. The first time this is run a ResourcePackage will be created and cached,
		/// 	succedent calls will simply return the URL.
		/// </summary>
		public static string CombinedJavascriptUrl(this UrlHelper urlHelper, string key, bool doMinify, params string[] virtualPaths)
		{
			var package = ResourceRegistry.LoadJavascript(key, doMinify, virtualPaths);
			return GetUrl(urlHelper, package);
		}

		/// <summary>
		/// 	Returns a link tag referencing to the given stylesheet package. The first time this is run a ResourcePackage will be created and cached,
		/// 	succedent calls will simply return the link tag reference.
		/// </summary>
		public static MvcHtmlString CombinedStylesheet(this HtmlHelper htmlHelper, string key, params string[] virtualPaths)
		{
			return CombinedStylesheet(htmlHelper, key, true, virtualPaths);
		}

		/// <summary>
		/// 	Returns a link tag referencing to the given stylesheet package. The first time this is run a ResourcePackage will be created and cached,
		/// 	succedent calls will simply return the link tag reference.
		/// </summary>
		public static MvcHtmlString CombinedStylesheet(this HtmlHelper htmlHelper, string key, bool doMinify, params string[] virtualPaths)
		{
			var package = ResourceRegistry.LoadStylesheet(key, doMinify, virtualPaths);
			return
				MvcHtmlString.Create(
					"<link rel=\"stylesheet\" type=\"text/css\" href=\"" + GetUrl(htmlHelper.ViewContext.RequestContext, package) + "\" />");
		}

		/// <summary>
		/// 	Returns the URL to the given stylesheet package. The first time this is run a ResourcePackage will be created and cached,
		/// 	succedent calls will simply return the URL.
		/// </summary>
		public static string CombinedStylesheetUrl(this UrlHelper urlHelper, string key, params string[] virtualPaths)
		{
			return CombinedStylesheetUrl(urlHelper, key, true, virtualPaths);
		}

		/// <summary>
		/// 	Returns the URL to the given stylesheet package. The first time this is run a ResourcePackage will be created and cached,
		/// 	succedent calls will simply return the URL.
		/// </summary>
		public static string CombinedStylesheetUrl(this UrlHelper urlHelper, string key, bool doMinify, params string[] virtualPaths)
		{
			var package = ResourceRegistry.LoadStylesheet(key, doMinify, virtualPaths);
			return GetUrl(urlHelper, package);
		}

		/// <summary>
		/// 	Gets a ResourcePackage and returns it in the form of an ActionResult.
		/// </summary>
		public static ActionResult GetActionResult(HttpContextBase httpContext, string key, bool setCacheHeaders = true, bool useCompression = true)
		{
			var package = ResourceRegistry.Get(key);

			if (httpContext.Request.Headers["If-None-Match"] == package.GetETag(true))
			{
				httpContext.Response.StatusCode = (int)HttpStatusCode.NotModified;
				return new EmptyResult();
			}

			if (setCacheHeaders)
			{
				httpContext.Response.CacheDuration(TimeSpan.FromDays(365), package.GetETag());
			}

			if (useCompression)
			{
				httpContext.CompressResponse();
			}

			return new ContentResult { Content = package.GetContent(), ContentType = package.GetMimeType() };
		}

		private static string GetUrl(RequestContext requestContext, ResourcePackage package)
		{
			var urlHelper = new UrlHelper(requestContext);
			return GetUrl(urlHelper, package);
		}

		private static string GetUrl(UrlHelper urlHelper, ResourcePackage package)
		{
			if (string.IsNullOrEmpty(_urlFormat))
			{
				throw new Exception(
					"The ResourceMinifier Mvc Extension is not configured."
						+ " Run ResourceMvcExtensions.ConfigureRouting(string routePattern) at application startup."
					);
			}

			string virtualPath, url;

			if (_etagInPath)
			{
				virtualPath = "~/" + string.Format(_urlFormat, package.Key, package.GetETag());
				url = urlHelper.Content(virtualPath);
			}
			else
			{
				virtualPath = "~/" + string.Format(_urlFormat, package.Key);
				url = urlHelper.Content(virtualPath) + "?etag=" + package.GetETag();
			}

			return url;
		}
	}
}

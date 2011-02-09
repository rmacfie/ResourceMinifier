# ResourceMinifier #

Combines and minifies CSS and Javascript files on the fly.
Use with ASP.Net WebForms or MVC.

### Instructions for ASP.NET MVC ###

Global.asax:
	public class MvcApplication : HttpApplication
	{
		protected override void Application_Start(object sender, EventArgs e)
		{
			ResourceMvcExtensions.ConfigureRouting("content/minifier/{etag}/{key}");
		}
	}

Controller:
	public class ContentController : Controller
	{
		public ActionResult Minifier(string etag, string key)
		{
			return ResourceMvcExtensions.GetActionResult(ControllerContext.HttpContext, key);
		}
	}

View:
	<head>
		<%= Html.CombinedStylesheet("styles.css", "~/content/reset.css", "~/content/layout.css", "~/content/grids.css") %>
		<%= Html.CombinedJavascript("scripts.js", "~/content/jquery-1.4.2.js", "~/content/global.js",) %>
	</head>


That's it.

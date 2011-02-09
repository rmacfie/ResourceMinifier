using System;
using System.IO.Compression;
using System.Web;

namespace ResourceMinifier
{
	internal static class HttpExtensions
	{
		private const string _acceptEncoding = "Accept-Encoding";
		private const string _contentEncoding = "Content-Encoding";
		private const string _deflate = "deflate";
		private const string _gzip = "gzip";

		internal static void CacheDuration(this HttpResponseBase response, TimeSpan duration, string eTag)
		{
			var now = DateTime.Now;

			response.Cache.VaryByHeaders["Accept-Encoding"] = true;
			response.Cache.SetCacheability(HttpCacheability.Public);
			response.Cache.SetMaxAge(duration);
			response.Cache.SetExpires(now.Add(duration));
			response.Cache.SetLastModified(now);

			if (!string.IsNullOrEmpty(eTag))
			{
				response.Cache.SetETag(eTag);
			}
		}

		internal static void CompressResponse(this HttpContextBase httpContext)
		{
			const string key = "ResourceMinifier.HttpExtensions.CompressResponse";

			if ((httpContext.Items[key] as string) == bool.TrueString)
			{
				return;
			}

			var httpRequest = httpContext.Request;
			var httpResponse = httpContext.Response;

			if (IsEncodingAccepted(httpRequest, _gzip))
			{
				httpResponse.Filter = new GZipStream(httpResponse.Filter, CompressionMode.Compress);
				SetEncoding(httpResponse, _gzip);
			}
			else if (IsEncodingAccepted(httpRequest, _deflate))
			{
				httpResponse.Filter = new DeflateStream(httpResponse.Filter, CompressionMode.Compress);
				SetEncoding(httpResponse, _deflate);
			}

			httpContext.Items[key] = bool.TrueString;
		}

		private static bool IsEncodingAccepted(HttpRequestBase httpRequest, string encoding)
		{
			return httpRequest.Headers[_acceptEncoding] != null && httpRequest.Headers[_acceptEncoding].Contains(encoding);
		}

		private static void SetEncoding(HttpResponseBase httpResponse, string encoding)
		{
			httpResponse.AppendHeader(_contentEncoding, encoding);
			httpResponse.Cache.VaryByHeaders[_acceptEncoding] = true;
		}
	}
}

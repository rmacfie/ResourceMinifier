using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

namespace ResourceMinifier
{
	public abstract class ResourcePackage
	{
		private static readonly string _keyFormat = typeof(ResourcePackage).FullName + "[{0}]";

		protected ResourcePackage(string key, bool doMinify, params string[] paths)
		{
			Key = key;
			Paths = paths;
			DoMinify = doMinify;

			if (HttpRuntime.Cache[GetCacheKey()] == null)
			{
				UpdateCache(this);
			}
		}

		public string Key { get; private set; }
		public bool DoMinify { get; private set; }
		public string[] Paths { get; private set; }
		public DateTime UpdatedOn { get; private set; }

		public string GetETag()
		{
			return GetETag(false);
		}

		public string GetETag(bool updateFirst)
		{
			if (updateFirst && HttpRuntime.Cache[GetCacheKey()] == null)
			{
				UpdateCache(this);
			}

			return UpdatedOn.Ticks.ToString().Substring(4, 10);
		}

		public string GetContent()
		{
			return (HttpRuntime.Cache[GetCacheKey()] ?? UpdateCache(this)) as string;
		}

		public abstract string GetMimeType();

		protected abstract string Minify(string content);

		private string UpdateCache(ResourcePackage package)
		{
			lock (package)
			{
				var physicalPaths = new List<string>();
				var contents = string.Empty;

				foreach (var virtualPath in package.Paths)
				{
					if (!virtualPath.StartsWith("~"))
					{
						throw PathFormatException(virtualPath);
					}

					string tmpPathPhysical;
					var tmpContent = RetrieveFromDisk(virtualPath, out tmpPathPhysical);

					physicalPaths.Add(tmpPathPhysical);

					if (tmpContent == null)
					{
						contents += "/* ResourceMinifier error: Missing file */";
					}
					else if (DoMinify)
					{
						contents += Minify(tmpContent);
					}
					else
					{
						contents += tmpContent;
					}
					contents += Environment.NewLine + " " + Environment.NewLine;
				}

				var cacheDependency = physicalPaths.Count > 0 ? new CacheDependency(physicalPaths.ToArray()) : null;

				HttpRuntime.Cache.Insert(
					package.GetCacheKey(),
					contents,
					cacheDependency,
					Cache.NoAbsoluteExpiration,
					Cache.NoSlidingExpiration
					);

				package.UpdatedOn = DateTime.Now;

				return contents;
			}
		}

		private string GetCacheKey()
		{
			return string.Format(_keyFormat, Key);
		}

		private static string RetrieveFromDisk(string virtualPath, out string absolutePath)
		{
			absolutePath = HostingEnvironment.MapPath(virtualPath);

			if (absolutePath == null)
			{
				throw PathFormatException(virtualPath);
			}

			try
			{
				return File.ReadAllText(absolutePath);
			}
			catch
			{
				return null;
			}
		}

		private static Exception PathFormatException(string virtualPath)
		{
			return new Exception("The script and stylesheet paths must be valid virtual paths (prefixed with '~/') [" + virtualPath + "].");
		}
	}
}

using System;
using System.Collections.Generic;

namespace ResourceMinifier
{
	public static class ResourceRegistry
	{
		private static readonly IDictionary<string, ResourcePackage> _packages = new Dictionary<string, ResourcePackage>();
		private static readonly object _lock = new object();

		///<summary>
		///	Gets a Stylesheet ResourcePackage from the registry. If the package has not been registered yet, it will be created first,
		///	using the provided input.
		///</summary>
		public static ResourcePackage LoadJavascript(string key, params string[] virtualPaths)
		{
			return LoadJavascript(key, true, virtualPaths);
		}

		///<summary>
		///	Gets a Stylesheet ResourcePackage from the registry. If the package has not been registered yet, it will be created first,
		///	using the provided input.
		///</summary>
		public static ResourcePackage LoadJavascript(string key, bool doMinify, params string[] virtualPaths)
		{
			return Load(key, () => new JavascriptPackage(key, doMinify, virtualPaths), virtualPaths);
		}

		///<summary>
		///	Gets a Javascript ResourcePackage from the registry. If the package has not been registered yet, it will be created first,
		///	using the provided input.
		///</summary>
		public static ResourcePackage LoadStylesheet(string key, params string[] virtualPaths)
		{
			return LoadStylesheet(key, true, virtualPaths);
		}

		///<summary>
		///	Gets a Javascript ResourcePackage from the registry. If the package has not been registered yet, it will be created first,
		///	using the provided input.
		///</summary>
		public static ResourcePackage LoadStylesheet(string key, bool doMinify, params string[] virtualPaths)
		{
			return Load(key, () => new StylesheetPackage(key, doMinify, virtualPaths), virtualPaths);
		}

		/// <summary>
		/// 	Gets a ResourcePackage from the registry.
		/// </summary>
		public static ResourcePackage Get(string key)
		{
			return _packages[key];
		}

		///<summary>
		///	Gets a ResourcePackage from the registry. If the package has not been registered yet, it will be created first,
		///	using the provided creation function.
		///</summary>
		public static ResourcePackage Load(string key, Func<ResourcePackage> create, params string[] virtualPaths)
		{
			if (virtualPaths == null || virtualPaths.Length == 0)
			{
				throw new ArgumentException("You must provide at least one path.", "virtualPaths");
			}

			lock (_lock)
			{
				if (!_packages.ContainsKey(key))
				{
					_packages.Add(key, create());
				}
			}

			return _packages[key];
		}
	}
}

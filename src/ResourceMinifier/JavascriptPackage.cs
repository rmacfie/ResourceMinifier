using Yahoo.Yui.Compressor;

namespace ResourceMinifier
{
	public class JavascriptPackage : ResourcePackage
	{
		public JavascriptPackage(string key, bool doMinify, params string[] paths) : base(key, doMinify, paths)
		{
		}


		#region Overrides of ResourcePackage

		public override string GetMimeType()
		{
			return "text/javascript";
		}

		protected override string Minify(string content)
		{
			return JavaScriptCompressor.Compress(content);
		}

		#endregion
	}
}

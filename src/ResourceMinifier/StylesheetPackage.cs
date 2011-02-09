using Yahoo.Yui.Compressor;

namespace ResourceMinifier
{
	public class StylesheetPackage : ResourcePackage
	{
		public StylesheetPackage(string key, bool doMinify, params string[] paths)
			: base(key, doMinify, paths)
		{
		}


		#region Overrides of ResourcePackage

		public override string GetMimeType()
		{
			return "text/css";
		}

		protected override string Minify(string content)
		{
			return CssCompressor.Compress(content);
		}

		#endregion
	}
}

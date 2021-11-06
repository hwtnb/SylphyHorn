using System;
using System.Reflection;

namespace SylphyHorn.Properties
{
	public static class ProductInfo
	{
		private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();
		private static readonly Lazy<string> _titleLazy = new Lazy<string>(() => ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(_assembly, typeof(AssemblyTitleAttribute))).Title);
		private static readonly Lazy<string> _descriptionLazy = new Lazy<string>(() => ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(_assembly, typeof(AssemblyDescriptionAttribute))).Description);
		private static readonly Lazy<string> _companyLazy = new Lazy<string>(() => ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(_assembly, typeof(AssemblyCompanyAttribute))).Company);
		private static readonly Lazy<string> _productLazy = new Lazy<string>(() => ((AssemblyProductAttribute)Attribute.GetCustomAttribute(_assembly, typeof(AssemblyProductAttribute))).Product);
		private static readonly Lazy<string> _copyrightLazy = new Lazy<string>(() => ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(_assembly, typeof(AssemblyCopyrightAttribute))).Copyright);
		private static readonly Lazy<string> _trademarkLazy = new Lazy<string>(() => ((AssemblyTrademarkAttribute)Attribute.GetCustomAttribute(_assembly, typeof(AssemblyTrademarkAttribute))).Trademark);
		private static readonly Lazy<string> _versionLazy = new Lazy<string>(() => $"{Version.ToString(3)}{(IsBetaRelease ? " β" : "")}{(Version.Revision == 0 ? "" : " rev." + Version.Revision)}");
		private static readonly Lazy<string> _extraVersionLazy = new Lazy<string>(() => $"{((AssemblyMetadataAttribute)Attribute.GetCustomAttribute(_assembly, typeof(AssemblyMetadataAttribute))).Value}");


		public static string Title => _titleLazy.Value;

		public static string Description => _descriptionLazy.Value;

		public static string Company => _companyLazy.Value;

		public static string Product => _productLazy.Value;

		public static string Copyright => _copyrightLazy.Value;

		public static string Trademark => _trademarkLazy.Value;

		public static Version Version => _assembly.GetName().Version;

		public static string VersionString => _versionLazy.Value;

		public static string ExtraVersionString => _extraVersionLazy.Value;


		public static bool IsBetaRelease
		{
			get
			{
#if BETA
				return true;
#else
				return false;
#endif
			}
		}

		public static bool IsDebug
		{
			get
			{
#if DEBUG
				return true;
#else
				return false;
#endif
			}
		}


		internal static int OSBuild => Environment.OSVersion.Version.Build;

		internal static bool IsWindows11OrLater => OSBuild >= 22000;

		internal static bool IsNameSupportBuild => OSBuild >= 18975;

		internal static bool IsWallpaperSupportBuild => OSBuild >= 21337;

		internal static bool IsReorderingSupportBuild => IsWallpaperSupportBuild;
	}
}

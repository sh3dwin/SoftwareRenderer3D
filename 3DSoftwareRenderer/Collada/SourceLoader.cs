using System.IO;
using System.Reflection;

namespace SoftwareRenderer3D.Collada
{
	public static class SourceLoader
	{
		public static Stream AsStream(string path)
		{
			var assembly = Assembly.GetEntryAssembly();
			return assembly.GetManifestResourceStream(path);
		}

		public static string AsString(string path) 
		{
			var assembly = Assembly.GetEntryAssembly();

			using (var stream = AsStream(path))
				using (var reader = new StreamReader(stream))
					return reader.ReadToEnd();
		}
	}
}

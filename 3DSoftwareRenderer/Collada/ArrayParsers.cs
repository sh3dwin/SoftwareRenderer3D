using System.Collections.Generic;
using System.Linq;

namespace SoftwareRenderer3D.Collada
{
    /// <summary>
    /// Adapted from:
    /// https://github.com/larsjarlvik/collada-parser/tree/master
    /// </summary>
    public static class ArrayParsers
	{
		public static List<float> ParseFloats(string input) 
		{
			return input.Trim(' ').Split(' ').Select(x => float.Parse(x)).ToList();
		}

		public static List<int> ParseInts(string input) 
		{
			return input.Trim(' ').Split(' ').Select(x => int.Parse(x)).ToList();
		}
	}
}
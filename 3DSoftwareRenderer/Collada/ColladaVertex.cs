using System.Numerics;

namespace SoftwareRenderer3D.Collada
{
    /// <summary>
    /// Adapted from:
    /// https://github.com/larsjarlvik/collada-parser/tree/master
    /// </summary>
    public class ColladaVertex 
	{	
		private const int NO_INDEX = -1;
		
		public Vector3 Position { get; set; }
		public int TextureIndex { get; set; }
		public int NormalIndex { get; set; }
		public int ColorIndex { get; set; }
		public int Index { get; private set; }
		public ColladaVertex DuplicateVertex { get; set; }

		public bool IsSet => NormalIndex != NO_INDEX;
		
		public ColladaVertex(int index, Vector3 position)
		{
			Index = index;
			NormalIndex = NO_INDEX;
			TextureIndex = NO_INDEX;
			ColorIndex = NO_INDEX;
			Position = position;
		}
		
		public bool HasSameInformation(int normalIndexOther, int textureIndexOther, int colorIndexOther)
		{
			return 
				textureIndexOther == TextureIndex && 
				normalIndexOther == NormalIndex &&
				colorIndexOther == ColorIndex;
		}
	}
}

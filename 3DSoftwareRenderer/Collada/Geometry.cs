using System;
using System.Numerics;

namespace SoftwareRenderer3D.Collada
{
    public class Geometry
	{
		private int modelBuffer;
		private int positionBuffer;
		private int normalBuffer;
		private int textureBuffer;
		private int colorBuffer;
		private int indexBuffer;

		private int numIndices;

		private Vector3[] vertices;
		private Vector3[] normals;
		private Vector2[] textures;
		private Vector3[] colors;

		private int[] indices;

		public Geometry(Vector3[] vertices, Vector3[] normals, Vector2[] textures, Vector3[] colors, int[] indices)
		{
			this.vertices = vertices;
			this.normals = normals;
			this.textures = textures;
			this.colors = colors;
			this.indices = indices;
		}
	}
}
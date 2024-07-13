namespace SoftwareRenderer3D.Utils
{
    public static class ArrayUtils
    {
        public static float[] GetEmptyFloatBuffer(int width, int height, float value = 0.0f)
        {
            var result = new float[height * width];
            for (var row = 0; row < height; row++)
            {
                for (var col = 0; col < width; col++)
                {
                    int index = col + row * width;
                    result[index] = value;
                }
            }
            return result;
        }

        public static int[] GetEmptyIntBuffer(int width, int height, int value = 0)
        {
            var result = new int[height * width];
            for (var row = 0; row < height; row++)
            {
                for (var col = 0; col < width; col++)
                {
                    int index = col + row * width;
                    result[index] = value;
                }
            }
            return result;
        }

        public static double[] GetEmptyDoubleBuffer(int width, int height, double value = 0.0)
        {
            var result = new double[height * width];
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    int index = j + i * width;
                    result[index] = value;
                }
            }
            return result;
        }
    }
}

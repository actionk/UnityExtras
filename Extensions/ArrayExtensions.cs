namespace Plugins.Shared.UnityMonstackCore.Extensions
{
    public static class ArrayExtensions
    {
        public static T[] Flatten<T>(this T[,] map)
        {
            var width = map.GetLength(0);
            var height = map.GetLength(1);
            var size = width * height;
            var result = new T[size];

            var write = 0;
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    result[write++] = map[y, x];
                }
            }

            return result;
        }
    }
}
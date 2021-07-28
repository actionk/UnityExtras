using Plugins.UnityMonstackCore.Extensions;
using Unity.Mathematics;

namespace Plugins.Shared.UnityMonstackCore.Utils
{
    public static class Map2DResizer
    {
        public enum ResizeType
        {
            BILINEAR_INTERPOLATION,
            CLOSEST_VALUE
        }

        public static float[,] Resize(this float[,] source, int2 targetSize, ResizeType resizeType = ResizeType.CLOSEST_VALUE)
        {
            var sourceSize = new int2(source.GetLength(0), source.GetLength(1));
            if (sourceSize.x == targetSize.x && sourceSize.y == targetSize.y)
                return source;

            var target = new float[targetSize.x, targetSize.y];
            for (int x = 0; x < targetSize.x; x++)
            {
                for (int y = 0; y < targetSize.y; y++)
                {
                    var value = 0.0f;
                    switch (resizeType)
                    {
                        case ResizeType.BILINEAR_INTERPOLATION:
                        {
                            var normalizedPosition = new int2(x, y).GetNormalizedCoordinates(targetSize);
                            var corners = new[]
                            {
                                new int2((int) math.floor(normalizedPosition.x * (sourceSize.x - 1)), (int) math.floor(normalizedPosition.y * (sourceSize.y - 1))),
                                new int2((int) math.floor(normalizedPosition.x * (sourceSize.x - 1)), (int) math.floor(normalizedPosition.y * (sourceSize.y - 1) + 1)),
                                new int2((int) math.floor(normalizedPosition.x * (sourceSize.x - 1) + 1), (int) math.floor(normalizedPosition.y * (sourceSize.y - 1) + 1)),
                                new int2((int) math.floor(normalizedPosition.x * (sourceSize.x - 1) + 1), (int) math.floor(normalizedPosition.y * (sourceSize.y - 1))),
                            };

                            var cornersValues = new[]
                            {
                                source[corners[0].x, corners[0].y],
                                source[corners[1].x, corners[1].y],
                                source[corners[2].x, corners[2].y],
                                source[corners[3].x, corners[3].y]
                            };

                            var cornersNormalized = new[]
                            {
                                corners[0].GetNormalizedCoordinates(sourceSize),
                                corners[1].GetNormalizedCoordinates(sourceSize),
                                corners[2].GetNormalizedCoordinates(sourceSize),
                                corners[3].GetNormalizedCoordinates(sourceSize)
                            };

                            var uv = new float2(
                                (normalizedPosition.x - cornersNormalized[1].x) / math.abs(cornersNormalized[2].x - cornersNormalized[1].x),
                                1.0f - (normalizedPosition.y - cornersNormalized[0].y) / math.abs(cornersNormalized[1].y - cornersNormalized[0].y)
                            );

                            var lerpTop = math.lerp(cornersValues[1], cornersValues[2], uv.x);
                            var lerpBottom = math.lerp(cornersValues[0], cornersValues[3], uv.x);
                            var lerpUV = math.lerp(lerpTop, lerpBottom, uv.y);
                            value = lerpUV;
                            break;
                        }

                        case ResizeType.CLOSEST_VALUE:
                        {
                            var normalizedPosition = new int2(x, y).GetNormalizedCoordinates(targetSize);
                            var closestSourcePosition = new int2((int) math.floor(normalizedPosition.x * (sourceSize.x - 1)),
                                (int) math.floor(normalizedPosition.y * (sourceSize.y - 1)));
                            value = source[closestSourcePosition.x, closestSourcePosition.y];
                            break;
                        }
                    }

                    target[x, y] = value;
                }
            }

            return target;
        }

        public static T[,] Resize<T>(this T[,] source, int2 targetSize)
        {
            var sourceSize = new int2(source.GetLength(0), source.GetLength(1));
            if (sourceSize.x == targetSize.x && sourceSize.y == targetSize.y)
                return source;

            var target = new T[targetSize.x, targetSize.y];
            for (int x = 0; x < targetSize.x; x++)
            {
                for (int y = 0; y < targetSize.y; y++)
                {
                    var value = 0.0f;

                    var normalizedPosition = new int2(x, y).GetNormalizedCoordinates(targetSize);
                    var closestSourcePosition = new int2((int) math.floor(normalizedPosition.x * (sourceSize.x - 1)),
                        (int) math.floor(normalizedPosition.y * (sourceSize.y - 1)));
                    target[x, y] = source[closestSourcePosition.x, closestSourcePosition.y];
                }
            }

            return target;
        }
    }
}
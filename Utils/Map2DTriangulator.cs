using System;
using System.Collections.Generic;
using Pathfinding.Poly2Tri;
using Plugins.Shared.UnityMonstackCore.Extensions;
using Plugins.UnityMonstackCore.Loggers;
using Unity.Mathematics;
using UnityEngine;

namespace Plugins.Shared.UnityMonstackCore.Utils
{
    public static class Map2DTriangulatorExtensions
    {
        public static MeshData ToMesh(this bool[,] map, float scale, float3 offset)
        {
            if (map.GetLength(0) != map.GetLength(1))
            {
                UnityLogger.Error("Width != Height");
                return null;
            }

            var triangulator = new Map2DTriangulator(map, scale, offset);
            return triangulator.Triangulate();
        }
    }

    public class MeshData
    {
        public float3[] vertices;
        public int[] triangles;
    }

    public class Map2DTriangulator
    {
        private readonly Voxel[] m_voxels;
        private readonly int m_resolution;

        private readonly List<float3> m_vertices = new List<float3>();
        private readonly List<int> m_triangles = new List<int>();

        public Map2DTriangulator(bool[,] map, float scale, float3 offset)
        {
            m_resolution = map.GetLength(0);
            m_voxels = new Voxel[m_resolution * m_resolution];
            for (int i = 0, y = 0; y < m_resolution; y++)
            {
                for (var x = 0; x < m_resolution; x++, i++)
                {
                    m_voxels[i] = new Voxel(x, y, map[x, y], scale, offset);
                }
            }
        }

        public MeshData Triangulate()
        {
            int cells = m_resolution - 1;
            for (int i = 0, y = 0; y < cells; y++, i++)
            {
                for (int x = 0; x < cells; x++, i++)
                {
                    TriangulateCell(
                        m_voxels[i],
                        m_voxels[i + 1],
                        m_voxels[i + m_resolution],
                        m_voxels[i + m_resolution + 1]);
                }
            }
            
            return new MeshData
            {
                vertices = m_vertices.ToArray(),
                triangles = m_triangles.ToArray()
            };
        }

        private void TriangulateCell(Voxel a, Voxel b, Voxel c, Voxel d)
        {
            int cellType = 0;
            if (a.state)
            {
                cellType |= 1;
            }

            if (b.state)
            {
                cellType |= 2;
            }

            if (c.state)
            {
                cellType |= 4;
            }

            if (d.state)
            {
                cellType |= 8;
            }

            switch (cellType)
            {
                case 0:
                    return;
                case 1:
                    AddTriangle(a.position, a.yEdgePosition, a.xEdgePosition);
                    break;
                case 2:
                    AddTriangle(b.position, a.xEdgePosition, b.yEdgePosition);
                    break;
                case 3:
                    AddQuad(a.position, a.yEdgePosition, b.yEdgePosition, b.position);
                    break;
                case 4:
                    AddTriangle(c.position, c.xEdgePosition, a.yEdgePosition);
                    break;
                case 5:
                    AddQuad(a.position, c.position, c.xEdgePosition, a.xEdgePosition);
                    break;
                case 6:
                    AddTriangle(b.position, a.xEdgePosition, b.yEdgePosition);
                    AddTriangle(c.position, c.xEdgePosition, a.yEdgePosition);
                    break;
                case 7:
                    AddPentagon(a.position, c.position, c.xEdgePosition, b.yEdgePosition, b.position);
                    break;
                case 8:
                    AddTriangle(d.position, b.yEdgePosition, c.xEdgePosition);
                    break;
                case 9:
                    AddTriangle(a.position, a.yEdgePosition, a.xEdgePosition);
                    AddTriangle(d.position, b.yEdgePosition, c.xEdgePosition);
                    break;
                case 10:
                    AddQuad(a.xEdgePosition, c.xEdgePosition, d.position, b.position);
                    break;
                case 11:
                    AddPentagon(b.position, a.position, a.yEdgePosition, c.xEdgePosition, d.position);
                    break;
                case 12:
                    AddQuad(a.yEdgePosition, c.position, d.position, b.yEdgePosition);
                    break;
                case 13:
                    AddPentagon(c.position, d.position, b.yEdgePosition, a.xEdgePosition, a.position);
                    break;
                case 14:
                    AddPentagon(d.position, b.position, a.xEdgePosition, a.yEdgePosition, c.position);
                    break;
                case 15:
                    AddQuad(a.position, c.position, d.position, b.position);
                    break;
            }
        }

        private void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            int vertexIndex = m_vertices.Count;
            m_vertices.Add(a);
            m_vertices.Add(b);
            m_vertices.Add(c);
            m_triangles.Add(vertexIndex);
            m_triangles.Add(vertexIndex + 1);
            m_triangles.Add(vertexIndex + 2);
        }

        private void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            int vertexIndex = m_vertices.Count;
            m_vertices.Add(a);
            m_vertices.Add(b);
            m_vertices.Add(c);
            m_vertices.Add(d);
            m_triangles.Add(vertexIndex);
            m_triangles.Add(vertexIndex + 1);
            m_triangles.Add(vertexIndex + 2);
            m_triangles.Add(vertexIndex);
            m_triangles.Add(vertexIndex + 2);
            m_triangles.Add(vertexIndex + 3);
        }

        private void AddPentagon(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 e)
        {
            int vertexIndex = m_vertices.Count;
            m_vertices.Add(a);
            m_vertices.Add(b);
            m_vertices.Add(c);
            m_vertices.Add(d);
            m_vertices.Add(e);
            m_triangles.Add(vertexIndex);
            m_triangles.Add(vertexIndex + 1);
            m_triangles.Add(vertexIndex + 2);
            m_triangles.Add(vertexIndex);
            m_triangles.Add(vertexIndex + 2);
            m_triangles.Add(vertexIndex + 3);
            m_triangles.Add(vertexIndex);
            m_triangles.Add(vertexIndex + 3);
            m_triangles.Add(vertexIndex + 4);
        }

        [Serializable]
        public class Voxel
        {
            public bool state;

            public Vector3 position, xEdgePosition, yEdgePosition;

            public Voxel(int x, int y, bool state, float size, float3 offset)
            {
                position.x = (x + 0.5f) * size + offset.x;
                position.y = offset.y;
                position.z = (y + 0.5f) * size + offset.z;
                this.state = state;

                xEdgePosition = position;
                xEdgePosition.x += size * 0.5f;
                yEdgePosition = position;
                yEdgePosition.z += size * 0.5f;
            }
        }
    }
}
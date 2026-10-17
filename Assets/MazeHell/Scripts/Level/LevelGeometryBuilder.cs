using System;
using System.Collections.Generic;
using UnityEngine;

namespace MazeHell.Level
{
    public class LevelGeometryBuilder
    {
        private List<Mesh> _runtimeMeshes = new List<Mesh>();
        private Material _wallMaterial;
        private Material _floorMaterial;
        private Material _ceilingMaterial;

        public LevelGeometryBuilder(Material wallMaterial, Material floorMaterial, Material ceilingMaterial)
        {
            if (wallMaterial == null)
            {
                throw new ArgumentNullException(nameof(wallMaterial));
            }

            if (floorMaterial == null)
            {
                throw new ArgumentNullException(nameof(floorMaterial));
            }

            if (ceilingMaterial == null)
            {
                throw new ArgumentNullException(nameof(ceilingMaterial));
            }

            _wallMaterial = wallMaterial;
            _floorMaterial = floorMaterial;
            _ceilingMaterial = ceilingMaterial;
        }

        public Mesh[] GetRuntimeMeshes()
        {
            return _runtimeMeshes.ToArray();
        }

        public GameObject CreateBox(Transform parent, string objectName, Vector3 center, Vector3 size)
        {
            return CreateBoxWithMaterial(parent, objectName, center, size, _wallMaterial);
        }

        public GameObject CreateBoxWithMaterial(Transform parent, string objectName, Vector3 center, Vector3 size, Material material)
        {
            if (material == null)
            {
                throw new ArgumentNullException(nameof(material));
            }

            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = objectName;
            box.isStatic = true;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = center;
            box.transform.localScale = size;

            box.TryGetComponent(out Renderer renderer);
            renderer.sharedMaterial = material;
            return box;
        }

        public GameObject CreateFloorSlab(Transform parent, string objectName, Vector3 center, Vector3 size, float metersPerTextureTile)
        {
            return CreateSlabWithMaterial(parent, objectName, center, size, metersPerTextureTile, _floorMaterial);
        }

        public GameObject CreateCeilingSlab(Transform parent, string objectName, Vector3 center, Vector3 size, float metersPerTextureTile)
        {
            return CreateSlabWithMaterial(parent, objectName, center, size, metersPerTextureTile, _ceilingMaterial);
        }

        public GameObject CreateSlabWithMaterial(Transform parent, string objectName, Vector3 center, Vector3 size, float metersPerTextureTile, Material material)
        {
            if (material == null)
            {
                throw new ArgumentNullException(nameof(material));
            }

            GameObject slab = new GameObject(objectName);
            slab.isStatic = true;
            slab.transform.SetParent(parent, false);
            slab.transform.localPosition = center;

            MeshFilter meshFilter = slab.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = CreateTiledBoxMesh(size, metersPerTextureTile);

            MeshRenderer meshRenderer = slab.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;

            BoxCollider boxCollider = slab.AddComponent<BoxCollider>();
            boxCollider.size = size;
            return slab;
        }

        public void CreateCorridorShell(Transform parent, string namePrefix, float xBegin, float length, float axisZ, float width, float height, float floorThickness, float wallThickness, float metersPerTextureTile, bool capFarEnd)
        {
            float centerLineX = xBegin + length * 0.5f;

            CreateFloorSlab(parent, $"{namePrefix}_Floor", new Vector3(centerLineX, -floorThickness * 0.5f, axisZ), new Vector3(length, floorThickness, width), metersPerTextureTile);
            CreateCeilingSlab(parent, $"{namePrefix}_Ceiling", new Vector3(centerLineX, height + floorThickness * 0.5f, axisZ), new Vector3(length, floorThickness, width + wallThickness * 2f), metersPerTextureTile);
            CreateBox(parent, $"{namePrefix}_WallSouth", new Vector3(centerLineX, height * 0.5f, axisZ - (width * 0.5f + wallThickness * 0.5f)), new Vector3(length, height, wallThickness));
            CreateBox(parent, $"{namePrefix}_WallNorth", new Vector3(centerLineX, height * 0.5f, axisZ + (width * 0.5f + wallThickness * 0.5f)), new Vector3(length, height, wallThickness));

            if (capFarEnd)
            {
                CreateBox(parent, $"{namePrefix}_EndCap", new Vector3(xBegin + length + wallThickness * 0.5f, height * 0.5f, axisZ), new Vector3(wallThickness, height, width + wallThickness * 2f));
            }
        }

        private Mesh CreateTiledBoxMesh(Vector3 size, float metersPerTextureTile)
        {
            List<Vector3> vertices = new List<Vector3>(24);
            List<Vector3> normals = new List<Vector3>(24);
            List<Vector2> uvs = new List<Vector2>(24);
            List<int> triangles = new List<int>(36);

            AppendFace(vertices, normals, uvs, triangles, size, new Vector3(1f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(0f, 0f, 1f), metersPerTextureTile);
            AppendFace(vertices, normals, uvs, triangles, size, new Vector3(-1f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(0f, 0f, -1f), metersPerTextureTile);
            AppendFace(vertices, normals, uvs, triangles, size, new Vector3(0f, 0f, -1f), new Vector3(0f, 1f, 0f), new Vector3(1f, 0f, 0f), metersPerTextureTile);
            AppendFace(vertices, normals, uvs, triangles, size, new Vector3(0f, 0f, -1f), new Vector3(0f, 1f, 0f), new Vector3(-1f, 0f, 0f), metersPerTextureTile);
            AppendFace(vertices, normals, uvs, triangles, size, new Vector3(0f, 0f, -1f), new Vector3(-1f, 0f, 0f), new Vector3(0f, 1f, 0f), metersPerTextureTile);
            AppendFace(vertices, normals, uvs, triangles, size, new Vector3(0f, 0f, -1f), new Vector3(-1f, 0f, 0f), new Vector3(0f, -1f, 0f), metersPerTextureTile);

            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateTangents();
            _runtimeMeshes.Add(mesh);
            return mesh;
        }

        private void AppendFace(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> triangles, Vector3 size, Vector3 uDirection, Vector3 vDirection, Vector3 normalDirection, float metersPerTextureTile)
        {
            Vector3 halfSize = size * 0.5f;
            Vector3 uExtent = Vector3.Scale(new Vector3(Mathf.Abs(uDirection.x), Mathf.Abs(uDirection.y), Mathf.Abs(uDirection.z)), halfSize);
            Vector3 vExtent = Vector3.Scale(new Vector3(Mathf.Abs(vDirection.x), Mathf.Abs(vDirection.y), Mathf.Abs(vDirection.z)), halfSize);
            Vector3 normalExtent = Vector3.Scale(new Vector3(Mathf.Abs(normalDirection.x), Mathf.Abs(normalDirection.y), Mathf.Abs(normalDirection.z)), halfSize);

            Vector3 corner = Vector3.Scale(normalDirection, normalExtent) - Vector3.Scale(uDirection, uExtent) - Vector3.Scale(vDirection, vExtent);
            Vector3 uStep = Vector3.Scale(uDirection, uExtent) * 2f;
            Vector3 vStep = Vector3.Scale(vDirection, vExtent) * 2f;

            int firstIndex = vertices.Count;

            for (int cornerIndex = 0; cornerIndex < 4; cornerIndex++)
            {
                Vector3 vertex = corner + uStep * (cornerIndex % 2) + vStep * (cornerIndex / 2);
                float uCoordinate = Vector3.Dot(vertex, uDirection) / metersPerTextureTile;
                float vCoordinate = Vector3.Dot(vertex, vDirection) / metersPerTextureTile;

                vertices.Add(vertex);
                normals.Add(normalDirection);
                uvs.Add(new Vector2(uCoordinate, vCoordinate));
            }

            if (Vector3.Dot(Vector3.Cross(uDirection, vDirection), normalDirection) > 0f)
            {
                triangles.Add(firstIndex);
                triangles.Add(firstIndex + 1);
                triangles.Add(firstIndex + 2);
                triangles.Add(firstIndex + 1);
                triangles.Add(firstIndex + 3);
                triangles.Add(firstIndex + 2);
                return;
            }

            triangles.Add(firstIndex);
            triangles.Add(firstIndex + 2);
            triangles.Add(firstIndex + 1);
            triangles.Add(firstIndex + 1);
            triangles.Add(firstIndex + 2);
            triangles.Add(firstIndex + 3);
        }
    }
}

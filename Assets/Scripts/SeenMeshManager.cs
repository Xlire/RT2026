using System.Collections.Generic;
using UnityEngine;

namespace PointCloudScanner
{
    /// <summary>
    /// Class for managing the seen mesh in the AR environment.
    /// </summary>
    public class SeenMeshManager
    {
        private readonly Mesh _seenMesh;
        private readonly HashSet<int> _seenTriangles;
        private readonly List<Vector3> _seenVertices;
        private readonly List<int> _seenIndices;
        private readonly MeshCollider _meshCollider;
        private readonly MeshRenderer _meshRenderer;
        private readonly Dictionary<Mesh, int[]> _meshTrianglesCache = new Dictionary<Mesh, int[]>(); // Cache for triangle arrays
        private readonly Dictionary<Mesh, Vector3[]> _meshVerticesCache = new Dictionary<Mesh, Vector3[]>(); // Cache for vertex arrays

        private float _colliderUpdateTimer = 0f; // Timer for updating the MeshCollider
        private const float ColliderUpdateInterval = 1f; // Interval in seconds

        /// <summary>
        /// Constructor for SeenMeshManager. Initializes the mesh and its components.
        /// </summary>
        public SeenMeshManager()
        {
            _seenMesh = new Mesh
            {
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 // Enable 32-bit indices
            };
            _seenTriangles = new HashSet<int>();
            _seenVertices = new List<Vector3>();
            _seenIndices = new List<int>();

            // Create a new GameObject for the seen mesh
            GameObject seenObject = new GameObject("SeenMesh", typeof(MeshFilter), typeof(MeshCollider), typeof(MeshRenderer));
            MeshFilter meshFilter = seenObject.GetComponent<MeshFilter>();
            _meshCollider = seenObject.GetComponent<MeshCollider>();
            _meshRenderer = seenObject.GetComponent<MeshRenderer>();
            _meshRenderer.enabled = false; // Disable the renderer initially

            // Assign the shared mesh to both the MeshFilter and MeshCollider
            meshFilter.mesh = _seenMesh;
            _meshCollider.sharedMesh = _seenMesh;

            seenObject.tag = "ARMesh";
            // Add the object to layer called ARMesh
            seenObject.layer = LayerMask.NameToLayer("ARMesh");
        }

        /// <summary>
        /// Sets the material for the mesh renderer.
        /// </summary>
        /// <param name="meshMaterial">The material to set for the mesh renderer.</param>
        public void SetMeshMaterial(Material meshMaterial)
        {
            if (_meshRenderer == null)
            {
                Debug.LogWarning("MeshRenderer is not assigned.");
                return;
            }
            _meshRenderer.material = meshMaterial;
        }

        /// <summary>
        /// Adds a triangle to the seen list if it is not already present.
        /// </summary>
        /// <param name="meshFilter">The MeshFilter of the mesh being scanned.</param>
        /// <param name="hit">The RaycastHit information from the scan.</param> 
        public void AddTriangleToSeenList(MeshFilter meshFilter, RaycastHit hit)
        {
            int hitTriangleIndex = hit.triangleIndex;
            if (hitTriangleIndex != -1 && !_seenTriangles.Contains(hitTriangleIndex))
            {
                Mesh mesh = meshFilter.mesh;

                // Cache the triangle array to avoid repeated access
                if (!_meshTrianglesCache.TryGetValue(mesh, out int[] triangles))
                {
                    triangles = mesh.triangles; // Access once and cache
                    _meshTrianglesCache[mesh] = triangles;
                }

                // Cache the vertex array to avoid repeated access
                if (!_meshVerticesCache.TryGetValue(mesh, out Vector3[] vertices))
                {
                    vertices = mesh.vertices; // Access once and cache
                    _meshVerticesCache[mesh] = vertices;
                }

                // Validate triangle index bounds
                if (hitTriangleIndex * 3 + 2 >= triangles.Length)
                {
                    Debug.LogWarning("Triangle index is out of bounds.");
                    return;
                }

                // Get the indices of the triangle
                int i0 = triangles[hitTriangleIndex * 3];
                int i1 = triangles[hitTriangleIndex * 3 + 1];
                int i2 = triangles[hitTriangleIndex * 3 + 2];

                Transform meshTransform = meshFilter.transform;
                Vector3 worldVertex0 = meshTransform.TransformPoint(vertices[i0]);
                Vector3 worldVertex1 = meshTransform.TransformPoint(vertices[i1]);
                Vector3 worldVertex2 = meshTransform.TransformPoint(vertices[i2]);

                // Add unique vertices and update indices
                _seenVertices.Add(worldVertex0);
                _seenVertices.Add(worldVertex1);
                _seenVertices.Add(worldVertex2);
                _seenIndices.Add(_seenVertices.Count - 3);
                _seenIndices.Add(_seenVertices.Count - 2);
                _seenIndices.Add(_seenVertices.Count - 1);

                _seenTriangles.Add(hitTriangleIndex);

                UpdateSeenMesh(); // Update the mesh immediately
            }
        }

        /// <summary>
        /// Updates the seen mesh with the current vertices and indices.
        /// </summary>
        private void UpdateSeenMesh()
        {
            _seenMesh.Clear();
            _seenMesh.SetVertices(_seenVertices);
            _seenMesh.SetTriangles(_seenIndices, 0);

            // Recalculate normals only if necessary
            if (_meshRenderer.enabled) 
            {
                _seenMesh.RecalculateNormals();
            }

            // Ensure MeshCollider is updated
            UpdateMeshCollider();
        }

        /// <summary>
        /// Updates the MeshCollider with the current mesh.
        /// </summary>
        private void UpdateMeshCollider()
        {
            _colliderUpdateTimer += Time.deltaTime;

            if (_colliderUpdateTimer >= ColliderUpdateInterval)
            {
                if (_meshCollider == null)
                {
                    Debug.LogWarning("MeshCollider is not assigned.");
                    return;
                }
                _meshCollider.sharedMesh = null; // Force collider update
                _meshCollider.sharedMesh = _seenMesh;

                _colliderUpdateTimer = 0f; // Reset the timer      
            }
        }

        /// <summary>
        /// Toggles the visibility of the mesh renderer.
        /// </summary>
        public void ToggleVisibility()
        {
            if (_meshRenderer != null)
            {
                _meshRenderer.enabled = !_meshRenderer.enabled;
                UpdateSeenMesh(); // Update the mesh when toggling visibility
            }
        }
    }
}
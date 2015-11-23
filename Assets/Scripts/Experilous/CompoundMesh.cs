using UnityEngine;
using System.Collections.Generic;

namespace Experilous
{
	public class CompoundMesh : MonoBehaviour
	{
		public UniqueMesh SubmeshPrefab;

		[SerializeField, HideInInspector]
		private Transform _root;

		[SerializeField, HideInInspector]
		private int _submeshCount;

		private Transform _rootCache;
		private List<UniqueMesh> _submeshCache;

		private UniqueMesh CreateSubmesh()
		{
			if (_submeshCache == null || _submeshCache.Count == 0)
			{
				return Instantiate(SubmeshPrefab);
			}
			else
			{
				var lastIndex = _submeshCache.Count - 1;
				var submesh = _submeshCache[lastIndex];
				if (lastIndex == 0)
				{
					_submeshCache = null;
				}
				else
				{
					_submeshCache.RemoveAt(lastIndex);
				}
				submesh.gameObject.SetActive(true);
				return submesh;
			}
		}

		public void PushSubmesh(Vector3[] vertices, Color[] colors, int[] triangles)
		{
			Mesh mesh;
			if (_submeshCount == 0 || _root == null)
			{
				var uniqueMesh = CreateSubmesh();
				uniqueMesh.name = "Mesh";
				_root = uniqueMesh.transform;
				_root.SetParent(transform, false);
				mesh = uniqueMesh.mesh;
			}
			else
			{
				if (_submeshCount == 1)
				{
					var firstSubmesh = _root;
					_root = (_rootCache != null) ? _rootCache : new GameObject("Submeshes").transform;
					_root.SetParent(transform);
					firstSubmesh.SetParent(_root, false);
					firstSubmesh.name = "Submesh [0]";
				}

				var uniqueMesh = CreateSubmesh();
				uniqueMesh.name = "Submesh [" + _submeshCount.ToString() + "]";
				uniqueMesh.transform.SetParent(_root, false);
				mesh = uniqueMesh.mesh;
			}

			mesh.Clear();

			mesh.vertices = vertices;
			mesh.colors = colors;
			mesh.triangles = triangles;

			mesh.RecalculateBounds();

			++_submeshCount;
		}

		public void ClearSubmeshes()
		{
			if (_submeshCount > 0)
			{
				if (_root != null)
				{
					if (_submeshCache == null)
					{
						_submeshCache = new List<UniqueMesh>();
					}

					if (_submeshCount == 1)
					{
						_submeshCache.Add(_root.GetComponent<UniqueMesh>());
						_root.SetParent(null, false);
						_root.gameObject.SetActive(false);
						_root = null;
					}
					else
					{
						var submeshes = _root.GetComponentsInChildren<UniqueMesh>();
						foreach (var submesh in submeshes)
						{
							_submeshCache.Add(submesh);
							submesh.transform.SetParent(null, false);
							submesh.gameObject.SetActive(false);
						}
						_rootCache = _root;
						_root = null;
					}
				}

				_submeshCount = 0;
			}
		}

		public void ClearSubmeshCache()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.delayCall += () =>
			{
				if (this != null && gameObject != null)
				{
#endif
					if (_submeshCache != null)
					{
						foreach (var submesh in _submeshCache)
						{
							DestroyImmediate(submesh.gameObject);
						}
						_submeshCache = null;
					}

					if (_rootCache != null)
					{
						DestroyImmediate(_rootCache.gameObject);
						 _rootCache = null;
					}
#if UNITY_EDITOR
				}
			};
#endif
		}
	}
}

using UnityEngine;
using System.Collections.Generic;

namespace Experilous
{
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
	public class UniqueMesh : MonoBehaviour
	{
		private int _ownerId;
		private MeshFilter _meshFilter;
		private Mesh _mesh;

		public MeshFilter meshFilter
		{
			get
			{
				if (_meshFilter == null)
				{
					_meshFilter = gameObject.GetComponent<MeshFilter>();
				}
				return _meshFilter;
			}
		}

		public Mesh mesh
		{
			get
			{
				if (meshFilter.sharedMesh == null || gameObject.GetInstanceID() != _ownerId)
				{
					meshFilter.sharedMesh = _mesh = new Mesh();
					_ownerId = gameObject.GetInstanceID();
					_mesh.name = "Mesh [" + _ownerId + "]";
				}
				return _mesh;
			}
		}
	}
}

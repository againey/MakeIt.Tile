using UnityEngine;
using System;

namespace Experilous
{
	/// <summary>
	/// A collection of meshes, together with their positions and rotations relative to each other.
	/// </summary>
	public class MeshCollection : ScriptableObject
	{
		[Serializable] public struct OrientedMesh
		{
			public Mesh mesh;
			public Vector3 position;
			public Quaternion rotation;

			public OrientedMesh(Mesh mesh)
			{
				this.mesh = mesh;
				position = new Vector3(0f, 0f, 0f);
				rotation = Quaternion.identity;
			}

			public OrientedMesh(Mesh mesh, Vector3 position)
			{
				this.mesh = mesh;
				this.position = position;
				rotation = Quaternion.identity;
			}

			public OrientedMesh(Mesh mesh, Vector3 position, Quaternion rotation)
			{
				this.mesh = mesh;
				this.position = position;
				this.rotation = rotation;
			}

			public OrientedMesh(Mesh mesh, Quaternion rotation)
			{
				this.mesh = mesh;
				position = new Vector3(0f, 0f, 0f);
				this.rotation = rotation;
			}
		}

		public OrientedMesh[] meshes;

		public static MeshCollection Create(int meshCount)
		{
			var instance = CreateInstance<MeshCollection>();
			instance.meshes = new OrientedMesh[meshCount];
			return instance;
		}

		public static MeshCollection Create(int meshCount, string name)
		{
			var instance = Create(meshCount);
			instance.name = name;
			return instance;
		}
	}
}

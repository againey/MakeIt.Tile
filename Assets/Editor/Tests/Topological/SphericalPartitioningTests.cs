using UnityEngine;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Experilous.Topological;

public class SphericalPartitioningTests
{
	private void ManifoldFaceCenterIntersections(Manifold manifold, SphericalPartitioning partitioning)
	{
		foreach (var face in manifold.topology.faces)
		{
			var centroid = new Vector3();
			foreach (var edge in face.edges)
			{
				centroid += manifold.vertexPositions[edge.prevVertex];
			}
			centroid.Normalize();

			Assert.AreEqual(face, partitioning.Intersect(centroid));
		}
	}

	private void ManifoldFaceVertexWeightedIntersections(Manifold manifold, SphericalPartitioning partitioning)
	{
		foreach (var face in manifold.topology.faces)
		{
			foreach (var firstEdge in face.edges)
			{
				var centroid = manifold.vertexPositions[firstEdge.prevVertex] * 4;
				var edge = firstEdge.next;
				do
				{
					centroid += manifold.vertexPositions[edge.prevVertex];
					edge = edge.next;
				} while (edge != firstEdge);
				centroid.Normalize();

				Assert.AreEqual(face, partitioning.Intersect(centroid));
			}
		}
	}

	private void ManifoldFaceEdgeWeightedIntersections(Manifold manifold, SphericalPartitioning partitioning)
	{
		foreach (var face in manifold.topology.faces)
		{
			foreach (var firstEdge in face.edges)
			{
				var centroid = manifold.vertexPositions[firstEdge.prevVertex] * 4;
				var edge = firstEdge.next;
				centroid += manifold.vertexPositions[edge.prevVertex] * 4;
				edge = edge.next;
				do
				{
					centroid += manifold.vertexPositions[edge.prevVertex];
					edge = edge.next;
				} while (edge != firstEdge);
				centroid.Normalize();

				Assert.AreEqual(face, partitioning.Intersect(centroid));
			}
		}
	}

	private void ManifoldExternalRayIntersections(Manifold manifold, SphericalPartitioning partitioning, int randomSeed)
	{
		var centroids = new Vector3[manifold.topology.faces.Count];
		foreach (var face in manifold.topology.faces)
		{
			var centroid = new Vector3();
			foreach (var edge in face.edges)
			{
				centroid += manifold.vertexPositions[edge.prevVertex];
			}
			centroids[face] = centroid.normalized;
		}

		var random = new System.Random(randomSeed);

		foreach (var face0 in manifold.topology.faces)
		{
			foreach (var face1 in manifold.topology.faces)
			{
				if (face0 != face1)
				{
					var direction = (centroids[face1] - centroids[face0]).normalized;
					if (Vector3.Dot(centroids[face0], direction) < 0f)
					{
						var sphereCenter = new Vector3((float)random.NextDouble() * 4.0f - 2.0f, (float)random.NextDouble() * 4.0f - 2.0f, (float)random.NextDouble() * 4.0f - 2.0f);
						var sphereRadius = (float)random.NextDouble() + 0.5f;
						var distance = (float)random.NextDouble() * 3.0f + 0.5f;
						var origin = centroids[face0] * sphereRadius - direction * distance + sphereCenter;
						Topology.Face intersectedFace;
						Assert.IsTrue(partitioning.Intersect(new Ray(origin, direction), sphereCenter, sphereRadius, out intersectedFace));
						Assert.AreEqual(face0, intersectedFace);
					}
				}
			}
		}
	}

	private void ManifoldInternalRayIntersections(Manifold manifold, SphericalPartitioning partitioning, int randomSeed)
	{
		var centroids = new Vector3[manifold.topology.faces.Count];
		foreach (var face in manifold.topology.faces)
		{
			var centroid = new Vector3();
			foreach (var edge in face.edges)
			{
				centroid += manifold.vertexPositions[edge.prevVertex];
			}
			centroids[face] = centroid.normalized;
		}

		var random = new System.Random(randomSeed);

		foreach (var face0 in manifold.topology.faces)
		{
			foreach (var face1 in manifold.topology.faces)
			{
				if (face0 != face1)
				{
					var direction = (centroids[face1] - centroids[face0]).normalized;
					if (Vector3.Dot(centroids[face0], direction) < 0f)
					{
						var sphereCenter = new Vector3((float)random.NextDouble() * 4.0f - 2.0f, (float)random.NextDouble() * 4.0f - 2.0f, (float)random.NextDouble() * 4.0f - 2.0f);
						var sphereRadius = (float)random.NextDouble() + 0.5f;
						var origin = ((centroids[face1] - centroids[face0]) * 0.5f + centroids[face0]) * sphereRadius + sphereCenter;
						Topology.Face intersectedFace;
						Assert.IsTrue(partitioning.Intersect(new Ray(origin, direction), sphereCenter, sphereRadius, out intersectedFace));
						Assert.AreEqual(face1, intersectedFace);
					}
				}
			}
		}
	}

	private void ManifoldRecedingRayIntersections(Manifold manifold, SphericalPartitioning partitioning, int randomSeed)
	{
		var centroids = new Vector3[manifold.topology.faces.Count];
		foreach (var face in manifold.topology.faces)
		{
			var centroid = new Vector3();
			foreach (var edge in face.edges)
			{
				centroid += manifold.vertexPositions[edge.prevVertex];
			}
			centroids[face] = centroid.normalized;
		}

		var random = new System.Random(randomSeed);

		foreach (var face0 in manifold.topology.faces)
		{
			foreach (var face1 in manifold.topology.faces)
			{
				if (face0 != face1)
				{
					var direction = (centroids[face1] - centroids[face0]).normalized;
					if (Vector3.Dot(centroids[face0], direction) < 0f)
					{
						var sphereCenter = new Vector3((float)random.NextDouble() * 4.0f - 2.0f, (float)random.NextDouble() * 4.0f - 2.0f, (float)random.NextDouble() * 4.0f - 2.0f);
						var sphereRadius = (float)random.NextDouble() + 0.5f;
						var distance = (float)random.NextDouble() * 3.0f + 0.5f;
						var origin = centroids[face1] * sphereRadius + direction * distance + sphereCenter;
						Topology.Face intersectedFace;
						Assert.IsFalse(partitioning.Intersect(new Ray(origin, direction), sphereCenter, sphereRadius, out intersectedFace));
					}
				}
			}
		}
	}

	private void ManifoldFailedRayIntersections(Manifold manifold, SphericalPartitioning partitioning, int randomSeed, int iterations)
	{
		var random = new System.Random(randomSeed);

		while (iterations > 0)
		{
			var origin = new Vector3((float)random.NextDouble() * 16.0f - 8.0f, (float)random.NextDouble() * 16.0f - 8.0f, (float)random.NextDouble() * 16.0f - 8.0f);
			var sphereRadius = (float)random.NextDouble() + 0.5f;
			if (origin.magnitude > sphereRadius * 1.1f)
			{
				var lookAt = new Vector3((float)random.NextDouble() * 16.0f - 8.0f, (float)random.NextDouble() * 16.0f - 8.0f, (float)random.NextDouble() * 16.0f - 8.0f);
				if (origin.magnitude > sphereRadius * 1.1f)
				{
					var direction = (lookAt - origin).normalized;
					var cosine = Vector3.Dot(direction, origin);
					var originLength = origin.magnitude;
					var square = cosine * cosine - originLength * originLength + sphereRadius * sphereRadius;
					if (square < -0.0001f)
					{
						var sphereCenter = new Vector3((float)random.NextDouble() * 4.0f - 2.0f, (float)random.NextDouble() * 4.0f - 2.0f, (float)random.NextDouble() * 4.0f - 2.0f);
						Topology.Face intersectedFace;
						Assert.IsFalse(partitioning.Intersect(new Ray(origin + sphereCenter, direction), sphereCenter, sphereRadius, out intersectedFace));
						--iterations;
					}
				}
			}
		}
	}

	[Test]
	public void TetrahedronFaceCenterIntersections()
	{
		var manifold = SphericalManifold.CreateTetrahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldFaceCenterIntersections(manifold, partitioning);
	}

	[Test]
	public void HexahedronFaceCenterIntersections()
	{
		var manifold = SphericalManifold.CreateCube();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldFaceCenterIntersections(manifold, partitioning);
	}

	[Test]
	public void OctahedronFaceCenterIntersections()
	{
		var manifold = SphericalManifold.CreateOctahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldFaceCenterIntersections(manifold, partitioning);
	}

	[Test]
	public void DodecahedronFaceCenterIntersections()
	{
		var manifold = SphericalManifold.CreateDodecahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldFaceCenterIntersections(manifold, partitioning);
	}

	[Test]
	public void IcosahedronFaceCenterIntersections()
	{
		var manifold = SphericalManifold.CreateIcosahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldFaceCenterIntersections(manifold, partitioning);
	}

	[Test]
	public void TetrahedronFaceVertexWeightedIntersections()
	{
		var manifold = SphericalManifold.CreateTetrahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldFaceVertexWeightedIntersections(manifold, partitioning);
	}

	[Test]
	public void HexahedronFaceVertexWeightedIntersections()
	{
		var manifold = SphericalManifold.CreateCube();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldFaceVertexWeightedIntersections(manifold, partitioning);
	}

	[Test]
	public void OctahedronFaceVertexWeightedIntersections()
	{
		var manifold = SphericalManifold.CreateOctahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldFaceVertexWeightedIntersections(manifold, partitioning);
	}

	[Test]
	public void DodecahedronFaceVertexWeightedIntersections()
	{
		var manifold = SphericalManifold.CreateDodecahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldFaceVertexWeightedIntersections(manifold, partitioning);
	}

	[Test]
	public void IcosahedronFaceVertexWeightedIntersections()
	{
		var manifold = SphericalManifold.CreateIcosahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldFaceVertexWeightedIntersections(manifold, partitioning);
	}

	[Test]
	public void TetrahedronFaceEdgeWeightedIntersections()
	{
		var manifold = SphericalManifold.CreateTetrahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldFaceEdgeWeightedIntersections(manifold, partitioning);
	}

	[Test]
	public void HexahedronFaceEdgeWeightedIntersections()
	{
		var manifold = SphericalManifold.CreateCube();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldFaceEdgeWeightedIntersections(manifold, partitioning);
	}

	[Test]
	public void OctahedronFaceEdgeWeightedIntersections()
	{
		var manifold = SphericalManifold.CreateOctahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldFaceEdgeWeightedIntersections(manifold, partitioning);
	}

	[Test]
	public void DodecahedronFaceEdgeWeightedIntersections()
	{
		var manifold = SphericalManifold.CreateDodecahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldFaceEdgeWeightedIntersections(manifold, partitioning);
	}

	[Test]
	public void IcosahedronFaceEdgeWeightedIntersections()
	{
		var manifold = SphericalManifold.CreateIcosahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldFaceEdgeWeightedIntersections(manifold, partitioning);
	}

	[Test]
	public void TetrahedronExternalRayIntersections()
	{
		var manifold = SphericalManifold.CreateTetrahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldExternalRayIntersections(manifold, partitioning, 1);
	}

	[Test]
	public void HexahedronExternalRayIntersections()
	{
		var manifold = SphericalManifold.CreateCube();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldExternalRayIntersections(manifold, partitioning, 1);
	}

	[Test]
	public void OctahedronExternalRayIntersections()
	{
		var manifold = SphericalManifold.CreateOctahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldExternalRayIntersections(manifold, partitioning, 1);
	}

	[Test]
	public void DodecahedronExternalRayIntersections()
	{
		var manifold = SphericalManifold.CreateDodecahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldExternalRayIntersections(manifold, partitioning, 1);
	}

	[Test]
	public void IcosahedronExternalRayIntersections()
	{
		var manifold = SphericalManifold.CreateIcosahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldExternalRayIntersections(manifold, partitioning, 1);
	}

	[Test]
	public void TetrahedronInternalRayIntersections()
	{
		var manifold = SphericalManifold.CreateTetrahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldInternalRayIntersections(manifold, partitioning, 1);
	}

	[Test]
	public void HexahedronInternalRayIntersections()
	{
		var manifold = SphericalManifold.CreateCube();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldInternalRayIntersections(manifold, partitioning, 1);
	}

	[Test]
	public void OctahedronInternalRayIntersections()
	{
		var manifold = SphericalManifold.CreateOctahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldInternalRayIntersections(manifold, partitioning, 1);
	}

	[Test]
	public void DodecahedronInternalRayIntersections()
	{
		var manifold = SphericalManifold.CreateDodecahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldInternalRayIntersections(manifold, partitioning, 1);
	}

	[Test]
	public void IcosahedronInternalRayIntersections()
	{
		var manifold = SphericalManifold.CreateIcosahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldInternalRayIntersections(manifold, partitioning, 1);
	}

	[Test]
	public void TetrahedronRecedingRayIntersections()
	{
		var manifold = SphericalManifold.CreateTetrahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldRecedingRayIntersections(manifold, partitioning, 1);
	}

	[Test]
	public void HexahedronRecedingRayIntersections()
	{
		var manifold = SphericalManifold.CreateCube();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldRecedingRayIntersections(manifold, partitioning, 1);
	}

	[Test]
	public void OctahedronRecedingRayIntersections()
	{
		var manifold = SphericalManifold.CreateOctahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldRecedingRayIntersections(manifold, partitioning, 1);
	}

	[Test]
	public void DodecahedronRecedingRayIntersections()
	{
		var manifold = SphericalManifold.CreateDodecahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldRecedingRayIntersections(manifold, partitioning, 1);
	}

	[Test]
	public void IcosahedronRecedingRayIntersections()
	{
		var manifold = SphericalManifold.CreateIcosahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldRecedingRayIntersections(manifold, partitioning, 1);
	}

	[Test]
	public void TetrahedronFailedRayIntersections()
	{
		var manifold = SphericalManifold.CreateTetrahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldFailedRayIntersections(manifold, partitioning, 1, 64);
	}

	[Test]
	public void HexahedronFailedRayIntersections()
	{
		var manifold = SphericalManifold.CreateCube();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldFailedRayIntersections(manifold, partitioning, 1, 64);
	}

	[Test]
	public void OctahedronFailedRayIntersections()
	{
		var manifold = SphericalManifold.CreateOctahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldFailedRayIntersections(manifold, partitioning, 1, 64);
	}

	[Test]
	public void DodecahedronFailedRayIntersections()
	{
		var manifold = SphericalManifold.CreateDodecahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldFailedRayIntersections(manifold, partitioning, 1, 64);
	}

	[Test]
	public void IcosahedronFailedRayIntersections()
	{
		var manifold = SphericalManifold.CreateIcosahedron();
		var partitioning = new SphericalPartitioning(manifold);
		ManifoldFailedRayIntersections(manifold, partitioning, 1, 64);
	}
}

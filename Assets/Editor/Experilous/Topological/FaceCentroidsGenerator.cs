﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Experilous.Topological
{
	[AssetGenerator(typeof(TopologyGeneratorBundle), typeof(FaceAttributesCategory), "Face Centroids")]
	public class FaceCentroidsGenerator : AssetGenerator
	{
		public enum SurfaceType
		{
			Flat,
			Spherical,
		}

		public SurfaceType surfaceType;

		public TopologyGeneratedAsset topology;
		public Vector3VertexAttributeGeneratedAsset vertexPositions;

		public Vector3FaceAttributeGeneratedAsset faceCentroids;

		public static FaceCentroidsGenerator CreateDefaultInstance(AssetGeneratorBundle bundle, string name)
		{
			var generator = CreateInstance<FaceCentroidsGenerator>();
			generator.bundle = bundle;
			generator.name = name;
			generator.hideFlags = HideFlags.HideInHierarchy;
			return generator;
		}

		public override IEnumerable<GeneratedAsset> dependencies
		{
			get
			{
				if (topology != null) yield return topology;
				if (vertexPositions != null) yield return vertexPositions;
			}
		}

		public override IEnumerable<GeneratedAsset> outputs
		{
			get
			{
				if (faceCentroids == null) faceCentroids = Vector3FaceAttributeGeneratedAsset.CreateDefaultInstance(this, "Face Centroids");
				yield return faceCentroids;
			}
		}

		public override void ResetDependency(GeneratedAsset dependency)
		{
			if (dependency == null) throw new System.ArgumentNullException("dependency");
			if (!ResetMemberDependency(dependency, ref topology, ref vertexPositions))
			{
				throw new System.ArgumentException(string.Format("Generated asset \"{0}\" of type {1} is not a dependency of this face centroids generator.", dependency.name, dependency.GetType().Name), "dependency");
			}
		}

		public override void Generate(string location, string name)
		{
			var faceCentroids = Vector3FaceAttribute.CreateInstance(new Vector3[topology.generatedInstance.internalFaces.Count], "Face Centroids");
			switch (surfaceType)
			{
				case SurfaceType.Flat:
					FaceAttributeUtility.CalculateFaceCentroidsFromVertexPositions(topology.generatedInstance.internalFaces, vertexPositions.generatedInstance, faceCentroids);
					break;
				case SurfaceType.Spherical:
					FaceAttributeUtility.CalculateSphericalFaceCentroidsFromVertexPositions(topology.generatedInstance.internalFaces, vertexPositions.generatedInstance, 1f, faceCentroids);
					break;
				default:
					throw new System.NotImplementedException();
			}

			this.faceCentroids.SetGeneratedInstance(location, name, faceCentroids);
		}

		public override bool CanGenerate()
		{
			return vertexPositions != null;
		}
	}
}

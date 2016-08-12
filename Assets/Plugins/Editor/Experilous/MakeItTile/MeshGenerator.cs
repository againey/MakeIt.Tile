/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Experilous.MakeIt.Generate;

namespace Experilous.MakeIt.Tile
{
	[Generator(typeof(TopologyGeneratorCollection), "Unity Asset/Mesh")]
	public class MeshGenerator : Generator
	{
		public enum SourceType
		{
			InternalFaces,
			FaceGroupCollection,
			FaceGroup,
		}

		public enum Triangulation
		{
			Strip,
			Fan,
			Umbrella,
		}

		public SourceType sourceType;
		public Triangulation triangulation;
		public int ringDepth;
		public int maxVerticesPerSubmesh;
		public DynamicMesh.VertexAttributes vertexAttributes = DynamicMesh.VertexAttributes.Position | DynamicMesh.VertexAttributes.Normal | DynamicMesh.VertexAttributes.UV;

		public bool showMeshOutputs = false;

		[AutoSelect] public InputSlot topologyInputSlot;
		public InputSlot faceGroupCollectionInputSlot;
		public InputSlot faceGroupInputSlot;

		public InputSlot[] ringVertexPositionsInputSlots;
		public InputSlot[] ringVertexNormalsInputSlots;
		public InputSlot[] ringVertexColorsInputSlots;
		public InputSlot[] ringVertexColor32sInputSlots;
		public InputSlot[] ringVertexUV1sInputSlots;
		public InputSlot[] ringVertexUV2sInputSlots;
		public InputSlot[] ringVertexUV3sInputSlots;
		public InputSlot[] ringVertexUV4sInputSlots;
		public InputSlot[] ringVertexTangentsInputSlots;

		public InputSlot[] centerVertexPositionsInputSlots;
		public InputSlot[] centerVertexNormalsInputSlots;
		public InputSlot[] centerVertexColorsInputSlots;
		public InputSlot[] centerVertexColor32sInputSlots;
		public InputSlot[] centerVertexUV1sInputSlots;
		public InputSlot[] centerVertexUV2sInputSlots;
		public InputSlot[] centerVertexUV3sInputSlots;
		public InputSlot[] centerVertexUV4sInputSlots;
		public InputSlot[] centerVertexTangentsInputSlots;

		public OutputSlot dynamicMeshOutputSlot;
		public OutputSlot[] meshOutputSlots;

		protected override void Initialize()
		{
			// Inputs
			InputSlot.CreateOrResetRequired<Topology>(ref topologyInputSlot, this);
			InputSlot.CreateOrResetRequired<FaceGroupCollection>(ref faceGroupCollectionInputSlot, this);
			InputSlot.CreateOrResetRequired<FaceGroup>(ref faceGroupInputSlot, this);
			ringVertexPositionsInputSlots = new InputSlot[0];
			ringVertexNormalsInputSlots = new InputSlot[0];
			ringVertexColorsInputSlots = new InputSlot[0];
			ringVertexColor32sInputSlots = new InputSlot[0];
			ringVertexUV1sInputSlots = new InputSlot[0];
			ringVertexUV2sInputSlots = new InputSlot[0];
			ringVertexUV3sInputSlots = new InputSlot[0];
			ringVertexUV4sInputSlots = new InputSlot[0];
			ringVertexTangentsInputSlots = new InputSlot[0];
			centerVertexPositionsInputSlots = new InputSlot[0];
			centerVertexNormalsInputSlots = new InputSlot[0];
			centerVertexColorsInputSlots = new InputSlot[0];
			centerVertexColor32sInputSlots = new InputSlot[0];
			centerVertexUV1sInputSlots = new InputSlot[0];
			centerVertexUV2sInputSlots = new InputSlot[0];
			centerVertexUV3sInputSlots = new InputSlot[0];
			centerVertexUV4sInputSlots = new InputSlot[0];
			centerVertexTangentsInputSlots = new InputSlot[0];
			UpdateVertexAttributeInputSlots();

			// Fields
			sourceType = SourceType.InternalFaces;
			triangulation = Triangulation.Fan;
			ringDepth = 1;
			maxVerticesPerSubmesh = 65534;

			// Outputs
			OutputSlot.CreateOrReset<DynamicMesh>(ref dynamicMeshOutputSlot, this, "Dynamic Mesh");
			meshOutputSlots = new OutputSlot[0];
		}

		protected override void OnUpdate()
		{
			topologyInputSlot.isActive = (sourceType == SourceType.InternalFaces);
			faceGroupCollectionInputSlot.isActive = (sourceType == SourceType.FaceGroupCollection);
			faceGroupInputSlot.isActive = (sourceType == SourceType.FaceGroup);
		}

		public void UpdateVertexAttributeInputSlots()
		{
			UpdateVertexAttributeInputSlots<Vector3>((vertexAttributes & DynamicMesh.VertexAttributes.Position) != 0, ref ringVertexPositionsInputSlots, ref centerVertexPositionsInputSlots);
			UpdateVertexAttributeInputSlots<Vector3>((vertexAttributes & DynamicMesh.VertexAttributes.Normal) != 0, ref ringVertexNormalsInputSlots, ref centerVertexNormalsInputSlots);
			UpdateVertexAttributeInputSlots<Color>((vertexAttributes & DynamicMesh.VertexAttributes.Color) != 0, ref ringVertexColorsInputSlots, ref centerVertexColorsInputSlots);
			UpdateVertexAttributeInputSlots<Color32>((vertexAttributes & DynamicMesh.VertexAttributes.Color32) != 0, ref ringVertexColor32sInputSlots, ref centerVertexColor32sInputSlots);
			UpdateVertexAttributeInputSlots<Vector2>((vertexAttributes & DynamicMesh.VertexAttributes.UV1) != 0, ref ringVertexUV1sInputSlots, ref centerVertexUV1sInputSlots);
			UpdateVertexAttributeInputSlots<Vector2>((vertexAttributes & DynamicMesh.VertexAttributes.UV2) != 0, ref ringVertexUV2sInputSlots, ref centerVertexUV2sInputSlots);
			UpdateVertexAttributeInputSlots<Vector2>((vertexAttributes & DynamicMesh.VertexAttributes.UV3) != 0, ref ringVertexUV3sInputSlots, ref centerVertexUV3sInputSlots);
			UpdateVertexAttributeInputSlots<Vector2>((vertexAttributes & DynamicMesh.VertexAttributes.UV4) != 0, ref ringVertexUV4sInputSlots, ref centerVertexUV4sInputSlots);
			UpdateVertexAttributeInputSlots<Vector4>((vertexAttributes & DynamicMesh.VertexAttributes.Tangent) != 0, ref ringVertexTangentsInputSlots, ref centerVertexTangentsInputSlots);
		}

		private void UpdateVertexAttributeInputSlots<TAttributeValue>(bool use, ref InputSlot[] ringVertexAttributesInputSlots, ref InputSlot[] centerVertexAttributesInputSlots)
		{
			if (use == false)
			{
				if (ringVertexAttributesInputSlots.Length > 0)
				{
					for (int i = 0; i < ringVertexAttributesInputSlots.Length; ++i)
					{
						ringVertexAttributesInputSlots[i].Disconnect();
					}
					ringVertexAttributesInputSlots = new InputSlot[0];
				}
			}
			else
			{
				if (ringVertexAttributesInputSlots.Length != ringDepth)
				{
					var newArray = new InputSlot[ringDepth];
					System.Array.Copy(ringVertexAttributesInputSlots, newArray, Mathf.Min(ringVertexAttributesInputSlots.Length, ringDepth));
					for (int i = ringVertexAttributesInputSlots.Length; i < ringDepth; ++i)
					{
						newArray[i] = InputSlot.CreateRequired<IEdgeAttribute<TAttributeValue>>(this);
					}
					for (int i = ringDepth; i < ringVertexAttributesInputSlots.Length; ++i)
					{
						ringVertexAttributesInputSlots[i].Disconnect();
					}
					ringVertexAttributesInputSlots = newArray;
				}
			}

			if (use == false || triangulation != Triangulation.Umbrella)
			{
				if (centerVertexAttributesInputSlots.Length > 0)
				{
					centerVertexAttributesInputSlots[0].Disconnect();
					centerVertexAttributesInputSlots = new InputSlot[0];
				}
			}
			else
			{
				if (centerVertexAttributesInputSlots.Length == 0)
				{
					centerVertexAttributesInputSlots = new InputSlot[] { InputSlot.CreateRequired<IFaceAttribute<TAttributeValue>>(this) };
				}
			}
		}

		public override IEnumerable<InputSlot> inputs
		{
			get
			{
				yield return topologyInputSlot;
				yield return faceGroupCollectionInputSlot;
				yield return faceGroupInputSlot;

				foreach (var inputSlot in ringVertexPositionsInputSlots) yield return inputSlot;
				foreach (var inputSlot in centerVertexPositionsInputSlots) yield return inputSlot;
				foreach (var inputSlot in ringVertexNormalsInputSlots) yield return inputSlot;
				foreach (var inputSlot in centerVertexNormalsInputSlots) yield return inputSlot;
				foreach (var inputSlot in ringVertexColorsInputSlots) yield return inputSlot;
				foreach (var inputSlot in centerVertexColorsInputSlots) yield return inputSlot;
				foreach (var inputSlot in ringVertexColor32sInputSlots) yield return inputSlot;
				foreach (var inputSlot in centerVertexColor32sInputSlots) yield return inputSlot;
				foreach (var inputSlot in ringVertexUV1sInputSlots) yield return inputSlot;
				foreach (var inputSlot in centerVertexUV1sInputSlots) yield return inputSlot;
				foreach (var inputSlot in ringVertexUV2sInputSlots) yield return inputSlot;
				foreach (var inputSlot in centerVertexUV2sInputSlots) yield return inputSlot;
				foreach (var inputSlot in ringVertexUV3sInputSlots) yield return inputSlot;
				foreach (var inputSlot in centerVertexUV3sInputSlots) yield return inputSlot;
				foreach (var inputSlot in ringVertexUV4sInputSlots) yield return inputSlot;
				foreach (var inputSlot in centerVertexUV4sInputSlots) yield return inputSlot;
				foreach (var inputSlot in ringVertexTangentsInputSlots) yield return inputSlot;
				foreach (var inputSlot in centerVertexTangentsInputSlots) yield return inputSlot;
			}
		}

		public override IEnumerable<OutputSlot> outputs
		{
			get
			{
				if (dynamicMeshOutputSlot == null) dynamicMeshOutputSlot = OutputSlot.Create<DynamicMesh>(this, "Dynamic Mesh");
				yield return dynamicMeshOutputSlot;

				foreach (var meshOutputSlot in meshOutputSlots)
				{
					yield return meshOutputSlot;
				}
			}
		}

		public override IEnumerable<OutputSlot> visibleOutputs
		{
			get
			{
				yield return dynamicMeshOutputSlot;

				if (showMeshOutputs)
				{
					foreach (var meshOutputSlot in meshOutputSlots)
					{
						yield return meshOutputSlot;
					}
				}
			}
		}

		public override IEnumerable<InternalSlotConnection> internalConnections
		{
			get
			{
				foreach (var meshOutputSlot in meshOutputSlots)
				{
					yield return dynamicMeshOutputSlot.Uses(meshOutputSlot);
				}
			}
		}

		private class VertexAttributeSources
		{
			public IEdgeAttribute<Vector3>[] ringPositions;
			public IEdgeAttribute<Vector3>[] ringNormals;
			public IEdgeAttribute<Color>[] ringColors;
			public IEdgeAttribute<Color32>[] ringColor32s;
			public IEdgeAttribute<Vector2>[] ringUV1s;
			public IEdgeAttribute<Vector2>[] ringUV2s;
			public IEdgeAttribute<Vector2>[] ringUV3s;
			public IEdgeAttribute<Vector2>[] ringUV4s;
			public IEdgeAttribute<Vector4>[] ringTangents;

			public IFaceAttribute<Vector3> centerPositions;
			public IFaceAttribute<Vector3> centerNormals;
			public IFaceAttribute<Color> centerColors;
			public IFaceAttribute<Color32> centerColor32s;
			public IFaceAttribute<Vector2> centerUV1s;
			public IFaceAttribute<Vector2> centerUV2s;
			public IFaceAttribute<Vector2> centerUV3s;
			public IFaceAttribute<Vector2> centerUV4s;
			public IFaceAttribute<Vector4> centerTangents;

			public static IEdgeAttribute<TAttributeValue>[] GetRingAttributes<TAttributeValue>(Topology topology, InputSlot[] inputSlots)
			{
				if (inputSlots.Length == 0) return null;
				var array = new IEdgeAttribute<TAttributeValue>[inputSlots.Length];
				for (int i = 0; i < inputSlots.Length; ++i)
				{
					var attribute = inputSlots[i].GetAsset<IEdgeAttribute<TAttributeValue>>();
					if (attribute is IFaceAttribute<TAttributeValue>)
					{
						attribute = new TwinEdgeAttributeWrapper<TAttributeValue>(topology ,attribute);
					}
					array[i] = attribute;
				}
				return array;
			}

			public static IFaceAttribute<TAttributeValue> GetCenterAttributes<TAttributeValue>(InputSlot[] inputSlots)
			{
				if (inputSlots.Length == 0) return null;
				return inputSlots[0].GetAsset<IFaceAttribute<TAttributeValue>>();
			}
		}

		public override IEnumerator BeginGeneration()
		{
			var topology = topologyInputSlot.GetAsset<Topology>();

			var vertexAttributeSources = new VertexAttributeSources();
			vertexAttributeSources.ringPositions = VertexAttributeSources.GetRingAttributes<Vector3>(topology, ringVertexPositionsInputSlots);
			vertexAttributeSources.ringNormals = VertexAttributeSources.GetRingAttributes<Vector3>(topology, ringVertexNormalsInputSlots);
			vertexAttributeSources.ringColors = VertexAttributeSources.GetRingAttributes<Color>(topology, ringVertexColorsInputSlots);
			vertexAttributeSources.ringColor32s = VertexAttributeSources.GetRingAttributes<Color32>(topology, ringVertexColor32sInputSlots);
			vertexAttributeSources.ringUV1s = VertexAttributeSources.GetRingAttributes<Vector2>(topology, ringVertexUV1sInputSlots);
			vertexAttributeSources.ringUV2s = VertexAttributeSources.GetRingAttributes<Vector2>(topology, ringVertexUV2sInputSlots);
			vertexAttributeSources.ringUV3s = VertexAttributeSources.GetRingAttributes<Vector2>(topology, ringVertexUV3sInputSlots);
			vertexAttributeSources.ringUV4s = VertexAttributeSources.GetRingAttributes<Vector2>(topology, ringVertexUV4sInputSlots);
			vertexAttributeSources.ringTangents = VertexAttributeSources.GetRingAttributes<Vector4>(topology, ringVertexTangentsInputSlots);
			vertexAttributeSources.centerPositions = VertexAttributeSources.GetCenterAttributes<Vector3>(centerVertexPositionsInputSlots);
			vertexAttributeSources.centerNormals = VertexAttributeSources.GetCenterAttributes<Vector3>(centerVertexNormalsInputSlots);
			vertexAttributeSources.centerColors = VertexAttributeSources.GetCenterAttributes<Color>(centerVertexColorsInputSlots);
			vertexAttributeSources.centerColor32s = VertexAttributeSources.GetCenterAttributes<Color32>(centerVertexColor32sInputSlots);
			vertexAttributeSources.centerUV1s = VertexAttributeSources.GetCenterAttributes<Vector2>(centerVertexUV1sInputSlots);
			vertexAttributeSources.centerUV2s = VertexAttributeSources.GetCenterAttributes<Vector2>(centerVertexUV2sInputSlots);
			vertexAttributeSources.centerUV3s = VertexAttributeSources.GetCenterAttributes<Vector2>(centerVertexUV3sInputSlots);
			vertexAttributeSources.centerUV4s = VertexAttributeSources.GetCenterAttributes<Vector2>(centerVertexUV4sInputSlots);
			vertexAttributeSources.centerTangents = VertexAttributeSources.GetCenterAttributes<Vector4>(centerVertexTangentsInputSlots);

			System.Action<Topology.FaceEdge, DynamicMesh.IIndexedVertexAttributes> setRingVertexAttributes =
				(Topology.FaceEdge edge, DynamicMesh.IIndexedVertexAttributes va) =>
				{
					for (int i = 0; i < ringDepth; ++i)
					{
						if (vertexAttributeSources.ringPositions != null) va.position = vertexAttributeSources.ringPositions[i][edge];
						if (vertexAttributeSources.ringNormals != null) va.normal = vertexAttributeSources.ringNormals[i][edge];
						if (vertexAttributeSources.ringColors != null) va.color = vertexAttributeSources.ringColors[i][edge];
						if (vertexAttributeSources.ringColor32s != null) va.color32 = vertexAttributeSources.ringColor32s[i][edge];
						if (vertexAttributeSources.ringUV1s != null) va.uv1 = vertexAttributeSources.ringUV1s[i][edge];
						if (vertexAttributeSources.ringUV2s != null) va.uv2 = vertexAttributeSources.ringUV2s[i][edge];
						if (vertexAttributeSources.ringUV3s != null) va.uv3 = vertexAttributeSources.ringUV3s[i][edge];
						if (vertexAttributeSources.ringUV4s != null) va.uv4 = vertexAttributeSources.ringUV4s[i][edge];
						if (vertexAttributeSources.ringTangents != null) va.tangent = vertexAttributeSources.ringTangents[i][edge];
						va.Advance();
					}
				};

			System.Action<Topology.Face, DynamicMesh.IIndexedVertexAttributes> setCenterVertexAttributes =
				(Topology.Face face, DynamicMesh.IIndexedVertexAttributes va) =>
				{
					if (vertexAttributeSources.centerPositions != null) va.position = vertexAttributeSources.centerPositions[face];
					if (vertexAttributeSources.centerNormals != null) va.normal = vertexAttributeSources.centerNormals[face];
					if (vertexAttributeSources.centerColors != null) va.color = vertexAttributeSources.centerColors[face];
					if (vertexAttributeSources.centerColor32s != null) va.color32 = vertexAttributeSources.centerColor32s[face];
					if (vertexAttributeSources.centerUV1s != null) va.uv1 = vertexAttributeSources.centerUV1s[face];
					if (vertexAttributeSources.centerUV2s != null) va.uv2 = vertexAttributeSources.centerUV2s[face];
					if (vertexAttributeSources.centerUV3s != null) va.uv3 = vertexAttributeSources.centerUV3s[face];
					if (vertexAttributeSources.centerUV4s != null) va.uv4 = vertexAttributeSources.centerUV4s[face];
					if (vertexAttributeSources.centerTangents != null) va.tangent = vertexAttributeSources.centerTangents[face];
					va.Advance();
				};

			DynamicMesh.ITriangulation triangulationInstance = null;

			switch (triangulation)
			{
				case Triangulation.Strip:
					triangulationInstance = new SeparatedFacesStripTriangulation(ringDepth, setRingVertexAttributes);
					break;
				case Triangulation.Fan:
					triangulationInstance = new SeparatedFacesFanTriangulation(ringDepth, setRingVertexAttributes);
					break;
				case Triangulation.Umbrella:
					triangulationInstance = new SeparatedFacesUmbrellaTriangulation(ringDepth, setRingVertexAttributes, setCenterVertexAttributes);
					break;
				default:
					throw new System.NotImplementedException();
			}

			DynamicMesh dynamicMesh;

			switch (sourceType)
			{
				case SourceType.InternalFaces:
					dynamicMesh = DynamicMesh.Create(topology.internalFaces, vertexAttributes, triangulationInstance, maxVerticesPerSubmesh);
					break;
				case SourceType.FaceGroupCollection:
					dynamicMesh = DynamicMesh.Create(faceGroupCollectionInputSlot.GetAsset<FaceGroupCollection>(), vertexAttributes, triangulationInstance, maxVerticesPerSubmesh);
					break;
				case SourceType.FaceGroup:
					dynamicMesh = DynamicMesh.Create(faceGroupInputSlot.GetAsset<FaceGroup>(), vertexAttributes, triangulationInstance, maxVerticesPerSubmesh);
					break;
				default:
					throw new System.NotImplementedException();
			}

			if (meshOutputSlots.Length != dynamicMesh.submeshCount)
			{
				var newMeshes = new OutputSlot[dynamicMesh.submeshCount];

				System.Array.Copy(meshOutputSlots, newMeshes, Mathf.Min(meshOutputSlots.Length, newMeshes.Length));
				for (var i = meshOutputSlots.Length; i < newMeshes.Length; ++i)
				{
					newMeshes[i] = OutputSlot.CreateGrouped<Mesh>(this, "Mesh", "Meshes");
				}

				meshOutputSlots = newMeshes;
			}

			if (meshOutputSlots.Length == 1)
			{
				meshOutputSlots[0].name = "Mesh";
			}
			else if (meshOutputSlots.Length > 1)
			{
				for (int i = 0; i < meshOutputSlots.Length; ++i)
				{
					meshOutputSlots[i].name = string.Format("Submesh {0}", i);
				}
			}

			for (int i = 0; i < meshOutputSlots.Length; ++i)
			{
				meshOutputSlots[i].SetAsset(dynamicMesh.GetSubmesh(i));
				dynamicMesh.ReplaceSubmesh(i, meshOutputSlots[i].GetAsset<Mesh>());
			}

			dynamicMeshOutputSlot.SetAsset(dynamicMesh);

			yield break;
		}

		public override float estimatedGenerationTime
		{
			get
			{
				return 0.2f;
			}
		}
	}
}

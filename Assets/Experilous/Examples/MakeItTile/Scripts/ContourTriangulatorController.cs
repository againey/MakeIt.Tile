using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Experilous.Topologies;

namespace Experilous.Examples.MakeItTile
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class ContourTriangulatorController : MonoBehaviour
	{
		public DynamicGraph siteGraph;

		[Serializable] public class GraphNodePositionList : GraphNodeDataListWrapper<Vector3>
		{
			public void CopyTo(GraphNodePositionList other)
			{
				other.Clear();
				foreach (var position in this)
				{
					other.Add(position);
				}
			}
		}

		public GraphNodePositionList pointSitePositions;

		public float maxCurvaturePerSegment = 0.25f;
		public bool twinEdge = false;
		[Range(0.1f, 10f)]
		public float contourDistanceScale = 1f;

		public float[] contourOffsets;
		public Color[] contourColors;

		protected void OnEnable()
		{
			if (contourOffsets == null || contourOffsets.Length < 2)
			{
				contourOffsets = new float[] { 0f, 1f };
			}

			if (contourColors == null || contourColors.Length < 2)
			{
				contourColors = new Color[] { Color.white, Color.black };
			}

			if (contourOffsets.Length != contourColors.Length)
			{
				contourOffsets = new float[] { 0f, 1f };
				contourColors = new Color[] { Color.white, Color.black };
			}
		}
	}
}

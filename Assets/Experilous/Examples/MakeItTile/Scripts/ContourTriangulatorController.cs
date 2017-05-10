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

		[Serializable] public class GraphNodePositionList : GraphNodeDataListWrapper<Vector3> { }

		public GraphNodePositionList pointSitePositions;

		public float maxCurvaturPerSegment = 0.25f;

		public float[] contourDistances;
		public Color[] contourColors;
	}
}

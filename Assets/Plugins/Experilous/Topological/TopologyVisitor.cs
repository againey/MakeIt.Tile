/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Experilous.Randomization;

namespace Experilous.Topological
{
	public abstract class TopologyVisitor
	{
		protected Topology _topology;

		protected int _depth;

		protected bool _ignore;
		protected bool _break;

		public int depth { get { return _depth; } }

		public void Ignore()
		{
			_ignore = true;
		}

		public void Break()
		{
			_break = true;
		}

		protected TopologyVisitor(Topology topology)
		{
			_topology = topology;
		}

		#region Arbitrary Order Visitation

		#region Vertices

		public static void VisitVertices(Topology.Vertex rootVertex, VertexVisitor.VisitDelegate visitDelegate)
		{
			VertexVisitor.VisitAll(rootVertex, new Containers.Stack<VertexVisitor.QueueItem>(), visitDelegate);
		}

		public static void VisitVertices(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor.VisitDelegate visitDelegate)
		{
			VertexVisitor.VisitAll(rootVertices, new Containers.Stack<VertexVisitor.QueueItem>(), visitDelegate);
		}

		public static void VisitVertices<TDistance>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertex, new Containers.Stack<VertexVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		public static void VisitVertices<TDistance>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertices, new Containers.Stack<VertexVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		public static void VisitVertices(Topology.VertexEdge rootEdge, VertexEdgeVisitor.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor.VisitAll(rootEdge, new Containers.Stack<VertexEdgeVisitor.QueueItem>(), visitDelegate);
		}

		public static void VisitVertices(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor.VisitAll(rootEdges, new Containers.Stack<VertexEdgeVisitor.QueueItem>(), visitDelegate);
		}

		public static void VisitVertices<TDistance>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdge, new Containers.Stack<VertexEdgeVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		public static void VisitVertices<TDistance>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdges, new Containers.Stack<VertexEdgeVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		#endregion

		#region Faces

		public static void VisitFaces(Topology.Face rootFace, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.VisitAll(rootFace, new Containers.Stack<FaceVisitor.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.VisitAll(rootFaces, new Containers.Stack<FaceVisitor.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces<TDistance>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFace, new Containers.Stack<FaceVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFaces, new Containers.Stack<FaceVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.VisitAll(rootEdge, new Containers.Stack<FaceEdgeVisitor.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.VisitAll(rootEdges, new Containers.Stack<FaceEdgeVisitor.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdge, new Containers.Stack<FaceEdgeVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdges, new Containers.Stack<FaceEdgeVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		#endregion

		#endregion

		#region Breadth First Visitation

		#region Vertices

		public static void VisitVerticesInBreadthFirstOrder(Topology.Vertex rootVertex, VertexVisitor.VisitDelegate visitDelegate)
		{
			VertexVisitor.VisitAll(rootVertex, new Containers.PriorityQueue<VertexVisitor.QueueItem>(VertexVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitVerticesInBreadthFirstOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor.VisitDelegate visitDelegate)
		{
			VertexVisitor.VisitAll(rootVertices, new Containers.PriorityQueue<VertexVisitor.QueueItem>(VertexVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitVerticesInBreadthFirstOrder<TDistance>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertex, new Containers.PriorityQueue<VertexVisitor<TDistance>.QueueItem>(VertexVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitVerticesInBreadthFirstOrder<TDistance>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertices, new Containers.PriorityQueue<VertexVisitor<TDistance>.QueueItem>(VertexVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitVerticesInBreadthFirstOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor.VisitAll(rootEdge, new Containers.PriorityQueue<VertexEdgeVisitor.QueueItem>(VertexEdgeVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitVerticesInBreadthFirstOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor.VisitAll(rootEdges, new Containers.PriorityQueue<VertexEdgeVisitor.QueueItem>(VertexEdgeVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitVerticesInBreadthFirstOrder<TDistance>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdge, new Containers.PriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(VertexEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitVerticesInBreadthFirstOrder<TDistance>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdges, new Containers.PriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(VertexEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		#endregion

		#region Faces

		public static void VisitFacesInBreadthFirstOrder(Topology.Face rootFace, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.VisitAll(rootFace, new Containers.PriorityQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.VisitAll(rootFaces, new Containers.PriorityQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder<TDistance>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFace, new Containers.PriorityQueue<FaceVisitor<TDistance>.QueueItem>(FaceVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFaces, new Containers.PriorityQueue<FaceVisitor<TDistance>.QueueItem>(FaceVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.VisitAll(rootEdge, new Containers.PriorityQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.VisitAll(rootEdges, new Containers.PriorityQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdge, new Containers.PriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdges, new Containers.PriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		#endregion

		#endregion

		#region Depth First Visitation

		#region Vertices

		public static void VisitVerticesInDepthFirstOrder(Topology.Vertex rootVertex, VertexVisitor.VisitDelegate visitDelegate)
		{
			VertexVisitor.VisitAll(rootVertex, new Containers.PriorityQueue<VertexVisitor.QueueItem>(VertexVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitVerticesInDepthFirstOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor.VisitDelegate visitDelegate)
		{
			VertexVisitor.VisitAll(rootVertices, new Containers.PriorityQueue<VertexVisitor.QueueItem>(VertexVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitVerticesInDepthFirstOrder<TDistance>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertex, new Containers.PriorityQueue<VertexVisitor<TDistance>.QueueItem>(VertexVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitVerticesInDepthFirstOrder<TDistance>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertices, new Containers.PriorityQueue<VertexVisitor<TDistance>.QueueItem>(VertexVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitVerticesInDepthFirstOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor.VisitAll(rootEdge, new Containers.PriorityQueue<VertexEdgeVisitor.QueueItem>(VertexEdgeVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitVerticesInDepthFirstOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor.VisitAll(rootEdges, new Containers.PriorityQueue<VertexEdgeVisitor.QueueItem>(VertexEdgeVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitVerticesInDepthFirstOrder<TDistance>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdge, new Containers.PriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(VertexEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitVerticesInDepthFirstOrder<TDistance>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdges, new Containers.PriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(VertexEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		#endregion

		#region Faces

		public static void VisitFacesInDepthFirstOrder(Topology.Face rootFace, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.VisitAll(rootFace, new Containers.PriorityQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.VisitAll(rootFaces, new Containers.PriorityQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder<TDistance>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFace, new Containers.PriorityQueue<FaceVisitor<TDistance>.QueueItem>(FaceVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFaces, new Containers.PriorityQueue<FaceVisitor<TDistance>.QueueItem>(FaceVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.VisitAll(rootEdge, new Containers.PriorityQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.VisitAll(rootEdges, new Containers.PriorityQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdge, new Containers.PriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdges, new Containers.PriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		#endregion

		#endregion

		#region Nearest Distance Visitation

		#region Distance Comparisons

		#region Vertices

		private static bool AreOrderedByNearestDistance(VertexVisitor<int>.QueueItem lhs, VertexVisitor<int>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByNearestDistance(VertexEdgeVisitor<int>.QueueItem lhs, VertexEdgeVisitor<int>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByNearestDistance(VertexVisitor<uint>.QueueItem lhs, VertexVisitor<uint>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByNearestDistance(VertexEdgeVisitor<uint>.QueueItem lhs, VertexEdgeVisitor<uint>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByNearestDistance(VertexVisitor<float>.QueueItem lhs, VertexVisitor<float>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByNearestDistance(VertexEdgeVisitor<float>.QueueItem lhs, VertexEdgeVisitor<float>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByNearestDistance(VertexVisitor<double>.QueueItem lhs, VertexVisitor<double>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByNearestDistance(VertexEdgeVisitor<double>.QueueItem lhs, VertexEdgeVisitor<double>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		#endregion

		#region Faces

		private static bool AreOrderedByNearestDistance(FaceVisitor<int>.QueueItem lhs, FaceVisitor<int>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByNearestDistance(FaceEdgeVisitor<int>.QueueItem lhs, FaceEdgeVisitor<int>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByNearestDistance(FaceVisitor<uint>.QueueItem lhs, FaceVisitor<uint>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByNearestDistance(FaceEdgeVisitor<uint>.QueueItem lhs, FaceEdgeVisitor<uint>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByNearestDistance(FaceVisitor<float>.QueueItem lhs, FaceVisitor<float>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByNearestDistance(FaceEdgeVisitor<float>.QueueItem lhs, FaceEdgeVisitor<float>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByNearestDistance(FaceVisitor<double>.QueueItem lhs, FaceVisitor<double>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByNearestDistance(FaceEdgeVisitor<double>.QueueItem lhs, FaceEdgeVisitor<double>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		#endregion

		#endregion

		#region Vertices

		#region Int32

		public static void VisitVerticesInNearestDistanceOrder(Topology.Vertex rootVertex, VertexVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexVisitor<int>.VisitAll(rootVertex, new Containers.PriorityQueue<VertexVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexVisitor<int>.VisitAll(rootVertices, new Containers.PriorityQueue<VertexVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<int>.VisitAll(rootEdge, new Containers.PriorityQueue<VertexEdgeVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<int>.VisitAll(rootEdges, new Containers.PriorityQueue<VertexEdgeVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		#endregion

		#region UInt32

		public static void VisitVerticesInNearestDistanceOrder(Topology.Vertex rootVertex, VertexVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexVisitor<uint>.VisitAll(rootVertex, new Containers.PriorityQueue<VertexVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexVisitor<uint>.VisitAll(rootVertices, new Containers.PriorityQueue<VertexVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<uint>.VisitAll(rootEdge, new Containers.PriorityQueue<VertexEdgeVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<uint>.VisitAll(rootEdges, new Containers.PriorityQueue<VertexEdgeVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		#endregion

		#region Single

		public static void VisitVerticesInNearestDistanceOrder(Topology.Vertex rootVertex, VertexVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexVisitor<float>.VisitAll(rootVertex, new Containers.PriorityQueue<VertexVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexVisitor<float>.VisitAll(rootVertices, new Containers.PriorityQueue<VertexVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<float>.VisitAll(rootEdge, new Containers.PriorityQueue<VertexEdgeVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<float>.VisitAll(rootEdges, new Containers.PriorityQueue<VertexEdgeVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		#endregion

		#region Double

		public static void VisitVerticesInNearestDistanceOrder(Topology.Vertex rootVertex, VertexVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexVisitor<double>.VisitAll(rootVertex, new Containers.PriorityQueue<VertexVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexVisitor<double>.VisitAll(rootVertices, new Containers.PriorityQueue<VertexVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<double>.VisitAll(rootEdge, new Containers.PriorityQueue<VertexEdgeVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<double>.VisitAll(rootEdges, new Containers.PriorityQueue<VertexEdgeVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		#endregion

		#endregion

		#region Faces

		#region Int32

		public static void VisitFacesInNearestDistanceOrder(Topology.Face rootFace, FaceVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceVisitor<int>.VisitAll(rootFace, new Containers.PriorityQueue<FaceVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceVisitor<int>.VisitAll(rootFaces, new Containers.PriorityQueue<FaceVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<int>.VisitAll(rootEdge, new Containers.PriorityQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<int>.VisitAll(rootEdges, new Containers.PriorityQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		#endregion

		#region UInt32

		public static void VisitFacesInNearestDistanceOrder(Topology.Face rootFace, FaceVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceVisitor<uint>.VisitAll(rootFace, new Containers.PriorityQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceVisitor<uint>.VisitAll(rootFaces, new Containers.PriorityQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<uint>.VisitAll(rootEdge, new Containers.PriorityQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<uint>.VisitAll(rootEdges, new Containers.PriorityQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		#endregion

		#region Single

		public static void VisitFacesInNearestDistanceOrder(Topology.Face rootFace, FaceVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceVisitor<float>.VisitAll(rootFace, new Containers.PriorityQueue<FaceVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceVisitor<float>.VisitAll(rootFaces, new Containers.PriorityQueue<FaceVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<float>.VisitAll(rootEdge, new Containers.PriorityQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<float>.VisitAll(rootEdges, new Containers.PriorityQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		#endregion

		#region Double

		public static void VisitFacesInNearestDistanceOrder(Topology.Face rootFace, FaceVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceVisitor<double>.VisitAll(rootFace, new Containers.PriorityQueue<FaceVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceVisitor<double>.VisitAll(rootFaces, new Containers.PriorityQueue<FaceVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<double>.VisitAll(rootEdge, new Containers.PriorityQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<double>.VisitAll(rootEdges, new Containers.PriorityQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		#endregion

		#endregion

		#endregion

		#region Farthest Distance Visitation

		#region Distance Comparisons

		#region Vertices

		private static bool AreOrderedByFarthestDistance(VertexVisitor<int>.QueueItem lhs, VertexVisitor<int>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByFarthestDistance(VertexEdgeVisitor<int>.QueueItem lhs, VertexEdgeVisitor<int>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByFarthestDistance(VertexVisitor<uint>.QueueItem lhs, VertexVisitor<uint>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByFarthestDistance(VertexEdgeVisitor<uint>.QueueItem lhs, VertexEdgeVisitor<uint>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByFarthestDistance(VertexVisitor<float>.QueueItem lhs, VertexVisitor<float>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByFarthestDistance(VertexEdgeVisitor<float>.QueueItem lhs, VertexEdgeVisitor<float>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByFarthestDistance(VertexVisitor<double>.QueueItem lhs, VertexVisitor<double>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByFarthestDistance(VertexEdgeVisitor<double>.QueueItem lhs, VertexEdgeVisitor<double>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		#endregion

		#region Faces

		private static bool AreOrderedByFarthestDistance(FaceVisitor<int>.QueueItem lhs, FaceVisitor<int>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByFarthestDistance(FaceEdgeVisitor<int>.QueueItem lhs, FaceEdgeVisitor<int>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByFarthestDistance(FaceVisitor<uint>.QueueItem lhs, FaceVisitor<uint>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByFarthestDistance(FaceEdgeVisitor<uint>.QueueItem lhs, FaceEdgeVisitor<uint>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByFarthestDistance(FaceVisitor<float>.QueueItem lhs, FaceVisitor<float>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByFarthestDistance(FaceEdgeVisitor<float>.QueueItem lhs, FaceEdgeVisitor<float>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByFarthestDistance(FaceVisitor<double>.QueueItem lhs, FaceVisitor<double>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByFarthestDistance(FaceEdgeVisitor<double>.QueueItem lhs, FaceEdgeVisitor<double>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		#endregion

		#endregion

		#region Vertices

		#region Int32

		public static void VisitVerticesInFarthestDistanceOrder(Topology.Vertex rootVertex, VertexVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexVisitor<int>.VisitAll(rootVertex, new Containers.PriorityQueue<VertexVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexVisitor<int>.VisitAll(rootVertices, new Containers.PriorityQueue<VertexVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<int>.VisitAll(rootEdge, new Containers.PriorityQueue<VertexEdgeVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<int>.VisitAll(rootEdges, new Containers.PriorityQueue<VertexEdgeVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		#endregion

		#region UInt32

		public static void VisitVerticesInFarthestDistanceOrder(Topology.Vertex rootVertex, VertexVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexVisitor<uint>.VisitAll(rootVertex, new Containers.PriorityQueue<VertexVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexVisitor<uint>.VisitAll(rootVertices, new Containers.PriorityQueue<VertexVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<uint>.VisitAll(rootEdge, new Containers.PriorityQueue<VertexEdgeVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<uint>.VisitAll(rootEdges, new Containers.PriorityQueue<VertexEdgeVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		#endregion

		#region Single

		public static void VisitVerticesInFarthestDistanceOrder(Topology.Vertex rootVertex, VertexVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexVisitor<float>.VisitAll(rootVertex, new Containers.PriorityQueue<VertexVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexVisitor<float>.VisitAll(rootVertices, new Containers.PriorityQueue<VertexVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<float>.VisitAll(rootEdge, new Containers.PriorityQueue<VertexEdgeVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<float>.VisitAll(rootEdges, new Containers.PriorityQueue<VertexEdgeVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		#endregion

		#region Double

		public static void VisitVerticesInFarthestDistanceOrder(Topology.Vertex rootVertex, VertexVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexVisitor<double>.VisitAll(rootVertex, new Containers.PriorityQueue<VertexVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexVisitor<double>.VisitAll(rootVertices, new Containers.PriorityQueue<VertexVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<double>.VisitAll(rootEdge, new Containers.PriorityQueue<VertexEdgeVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<double>.VisitAll(rootEdges, new Containers.PriorityQueue<VertexEdgeVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		#endregion

		#endregion

		#region Faces

		#region Int32

		public static void VisitFacesInFarthestDistanceOrder(Topology.Face rootFace, FaceVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceVisitor<int>.VisitAll(rootFace, new Containers.PriorityQueue<FaceVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceVisitor<int>.VisitAll(rootFaces, new Containers.PriorityQueue<FaceVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<int>.VisitAll(rootEdge, new Containers.PriorityQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<int>.VisitAll(rootEdges, new Containers.PriorityQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		#endregion

		#region UInt32

		public static void VisitFacesInFarthestDistanceOrder(Topology.Face rootFace, FaceVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceVisitor<uint>.VisitAll(rootFace, new Containers.PriorityQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceVisitor<uint>.VisitAll(rootFaces, new Containers.PriorityQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<uint>.VisitAll(rootEdge, new Containers.PriorityQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<uint>.VisitAll(rootEdges, new Containers.PriorityQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		#endregion

		#region Single

		public static void VisitFacesInFarthestDistanceOrder(Topology.Face rootFace, FaceVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceVisitor<float>.VisitAll(rootFace, new Containers.PriorityQueue<FaceVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceVisitor<float>.VisitAll(rootFaces, new Containers.PriorityQueue<FaceVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<float>.VisitAll(rootEdge, new Containers.PriorityQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<float>.VisitAll(rootEdges, new Containers.PriorityQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		#endregion

		#region Double

		public static void VisitFacesInFarthestDistanceOrder(Topology.Face rootFace, FaceVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceVisitor<double>.VisitAll(rootFace, new Containers.PriorityQueue<FaceVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceVisitor<double>.VisitAll(rootFaces, new Containers.PriorityQueue<FaceVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<double>.VisitAll(rootEdge, new Containers.PriorityQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<double>.VisitAll(rootEdges, new Containers.PriorityQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		#endregion

		#endregion

		#endregion

		#region Delegate Order Visitation

		#region Vertex

		public static void VisitVerticesInOrder<TDistance>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate visitDelegate, Containers.PriorityQueue<VertexVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertex, new Containers.PriorityQueue<VertexVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		public static void VisitVerticesInOrder<TDistance>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate visitDelegate, Containers.PriorityQueue<VertexVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertices, new Containers.PriorityQueue<VertexVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		public static void VisitVerticesInOrder<TDistance>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate, Containers.PriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdge, new Containers.PriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		public static void VisitVerticesInOrder<TDistance>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate, Containers.PriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdges, new Containers.PriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		#endregion

		#region Face

		public static void VisitFacesInOrder<TDistance>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate visitDelegate, Containers.PriorityQueue<FaceVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFace, new Containers.PriorityQueue<FaceVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		public static void VisitFacesInOrder<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate visitDelegate, Containers.PriorityQueue<FaceVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFaces, new Containers.PriorityQueue<FaceVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		public static void VisitFacesInOrder<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate, Containers.PriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdge, new Containers.PriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		public static void VisitFacesInOrder<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate, Containers.PriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdges, new Containers.PriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		#endregion

		#endregion

		#region Random Order Visitation

		#region Vertices

		public static void VisitVerticesInRandomOrder(Topology.Vertex rootVertex, VertexVisitor.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			VertexVisitor.VisitAll(rootVertex, new Containers.RandomOrderQueue<VertexVisitor.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitVerticesInRandomOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			VertexVisitor.VisitAll(rootVertices, new Containers.RandomOrderQueue<VertexVisitor.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitVerticesInRandomOrder<TDistance>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertex, new Containers.RandomOrderQueue<VertexVisitor<TDistance>.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitVerticesInRandomOrder<TDistance>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertices, new Containers.RandomOrderQueue<VertexVisitor<TDistance>.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitVerticesInRandomOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			VertexEdgeVisitor.VisitAll(rootEdge, new Containers.RandomOrderQueue<VertexEdgeVisitor.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitVerticesInRandomOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			VertexEdgeVisitor.VisitAll(rootEdges, new Containers.RandomOrderQueue<VertexEdgeVisitor.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitVerticesInRandomOrder<TDistance>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdge, new Containers.RandomOrderQueue<VertexEdgeVisitor<TDistance>.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitVerticesInRandomOrder<TDistance>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdges, new Containers.RandomOrderQueue<VertexEdgeVisitor<TDistance>.QueueItem>(randomEngine), visitDelegate);
		}

		#endregion

		#region Faces

		public static void VisitFacesInRandomOrder(Topology.Face rootFace, FaceVisitor.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceVisitor.VisitAll(rootFace, new Containers.RandomOrderQueue<FaceVisitor.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitFacesInRandomOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceVisitor.VisitAll(rootFaces, new Containers.RandomOrderQueue<FaceVisitor.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitFacesInRandomOrder<TDistance>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceVisitor<TDistance>.VisitAll(rootFace, new Containers.RandomOrderQueue<FaceVisitor<TDistance>.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitFacesInRandomOrder<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceVisitor<TDistance>.VisitAll(rootFaces, new Containers.RandomOrderQueue<FaceVisitor<TDistance>.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitFacesInRandomOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceEdgeVisitor.VisitAll(rootEdge, new Containers.RandomOrderQueue<FaceEdgeVisitor.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitFacesInRandomOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceEdgeVisitor.VisitAll(rootEdges, new Containers.RandomOrderQueue<FaceEdgeVisitor.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitFacesInRandomOrder<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdge, new Containers.RandomOrderQueue<FaceEdgeVisitor<TDistance>.QueueItem>(randomEngine), visitDelegate);
		}

		public static void VisitFacesInRandomOrder<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate, IRandomEngine randomEngine)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdges, new Containers.RandomOrderQueue<FaceEdgeVisitor<TDistance>.QueueItem>(randomEngine), visitDelegate);
		}

		#endregion

		#endregion
	}

	#region Vertex Visitors

	public sealed class VertexVisitor : TopologyVisitor
	{
		public struct QueueItem : IEquatable<QueueItem>
		{
			public int vertexIndex;
			public int depth;

			public QueueItem(int vertexIndex, int depth)
			{
				this.vertexIndex = vertexIndex;
				this.depth = depth;
			}

			public bool Equals(QueueItem other)
			{
				return vertexIndex == other.vertexIndex;
			}
		}

		public delegate void VisitDelegate(VertexVisitor visitor);

		private Containers.IPushPopContainer<QueueItem> _queue;
		private VisitDelegate _visitDelegate;
		private BitArray _visitedVertices;

		private Topology.Vertex _vertex;

		public Topology.Vertex vertex { get { return _vertex; } }

		public void VisitNeighbor(Topology.Vertex vertex)
		{
			_queue.Push(new QueueItem(vertex.index, _depth + 1));
		}

		public void RevisitNeighbor(Topology.Vertex vertex)
		{
			_visitedVertices[vertex.index] = false;
			_queue.Push(new QueueItem(vertex.index, _depth + 1));
		}

		public static bool AreOrderedBreadthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth <= rhs.depth;
		}

		public static bool AreOrderedDepthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth >= rhs.depth;
		}

		private VertexVisitor(Topology topology, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
			: base(topology)
		{
			_queue = queue;
			_visitDelegate = visitDelegate;
			_visitedVertices = new BitArray(topology.vertices.Count);
		}

		public static void VisitAll(Topology.Vertex rootVertex, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootVertex, queue), queue, visitDelegate);
		}

		public static void VisitAll(IEnumerable<Topology.Vertex> rootVertices, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootVertices, queue), queue, visitDelegate);
		}

		private static Topology InitializeQueue(Topology.Vertex rootVertex, Containers.IPushPopContainer<QueueItem> queue)
		{
			queue.Push(new QueueItem(rootVertex.index, 0));
			return rootVertex.topology;
		}

		private static Topology InitializeQueue(IEnumerable<Topology.Vertex> rootVertices, Containers.IPushPopContainer<QueueItem> queue)
		{
			var rootVerticesEnumerator = rootVertices.GetEnumerator();
			if (!rootVerticesEnumerator.MoveNext()) return null;

			var topology = rootVerticesEnumerator.Current.topology;

			do
			{
				queue.Push(new QueueItem(rootVerticesEnumerator.Current.index, 0));
			} while (rootVerticesEnumerator.MoveNext());

			return topology;
		}

		private static void VisitAll(Topology topology, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			if (topology == null) return;
			var visitor = new VertexVisitor(topology, queue, visitDelegate);
			visitor.VisitAll();
		}

		private void VisitAll()
		{
			while (_queue.isEmpty == false)
			{
				var queueItem = _queue.Pop();

				// Move on to the next queue item if we've already visited the target vertex.
				if (_visitedVertices[queueItem.vertexIndex] == true) continue;

				// Visit the vertex and get the list of adjacent neighbors.
				_vertex = new Topology.Vertex(_topology, queueItem.vertexIndex);
				_depth = queueItem.depth;
				_break = _ignore = false;
				_visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedVertices[queueItem.vertexIndex] = true;
			}
		}
	}

	public class VertexVisitor<TDistance> : TopologyVisitor
	{
		public struct QueueItem : IEquatable<QueueItem>
		{
			public int vertexIndex;
			public int depth;
			public TDistance distance;

			public QueueItem(int vertexIndex, int depth, TDistance distance)
			{
				this.vertexIndex = vertexIndex;
				this.depth = depth;
				this.distance = distance;
			}

			public bool Equals(QueueItem other)
			{
				return vertexIndex == other.vertexIndex;
			}
		}

		public delegate void VisitDelegate(VertexVisitor<TDistance> visitor);

		private Containers.IPushPopContainer<QueueItem> _queue;
		private VisitDelegate _visitDelegate;
		private BitArray _visitedVertices;

		private Topology.Vertex _vertex;
		private TDistance _distance;

		public Topology.Vertex vertex { get { return _vertex; } }
		public TDistance distance { get { return _distance; } }

		public void VisitNeighbor(Topology.Vertex vertex, TDistance distance)
		{
			_queue.Push(new QueueItem(vertex.index, _depth + 1, distance));
		}

		public void RevisitNeighbor(Topology.Vertex vertex, TDistance distance)
		{
			_visitedVertices[vertex.index] = false;
			_queue.Push(new QueueItem(vertex.index, _depth + 1, distance));
		}

		public static bool AreOrderedBreadthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth <= rhs.depth;
		}

		public static bool AreOrderedDepthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth >= rhs.depth;
		}

		private VertexVisitor(Topology topology, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
			: base(topology)
		{
			_queue = queue;
			_visitDelegate = visitDelegate;
			_visitedVertices = new BitArray(topology.vertices.Count);
		}

		public static void VisitAll(Topology.Vertex rootVertex, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootVertex, queue), queue, visitDelegate);
		}

		public static void VisitAll(IEnumerable<Topology.Vertex> rootVertices, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootVertices, queue), queue, visitDelegate);
		}

		private static Topology InitializeQueue(Topology.Vertex rootVertex, Containers.IPushPopContainer<QueueItem> queue)
		{
			queue.Push(new QueueItem(rootVertex.index, 0, default(TDistance)));
			return rootVertex.topology;
		}

		private static Topology InitializeQueue(IEnumerable<Topology.Vertex> rootVertices, Containers.IPushPopContainer<QueueItem> queue)
		{
			var rootVerticesEnumerator = rootVertices.GetEnumerator();
			if (!rootVerticesEnumerator.MoveNext()) return null;

			var topology = rootVerticesEnumerator.Current.topology;

			do
			{
				queue.Push(new QueueItem(rootVerticesEnumerator.Current.index, 0, default(TDistance)));
			} while (rootVerticesEnumerator.MoveNext());

			return topology;
		}

		private static void VisitAll(Topology topology, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			if (topology == null) return;
			var visitor = new VertexVisitor<TDistance>(topology, queue, visitDelegate);
			visitor.VisitAll();
		}

		private void VisitAll()
		{
			while (_queue.isEmpty == false)
			{
				var queueItem = _queue.Pop();

				// Move on to the next queue item if we've already visited the target vertex.
				if (_visitedVertices[queueItem.vertexIndex] == true) continue;

				// Visit the vertex and get the list of adjacent neighbors.
				_vertex = new Topology.Vertex(_topology, queueItem.vertexIndex);
				_depth = queueItem.depth;
				_distance = queueItem.distance;
				_break = _ignore = false;
				_visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedVertices[queueItem.vertexIndex] = true;
			}
		}
	}

	public sealed class VertexEdgeVisitor : TopologyVisitor
	{
		public struct QueueItem : IEquatable<QueueItem>
		{
			public int edgeIndex;
			public int depth;

			public QueueItem(int edgeIndex, int depth)
			{
				this.edgeIndex = edgeIndex;
				this.depth = depth;
			}

			public bool Equals(QueueItem other)
			{
				return edgeIndex == other.edgeIndex;
			}
		}

		public delegate void VisitDelegate(VertexEdgeVisitor visitor);

		private Containers.IPushPopContainer<QueueItem> _queue;
		private VisitDelegate _visitDelegate;
		private BitArray _visitedVertices;

		private Topology.VertexEdge _edge;

		public Topology.VertexEdge edge { get { return _edge; } }

		public void VisitNeighbor(Topology.VertexEdge edge)
		{
			_queue.Push(new QueueItem(edge.index, _depth + 1));
		}

		public void RevisitNeighbor(Topology.VertexEdge edge)
		{
			_visitedVertices[edge.farVertex.index] = false;
			_queue.Push(new QueueItem(edge.index, _depth + 1));
		}

		public static bool AreOrderedBreadthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth <= rhs.depth;
		}

		public static bool AreOrderedDepthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth >= rhs.depth;
		}

		private VertexEdgeVisitor(Topology topology, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
			: base(topology)
		{
			_queue = queue;
			_visitDelegate = visitDelegate;
			_visitedVertices = new BitArray(topology.vertices.Count);
		}

		public static void VisitAll(Topology.VertexEdge rootEdge, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootEdge, queue), queue, visitDelegate);
		}

		public static void VisitAll(IEnumerable<Topology.VertexEdge> rootEdges, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootEdges, queue), queue, visitDelegate);
		}

		private static Topology InitializeQueue(Topology.VertexEdge rootEdge, Containers.IPushPopContainer<QueueItem> queue)
		{
			queue.Push(new QueueItem(rootEdge.index, 0));
			return rootEdge.topology;
		}

		private static Topology InitializeQueue(IEnumerable<Topology.VertexEdge> rootEdges, Containers.IPushPopContainer<QueueItem> queue)
		{
			var rootEdgesEnumerator = rootEdges.GetEnumerator();
			if (!rootEdgesEnumerator.MoveNext()) return null;

			var topology = rootEdgesEnumerator.Current.topology;

			do
			{
				queue.Push(new QueueItem(rootEdgesEnumerator.Current.index, 0));
			} while (rootEdgesEnumerator.MoveNext());

			return topology;
		}

		private static void VisitAll(Topology topology, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			if (topology == null) return;
			var visitor = new VertexEdgeVisitor(topology, queue, visitDelegate);
			visitor.VisitAll();
		}

		private void VisitAll()
		{
			while (_queue.isEmpty == false)
			{
				var queueItem = _queue.Pop();
				var vertexIndex = _topology.edgeData[queueItem.edgeIndex].vertex;

				// Move on to the next queue item if we've already visited the target vertex.
				if (_visitedVertices[vertexIndex] == true) continue;

				// Visit the vertex and get the list of adjacent neighbors.
				_edge = new Topology.VertexEdge(_topology, queueItem.edgeIndex);
				_depth = queueItem.depth;
				_break = _ignore = false;
				_visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedVertices[vertexIndex] = true;
			}
		}
	}

	public class VertexEdgeVisitor<TDistance> : TopologyVisitor
	{
		public struct QueueItem : IEquatable<QueueItem>
		{
			public int edgeIndex;
			public int depth;
			public TDistance distance;

			public QueueItem(int edgeIndex, int depth, TDistance distance)
			{
				this.edgeIndex = edgeIndex;
				this.depth = depth;
				this.distance = distance;
			}

			public bool Equals(QueueItem other)
			{
				return edgeIndex == other.edgeIndex;
			}
		}

		public delegate void VisitDelegate(VertexEdgeVisitor<TDistance> visitor);

		private Containers.IPushPopContainer<QueueItem> _queue;
		private VisitDelegate _visitDelegate;
		private BitArray _visitedVertices;

		private Topology.VertexEdge _edge;
		private TDistance _distance;

		public Topology.VertexEdge edge { get { return _edge; } }
		public TDistance distance { get { return _distance; } }

		public void VisitNeighbor(Topology.VertexEdge edge, TDistance distance)
		{
			_queue.Push(new QueueItem(edge.index, _depth + 1, distance));
		}

		public void RevisitNeighbor(Topology.VertexEdge edge, TDistance distance)
		{
			_visitedVertices[edge.farVertex.index] = false;
			_queue.Push(new QueueItem(edge.index, _depth + 1, distance));
		}

		public static bool AreOrderedBreadthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth <= rhs.depth;
		}

		public static bool AreOrderedDepthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth >= rhs.depth;
		}

		private VertexEdgeVisitor(Topology topology, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
			: base(topology)
		{
			_queue = queue;
			_visitDelegate = visitDelegate;
			_visitedVertices = new BitArray(topology.vertices.Count);
		}

		public static void VisitAll(Topology.VertexEdge rootEdge, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootEdge, queue), queue, visitDelegate);
		}

		public static void VisitAll(IEnumerable<Topology.VertexEdge> rootEdges, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootEdges, queue), queue, visitDelegate);
		}

		private static Topology InitializeQueue(Topology.VertexEdge rootEdge, Containers.IPushPopContainer<QueueItem> queue)
		{
			queue.Push(new QueueItem(rootEdge.index, 0, default(TDistance)));
			return rootEdge.topology;
		}

		private static Topology InitializeQueue(IEnumerable<Topology.VertexEdge> rootEdge, Containers.IPushPopContainer<QueueItem> queue)
		{
			var rootEdgesEnumerator = rootEdge.GetEnumerator();
			if (!rootEdgesEnumerator.MoveNext()) return null;

			var topology = rootEdgesEnumerator.Current.topology;

			do
			{
				queue.Push(new QueueItem(rootEdgesEnumerator.Current.index, 0, default(TDistance)));
			} while (rootEdgesEnumerator.MoveNext());

			return topology;
		}

		private static void VisitAll(Topology topology, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			if (topology == null) return;
			var visitor = new VertexEdgeVisitor<TDistance>(topology, queue, visitDelegate);
			visitor.VisitAll();
		}

		private void VisitAll()
		{
			while (_queue.isEmpty == false)
			{
				var queueItem = _queue.Pop();
				var vertexIndex = _topology.edgeData[queueItem.edgeIndex].vertex;

				// Move on to the next queue item if we've already visited the target vertex.
				if (_visitedVertices[vertexIndex] == true) continue;

				// Visit the vertex and get the list of adjacent neighbors.
				_edge = new Topology.VertexEdge(_topology, queueItem.edgeIndex);
				_depth = queueItem.depth;
				_distance = queueItem.distance;
				_break = _ignore = false;
				_visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedVertices[vertexIndex] = true;
			}
		}
	}

	#endregion

	#region Face Visitors

	public sealed class FaceVisitor : TopologyVisitor
	{
		public struct QueueItem : IEquatable<QueueItem>
		{
			public int faceIndex;
			public int depth;

			public QueueItem(int faceIndex, int depth)
			{
				this.faceIndex = faceIndex;
				this.depth = depth;
			}

			public bool Equals(QueueItem other)
			{
				return faceIndex == other.faceIndex;
			}
		}

		public delegate void VisitDelegate(FaceVisitor visitor);

		private Containers.IPushPopContainer<QueueItem> _queue;
		private VisitDelegate _visitDelegate;
		private BitArray _visitedFaces;

		private Topology.Face _face;

		public Topology.Face face { get { return _face; } }

		public void VisitNeighbor(Topology.Face face)
		{
			_queue.Push(new QueueItem(face.index, _depth + 1));
		}

		public void RevisitNeighbor(Topology.Face face)
		{
			_visitedFaces[face.index] = false;
			_queue.Push(new QueueItem(face.index, _depth + 1));
		}

		public static bool AreOrderedBreadthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth <= rhs.depth;
		}

		public static bool AreOrderedDepthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth >= rhs.depth;
		}

		private FaceVisitor(Topology topology, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
			: base(topology)
		{
			_queue = queue;
			_visitDelegate = visitDelegate;
			_visitedFaces = new BitArray(topology.faces.Count);
		}

		public static void VisitAll(Topology.Face rootFace, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootFace, queue), queue, visitDelegate);
		}

		public static void VisitAll(IEnumerable<Topology.Face> rootFaces, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootFaces, queue), queue, visitDelegate);
		}

		private static Topology InitializeQueue(Topology.Face rootFace, Containers.IPushPopContainer<QueueItem> queue)
		{
			queue.Push(new QueueItem(rootFace.index, 0));
			return rootFace.topology;
		}

		private static Topology InitializeQueue(IEnumerable<Topology.Face> rootFaces, Containers.IPushPopContainer<QueueItem> queue)
		{
			var rootFacesEnumerator = rootFaces.GetEnumerator();
			if (!rootFacesEnumerator.MoveNext()) return null;

			var topology = rootFacesEnumerator.Current.topology;

			do
			{
				queue.Push(new QueueItem(rootFacesEnumerator.Current.index, 0));
			} while (rootFacesEnumerator.MoveNext());

			return topology;
		}

		private static void VisitAll(Topology topology, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			if (topology == null) return;
			var visitor = new FaceVisitor(topology, queue, visitDelegate);
			visitor.VisitAll();
		}

		private void VisitAll()
		{
			while (_queue.isEmpty == false)
			{
				var queueItem = _queue.Pop();

				// Move on to the next queue item if we've already visited the target face.
				if (_visitedFaces[queueItem.faceIndex] == true) continue;

				// Visit the face and get the list of adjacent neighbors.
				_face = new Topology.Face(_topology, queueItem.faceIndex);
				_depth = queueItem.depth;
				_break = _ignore = false;
				_visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedFaces[queueItem.faceIndex] = true;
			}
		}
	}

	public class FaceVisitor<TDistance> : TopologyVisitor
	{
		public struct QueueItem : IEquatable<QueueItem>
		{
			public int faceIndex;
			public int depth;
			public TDistance distance;

			public QueueItem(int faceIndex, int depth, TDistance distance)
			{
				this.faceIndex = faceIndex;
				this.depth = depth;
				this.distance = distance;
			}

			public bool Equals(QueueItem other)
			{
				return faceIndex == other.faceIndex;
			}
		}

		public delegate void VisitDelegate(FaceVisitor<TDistance> visitor);

		private Containers.IPushPopContainer<QueueItem> _queue;
		private VisitDelegate _visitDelegate;
		private BitArray _visitedFaces;

		private Topology.Face _face;
		private TDistance _distance;

		public Topology.Face face { get { return _face; } }
		public TDistance distance { get { return _distance; } }

		public void VisitNeighbor(Topology.Face face, TDistance distance)
		{
			_queue.Push(new QueueItem(face.index, _depth + 1, distance));
		}

		public void RevisitNeighbor(Topology.Face face, TDistance distance)
		{
			_visitedFaces[face.index] = false;
			_queue.Push(new QueueItem(face.index, _depth + 1, distance));
		}

		public static bool AreOrderedBreadthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth <= rhs.depth;
		}

		public static bool AreOrderedDepthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth >= rhs.depth;
		}

		private FaceVisitor(Topology topology, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
			: base(topology)
		{
			_queue = queue;
			_visitDelegate = visitDelegate;
			_visitedFaces = new BitArray(topology.faces.Count);
		}

		public static void VisitAll(Topology.Face rootFace, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootFace, queue), queue, visitDelegate);
		}

		public static void VisitAll(IEnumerable<Topology.Face> rootFaces, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootFaces, queue), queue, visitDelegate);
		}

		private static Topology InitializeQueue(Topology.Face rootFace, Containers.IPushPopContainer<QueueItem> queue)
		{
			queue.Push(new QueueItem(rootFace.index, 0, default(TDistance)));
			return rootFace.topology;
		}

		private static Topology InitializeQueue(IEnumerable<Topology.Face> rootFaces, Containers.IPushPopContainer<QueueItem> queue)
		{
			var rootFacesEnumerator = rootFaces.GetEnumerator();
			if (!rootFacesEnumerator.MoveNext()) return null;

			var topology = rootFacesEnumerator.Current.topology;

			do
			{
				queue.Push(new QueueItem(rootFacesEnumerator.Current.index, 0, default(TDistance)));
			} while (rootFacesEnumerator.MoveNext());

			return topology;
		}

		private static void VisitAll(Topology topology, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			if (topology == null) return;
			var visitor = new FaceVisitor<TDistance>(topology, queue, visitDelegate);
			visitor.VisitAll();
		}

		private void VisitAll()
		{
			while (_queue.isEmpty == false)
			{
				var queueItem = _queue.Pop();

				// Move on to the next queue item if we've already visited the target face.
				if (_visitedFaces[queueItem.faceIndex] == true) continue;

				// Visit the face and get the list of adjacent neighbors.
				_face = new Topology.Face(_topology, queueItem.faceIndex);
				_depth = queueItem.depth;
				_distance = queueItem.distance;
				_break = _ignore = false;
				_visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedFaces[queueItem.faceIndex] = true;
			}
		}
	}

	public sealed class FaceEdgeVisitor : TopologyVisitor
	{
		public struct QueueItem : IEquatable<QueueItem>
		{
			public int edgeIndex;
			public int depth;

			public QueueItem(int edgeIndex, int depth)
			{
				this.edgeIndex = edgeIndex;
				this.depth = depth;
			}

			public bool Equals(QueueItem other)
			{
				return edgeIndex == other.edgeIndex;
			}
		}

		public delegate void VisitDelegate(FaceEdgeVisitor visitor);

		private Containers.IPushPopContainer<QueueItem> _queue;
		private VisitDelegate _visitDelegate;
		private BitArray _visitedFaces;

		private Topology.FaceEdge _edge;

		public Topology.FaceEdge edge { get { return _edge; } }

		public void VisitNeighbor(Topology.FaceEdge edge)
		{
			_queue.Push(new QueueItem(edge.index, _depth + 1));
		}

		public void RevisitNeighbor(Topology.FaceEdge edge)
		{
			_visitedFaces[edge.farFace.index] = false;
			_queue.Push(new QueueItem(edge.index, _depth + 1));
		}

		public static bool AreOrderedBreadthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth <= rhs.depth;
		}

		public static bool AreOrderedDepthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth >= rhs.depth;
		}

		private FaceEdgeVisitor(Topology topology, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
			: base(topology)
		{
			_queue = queue;
			_visitDelegate = visitDelegate;
			_visitedFaces = new BitArray(topology.faces.Count);
		}

		public static void VisitAll(Topology.FaceEdge rootEdge, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootEdge, queue), queue, visitDelegate);
		}

		public static void VisitAll(IEnumerable<Topology.FaceEdge> rootEdges, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootEdges, queue), queue, visitDelegate);
		}

		private static Topology InitializeQueue(Topology.FaceEdge rootEdge, Containers.IPushPopContainer<QueueItem> queue)
		{
			queue.Push(new QueueItem(rootEdge.index, 0));
			return rootEdge.topology;
		}

		private static Topology InitializeQueue(IEnumerable<Topology.FaceEdge> rootEdges, Containers.IPushPopContainer<QueueItem> queue)
		{
			var rootEdgesEnumerator = rootEdges.GetEnumerator();
			if (!rootEdgesEnumerator.MoveNext()) return null;

			var topology = rootEdgesEnumerator.Current.topology;

			do
			{
				queue.Push(new QueueItem(rootEdgesEnumerator.Current.index, 0));
			} while (rootEdgesEnumerator.MoveNext());

			return topology;
		}

		private static void VisitAll(Topology topology, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			if (topology == null) return;
			var visitor = new FaceEdgeVisitor(topology, queue, visitDelegate);
			visitor.VisitAll();
		}

		private void VisitAll()
		{
			while (_queue.isEmpty == false)
			{
				var queueItem = _queue.Pop();
				var faceIndex = _topology.edgeData[queueItem.edgeIndex].face;

				// Move on to the next queue item if we've already visited the target face.
				if (_visitedFaces[faceIndex] == true) continue;

				// Visit the face and get the list of adjacent neighbors.
				_edge = new Topology.FaceEdge(_topology, queueItem.edgeIndex);
				_depth = queueItem.depth;
				_break = _ignore = false;
				_visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedFaces[faceIndex] = true;
			}
		}
	}

	public class FaceEdgeVisitor<TDistance> : TopologyVisitor
	{
		public struct QueueItem : IEquatable<QueueItem>
		{
			public int edgeIndex;
			public int depth;
			public TDistance distance;

			public QueueItem(int edgeIndex, int depth, TDistance distance)
			{
				this.edgeIndex = edgeIndex;
				this.depth = depth;
				this.distance = distance;
			}

			public bool Equals(QueueItem other)
			{
				return edgeIndex == other.edgeIndex;
			}
		}

		public delegate void VisitDelegate(FaceEdgeVisitor<TDistance> visitor);

		private Containers.IPushPopContainer<QueueItem> _queue;
		private VisitDelegate _visitDelegate;
		private BitArray _visitedFaces;

		private Topology.FaceEdge _edge;
		private TDistance _distance;

		public Topology.FaceEdge edge { get { return _edge; } }
		public TDistance distance { get { return _distance; } }

		public void VisitNeighbor(Topology.FaceEdge edge, TDistance distance)
		{
			_queue.Push(new QueueItem(edge.index, _depth + 1, distance));
		}

		public void RevisitNeighbor(Topology.FaceEdge edge, TDistance distance)
		{
			_visitedFaces[edge.farFace.index] = false;
			_queue.Push(new QueueItem(edge.index, _depth + 1, distance));
		}

		public static bool AreOrderedBreadthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth <= rhs.depth;
		}

		public static bool AreOrderedDepthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth >= rhs.depth;
		}

		private FaceEdgeVisitor(Topology topology, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
			: base(topology)
		{
			_queue = queue;
			_visitDelegate = visitDelegate;
			_visitedFaces = new BitArray(topology.faces.Count);
		}

		public static void VisitAll(Topology.FaceEdge rootEdge, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootEdge, queue), queue, visitDelegate);
		}

		public static void VisitAll(IEnumerable<Topology.FaceEdge> rootEdges, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootEdges, queue), queue, visitDelegate);
		}

		private static Topology InitializeQueue(Topology.FaceEdge rootEdge, Containers.IPushPopContainer<QueueItem> queue)
		{
			queue.Push(new QueueItem(rootEdge.index, 0, default(TDistance)));
			return rootEdge.topology;
		}

		private static Topology InitializeQueue(IEnumerable<Topology.FaceEdge> rootEdge, Containers.IPushPopContainer<QueueItem> queue)
		{
			var rootEdgesEnumerator = rootEdge.GetEnumerator();
			if (!rootEdgesEnumerator.MoveNext()) return null;

			var topology = rootEdgesEnumerator.Current.topology;

			do
			{
				queue.Push(new QueueItem(rootEdgesEnumerator.Current.index, 0, default(TDistance)));
			} while (rootEdgesEnumerator.MoveNext());

			return topology;
		}

		private static void VisitAll(Topology topology, Containers.IPushPopContainer<QueueItem> queue, VisitDelegate visitDelegate)
		{
			if (topology == null) return;
			var visitor = new FaceEdgeVisitor<TDistance>(topology, queue, visitDelegate);
			visitor.VisitAll();
		}

		private void VisitAll()
		{
			while (_queue.isEmpty == false)
			{
				var queueItem = _queue.Pop();
				var faceIndex = _topology.edgeData[queueItem.edgeIndex].face;

				// Move on to the next queue item if we've already visited the target face.
				if (_visitedFaces[faceIndex] == true) continue;

				// Visit the face and get the list of adjacent neighbors.
				_edge = new Topology.FaceEdge(_topology, queueItem.edgeIndex);
				_depth = queueItem.depth;
				_distance = queueItem.distance;
				_break = _ignore = false;
				_visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedFaces[faceIndex] = true;
			}
		}
	}

	#endregion
}

/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using Experilous.MakeIt.Random;
using Experilous.MakeIt.Utilities;

namespace Experilous.MakeIt.Tile
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
			VertexVisitor.VisitAll(rootVertex, new LifoQueue<VertexVisitor.QueueItem>(), visitDelegate);
		}

		public static void VisitVertices(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor.VisitDelegate visitDelegate)
		{
			VertexVisitor.VisitAll(rootVertices, new LifoQueue<VertexVisitor.QueueItem>(), visitDelegate);
		}

		public static void VisitVertices<TState>(Topology.Vertex rootVertex, VertexVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor.VisitAll(rootVertex, new LifoQueue<VertexVisitor.QueueItem>(), visitDelegate, state);
		}

		public static void VisitVertices<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor.VisitAll(rootVertices, new LifoQueue<VertexVisitor.QueueItem>(), visitDelegate, state);
		}

		public static void VisitVertices<TDistance>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertex, new LifoQueue<VertexVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		public static void VisitVertices<TDistance>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertices, new LifoQueue<VertexVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		public static void VisitVertices<TDistance, TState>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertex, new LifoQueue<VertexVisitor<TDistance>.QueueItem>(), visitDelegate, state);
		}

		public static void VisitVertices<TDistance, TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertices, new LifoQueue<VertexVisitor<TDistance>.QueueItem>(), visitDelegate, state);
		}

		public static void VisitVertices(Topology.VertexEdge rootEdge, VertexEdgeVisitor.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor.VisitAll(rootEdge, new LifoQueue<VertexEdgeVisitor.QueueItem>(), visitDelegate);
		}

		public static void VisitVertices(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor.VisitAll(rootEdges, new LifoQueue<VertexEdgeVisitor.QueueItem>(), visitDelegate);
		}

		public static void VisitVertices<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor.VisitAll(rootEdge, new LifoQueue<VertexEdgeVisitor.QueueItem>(), visitDelegate, state);
		}

		public static void VisitVertices<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor.VisitAll(rootEdges, new LifoQueue<VertexEdgeVisitor.QueueItem>(), visitDelegate, state);
		}

		public static void VisitVertices<TDistance>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdge, new LifoQueue<VertexEdgeVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		public static void VisitVertices<TDistance>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdges, new LifoQueue<VertexEdgeVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		public static void VisitVertices<TDistance, TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdge, new LifoQueue<VertexEdgeVisitor<TDistance>.QueueItem>(), visitDelegate, state);
		}

		public static void VisitVertices<TDistance, TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdges, new LifoQueue<VertexEdgeVisitor<TDistance>.QueueItem>(), visitDelegate, state);
		}

		#endregion

		#region Faces

		public static void VisitFaces(Topology.Face rootFace, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.VisitAll(rootFace, new LifoQueue<FaceVisitor.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.VisitAll(rootFaces, new LifoQueue<FaceVisitor.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces<TState>(Topology.Face rootFace, FaceVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor.VisitAll(rootFace, new LifoQueue<FaceVisitor.QueueItem>(), visitDelegate, state);
		}

		public static void VisitFaces<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor.VisitAll(rootFaces, new LifoQueue<FaceVisitor.QueueItem>(), visitDelegate, state);
		}

		public static void VisitFaces<TDistance>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFace, new LifoQueue<FaceVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFaces, new LifoQueue<FaceVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces<TDistance, TState>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<TDistance>.VisitAll(rootFace, new LifoQueue<FaceVisitor<TDistance>.QueueItem>(), visitDelegate, state);
		}

		public static void VisitFaces<TDistance, TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<TDistance>.VisitAll(rootFaces, new LifoQueue<FaceVisitor<TDistance>.QueueItem>(), visitDelegate, state);
		}

		public static void VisitFaces(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.VisitAll(rootEdge, new LifoQueue<FaceEdgeVisitor.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.VisitAll(rootEdges, new LifoQueue<FaceEdgeVisitor.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor.VisitAll(rootEdge, new LifoQueue<FaceEdgeVisitor.QueueItem>(), visitDelegate, state);
		}

		public static void VisitFaces<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor.VisitAll(rootEdges, new LifoQueue<FaceEdgeVisitor.QueueItem>(), visitDelegate, state);
		}

		public static void VisitFaces<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdge, new LifoQueue<FaceEdgeVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdges, new LifoQueue<FaceEdgeVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		public static void VisitFaces<TDistance, TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdge, new LifoQueue<FaceEdgeVisitor<TDistance>.QueueItem>(), visitDelegate, state);
		}

		public static void VisitFaces<TDistance, TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdges, new LifoQueue<FaceEdgeVisitor<TDistance>.QueueItem>(), visitDelegate, state);
		}

		#endregion

		#endregion

		#region Breadth First Visitation

		#region Vertices

		public static void VisitVerticesInBreadthFirstOrder(Topology.Vertex rootVertex, VertexVisitor.VisitDelegate visitDelegate)
		{
			VertexVisitor.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor.QueueItem>(VertexVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitVerticesInBreadthFirstOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor.VisitDelegate visitDelegate)
		{
			VertexVisitor.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor.QueueItem>(VertexVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitVerticesInBreadthFirstOrder<TState>(Topology.Vertex rootVertex, VertexVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor.QueueItem>(VertexVisitor.AreOrderedBreadthFirst), visitDelegate, state);
		}

		public static void VisitVerticesInBreadthFirstOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor.QueueItem>(VertexVisitor.AreOrderedBreadthFirst), visitDelegate, state);
		}

		public static void VisitVerticesInBreadthFirstOrder<TDistance>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(VertexVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitVerticesInBreadthFirstOrder<TDistance>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(VertexVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitVerticesInBreadthFirstOrder<TDistance, TState>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(VertexVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate, state);
		}

		public static void VisitVerticesInBreadthFirstOrder<TDistance, TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(VertexVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate, state);
		}

		public static void VisitVerticesInBreadthFirstOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor.QueueItem>(VertexEdgeVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitVerticesInBreadthFirstOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor.QueueItem>(VertexEdgeVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitVerticesInBreadthFirstOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor.QueueItem>(VertexEdgeVisitor.AreOrderedBreadthFirst), visitDelegate, state);
		}

		public static void VisitVerticesInBreadthFirstOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor.QueueItem>(VertexEdgeVisitor.AreOrderedBreadthFirst), visitDelegate, state);
		}

		public static void VisitVerticesInBreadthFirstOrder<TDistance>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(VertexEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitVerticesInBreadthFirstOrder<TDistance>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(VertexEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitVerticesInBreadthFirstOrder<TDistance, TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(VertexEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate, state);
		}

		public static void VisitVerticesInBreadthFirstOrder<TDistance, TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(VertexEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate, state);
		}

		#endregion

		#region Faces

		public static void VisitFacesInBreadthFirstOrder(Topology.Face rootFace, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder<TState>(Topology.Face rootFace, FaceVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedBreadthFirst), visitDelegate, state);
		}

		public static void VisitFacesInBreadthFirstOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedBreadthFirst), visitDelegate, state);
		}

		public static void VisitFacesInBreadthFirstOrder<TDistance>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>(FaceVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>(FaceVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder<TDistance, TState>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<TDistance>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>(FaceVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate, state);
		}

		public static void VisitFacesInBreadthFirstOrder<TDistance, TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<TDistance>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>(FaceVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate, state);
		}

		public static void VisitFacesInBreadthFirstOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedBreadthFirst), visitDelegate, state);
		}

		public static void VisitFacesInBreadthFirstOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedBreadthFirst), visitDelegate, state);
		}

		public static void VisitFacesInBreadthFirstOrder<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		public static void VisitFacesInBreadthFirstOrder<TDistance, TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate, state);
		}

		public static void VisitFacesInBreadthFirstOrder<TDistance, TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate, state);
		}

		#endregion

		#endregion

		#region Depth First Visitation

		#region Vertices

		public static void VisitVerticesInDepthFirstOrder(Topology.Vertex rootVertex, VertexVisitor.VisitDelegate visitDelegate)
		{
			VertexVisitor.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor.QueueItem>(VertexVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitVerticesInDepthFirstOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor.VisitDelegate visitDelegate)
		{
			VertexVisitor.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor.QueueItem>(VertexVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitVerticesInDepthFirstOrder<TState>(Topology.Vertex rootVertex, VertexVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor.QueueItem>(VertexVisitor.AreOrderedDepthFirst), visitDelegate, state);
		}

		public static void VisitVerticesInDepthFirstOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor.QueueItem>(VertexVisitor.AreOrderedDepthFirst), visitDelegate, state);
		}

		public static void VisitVerticesInDepthFirstOrder<TDistance>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(VertexVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitVerticesInDepthFirstOrder<TDistance>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(VertexVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitVerticesInDepthFirstOrder<TDistance, TState>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(VertexVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate, state);
		}

		public static void VisitVerticesInDepthFirstOrder<TDistance, TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(VertexVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate, state);
		}

		public static void VisitVerticesInDepthFirstOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor.QueueItem>(VertexEdgeVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitVerticesInDepthFirstOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor.QueueItem>(VertexEdgeVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitVerticesInDepthFirstOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor.QueueItem>(VertexEdgeVisitor.AreOrderedDepthFirst), visitDelegate, state);
		}

		public static void VisitVerticesInDepthFirstOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor.QueueItem>(VertexEdgeVisitor.AreOrderedDepthFirst), visitDelegate, state);
		}

		public static void VisitVerticesInDepthFirstOrder<TDistance>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(VertexEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitVerticesInDepthFirstOrder<TDistance>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(VertexEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitVerticesInDepthFirstOrder<TDistance, TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(VertexEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate, state);
		}

		public static void VisitVerticesInDepthFirstOrder<TDistance, TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(VertexEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate, state);
		}

		#endregion

		#region Faces

		public static void VisitFacesInDepthFirstOrder(Topology.Face rootFace, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder<TState>(Topology.Face rootFace, FaceVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedDepthFirst), visitDelegate, state);
		}

		public static void VisitFacesInDepthFirstOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedDepthFirst), visitDelegate, state);
		}

		public static void VisitFacesInDepthFirstOrder<TDistance>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>(FaceVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>(FaceVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder<TDistance, TState>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<TDistance>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>(FaceVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate, state);
		}

		public static void VisitFacesInDepthFirstOrder<TDistance, TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<TDistance>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>(FaceVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate, state);
		}

		public static void VisitFacesInDepthFirstOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedDepthFirst), visitDelegate, state);
		}

		public static void VisitFacesInDepthFirstOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedDepthFirst), visitDelegate, state);
		}

		public static void VisitFacesInDepthFirstOrder<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		public static void VisitFacesInDepthFirstOrder<TDistance, TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate, state);
		}

		public static void VisitFacesInDepthFirstOrder<TDistance, TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate, state);
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
			VertexVisitor<int>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexVisitor<int>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder<TState>(Topology.Vertex rootVertex, VertexVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<int>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInNearestDistanceOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<int>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInNearestDistanceOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<int>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<int>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<int>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInNearestDistanceOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<int>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		#endregion

		#region UInt32

		public static void VisitVerticesInNearestDistanceOrder(Topology.Vertex rootVertex, VertexVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexVisitor<uint>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexVisitor<uint>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder<TState>(Topology.Vertex rootVertex, VertexVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<uint>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInNearestDistanceOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<uint>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInNearestDistanceOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<uint>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<uint>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<uint>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInNearestDistanceOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<uint>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		#endregion

		#region Single

		public static void VisitVerticesInNearestDistanceOrder(Topology.Vertex rootVertex, VertexVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexVisitor<float>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexVisitor<float>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder<TState>(Topology.Vertex rootVertex, VertexVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<float>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInNearestDistanceOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<float>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInNearestDistanceOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<float>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<float>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<float>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInNearestDistanceOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<float>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		#endregion

		#region Double

		public static void VisitVerticesInNearestDistanceOrder(Topology.Vertex rootVertex, VertexVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexVisitor<double>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexVisitor<double>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder<TState>(Topology.Vertex rootVertex, VertexVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<double>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInNearestDistanceOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<double>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInNearestDistanceOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<double>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<double>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitVerticesInNearestDistanceOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<double>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInNearestDistanceOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<double>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		#endregion

		#endregion

		#region Faces

		#region Int32

		public static void VisitFacesInNearestDistanceOrder(Topology.Face rootFace, FaceVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceVisitor<int>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceVisitor<int>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder<TState>(Topology.Face rootFace, FaceVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<int>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitFacesInNearestDistanceOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<int>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitFacesInNearestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<int>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<int>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<int>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitFacesInNearestDistanceOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<int>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		#endregion

		#region UInt32

		public static void VisitFacesInNearestDistanceOrder(Topology.Face rootFace, FaceVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceVisitor<uint>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceVisitor<uint>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder<TState>(Topology.Face rootFace, FaceVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<uint>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitFacesInNearestDistanceOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<uint>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitFacesInNearestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<uint>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<uint>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<uint>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitFacesInNearestDistanceOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<uint>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		#endregion

		#region Single

		public static void VisitFacesInNearestDistanceOrder(Topology.Face rootFace, FaceVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceVisitor<float>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceVisitor<float>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder<TState>(Topology.Face rootFace, FaceVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<float>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitFacesInNearestDistanceOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<float>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitFacesInNearestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<float>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<float>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<float>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitFacesInNearestDistanceOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<float>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		#endregion

		#region Double

		public static void VisitFacesInNearestDistanceOrder(Topology.Face rootFace, FaceVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceVisitor<double>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceVisitor<double>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder<TState>(Topology.Face rootFace, FaceVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<double>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitFacesInNearestDistanceOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<double>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitFacesInNearestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<double>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<double>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate);
		}

		public static void VisitFacesInNearestDistanceOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<double>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
		}

		public static void VisitFacesInNearestDistanceOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<double>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByNearestDistance), visitDelegate, state);
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
			VertexVisitor<int>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexVisitor<int>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder<TState>(Topology.Vertex rootVertex, VertexVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<int>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInFarthestDistanceOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<int>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInFarthestDistanceOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<int>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<int>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<int>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInFarthestDistanceOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<int>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		#endregion

		#region UInt32

		public static void VisitVerticesInFarthestDistanceOrder(Topology.Vertex rootVertex, VertexVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexVisitor<uint>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexVisitor<uint>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder<TState>(Topology.Vertex rootVertex, VertexVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<uint>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInFarthestDistanceOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<uint>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInFarthestDistanceOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<uint>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<uint>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<uint>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInFarthestDistanceOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<uint>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		#endregion

		#region Single

		public static void VisitVerticesInFarthestDistanceOrder(Topology.Vertex rootVertex, VertexVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexVisitor<float>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexVisitor<float>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder<TState>(Topology.Vertex rootVertex, VertexVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<float>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInFarthestDistanceOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<float>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInFarthestDistanceOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<float>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<float>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<float>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInFarthestDistanceOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<float>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		#endregion

		#region Double

		public static void VisitVerticesInFarthestDistanceOrder(Topology.Vertex rootVertex, VertexVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexVisitor<double>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexVisitor<double>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder<TState>(Topology.Vertex rootVertex, VertexVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<double>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInFarthestDistanceOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<double>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInFarthestDistanceOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<double>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<double>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitVerticesInFarthestDistanceOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<double>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitVerticesInFarthestDistanceOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<double>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		#endregion

		#endregion

		#region Faces

		#region Int32

		public static void VisitFacesInFarthestDistanceOrder(Topology.Face rootFace, FaceVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceVisitor<int>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceVisitor<int>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder<TState>(Topology.Face rootFace, FaceVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<int>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitFacesInFarthestDistanceOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<int>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitFacesInFarthestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<int>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<int>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<int>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitFacesInFarthestDistanceOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<int>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		#endregion

		#region UInt32

		public static void VisitFacesInFarthestDistanceOrder(Topology.Face rootFace, FaceVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceVisitor<uint>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceVisitor<uint>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder<TState>(Topology.Face rootFace, FaceVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<uint>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitFacesInFarthestDistanceOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<uint>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitFacesInFarthestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<uint>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<uint>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<uint>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitFacesInFarthestDistanceOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<uint>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		#endregion

		#region Single

		public static void VisitFacesInFarthestDistanceOrder(Topology.Face rootFace, FaceVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceVisitor<float>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceVisitor<float>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder<TState>(Topology.Face rootFace, FaceVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<float>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitFacesInFarthestDistanceOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<float>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitFacesInFarthestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<float>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<float>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<float>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitFacesInFarthestDistanceOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<float>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		#endregion

		#region Double

		public static void VisitFacesInFarthestDistanceOrder(Topology.Face rootFace, FaceVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceVisitor<double>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceVisitor<double>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder<TState>(Topology.Face rootFace, FaceVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<double>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitFacesInFarthestDistanceOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<double>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitFacesInFarthestDistanceOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<double>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<double>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate);
		}

		public static void VisitFacesInFarthestDistanceOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<double>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		public static void VisitFacesInFarthestDistanceOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<double>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByFarthestDistance), visitDelegate, state);
		}

		#endregion

		#endregion

		#endregion

		#region Delegate Order Visitation

		#region Vertex

		public static void VisitVerticesInOrder<TDistance>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate visitDelegate, DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		public static void VisitVerticesInOrder<TDistance>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate visitDelegate, DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		public static void VisitVerticesInOrder<TDistance, TState>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate, state);
		}

		public static void VisitVerticesInOrder<TDistance, TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate, state);
		}

		public static void VisitVerticesInOrder<TDistance>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate, DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		public static void VisitVerticesInOrder<TDistance>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate, DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		public static void VisitVerticesInOrder<TDistance, TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate, state);
		}

		public static void VisitVerticesInOrder<TDistance, TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate, state);
		}

		#endregion

		#region Face

		public static void VisitFacesInOrder<TDistance>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate visitDelegate, DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		public static void VisitFacesInOrder<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate visitDelegate, DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		public static void VisitFacesInOrder<TDistance, TState>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate, state);
		}

		public static void VisitFacesInOrder<TDistance, TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceVisitor<TDistance>.VisitAll(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate, state);
		}

		public static void VisitFacesInOrder<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate, DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		public static void VisitFacesInOrder<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate, DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		public static void VisitFacesInOrder<TDistance, TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate, state);
		}

		public static void VisitFacesInOrder<TDistance, TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate, state);
		}

		#endregion

		#endregion

		#region Random Order Visitation

		#region Vertices

		public static void VisitVerticesInRandomOrder(Topology.Vertex rootVertex, VertexVisitor.VisitDelegate visitDelegate, IRandomEngine random)
		{
			VertexVisitor.VisitAll(rootVertex, new RandomOrderQueue<VertexVisitor.QueueItem>(random), visitDelegate);
		}

		public static void VisitVerticesInRandomOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor.VisitDelegate visitDelegate, IRandomEngine random)
		{
			VertexVisitor.VisitAll(rootVertices, new RandomOrderQueue<VertexVisitor.QueueItem>(random), visitDelegate);
		}

		public static void VisitVerticesInRandomOrder<TState>(Topology.Vertex rootVertex, VertexVisitor.VisitDelegate<TState> visitDelegate, TState state, IRandomEngine random)
		{
			VertexVisitor.VisitAll(rootVertex, new RandomOrderQueue<VertexVisitor.QueueItem>(random), visitDelegate, state);
		}

		public static void VisitVerticesInRandomOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor.VisitDelegate<TState> visitDelegate, TState state, IRandomEngine random)
		{
			VertexVisitor.VisitAll(rootVertices, new RandomOrderQueue<VertexVisitor.QueueItem>(random), visitDelegate, state);
		}

		public static void VisitVerticesInRandomOrder<TDistance>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate visitDelegate, IRandomEngine random)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertex, new RandomOrderQueue<VertexVisitor<TDistance>.QueueItem>(random), visitDelegate);
		}

		public static void VisitVerticesInRandomOrder<TDistance>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate visitDelegate, IRandomEngine random)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertices, new RandomOrderQueue<VertexVisitor<TDistance>.QueueItem>(random), visitDelegate);
		}

		public static void VisitVerticesInRandomOrder<TDistance, TState>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, IRandomEngine random)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertex, new RandomOrderQueue<VertexVisitor<TDistance>.QueueItem>(random), visitDelegate, state);
		}

		public static void VisitVerticesInRandomOrder<TDistance, TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, IRandomEngine random)
		{
			VertexVisitor<TDistance>.VisitAll(rootVertices, new RandomOrderQueue<VertexVisitor<TDistance>.QueueItem>(random), visitDelegate, state);
		}

		public static void VisitVerticesInRandomOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor.VisitDelegate visitDelegate, IRandomEngine random)
		{
			VertexEdgeVisitor.VisitAll(rootEdge, new RandomOrderQueue<VertexEdgeVisitor.QueueItem>(random), visitDelegate);
		}

		public static void VisitVerticesInRandomOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor.VisitDelegate visitDelegate, IRandomEngine random)
		{
			VertexEdgeVisitor.VisitAll(rootEdges, new RandomOrderQueue<VertexEdgeVisitor.QueueItem>(random), visitDelegate);
		}

		public static void VisitVerticesInRandomOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state, IRandomEngine random)
		{
			VertexEdgeVisitor.VisitAll(rootEdge, new RandomOrderQueue<VertexEdgeVisitor.QueueItem>(random), visitDelegate, state);
		}

		public static void VisitVerticesInRandomOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state, IRandomEngine random)
		{
			VertexEdgeVisitor.VisitAll(rootEdges, new RandomOrderQueue<VertexEdgeVisitor.QueueItem>(random), visitDelegate, state);
		}

		public static void VisitVerticesInRandomOrder<TDistance>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate, IRandomEngine random)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdge, new RandomOrderQueue<VertexEdgeVisitor<TDistance>.QueueItem>(random), visitDelegate);
		}

		public static void VisitVerticesInRandomOrder<TDistance>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate, IRandomEngine random)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdges, new RandomOrderQueue<VertexEdgeVisitor<TDistance>.QueueItem>(random), visitDelegate);
		}

		public static void VisitVerticesInRandomOrder<TDistance, TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, IRandomEngine random)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdge, new RandomOrderQueue<VertexEdgeVisitor<TDistance>.QueueItem>(random), visitDelegate, state);
		}

		public static void VisitVerticesInRandomOrder<TDistance, TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, IRandomEngine random)
		{
			VertexEdgeVisitor<TDistance>.VisitAll(rootEdges, new RandomOrderQueue<VertexEdgeVisitor<TDistance>.QueueItem>(random), visitDelegate, state);
		}

		#endregion

		#region Faces

		public static void VisitFacesInRandomOrder(Topology.Face rootFace, FaceVisitor.VisitDelegate visitDelegate, IRandomEngine random)
		{
			FaceVisitor.VisitAll(rootFace, new RandomOrderQueue<FaceVisitor.QueueItem>(random), visitDelegate);
		}

		public static void VisitFacesInRandomOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate visitDelegate, IRandomEngine random)
		{
			FaceVisitor.VisitAll(rootFaces, new RandomOrderQueue<FaceVisitor.QueueItem>(random), visitDelegate);
		}

		public static void VisitFacesInRandomOrder<TState>(Topology.Face rootFace, FaceVisitor.VisitDelegate<TState> visitDelegate, TState state, IRandomEngine random)
		{
			FaceVisitor.VisitAll(rootFace, new RandomOrderQueue<FaceVisitor.QueueItem>(random), visitDelegate, state);
		}

		public static void VisitFacesInRandomOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate<TState> visitDelegate, TState state, IRandomEngine random)
		{
			FaceVisitor.VisitAll(rootFaces, new RandomOrderQueue<FaceVisitor.QueueItem>(random), visitDelegate, state);
		}

		public static void VisitFacesInRandomOrder<TDistance>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate visitDelegate, IRandomEngine random)
		{
			FaceVisitor<TDistance>.VisitAll(rootFace, new RandomOrderQueue<FaceVisitor<TDistance>.QueueItem>(random), visitDelegate);
		}

		public static void VisitFacesInRandomOrder<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate visitDelegate, IRandomEngine random)
		{
			FaceVisitor<TDistance>.VisitAll(rootFaces, new RandomOrderQueue<FaceVisitor<TDistance>.QueueItem>(random), visitDelegate);
		}

		public static void VisitFacesInRandomOrder<TDistance, TState>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, IRandomEngine random)
		{
			FaceVisitor<TDistance>.VisitAll(rootFace, new RandomOrderQueue<FaceVisitor<TDistance>.QueueItem>(random), visitDelegate, state);
		}

		public static void VisitFacesInRandomOrder<TDistance, TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, IRandomEngine random)
		{
			FaceVisitor<TDistance>.VisitAll(rootFaces, new RandomOrderQueue<FaceVisitor<TDistance>.QueueItem>(random), visitDelegate, state);
		}

		public static void VisitFacesInRandomOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate visitDelegate, IRandomEngine random)
		{
			FaceEdgeVisitor.VisitAll(rootEdge, new RandomOrderQueue<FaceEdgeVisitor.QueueItem>(random), visitDelegate);
		}

		public static void VisitFacesInRandomOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate visitDelegate, IRandomEngine random)
		{
			FaceEdgeVisitor.VisitAll(rootEdges, new RandomOrderQueue<FaceEdgeVisitor.QueueItem>(random), visitDelegate);
		}

		public static void VisitFacesInRandomOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state, IRandomEngine random)
		{
			FaceEdgeVisitor.VisitAll(rootEdge, new RandomOrderQueue<FaceEdgeVisitor.QueueItem>(random), visitDelegate, state);
		}

		public static void VisitFacesInRandomOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state, IRandomEngine random)
		{
			FaceEdgeVisitor.VisitAll(rootEdges, new RandomOrderQueue<FaceEdgeVisitor.QueueItem>(random), visitDelegate, state);
		}

		public static void VisitFacesInRandomOrder<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate, IRandomEngine random)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdge, new RandomOrderQueue<FaceEdgeVisitor<TDistance>.QueueItem>(random), visitDelegate);
		}

		public static void VisitFacesInRandomOrder<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate, IRandomEngine random)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdges, new RandomOrderQueue<FaceEdgeVisitor<TDistance>.QueueItem>(random), visitDelegate);
		}

		public static void VisitFacesInRandomOrder<TDistance, TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, IRandomEngine random)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdge, new RandomOrderQueue<FaceEdgeVisitor<TDistance>.QueueItem>(random), visitDelegate, state);
		}

		public static void VisitFacesInRandomOrder<TDistance, TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, IRandomEngine random)
		{
			FaceEdgeVisitor<TDistance>.VisitAll(rootEdges, new RandomOrderQueue<FaceEdgeVisitor<TDistance>.QueueItem>(random), visitDelegate, state);
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
		public delegate void VisitDelegate<TState>(VertexVisitor visitor, TState state);

		private IQueue<QueueItem> _queue;
		private BitArray _visitedVertices;

		private Topology.Vertex _vertex;

		public Topology.Vertex vertex { get { return _vertex; } }

		public bool HasBeenVisited(Topology.Vertex vertex)
		{
			return _visitedVertices[vertex.index];
		}

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

		private VertexVisitor(Topology topology, IQueue<QueueItem> queue)
			: base(topology)
		{
			_queue = queue;
			_visitedVertices = new BitArray(topology.vertices.Count);
		}

		public static void VisitAll(Topology.Vertex rootVertex, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootVertex, queue), queue, visitDelegate);
		}

		public static void VisitAll(IEnumerable<Topology.Vertex> rootVertices, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootVertices, queue), queue, visitDelegate);
		}

		public static void VisitAll<TState>(Topology.Vertex rootVertex, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			VisitAll(InitializeQueue(rootVertex, queue), queue, visitDelegate, state);
		}

		public static void VisitAll<TState>(IEnumerable<Topology.Vertex> rootVertices, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			VisitAll(InitializeQueue(rootVertices, queue), queue, visitDelegate, state);
		}

		private static Topology InitializeQueue(Topology.Vertex rootVertex, IQueue<QueueItem> queue)
		{
			queue.Push(new QueueItem(rootVertex.index, 0));
			return rootVertex.topology;
		}

		private static Topology InitializeQueue(IEnumerable<Topology.Vertex> rootVertices, IQueue<QueueItem> queue)
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

		private static void VisitAll(Topology topology, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			if (topology == null) return;
			var visitor = new VertexVisitor(topology, queue);
			visitor.VisitAll(visitDelegate);
		}

		private static void VisitAll<TState>(Topology topology, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			if (topology == null) return;
			var visitor = new VertexVisitor(topology, queue);
			visitor.VisitAll(visitDelegate, state);
		}

		private void VisitAll(VisitDelegate visitDelegate)
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
				visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedVertices[queueItem.vertexIndex] = true;
			}
		}

		private void VisitAll<TState>(VisitDelegate<TState> visitDelegate, TState state)
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
				visitDelegate(this, state);

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
		public delegate void VisitDelegate<TState>(VertexVisitor<TDistance> visitor, TState state);

		private IQueue<QueueItem> _queue;
		private BitArray _visitedVertices;

		private Topology.Vertex _vertex;
		private TDistance _distance;

		public Topology.Vertex vertex { get { return _vertex; } }
		public TDistance distance { get { return _distance; } }

		public bool HasBeenVisited(Topology.Vertex vertex)
		{
			return _visitedVertices[vertex.index];
		}

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

		private VertexVisitor(Topology topology, IQueue<QueueItem> queue)
			: base(topology)
		{
			_queue = queue;
			_visitedVertices = new BitArray(topology.vertices.Count);
		}

		public static void VisitAll(Topology.Vertex rootVertex, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootVertex, queue), queue, visitDelegate);
		}

		public static void VisitAll(IEnumerable<Topology.Vertex> rootVertices, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootVertices, queue), queue, visitDelegate);
		}

		public static void VisitAll<TState>(Topology.Vertex rootVertex, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			VisitAll(InitializeQueue(rootVertex, queue), queue, visitDelegate, state);
		}

		public static void VisitAll<TState>(IEnumerable<Topology.Vertex> rootVertices, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			VisitAll(InitializeQueue(rootVertices, queue), queue, visitDelegate, state);
		}

		private static Topology InitializeQueue(Topology.Vertex rootVertex, IQueue<QueueItem> queue)
		{
			queue.Push(new QueueItem(rootVertex.index, 0, default(TDistance)));
			return rootVertex.topology;
		}

		private static Topology InitializeQueue(IEnumerable<Topology.Vertex> rootVertices, IQueue<QueueItem> queue)
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

		private static void VisitAll(Topology topology, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			if (topology == null) return;
			var visitor = new VertexVisitor<TDistance>(topology, queue);
			visitor.VisitAll(visitDelegate);
		}

		private static void VisitAll<TState>(Topology topology, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			if (topology == null) return;
			var visitor = new VertexVisitor<TDistance>(topology, queue);
			visitor.VisitAll(visitDelegate, state);
		}

		private void VisitAll(VisitDelegate visitDelegate)
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
				visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedVertices[queueItem.vertexIndex] = true;
			}
		}

		private void VisitAll<TState>(VisitDelegate<TState> visitDelegate, TState state)
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
				visitDelegate(this, state);

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
		public delegate void VisitDelegate<TState>(VertexEdgeVisitor visitor, TState state);

		private IQueue<QueueItem> _queue;
		private BitArray _visitedVertices;

		private Topology.VertexEdge _edge;

		public Topology.VertexEdge edge { get { return _edge; } }

		public bool HasBeenVisited(Topology.Vertex vertex)
		{
			return _visitedVertices[vertex.index];
		}

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

		private VertexEdgeVisitor(Topology topology, IQueue<QueueItem> queue, BitArray visitedVertices)
			: base(topology)
		{
			_queue = queue;
			_visitedVertices = visitedVertices;
		}

		public static void VisitAll(Topology.VertexEdge rootEdge, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Topology topology;
			BitArray visitedVertices;
			if (!Prepare(rootEdge, queue, out topology, out visitedVertices)) return;
			var visitor = new VertexEdgeVisitor(topology, queue, visitedVertices);
			visitor.VisitAll(visitDelegate);
		}

		public static void VisitAll(IEnumerable<Topology.VertexEdge> rootEdges, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Topology topology;
			BitArray visitedVertices;
			if (!Prepare(rootEdges, queue, out topology, out visitedVertices)) return;
			var visitor = new VertexEdgeVisitor(topology, queue, visitedVertices);
			visitor.VisitAll(visitDelegate);
		}

		public static void VisitAll<TState>(Topology.VertexEdge rootEdge, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Topology topology;
			BitArray visitedVertices;
			if (!Prepare(rootEdge, queue, out topology, out visitedVertices)) return;
			var visitor = new VertexEdgeVisitor(topology, queue, visitedVertices);
			visitor.VisitAll(visitDelegate, state);
		}

		public static void VisitAll<TState>(IEnumerable<Topology.VertexEdge> rootEdges, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Topology topology;
			BitArray visitedVertices;
			if (!Prepare(rootEdges, queue, out topology, out visitedVertices)) return;
			var visitor = new VertexEdgeVisitor(topology, queue, visitedVertices);
			visitor.VisitAll(visitDelegate, state);
		}

		private static bool Prepare(Topology.VertexEdge rootEdge, IQueue<QueueItem> queue, out Topology topology, out BitArray visitedVertices)
		{
			topology = rootEdge.topology;
			if (topology == null)
			{
				visitedVertices = null;
				return false;
			}

			visitedVertices = new BitArray(topology.vertices.Count);

			queue.Push(new QueueItem(rootEdge.index, 0));
			visitedVertices[rootEdge.nearVertex.index] = true;

			return true;;
		}

		private static bool Prepare(IEnumerable<Topology.VertexEdge> rootEdges, IQueue<QueueItem> queue, out Topology topology, out BitArray visitedVertices)
		{
			var rootEdgesEnumerator = rootEdges.GetEnumerator();
			if (!rootEdgesEnumerator.MoveNext())
			{
				topology = null;
				visitedVertices = null;
				return false;
			}

			topology = rootEdgesEnumerator.Current.topology;
			if (topology == null)
			{
				visitedVertices = null;
				return false;
			}

			visitedVertices = new BitArray(topology.vertices.Count);
			do
			{
				var rootEdge = rootEdgesEnumerator.Current;
				queue.Push(new QueueItem(rootEdge.index, 0));
				visitedVertices[rootEdge.nearVertex.index] = true;
			} while (rootEdgesEnumerator.MoveNext());

			return topology;
		}

		private void VisitAll(VisitDelegate visitDelegate)
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
				visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedVertices[vertexIndex] = true;
			}
		}

		private void VisitAll<TState>(VisitDelegate<TState> visitDelegate, TState state)
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
				visitDelegate(this, state);

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
		public delegate void VisitDelegate<TState>(VertexEdgeVisitor<TDistance> visitor, TState state);

		private IQueue<QueueItem> _queue;
		private BitArray _visitedVertices;

		private Topology.VertexEdge _edge;
		private TDistance _distance;

		public Topology.VertexEdge edge { get { return _edge; } }
		public TDistance distance { get { return _distance; } }

		public bool HasBeenVisited(Topology.Vertex vertex)
		{
			return _visitedVertices[vertex.index];
		}

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

		private VertexEdgeVisitor(Topology topology, IQueue<QueueItem> queue, BitArray visitedVertices)
			: base(topology)
		{
			_queue = queue;
			_visitedVertices = visitedVertices;
		}

		public static void VisitAll(Topology.VertexEdge rootEdge, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Topology topology;
			BitArray visitedVertices;
			if (!Prepare(rootEdge, queue, out topology, out visitedVertices)) return;
			var visitor = new VertexEdgeVisitor<TDistance>(topology, queue, visitedVertices);
			visitor.VisitAll(visitDelegate);
		}

		public static void VisitAll(IEnumerable<Topology.VertexEdge> rootEdges, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Topology topology;
			BitArray visitedVertices;
			if (!Prepare(rootEdges, queue, out topology, out visitedVertices)) return;
			var visitor = new VertexEdgeVisitor<TDistance>(topology, queue, visitedVertices);
			visitor.VisitAll(visitDelegate);
		}

		public static void VisitAll<TState>(Topology.VertexEdge rootEdge, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Topology topology;
			BitArray visitedVertices;
			if (!Prepare(rootEdge, queue, out topology, out visitedVertices)) return;
			var visitor = new VertexEdgeVisitor<TDistance>(topology, queue, visitedVertices);
			visitor.VisitAll(visitDelegate, state);
		}

		public static void VisitAll<TState>(IEnumerable<Topology.VertexEdge> rootEdges, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Topology topology;
			BitArray visitedVertices;
			if (!Prepare(rootEdges, queue, out topology, out visitedVertices)) return;
			var visitor = new VertexEdgeVisitor<TDistance>(topology, queue, visitedVertices);
			visitor.VisitAll(visitDelegate, state);
		}

		private static bool Prepare(Topology.VertexEdge rootEdge, IQueue<QueueItem> queue, out Topology topology, out BitArray visitedVertices)
		{
			topology = rootEdge.topology;
			if (topology == null)
			{
				visitedVertices = null;
				return false;
			}

			visitedVertices = new BitArray(topology.vertices.Count);

			queue.Push(new QueueItem(rootEdge.index, 0, default(TDistance)));
			visitedVertices[rootEdge.nearVertex.index] = true;

			return true;;
		}

		private static bool Prepare(IEnumerable<Topology.VertexEdge> rootEdges, IQueue<QueueItem> queue, out Topology topology, out BitArray visitedVertices)
		{
			var rootEdgesEnumerator = rootEdges.GetEnumerator();
			if (!rootEdgesEnumerator.MoveNext())
			{
				topology = null;
				visitedVertices = null;
				return false;
			}

			topology = rootEdgesEnumerator.Current.topology;
			if (topology == null)
			{
				visitedVertices = null;
				return false;
			}

			visitedVertices = new BitArray(topology.vertices.Count);
			do
			{
				var rootEdge = rootEdgesEnumerator.Current;
				queue.Push(new QueueItem(rootEdge.index, 0, default(TDistance)));
				visitedVertices[rootEdge.nearVertex.index] = true;
			} while (rootEdgesEnumerator.MoveNext());

			return topology;
		}

		private void VisitAll(VisitDelegate visitDelegate)
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
				visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedVertices[vertexIndex] = true;
			}
		}

		private void VisitAll<TState>(VisitDelegate<TState> visitDelegate, TState state)
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
				visitDelegate(this, state);

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
		public delegate void VisitDelegate<TState>(FaceVisitor visitor, TState state);

		private IQueue<QueueItem> _queue;
		private BitArray _visitedFaces;

		private Topology.Face _face;

		public Topology.Face face { get { return _face; } }

		public bool HasBeenVisited(Topology.Face face)
		{
			return _visitedFaces[face.index];
		}

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

		private FaceVisitor(Topology topology, IQueue<QueueItem> queue)
			: base(topology)
		{
			_queue = queue;
			_visitedFaces = new BitArray(topology.faces.Count);
		}

		public static void VisitAll(Topology.Face rootFace, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootFace, queue), queue, visitDelegate);
		}

		public static void VisitAll(IEnumerable<Topology.Face> rootFaces, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootFaces, queue), queue, visitDelegate);
		}

		public static void VisitAll<TState>(Topology.Face rootFace, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			VisitAll(InitializeQueue(rootFace, queue), queue, visitDelegate, state);
		}

		public static void VisitAll<TState>(IEnumerable<Topology.Face> rootFaces, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			VisitAll(InitializeQueue(rootFaces, queue), queue, visitDelegate, state);
		}

		private static Topology InitializeQueue(Topology.Face rootFace, IQueue<QueueItem> queue)
		{
			queue.Push(new QueueItem(rootFace.index, 0));
			return rootFace.topology;
		}

		private static Topology InitializeQueue(IEnumerable<Topology.Face> rootFaces, IQueue<QueueItem> queue)
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

		private static void VisitAll(Topology topology, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			if (topology == null) return;
			var visitor = new FaceVisitor(topology, queue);
			visitor.VisitAll(visitDelegate);
		}

		private static void VisitAll<TState>(Topology topology, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			if (topology == null) return;
			var visitor = new FaceVisitor(topology, queue);
			visitor.VisitAll(visitDelegate, state);
		}

		private void VisitAll(VisitDelegate visitDelegate)
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
				visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedFaces[queueItem.faceIndex] = true;
			}
		}

		private void VisitAll<TState>(VisitDelegate<TState> visitDelegate, TState state)
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
				visitDelegate(this, state);

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
		public delegate void VisitDelegate<TState>(FaceVisitor<TDistance> visitor, TState state);

		private IQueue<QueueItem> _queue;
		private BitArray _visitedFaces;

		private Topology.Face _face;
		private TDistance _distance;

		public Topology.Face face { get { return _face; } }
		public TDistance distance { get { return _distance; } }

		public bool HasBeenVisited(Topology.Face face)
		{
			return _visitedFaces[face.index];
		}

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

		private FaceVisitor(Topology topology, IQueue<QueueItem> queue)
			: base(topology)
		{
			_queue = queue;
			_visitedFaces = new BitArray(topology.faces.Count);
		}

		public static void VisitAll(Topology.Face rootFace, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootFace, queue), queue, visitDelegate);
		}

		public static void VisitAll(IEnumerable<Topology.Face> rootFaces, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			VisitAll(InitializeQueue(rootFaces, queue), queue, visitDelegate);
		}

		public static void VisitAll<TState>(Topology.Face rootFace, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			VisitAll(InitializeQueue(rootFace, queue), queue, visitDelegate, state);
		}

		public static void VisitAll<TState>(IEnumerable<Topology.Face> rootFaces, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			VisitAll(InitializeQueue(rootFaces, queue), queue, visitDelegate, state);
		}

		private static Topology InitializeQueue(Topology.Face rootFace, IQueue<QueueItem> queue)
		{
			queue.Push(new QueueItem(rootFace.index, 0, default(TDistance)));
			return rootFace.topology;
		}

		private static Topology InitializeQueue(IEnumerable<Topology.Face> rootFaces, IQueue<QueueItem> queue)
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

		private static void VisitAll(Topology topology, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			if (topology == null) return;
			var visitor = new FaceVisitor<TDistance>(topology, queue);
			visitor.VisitAll(visitDelegate);
		}

		private static void VisitAll<TState>(Topology topology, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			if (topology == null) return;
			var visitor = new FaceVisitor<TDistance>(topology, queue);
			visitor.VisitAll(visitDelegate, state);
		}

		private void VisitAll(VisitDelegate visitDelegate)
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
				visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedFaces[queueItem.faceIndex] = true;
			}
		}

		private void VisitAll<TState>(VisitDelegate<TState> visitDelegate, TState state)
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
				visitDelegate(this, state);

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
		public delegate void VisitDelegate<TState>(FaceEdgeVisitor visitor, TState state);

		private IQueue<QueueItem> _queue;
		private BitArray _visitedFaces;

		private Topology.FaceEdge _edge;

		public Topology.FaceEdge edge { get { return _edge; } }

		public bool HasBeenVisited(Topology.Face face)
		{
			return _visitedFaces[face.index];
		}

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

		private FaceEdgeVisitor(Topology topology, IQueue<QueueItem> queue, BitArray visitedFaces)
			: base(topology)
		{
			_queue = queue;
			_visitedFaces = visitedFaces;
		}

		public static void VisitAll(Topology.FaceEdge rootEdge, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Topology topology;
			BitArray visitedFaces;
			if (!Prepare(rootEdge, queue, out topology, out visitedFaces)) return;
			var visitor = new FaceEdgeVisitor(topology, queue, visitedFaces);
			visitor.VisitAll(visitDelegate);
		}

		public static void VisitAll(IEnumerable<Topology.FaceEdge> rootEdges, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Topology topology;
			BitArray visitedFaces;
			if (!Prepare(rootEdges, queue, out topology, out visitedFaces)) return;
			var visitor = new FaceEdgeVisitor(topology, queue, visitedFaces);
			visitor.VisitAll(visitDelegate);
		}

		public static void VisitAll<TState>(Topology.FaceEdge rootEdge, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Topology topology;
			BitArray visitedFaces;
			if (!Prepare(rootEdge, queue, out topology, out visitedFaces)) return;
			var visitor = new FaceEdgeVisitor(topology, queue, visitedFaces);
			visitor.VisitAll(visitDelegate, state);
		}

		public static void VisitAll<TState>(IEnumerable<Topology.FaceEdge> rootEdges, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Topology topology;
			BitArray visitedFaces;
			if (!Prepare(rootEdges, queue, out topology, out visitedFaces)) return;
			var visitor = new FaceEdgeVisitor(topology, queue, visitedFaces);
			visitor.VisitAll(visitDelegate, state);
		}

		private static bool Prepare(Topology.FaceEdge rootEdge, IQueue<QueueItem> queue, out Topology topology, out BitArray visitedFaces)
		{
			topology = rootEdge.topology;
			if (topology == null)
			{
				visitedFaces = null;
				return false;
			}

			visitedFaces = new BitArray(topology.faces.Count);

			queue.Push(new QueueItem(rootEdge.index, 0));
			visitedFaces[rootEdge.nearFace.index] = true;

			return true;;
		}

		private static bool Prepare(IEnumerable<Topology.FaceEdge> rootEdges, IQueue<QueueItem> queue, out Topology topology, out BitArray visitedFaces)
		{
			var rootEdgesEnumerator = rootEdges.GetEnumerator();
			if (!rootEdgesEnumerator.MoveNext())
			{
				topology = null;
				visitedFaces = null;
				return false;
			}

			topology = rootEdgesEnumerator.Current.topology;
			if (topology == null)
			{
				visitedFaces = null;
				return false;
			}

			visitedFaces = new BitArray(topology.faces.Count);
			do
			{
				var rootEdge = rootEdgesEnumerator.Current;
				queue.Push(new QueueItem(rootEdge.index, 0));
				visitedFaces[rootEdge.nearFace.index] = true;
			} while (rootEdgesEnumerator.MoveNext());

			return topology;
		}

		private void VisitAll(VisitDelegate visitDelegate)
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
				visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedFaces[faceIndex] = true;
			}
		}

		private void VisitAll<TState>(VisitDelegate<TState> visitDelegate, TState state)
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
				visitDelegate(this, state);

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
		public delegate void VisitDelegate<TState>(FaceEdgeVisitor<TDistance> visitor, TState state);

		private IQueue<QueueItem> _queue;
		private BitArray _visitedFaces;

		private Topology.FaceEdge _edge;
		private TDistance _distance;

		public Topology.FaceEdge edge { get { return _edge; } }
		public TDistance distance { get { return _distance; } }

		public bool HasBeenVisited(Topology.Face face)
		{
			return _visitedFaces[face.index];
		}

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

		private FaceEdgeVisitor(Topology topology, IQueue<QueueItem> queue, BitArray visitedFaces)
			: base(topology)
		{
			_queue = queue;
			_visitedFaces = visitedFaces;
		}

		public static void VisitAll(Topology.FaceEdge rootEdge, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Topology topology;
			BitArray visitedFaces;
			if (!Prepare(rootEdge, queue, out topology, out visitedFaces)) return;
			var visitor = new FaceEdgeVisitor<TDistance>(topology, queue, visitedFaces);
			visitor.VisitAll(visitDelegate);
		}

		public static void VisitAll(IEnumerable<Topology.FaceEdge> rootEdges, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Topology topology;
			BitArray visitedFaces;
			if (!Prepare(rootEdges, queue, out topology, out visitedFaces)) return;
			var visitor = new FaceEdgeVisitor<TDistance>(topology, queue, visitedFaces);
			visitor.VisitAll(visitDelegate);
		}

		public static void VisitAll<TState>(Topology.FaceEdge rootEdge, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Topology topology;
			BitArray visitedFaces;
			if (!Prepare(rootEdge, queue, out topology, out visitedFaces)) return;
			var visitor = new FaceEdgeVisitor<TDistance>(topology, queue, visitedFaces);
			visitor.VisitAll(visitDelegate, state);
		}

		public static void VisitAll<TState>(IEnumerable<Topology.FaceEdge> rootEdges, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Topology topology;
			BitArray visitedFaces;
			if (!Prepare(rootEdges, queue, out topology, out visitedFaces)) return;
			var visitor = new FaceEdgeVisitor<TDistance>(topology, queue, visitedFaces);
			visitor.VisitAll(visitDelegate, state);
		}

		private static bool Prepare(Topology.FaceEdge rootEdge, IQueue<QueueItem> queue, out Topology topology, out BitArray visitedFaces)
		{
			topology = rootEdge.topology;
			if (topology == null)
			{
				visitedFaces = null;
				return false;
			}

			visitedFaces = new BitArray(topology.faces.Count);

			queue.Push(new QueueItem(rootEdge.index, 0, default(TDistance)));
			visitedFaces[rootEdge.nearFace.index] = true;

			return true;;
		}

		private static bool Prepare(IEnumerable<Topology.FaceEdge> rootEdges, IQueue<QueueItem> queue, out Topology topology, out BitArray visitedFaces)
		{
			var rootEdgesEnumerator = rootEdges.GetEnumerator();
			if (!rootEdgesEnumerator.MoveNext())
			{
				topology = null;
				visitedFaces = null;
				return false;
			}

			topology = rootEdgesEnumerator.Current.topology;
			if (topology == null)
			{
				visitedFaces = null;
				return false;
			}

			visitedFaces = new BitArray(topology.faces.Count);
			do
			{
				var rootEdge = rootEdgesEnumerator.Current;
				queue.Push(new QueueItem(rootEdge.index, 0, default(TDistance)));
				visitedFaces[rootEdge.nearFace.index] = true;
			} while (rootEdgesEnumerator.MoveNext());

			return topology;
		}

		private void VisitAll(VisitDelegate visitDelegate)
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
				visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedFaces[faceIndex] = true;
			}
		}

		private void VisitAll<TState>(VisitDelegate<TState> visitDelegate, TState state)
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
				visitDelegate(this, state);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedFaces[faceIndex] = true;
			}
		}
	}

	#endregion
}

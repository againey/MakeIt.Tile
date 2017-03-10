/******************************************************************************\
* Copyright Andy Gainey                                                        *
\******************************************************************************/
#if false
using System;
using System.Collections;
using System.Collections.Generic;
using Experilous.MakeItRandom;
using Experilous.Containers;
using Experilous.Topologies;

namespace Experilous.MakeItTile
{
	/// <summary>
	/// Abstract base class for visiting topology elements one at a time in some connected
	/// order, and which includes static functions to initiate visitations of various forms.
	/// </summary>
	/// <remarks><para>A visitor is an object that maintains a queue of elements ready to
	/// be visited, and a memory of which elements have already been visited.  When a
	/// visitation process is executed, the visitor repeatedly pops one element at a time
	/// and calls a supplied visit delegate with itself as a parameter.  The element just
	/// popped is then accessible through the visitor within the visit delegate.  It is
	/// then up to the visit delegate to push any new elements into the visitors queue,
	/// typically all or some of the immediate neighbors of the element currently being
	/// visited.</para></remarks>
	/// <seealso cref="VertexVisitor"/>
	/// <seealso cref="VertexVisitor{TDistance}"/>
	/// <seealso cref="VertexEdgeVisitor"/>
	/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
	/// <seealso cref="FaceVisitor"/>
	/// <seealso cref="FaceVisitor{TDistance}"/>
	/// <seealso cref="FaceEdgeVisitor"/>
	/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
	public abstract class TopologyVisitor
	{
		/// <summary>
		/// The topology containing the elements being visited.
		/// </summary>
		protected Topology _topology;

		/// <summary>
		/// The current depth of visitation, which is the number of edge connections that
		/// have been traversed from the initial element to reach the current element.
		/// </summary>
		protected int _depth;

		/// <summary>
		/// Indicates if visitation of the current item should be ignored, treating it as
		/// though it has not yet been visited.
		/// </summary>
		protected bool _ignore;

		/// <summary>
		/// Indicates if visitation of all remaining elements should immediately cease.
		/// </summary>
		protected bool _break;

		/// <summary>
		/// The current depth of visitation, which is the number of edge connections that
		/// have been traversed from the initial element to reach the current element.
		/// </summary>
		public int depth { get { return _depth; } }

		/// <summary>
		/// Instructs the visitor to treat the currently visited element as though it was
		/// not just visited.
		/// </summary>
		public void Ignore()
		{
			_ignore = true;
		}

		/// <summary>
		/// Instructs the visitor to cease visitation of any further elements.
		/// </summary>
		public void Break()
		{
			_break = true;
		}

		/// <summary>
		/// Constructs a visitor over elements of the given topology.
		/// </summary>
		/// <param name="topology">The topology containing the elements being visited.</param>
		protected TopologyVisitor(Topology topology)
		{
			_topology = topology;
		}

#region Arbitrary Order Visitation

#region Vertices

		/// <summary>
		/// Visits adjacent topology vertices in no particular order, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitVertices(Topology.Vertex rootVertex, VertexVisitor.VisitDelegate visitDelegate)
		{
			VertexVisitor.Visit(rootVertex, new LifoQueue<VertexVisitor.QueueItem>(), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in no particular order, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitVertices(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor.VisitDelegate visitDelegate)
		{
			VertexVisitor.Visit(rootVertices, new LifoQueue<VertexVisitor.QueueItem>(), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in no particular order, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitVertices<TState>(Topology.Vertex rootVertex, VertexVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor.Visit(rootVertex, new LifoQueue<VertexVisitor.QueueItem>(), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in no particular order, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitVertices<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor.Visit(rootVertices, new LifoQueue<VertexVisitor.QueueItem>(), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in no particular order, starting with the specified root vertex, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitVertices<TDistance>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexVisitor<TDistance>.Visit(rootVertex, new LifoQueue<VertexVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in no particular order, starting with the specified root vertices, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitVertices<TDistance>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexVisitor<TDistance>.Visit(rootVertices, new LifoQueue<VertexVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in no particular order, starting with the specified root vertex, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitVertices<TDistance, TState>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<TDistance>.Visit(rootVertex, new LifoQueue<VertexVisitor<TDistance>.QueueItem>(), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in no particular order, starting with the specified root vertices, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitVertices<TDistance, TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<TDistance>.Visit(rootVertices, new LifoQueue<VertexVisitor<TDistance>.QueueItem>(), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in no particular order, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitVertices(Topology.VertexEdge rootEdge, VertexEdgeVisitor.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor.Visit(rootEdge, new LifoQueue<VertexEdgeVisitor.QueueItem>(), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in no particular order, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitVertices(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor.Visit(rootEdges, new LifoQueue<VertexEdgeVisitor.QueueItem>(), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in no particular order, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitVertices<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor.Visit(rootEdge, new LifoQueue<VertexEdgeVisitor.QueueItem>(), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in no particular order, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitVertices<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor.Visit(rootEdges, new LifoQueue<VertexEdgeVisitor.QueueItem>(), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in no particular order, starting with the specified root edge, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitVertices<TDistance>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<TDistance>.Visit(rootEdge, new LifoQueue<VertexEdgeVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in no particular order, starting with the specified root edges, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitVertices<TDistance>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<TDistance>.Visit(rootEdges, new LifoQueue<VertexEdgeVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in no particular order, starting with the specified root edge, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitVertices<TDistance, TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<TDistance>.Visit(rootEdge, new LifoQueue<VertexEdgeVisitor<TDistance>.QueueItem>(), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in no particular order, starting with the specified root edges, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitVertices<TDistance, TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<TDistance>.Visit(rootEdges, new LifoQueue<VertexEdgeVisitor<TDistance>.QueueItem>(), visitDelegate, state);
		}

#endregion

#region Faces

		/// <summary>
		/// Visits adjacent topology faces in no particular order, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitFaces(Topology.Face rootFace, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.Visit(rootFace, new LifoQueue<FaceVisitor.QueueItem>(), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in no particular order, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitFaces<TState>(Topology.Face rootFace, FaceVisitor<TState>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TState>.Visit(rootFace, new LifoQueue<FaceVisitor<TState>.QueueItem>(), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in no particular order, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitFaces<TState>(Topology.Face rootFace, TState rootState, FaceVisitor<TState>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TState>.Visit(rootFace, rootState, new LifoQueue<FaceVisitor<TState>.QueueItem>(), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in no particular order, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitFaces<TState>(FaceVisitor<TState>.FaceStatePair rootFace, FaceVisitor<TState>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TState>.Visit(rootFace, new LifoQueue<FaceVisitor<TState>.QueueItem>(), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in no particular order, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitFaces(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.Visit(rootFaces, new LifoQueue<FaceVisitor.QueueItem>(), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in no particular order, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitFaces<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TState>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TState>.Visit(rootFaces, new LifoQueue<FaceVisitor<TState>.QueueItem>(), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in no particular order, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitFaces<TState>(IEnumerable<FaceVisitor<TState>.FaceStatePair> rootFaces, FaceVisitor<TState>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TState>.Visit(rootFaces, new LifoQueue<FaceVisitor<TState>.QueueItem>(), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in no particular order, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TSharedState">The type of the custom state object.</typeparam>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitFaces<TSharedState>(Topology.Face rootFace, FaceVisitor.VisitDelegate<TSharedState> visitDelegate, TSharedState sharedState)
		{
			FaceVisitor.Visit(rootFace, new LifoQueue<FaceVisitor.QueueItem>(), visitDelegate, sharedState);
		}

		/// <summary>
		/// Visits adjacent topology faces in no particular order, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TSharedState">The type of the custom state object.</typeparam>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitFaces<TSharedState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate<TSharedState> visitDelegate, TSharedState sharedState)
		{
			FaceVisitor.Visit(rootFaces, new LifoQueue<FaceVisitor.QueueItem>(), visitDelegate, sharedState);
		}

		/// <summary>
		/// Visits adjacent topology face edges in no particular order, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitFaces(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.Visit(rootEdge, new LifoQueue<FaceEdgeVisitor.QueueItem>(), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in no particular order, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitFaces(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.Visit(rootEdges, new LifoQueue<FaceEdgeVisitor.QueueItem>(), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in no particular order, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitFaces<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor.Visit(rootEdge, new LifoQueue<FaceEdgeVisitor.QueueItem>(), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in no particular order, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitFaces<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor.Visit(rootEdges, new LifoQueue<FaceEdgeVisitor.QueueItem>(), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in no particular order, starting with the specified root edge, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitFaces<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.Visit(rootEdge, new LifoQueue<FaceEdgeVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in no particular order, starting with the specified root edges, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitFaces<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.Visit(rootEdges, new LifoQueue<FaceEdgeVisitor<TDistance>.QueueItem>(), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in no particular order, starting with the specified root edge, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitFaces<TDistance, TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<TDistance>.Visit(rootEdge, new LifoQueue<FaceEdgeVisitor<TDistance>.QueueItem>(), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in no particular order, starting with the specified root edges, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="LifoQueue{T}"/>
		public static void VisitFaces<TDistance, TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<TDistance>.Visit(rootEdges, new LifoQueue<FaceEdgeVisitor<TDistance>.QueueItem>(), visitDelegate, state);
		}

#endregion

#endregion

#region Breadth First Visitation

#region Vertices

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on depth, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInBreadthFirstOrder(Topology.Vertex rootVertex, VertexVisitor.VisitDelegate visitDelegate)
		{
			VertexVisitor.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor.QueueItem>(VertexVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on depth, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInBreadthFirstOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor.VisitDelegate visitDelegate)
		{
			VertexVisitor.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor.QueueItem>(VertexVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on depth, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInBreadthFirstOrder<TState>(Topology.Vertex rootVertex, VertexVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor.QueueItem>(VertexVisitor.AreOrderedBreadthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on depth, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInBreadthFirstOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor.QueueItem>(VertexVisitor.AreOrderedBreadthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on depth, starting with the specified root vertex, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInBreadthFirstOrder<TDistance>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexVisitor<TDistance>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(VertexVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on depth, starting with the specified root vertices, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInBreadthFirstOrder<TDistance>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexVisitor<TDistance>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(VertexVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on depth, starting with the specified root vertex, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInBreadthFirstOrder<TDistance, TState>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<TDistance>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(VertexVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on depth, starting with the specified root vertices, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInBreadthFirstOrder<TDistance, TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<TDistance>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(VertexVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on depth, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInBreadthFirstOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor.QueueItem>(VertexEdgeVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on depth, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInBreadthFirstOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor.QueueItem>(VertexEdgeVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on depth, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInBreadthFirstOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor.QueueItem>(VertexEdgeVisitor.AreOrderedBreadthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on depth, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInBreadthFirstOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor.QueueItem>(VertexEdgeVisitor.AreOrderedBreadthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on depth, starting with the specified root edge, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInBreadthFirstOrder<TDistance>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<TDistance>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(VertexEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on depth, starting with the specified root edges, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInBreadthFirstOrder<TDistance>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<TDistance>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(VertexEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on depth, starting with the specified root edge, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInBreadthFirstOrder<TDistance, TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<TDistance>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(VertexEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on depth, starting with the specified root edges, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInBreadthFirstOrder<TDistance, TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<TDistance>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(VertexEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate, state);
		}

#endregion

#region Faces

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on depth, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInBreadthFirstOrder(Topology.Face rootFace, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on depth, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInBreadthFirstOrder<TState>(Topology.Face rootFace, FaceVisitor<TState>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TState>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<TState>.QueueItem>(FaceVisitor<TState>.AreOrderedBreadthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on depth, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInBreadthFirstOrder<TState>(Topology.Face rootFace, TState rootState, FaceVisitor<TState>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TState>.Visit(rootFace, rootState, new DelegateOrderedPriorityQueue<FaceVisitor<TState>.QueueItem>(FaceVisitor<TState>.AreOrderedBreadthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on depth, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInBreadthFirstOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on depth, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInBreadthFirstOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TState>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TState>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<TState>.QueueItem>(FaceVisitor<TState>.AreOrderedBreadthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on depth, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInBreadthFirstOrder<TState>(IEnumerable<FaceVisitor<TState>.FaceStatePair> rootFaces, FaceVisitor<TState>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TState>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<TState>.QueueItem>(FaceVisitor<TState>.AreOrderedBreadthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on depth, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInBreadthFirstOrder<TState>(Topology.Face rootFace, FaceVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedBreadthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on depth, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInBreadthFirstOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedBreadthFirst), visitDelegate, state);
		}
		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on depth, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInBreadthFirstOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on depth, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInBreadthFirstOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedBreadthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on depth, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInBreadthFirstOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedBreadthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on depth, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInBreadthFirstOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedBreadthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on depth, starting with the specified root edge, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInBreadthFirstOrder<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on depth, starting with the specified root edges, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInBreadthFirstOrder<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on depth, starting with the specified root edge, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInBreadthFirstOrder<TDistance, TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<TDistance>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on depth, starting with the specified root edges, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInBreadthFirstOrder<TDistance, TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<TDistance>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedBreadthFirst), visitDelegate, state);
		}

#endregion

#endregion

#region Depth First Visitation

#region Vertices

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on depth, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInDepthFirstOrder(Topology.Vertex rootVertex, VertexVisitor.VisitDelegate visitDelegate)
		{
			VertexVisitor.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor.QueueItem>(VertexVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on depth, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInDepthFirstOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor.VisitDelegate visitDelegate)
		{
			VertexVisitor.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor.QueueItem>(VertexVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on depth, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInDepthFirstOrder<TState>(Topology.Vertex rootVertex, VertexVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor.QueueItem>(VertexVisitor.AreOrderedDepthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on depth, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInDepthFirstOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor.QueueItem>(VertexVisitor.AreOrderedDepthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on depth, starting with the specified root vertex, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInDepthFirstOrder<TDistance>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexVisitor<TDistance>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(VertexVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on depth, starting with the specified root vertices, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInDepthFirstOrder<TDistance>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexVisitor<TDistance>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(VertexVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on depth, starting with the specified root vertex, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInDepthFirstOrder<TDistance, TState>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<TDistance>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(VertexVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on depth, starting with the specified root vertices, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInDepthFirstOrder<TDistance, TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<TDistance>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(VertexVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on depth, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInDepthFirstOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor.QueueItem>(VertexEdgeVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on depth, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInDepthFirstOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor.QueueItem>(VertexEdgeVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on depth, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInDepthFirstOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor.QueueItem>(VertexEdgeVisitor.AreOrderedDepthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on depth, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInDepthFirstOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor.QueueItem>(VertexEdgeVisitor.AreOrderedDepthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on depth, starting with the specified root edge, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInDepthFirstOrder<TDistance>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<TDistance>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(VertexEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on depth, starting with the specified root edges, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInDepthFirstOrder<TDistance>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<TDistance>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(VertexEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on depth, starting with the specified root edge, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInDepthFirstOrder<TDistance, TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<TDistance>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(VertexEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on depth, starting with the specified root edges, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInDepthFirstOrder<TDistance, TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<TDistance>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(VertexEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate, state);
		}

#endregion

#region Faces

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on depth, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInDepthFirstOrder(Topology.Face rootFace, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on depth, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInDepthFirstOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate visitDelegate)
		{
			FaceVisitor.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on depth, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInDepthFirstOrder<TState>(Topology.Face rootFace, FaceVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedDepthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on depth, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInDepthFirstOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor.QueueItem>(FaceVisitor.AreOrderedDepthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on depth, starting with the specified root face, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInDepthFirstOrder<TDistance>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TDistance>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>(FaceVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on depth, starting with the specified root faces, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInDepthFirstOrder<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceVisitor<TDistance>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>(FaceVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on depth, starting with the specified root face, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInDepthFirstOrder<TDistance, TState>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<TDistance>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>(FaceVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on depth, starting with the specified root faces, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInDepthFirstOrder<TDistance, TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<TDistance>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>(FaceVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on depth, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInDepthFirstOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on depth, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInDepthFirstOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedDepthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on depth, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInDepthFirstOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedDepthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on depth, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInDepthFirstOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor.QueueItem>(FaceEdgeVisitor.AreOrderedDepthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on depth, starting with the specified root edge, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInDepthFirstOrder<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on depth, starting with the specified root edges, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInDepthFirstOrder<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<TDistance>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on depth, starting with the specified root edge, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInDepthFirstOrder<TDistance, TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<TDistance>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on depth, starting with the specified root edges, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInDepthFirstOrder<TDistance, TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<TDistance>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(FaceEdgeVisitor<TDistance>.AreOrderedDepthFirst), visitDelegate, state);
		}

#endregion

#endregion

#region Shortest Distance First Visitation

#region Distance Comparisons

#region Vertices

		private static bool AreOrderedByShortestDistanceFirst(VertexVisitor<int>.QueueItem lhs, VertexVisitor<int>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByShortestDistanceFirst(VertexEdgeVisitor<int>.QueueItem lhs, VertexEdgeVisitor<int>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByShortestDistanceFirst(VertexVisitor<uint>.QueueItem lhs, VertexVisitor<uint>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByShortestDistanceFirst(VertexEdgeVisitor<uint>.QueueItem lhs, VertexEdgeVisitor<uint>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByShortestDistanceFirst(VertexVisitor<float>.QueueItem lhs, VertexVisitor<float>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByShortestDistanceFirst(VertexEdgeVisitor<float>.QueueItem lhs, VertexEdgeVisitor<float>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByShortestDistanceFirst(VertexVisitor<double>.QueueItem lhs, VertexVisitor<double>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByShortestDistanceFirst(VertexEdgeVisitor<double>.QueueItem lhs, VertexEdgeVisitor<double>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

#endregion

#region Faces

		private static bool AreOrderedByShortestDistanceFirst(FaceVisitor<int>.QueueItem lhs, FaceVisitor<int>.QueueItem rhs)
		{
			return lhs.state <= rhs.state;
		}

		private static bool AreOrderedByShortestDistanceFirst(FaceEdgeVisitor<int>.QueueItem lhs, FaceEdgeVisitor<int>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByShortestDistanceFirst(FaceVisitor<uint>.QueueItem lhs, FaceVisitor<uint>.QueueItem rhs)
		{
			return lhs.state <= rhs.state;
		}

		private static bool AreOrderedByShortestDistanceFirst(FaceEdgeVisitor<uint>.QueueItem lhs, FaceEdgeVisitor<uint>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByShortestDistanceFirst(FaceVisitor<float>.QueueItem lhs, FaceVisitor<float>.QueueItem rhs)
		{
			return lhs.state <= rhs.state;
		}

		private static bool AreOrderedByShortestDistanceFirst(FaceEdgeVisitor<float>.QueueItem lhs, FaceEdgeVisitor<float>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

		private static bool AreOrderedByShortestDistanceFirst(FaceVisitor<double>.QueueItem lhs, FaceVisitor<double>.QueueItem rhs)
		{
			return lhs.state <= rhs.state;
		}

		private static bool AreOrderedByShortestDistanceFirst(FaceEdgeVisitor<double>.QueueItem lhs, FaceEdgeVisitor<double>.QueueItem rhs)
		{
			return lhs.distance <= rhs.distance;
		}

#endregion

#endregion

#region Vertices

#region Int32

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on distance, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder(Topology.Vertex rootVertex, VertexVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexVisitor<int>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<int>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on distance, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexVisitor<int>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<int>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on distance, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder<TState>(Topology.Vertex rootVertex, VertexVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<int>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<int>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on distance, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<int>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<int>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<int>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<int>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<int>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<int>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<int>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<int>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<int>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<int>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

#endregion

#region UInt32

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on distance, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder(Topology.Vertex rootVertex, VertexVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexVisitor<uint>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<uint>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on distance, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexVisitor<uint>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<uint>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on distance, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder<TState>(Topology.Vertex rootVertex, VertexVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<uint>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<uint>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on distance, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<uint>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<uint>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<uint>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<uint>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<uint>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<uint>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<uint>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<uint>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<uint>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<uint>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

#endregion

#region Single

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on distance, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder(Topology.Vertex rootVertex, VertexVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexVisitor<float>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<float>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on distance, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexVisitor<float>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<float>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on distance, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder<TState>(Topology.Vertex rootVertex, VertexVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<float>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<float>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on distance, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<float>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<float>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<float>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<float>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<float>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<float>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<float>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<float>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<float>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<float>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

#endregion

#region Double

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on distance, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder(Topology.Vertex rootVertex, VertexVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexVisitor<double>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<double>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on distance, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexVisitor<double>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<double>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on distance, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder<TState>(Topology.Vertex rootVertex, VertexVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<double>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<double>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in breadth-first order based on distance, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<double>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<double>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<double>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<double>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<double>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<double>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<double>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<double>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in breadth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInShortestDistanceFirstOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<double>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<double>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

#endregion

#endregion

#region Faces

#region Int32

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on distance, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder(Topology.Face rootFace, FaceVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceVisitor<int>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<int>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on distance, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceVisitor<int>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<int>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on distance, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder<TState>(Topology.Face rootFace, FaceVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<int>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<int>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on distance, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<int>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<int>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<int>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<int>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<int>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<int>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

#endregion

#region UInt32

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on distance, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder(Topology.Face rootFace, FaceVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceVisitor<uint>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on distance, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceVisitor<uint>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on distance, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder<TState>(Topology.Face rootFace, FaceVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<uint>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on distance, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<uint>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<uint>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<uint>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<uint>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<uint>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

#endregion

#region Single

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on distance, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder(Topology.Face rootFace, FaceVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceVisitor<float>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<float>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on distance, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceVisitor<float>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<float>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on distance, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder<TState>(Topology.Face rootFace, FaceVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<float>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<float>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on distance, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<float>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<float>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<float>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<float>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<float>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<float>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

#endregion

#region Double

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on distance, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder(Topology.Face rootFace, FaceVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceVisitor<double>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<double>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on distance, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceVisitor<double>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<double>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on distance, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder<TState>(Topology.Face rootFace, FaceVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<double>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<double>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology faces in breadth-first order based on distance, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<double>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<double>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<double>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<double>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<double>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in breadth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInShortestDistanceFirstOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<double>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByShortestDistanceFirst), visitDelegate, state);
		}

#endregion

#endregion

#endregion

#region Longest Distance First Visitation

#region Distance Comparisons

#region Vertices

		private static bool AreOrderedByLongestDistanceFirst(VertexVisitor<int>.QueueItem lhs, VertexVisitor<int>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByLongestDistanceFirst(VertexEdgeVisitor<int>.QueueItem lhs, VertexEdgeVisitor<int>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByLongestDistanceFirst(VertexVisitor<uint>.QueueItem lhs, VertexVisitor<uint>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByLongestDistanceFirst(VertexEdgeVisitor<uint>.QueueItem lhs, VertexEdgeVisitor<uint>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByLongestDistanceFirst(VertexVisitor<float>.QueueItem lhs, VertexVisitor<float>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByLongestDistanceFirst(VertexEdgeVisitor<float>.QueueItem lhs, VertexEdgeVisitor<float>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByLongestDistanceFirst(VertexVisitor<double>.QueueItem lhs, VertexVisitor<double>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByLongestDistanceFirst(VertexEdgeVisitor<double>.QueueItem lhs, VertexEdgeVisitor<double>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

#endregion

#region Faces

		private static bool AreOrderedByLongestDistanceFirst(FaceVisitor<int>.QueueItem lhs, FaceVisitor<int>.QueueItem rhs)
		{
			return lhs.state >= rhs.state;
		}

		private static bool AreOrderedByLongestDistanceFirst(FaceEdgeVisitor<int>.QueueItem lhs, FaceEdgeVisitor<int>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByLongestDistanceFirst(FaceVisitor<uint>.QueueItem lhs, FaceVisitor<uint>.QueueItem rhs)
		{
			return lhs.state >= rhs.state;
		}

		private static bool AreOrderedByLongestDistanceFirst(FaceEdgeVisitor<uint>.QueueItem lhs, FaceEdgeVisitor<uint>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByLongestDistanceFirst(FaceVisitor<float>.QueueItem lhs, FaceVisitor<float>.QueueItem rhs)
		{
			return lhs.state >= rhs.state;
		}

		private static bool AreOrderedByLongestDistanceFirst(FaceEdgeVisitor<float>.QueueItem lhs, FaceEdgeVisitor<float>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

		private static bool AreOrderedByLongestDistanceFirst(FaceVisitor<double>.QueueItem lhs, FaceVisitor<double>.QueueItem rhs)
		{
			return lhs.state >= rhs.state;
		}

		private static bool AreOrderedByLongestDistanceFirst(FaceEdgeVisitor<double>.QueueItem lhs, FaceEdgeVisitor<double>.QueueItem rhs)
		{
			return lhs.distance >= rhs.distance;
		}

#endregion

#endregion

#region Vertices

#region Int32

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on distance, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder(Topology.Vertex rootVertex, VertexVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexVisitor<int>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<int>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on distance, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexVisitor<int>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<int>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on distance, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder<TState>(Topology.Vertex rootVertex, VertexVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<int>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<int>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on distance, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<int>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<int>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<int>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<int>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<int>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<int>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<int>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<int>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<int>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<int>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

#endregion

#region UInt32

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on distance, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder(Topology.Vertex rootVertex, VertexVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexVisitor<uint>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<uint>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on distance, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexVisitor<uint>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<uint>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on distance, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder<TState>(Topology.Vertex rootVertex, VertexVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<uint>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<uint>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on distance, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<uint>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<uint>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<uint>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<uint>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<uint>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<uint>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<uint>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<uint>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<uint>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<uint>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

#endregion

#region Single

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on distance, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder(Topology.Vertex rootVertex, VertexVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexVisitor<float>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<float>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on distance, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexVisitor<float>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<float>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on distance, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder<TState>(Topology.Vertex rootVertex, VertexVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<float>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<float>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on distance, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<float>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<float>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<float>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<float>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<float>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<float>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<float>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<float>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<float>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<float>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

#endregion

#region Double

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on distance, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder(Topology.Vertex rootVertex, VertexVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexVisitor<double>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<double>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on distance, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexVisitor<double>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<double>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on distance, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder<TState>(Topology.Vertex rootVertex, VertexVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<double>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<double>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in depth-first order based on distance, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexVisitor<double>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<double>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<double>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<double>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			VertexEdgeVisitor<double>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<double>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<double>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<double>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in depth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInLongestDistanceFirstOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			VertexEdgeVisitor<double>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<double>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

#endregion

#endregion

#region Faces

#region Int32

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on distance, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder(Topology.Face rootFace, FaceVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceVisitor<int>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<int>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on distance, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceVisitor<int>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<int>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on distance, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder<TState>(Topology.Face rootFace, FaceVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<int>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<int>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on distance, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<int>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<int>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<int>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<int>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<int>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<int>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<int>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<int>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<int>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

#endregion

#region UInt32

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on distance, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder(Topology.Face rootFace, FaceVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceVisitor<uint>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on distance, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceVisitor<uint>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on distance, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder<TState>(Topology.Face rootFace, FaceVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<uint>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on distance, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<uint>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<uint>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<uint>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<uint>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<uint>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<uint>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<uint>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<uint>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<uint>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

#endregion

#region Single

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on distance, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder(Topology.Face rootFace, FaceVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceVisitor<float>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<float>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on distance, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceVisitor<float>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<float>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on distance, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder<TState>(Topology.Face rootFace, FaceVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<float>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<float>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on distance, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<float>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<float>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<float>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<float>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<float>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<float>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<float>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<float>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<float>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

#endregion

#region Double

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on distance, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder(Topology.Face rootFace, FaceVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceVisitor<double>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<double>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on distance, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceVisitor<double>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<double>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on distance, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder<TState>(Topology.Face rootFace, FaceVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<double>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<double>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology faces in depth-first order based on distance, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceVisitor<double>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<double>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<double>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<double>.VisitDelegate visitDelegate)
		{
			FaceEdgeVisitor<double>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on distance, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<double>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in depth-first order based on distance, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInLongestDistanceFirstOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<double>.VisitDelegate<TState> visitDelegate, TState state)
		{
			FaceEdgeVisitor<double>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<double>.QueueItem>(AreOrderedByLongestDistanceFirst), visitDelegate, state);
		}

#endregion

#endregion

#endregion

#region Delegate Order Visitation

#region Vertex

		/// <summary>
		/// Visits adjacent topology vertices in an order determined by the supplied comparison delegate, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="areOrderedDelegate">The comparison delegate determines the order in which vertices are visited.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInOrder<TDistance>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate visitDelegate, DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			VertexVisitor<TDistance>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in an order determined by the supplied comparison delegate, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="areOrderedDelegate">The comparison delegate determines the order in which vertices are visited.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInOrder<TDistance>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate visitDelegate, DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			VertexVisitor<TDistance>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in an order determined by the supplied comparison delegate, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="areOrderedDelegate">The comparison delegate determines the order in which vertices are visited.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInOrder<TDistance, TState>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			VertexVisitor<TDistance>.Visit(rootVertex, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in an order determined by the supplied comparison delegate, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="areOrderedDelegate">The comparison delegate determines the order in which vertices are visited.</param>
		/// <seealso cref="VertexVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInOrder<TDistance, TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			VertexVisitor<TDistance>.Visit(rootVertices, new DelegateOrderedPriorityQueue<VertexVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in an order determined by the supplied comparison delegate, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="areOrderedDelegate">The comparison delegate determines the order in which vertices are visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInOrder<TDistance>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate, DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			VertexEdgeVisitor<TDistance>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in an order determined by the supplied comparison delegate, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="areOrderedDelegate">The comparison delegate determines the order in which vertices are visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInOrder<TDistance>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate, DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			VertexEdgeVisitor<TDistance>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in an order determined by the supplied comparison delegate, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="areOrderedDelegate">The comparison delegate determines the order in which vertices are visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInOrder<TDistance, TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			VertexEdgeVisitor<TDistance>.Visit(rootEdge, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in an order determined by the supplied comparison delegate, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="areOrderedDelegate">The comparison delegate determines the order in which vertices are visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitVerticesInOrder<TDistance, TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			VertexEdgeVisitor<TDistance>.Visit(rootEdges, new DelegateOrderedPriorityQueue<VertexEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate, state);
		}

#endregion

#region Face

		/// <summary>
		/// Visits adjacent topology faces in an order determined by the supplied comparison delegate, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="areOrderedDelegate">The comparison delegate determines the order in which faces are visited.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInOrder<TDistance>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate visitDelegate, DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceVisitor<TDistance>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in an order determined by the supplied comparison delegate, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="areOrderedDelegate">The comparison delegate determines the order in which faces are visited.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInOrder<TDistance>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate visitDelegate, DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceVisitor<TDistance>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in an order determined by the supplied comparison delegate, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="areOrderedDelegate">The comparison delegate determines the order in which faces are visited.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInOrder<TDistance, TState>(Topology.Face rootFace, FaceVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceVisitor<TDistance>.Visit(rootFace, new DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology faces in an order determined by the supplied comparison delegate, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="areOrderedDelegate">The comparison delegate determines the order in which faces are visited.</param>
		/// <seealso cref="FaceVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInOrder<TDistance, TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceVisitor<TDistance>.Visit(rootFaces, new DelegateOrderedPriorityQueue<FaceVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in an order determined by the supplied comparison delegate, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="areOrderedDelegate">The comparison delegate determines the order in which faces are visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInOrder<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate, DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceEdgeVisitor<TDistance>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in an order determined by the supplied comparison delegate, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="areOrderedDelegate">The comparison delegate determines the order in which faces are visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInOrder<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate, DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceEdgeVisitor<TDistance>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in an order determined by the supplied comparison delegate, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="areOrderedDelegate">The comparison delegate determines the order in which faces are visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInOrder<TDistance, TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceEdgeVisitor<TDistance>.Visit(rootEdge, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in an order determined by the supplied comparison delegate, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="areOrderedDelegate">The comparison delegate determines the order in which faces are visited.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="DelegateOrderedPriorityQueue{T}"/>
		public static void VisitFacesInOrder<TDistance, TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>.AreOrderedDelegate areOrderedDelegate)
		{
			FaceEdgeVisitor<TDistance>.Visit(rootEdges, new DelegateOrderedPriorityQueue<FaceEdgeVisitor<TDistance>.QueueItem>(areOrderedDelegate), visitDelegate, state);
		}

#endregion

#endregion

#region Random Order Visitation

#region Vertices

		/// <summary>
		/// Visits adjacent topology vertices in a uniformly random order driven by the supplied random engine, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="random">The random engine that determines the order in which vertices are popped from the queue of vertices to visit.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitVerticesInRandomOrder(Topology.Vertex rootVertex, VertexVisitor.VisitDelegate visitDelegate, IRandom random)
		{
			VertexVisitor.Visit(rootVertex, new RandomOrderQueue<VertexVisitor.QueueItem>(random), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in a uniformly random order driven by the supplied random engine, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="random">The random engine that determines the order in which vertices are popped from the queue of vertices to visit.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitVerticesInRandomOrder(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor.VisitDelegate visitDelegate, IRandom random)
		{
			VertexVisitor.Visit(rootVertices, new RandomOrderQueue<VertexVisitor.QueueItem>(random), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in a uniformly random order driven by the supplied random engine, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="random">The random engine that determines the order in which vertices are popped from the queue of vertices to visit.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitVerticesInRandomOrder<TState>(Topology.Vertex rootVertex, VertexVisitor.VisitDelegate<TState> visitDelegate, TState state, IRandom random)
		{
			VertexVisitor.Visit(rootVertex, new RandomOrderQueue<VertexVisitor.QueueItem>(random), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in a uniformly random order driven by the supplied random engine, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="random">The random engine that determines the order in which vertices are popped from the queue of vertices to visit.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitVerticesInRandomOrder<TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor.VisitDelegate<TState> visitDelegate, TState state, IRandom random)
		{
			VertexVisitor.Visit(rootVertices, new RandomOrderQueue<VertexVisitor.QueueItem>(random), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in a uniformly random order driven by the supplied random engine, starting with the specified root vertex, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="random">The random engine that determines the order in which vertices are popped from the queue of vertices to visit.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitVerticesInRandomOrder<TDistance>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate visitDelegate, IRandom random)
		{
			VertexVisitor<TDistance>.Visit(rootVertex, new RandomOrderQueue<VertexVisitor<TDistance>.QueueItem>(random), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in a uniformly random order driven by the supplied random engine, starting with the specified root vertices, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="random">The random engine that determines the order in which vertices are popped from the queue of vertices to visit.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitVerticesInRandomOrder<TDistance>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate visitDelegate, IRandom random)
		{
			VertexVisitor<TDistance>.Visit(rootVertices, new RandomOrderQueue<VertexVisitor<TDistance>.QueueItem>(random), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in a uniformly random order driven by the supplied random engine, starting with the specified root vertex, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="random">The random engine that determines the order in which vertices are popped from the queue of vertices to visit.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitVerticesInRandomOrder<TDistance, TState>(Topology.Vertex rootVertex, VertexVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, IRandom random)
		{
			VertexVisitor<TDistance>.Visit(rootVertex, new RandomOrderQueue<VertexVisitor<TDistance>.QueueItem>(random), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in a uniformly random order driven by the supplied random engine, starting with the specified root vertices, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="random">The random engine that determines the order in which vertices are popped from the queue of vertices to visit.</param>
		/// <seealso cref="VertexVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitVerticesInRandomOrder<TDistance, TState>(IEnumerable<Topology.Vertex> rootVertices, VertexVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, IRandom random)
		{
			VertexVisitor<TDistance>.Visit(rootVertices, new RandomOrderQueue<VertexVisitor<TDistance>.QueueItem>(random), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in a uniformly random order driven by the supplied random engine, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="random">The random engine that determines the order in which vertices are popped from the queue of vertices to visit.</param>
		/// <seealso cref="VertexEdgeVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitVerticesInRandomOrder(Topology.VertexEdge rootEdge, VertexEdgeVisitor.VisitDelegate visitDelegate, IRandom random)
		{
			VertexEdgeVisitor.Visit(rootEdge, new RandomOrderQueue<VertexEdgeVisitor.QueueItem>(random), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in a uniformly random order driven by the supplied random engine, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="random">The random engine that determines the order in which vertices are popped from the queue of vertices to visit.</param>
		/// <seealso cref="VertexEdgeVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitVerticesInRandomOrder(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor.VisitDelegate visitDelegate, IRandom random)
		{
			VertexEdgeVisitor.Visit(rootEdges, new RandomOrderQueue<VertexEdgeVisitor.QueueItem>(random), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in a uniformly random order driven by the supplied random engine, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="random">The random engine that determines the order in which vertices are popped from the queue of vertices to visit.</param>
		/// <seealso cref="VertexEdgeVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitVerticesInRandomOrder<TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state, IRandom random)
		{
			VertexEdgeVisitor.Visit(rootEdge, new RandomOrderQueue<VertexEdgeVisitor.QueueItem>(random), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in a uniformly random order driven by the supplied random engine, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="random">The random engine that determines the order in which vertices are popped from the queue of vertices to visit.</param>
		/// <seealso cref="VertexEdgeVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitVerticesInRandomOrder<TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state, IRandom random)
		{
			VertexEdgeVisitor.Visit(rootEdges, new RandomOrderQueue<VertexEdgeVisitor.QueueItem>(random), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in a uniformly random order driven by the supplied random engine, starting with the specified root edge, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="random">The random engine that determines the order in which vertices are popped from the queue of vertices to visit.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitVerticesInRandomOrder<TDistance>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate, IRandom random)
		{
			VertexEdgeVisitor<TDistance>.Visit(rootEdge, new RandomOrderQueue<VertexEdgeVisitor<TDistance>.QueueItem>(random), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in a uniformly random order driven by the supplied random engine, starting with the specified root edges, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="random">The random engine that determines the order in which vertices are popped from the queue of vertices to visit.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitVerticesInRandomOrder<TDistance>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate visitDelegate, IRandom random)
		{
			VertexEdgeVisitor<TDistance>.Visit(rootEdges, new RandomOrderQueue<VertexEdgeVisitor<TDistance>.QueueItem>(random), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in a uniformly random order driven by the supplied random engine, starting with the specified root edge, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="random">The random engine that determines the order in which vertices are popped from the queue of vertices to visit.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitVerticesInRandomOrder<TDistance, TState>(Topology.VertexEdge rootEdge, VertexEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, IRandom random)
		{
			VertexEdgeVisitor<TDistance>.Visit(rootEdge, new RandomOrderQueue<VertexEdgeVisitor<TDistance>.QueueItem>(random), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in a uniformly random order driven by the supplied random engine, starting with the specified root edges, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="random">The random engine that determines the order in which vertices are popped from the queue of vertices to visit.</param>
		/// <seealso cref="VertexEdgeVisitor{TDistance}"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitVerticesInRandomOrder<TDistance, TState>(IEnumerable<Topology.VertexEdge> rootEdges, VertexEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, IRandom random)
		{
			VertexEdgeVisitor<TDistance>.Visit(rootEdges, new RandomOrderQueue<VertexEdgeVisitor<TDistance>.QueueItem>(random), visitDelegate, state);
		}

#endregion

#region Faces

		/// <summary>
		/// Visits adjacent topology faces in a uniformly random order driven by the supplied random engine, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="random">The random engine that determines the order in which faces are popped from the queue of faces to visit.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitFacesInRandomOrder(Topology.Face rootFace, FaceVisitor.VisitDelegate visitDelegate, IRandom random)
		{
			FaceVisitor.Visit(rootFace, new RandomOrderQueue<FaceVisitor.QueueItem>(random), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in a uniformly random order driven by the supplied random engine, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="random">The random engine that determines the order in which faces are popped from the queue of faces to visit.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitFacesInRandomOrder<TState>(Topology.Face rootFace, TState state, FaceVisitor<TState>.VisitDelegate visitDelegate, IRandom random)
		{
			FaceVisitor<TState>.Visit(rootFace, state, new RandomOrderQueue<FaceVisitor<TState>.QueueItem>(random), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in a uniformly random order driven by the supplied random engine, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="random">The random engine that determines the order in which faces are popped from the queue of faces to visit.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitFacesInRandomOrder<TState>(FaceVisitor<TState>.FaceStatePair rootFace, FaceVisitor<TState>.VisitDelegate visitDelegate, IRandom random)
		{
			FaceVisitor<TState>.Visit(rootFace, new RandomOrderQueue<FaceVisitor<TState>.QueueItem>(random), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in a uniformly random order driven by the supplied random engine, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="random">The random engine that determines the order in which faces are popped from the queue of faces to visit.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitFacesInRandomOrder(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate visitDelegate, IRandom random)
		{
			FaceVisitor.Visit(rootFaces, new RandomOrderQueue<FaceVisitor.QueueItem>(random), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in a uniformly random order driven by the supplied random engine, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="random">The random engine that determines the order in which faces are popped from the queue of faces to visit.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitFacesInRandomOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor<TState>.VisitDelegate visitDelegate, IRandom random)
		{
			FaceVisitor<TState>.Visit(rootFaces, new RandomOrderQueue<FaceVisitor<TState>.QueueItem>(random), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in a uniformly random order driven by the supplied random engine, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="random">The random engine that determines the order in which faces are popped from the queue of faces to visit.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitFacesInRandomOrder<TState>(IEnumerable<FaceVisitor<TState>.FaceStatePair> rootFaces, FaceVisitor<TState>.VisitDelegate visitDelegate, IRandom random)
		{
			FaceVisitor<TState>.Visit(rootFaces, new RandomOrderQueue<FaceVisitor<TState>.QueueItem>(random), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in a uniformly random order driven by the supplied random engine, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="random">The random engine that determines the order in which faces are popped from the queue of faces to visit.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitFacesInRandomOrder<TState>(Topology.Face rootFace, FaceVisitor.VisitDelegate<TState> visitDelegate, TState state, IRandom random)
		{
			FaceVisitor.Visit(rootFace, new RandomOrderQueue<FaceVisitor.QueueItem>(random), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology faces in a uniformly random order driven by the supplied random engine, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="random">The random engine that determines the order in which faces are popped from the queue of faces to visit.</param>
		/// <seealso cref="FaceVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitFacesInRandomOrder<TState>(IEnumerable<Topology.Face> rootFaces, FaceVisitor.VisitDelegate<TState> visitDelegate, TState state, IRandom random)
		{
			FaceVisitor.Visit(rootFaces, new RandomOrderQueue<FaceVisitor.QueueItem>(random), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in a uniformly random order driven by the supplied random engine, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="random">The random engine that determines the order in which faces are popped from the queue of faces to visit.</param>
		/// <seealso cref="FaceEdgeVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitFacesInRandomOrder(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate visitDelegate, IRandom random)
		{
			FaceEdgeVisitor.Visit(rootEdge, new RandomOrderQueue<FaceEdgeVisitor.QueueItem>(random), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in a uniformly random order driven by the supplied random engine, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="random">The random engine that determines the order in which faces are popped from the queue of faces to visit.</param>
		/// <seealso cref="FaceEdgeVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitFacesInRandomOrder(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate visitDelegate, IRandom random)
		{
			FaceEdgeVisitor.Visit(rootEdges, new RandomOrderQueue<FaceEdgeVisitor.QueueItem>(random), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in a uniformly random order driven by the supplied random engine, starting with the specified root edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="random">The random engine that determines the order in which faces are popped from the queue of faces to visit.</param>
		/// <seealso cref="FaceEdgeVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitFacesInRandomOrder<TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state, IRandom random)
		{
			FaceEdgeVisitor.Visit(rootEdge, new RandomOrderQueue<FaceEdgeVisitor.QueueItem>(random), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in a uniformly random order driven by the supplied random engine, starting with the specified root edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="random">The random engine that determines the order in which faces are popped from the queue of faces to visit.</param>
		/// <seealso cref="FaceEdgeVisitor"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitFacesInRandomOrder<TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor.VisitDelegate<TState> visitDelegate, TState state, IRandom random)
		{
			FaceEdgeVisitor.Visit(rootEdges, new RandomOrderQueue<FaceEdgeVisitor.QueueItem>(random), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in a uniformly random order driven by the supplied random engine, starting with the specified root edge, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="random">The random engine that determines the order in which faces are popped from the queue of faces to visit.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitFacesInRandomOrder<TDistance>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate, IRandom random)
		{
			FaceEdgeVisitor<TDistance>.Visit(rootEdge, new RandomOrderQueue<FaceEdgeVisitor<TDistance>.QueueItem>(random), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in a uniformly random order driven by the supplied random engine, starting with the specified root edges, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="random">The random engine that determines the order in which faces are popped from the queue of faces to visit.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitFacesInRandomOrder<TDistance>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate visitDelegate, IRandom random)
		{
			FaceEdgeVisitor<TDistance>.Visit(rootEdges, new RandomOrderQueue<FaceEdgeVisitor<TDistance>.QueueItem>(random), visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in a uniformly random order driven by the supplied random engine, starting with the specified root edge, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The edge to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="random">The random engine that determines the order in which faces are popped from the queue of faces to visit.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitFacesInRandomOrder<TDistance, TState>(Topology.FaceEdge rootEdge, FaceEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, IRandom random)
		{
			FaceEdgeVisitor<TDistance>.Visit(rootEdge, new RandomOrderQueue<FaceEdgeVisitor<TDistance>.QueueItem>(random), visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in a uniformly random order driven by the supplied random engine, starting with the specified root edges, using the supplied visit delegate and keeping track of a custom distance metric along the way.
		/// </summary>
		/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of edges to visit first.</param>
		/// <param name="visitDelegate">The delegate that will be called for each edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		/// <param name="random">The random engine that determines the order in which faces are popped from the queue of faces to visit.</param>
		/// <seealso cref="FaceEdgeVisitor{TDistance}"/>
		/// <seealso cref="RandomOrderQueue{T}"/>
		public static void VisitFacesInRandomOrder<TDistance, TState>(IEnumerable<Topology.FaceEdge> rootEdges, FaceEdgeVisitor<TDistance>.VisitDelegate<TState> visitDelegate, TState state, IRandom random)
		{
			FaceEdgeVisitor<TDistance>.Visit(rootEdges, new RandomOrderQueue<FaceEdgeVisitor<TDistance>.QueueItem>(random), visitDelegate, state);
		}

#endregion

#endregion
	}

#region Vertex Visitors

	/// <summary>
	/// Class for visiting the vertices of a topology, one at a time in some connected order.
	/// </summary>
	/// <remarks><para>A visitor is an object that maintains a queue of elements ready to
	/// be visited, and a memory of which elements have already been visited.  When a
	/// visitation process is executed, the visitor repeatedly pops one element at a time
	/// and calls a supplied visit delegate with itself as a parameter.  The element just
	/// popped is then accessible through the visitor within the visit delegate.  It is
	/// then up to the visit delegate to push any new elements into the visitors queue,
	/// typically all or some of the immediate neighbors of the element currently being
	/// visited.</para></remarks>
	public sealed class VertexVisitor : TopologyVisitor
	{
		/// <summary>
		/// The data that needs to be stored in a queue for the visitor to know which
		/// vertex to visit next.  It also keeps track of informational data such as
		/// visit depth even if it does not affect visitation order.
		/// </summary>
		public struct QueueItem : IEquatable<QueueItem>
		{
			/// <summary>
			/// The index of the vertex to be visited.
			/// </summary>
			public int vertexIndex;

			/// <summary>
			/// The depth of visitation, which is the number of edges that had to be
			/// traversed to reach the vertex of this queue item.
			/// </summary>
			public int depth;

			/// <summary>
			/// Constructs a queue item with the given vertex index and visitation depth.
			/// </summary>
			/// <param name="vertexIndex">The index of the vertex to be visited.</param>
			/// <param name="depth">The depth of visitation.</param>
			public QueueItem(int vertexIndex, int depth)
			{
				this.vertexIndex = vertexIndex;
				this.depth = depth;
			}

			/// <summary>
			/// Determines if the current queue item references the same vertex as the specified queue item.
			/// </summary>
			/// <param name="other">The other queue item to compare.</param>
			/// <returns>True if both queue items refer to the same vertex, and false otherwise.</returns>
			public bool Equals(QueueItem other)
			{
				return vertexIndex == other.vertexIndex;
			}
		}

		/// <summary>
		/// A delegate signature which the visitor uses to inform the consumer of each vertex visited.
		/// </summary>
		/// <param name="visitor">The visitor executing the visitation, which stores the current state of visitation.</param>
		public delegate void VisitDelegate(VertexVisitor visitor);

		/// <summary>
		/// A delegate signature which the visitor uses to inform the consumer of each vertex visited.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="visitor">The visitor executing the visitation, which stores the current state of visitation.</param>
		/// <param name="state">The custom state object which was supplied at the beginning of visitation.  Ideally a class or immutable struct.</param>
		public delegate void VisitDelegate<TState>(VertexVisitor visitor, TState state);

		private IQueue<QueueItem> _queue;
		private BitArray _visitedVertices;

		private Topology.Vertex _vertex;

		/// <summary>
		/// The vertex currently being visited.
		/// </summary>
		public Topology.Vertex vertex { get { return _vertex; } }

		/// <summary>
		/// Checks if the specified vertex has already been visited.
		/// </summary>
		/// <param name="vertex">The vertex whose visitation status is to be checked.</param>
		/// <returns>True if the vertex has already been visited earlier in the visitation process, and false
		/// if either it has not yet been visited or if the visitor has been told to revisit the it.</returns>
		public bool HasBeenVisited(Topology.Vertex vertex)
		{
			return _visitedVertices[vertex.index];
		}

		/// <summary>
		/// Addes the specified vertex to the queue of vertices to visit.  Its visitation depth will be one
		/// greater than the depth of the vertex currently being visited.
		/// </summary>
		/// <param name="vertex">The vertex to be visited.</param>
		/// <param name="includeVisited">Indicates if the vertex should be added to the queue even if it has already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <remarks><para>If <paramref name="includeVisited"/> is true, the vertex is added to the queue
		/// even if it has already been visited.  Its visitation status will only be checked once it is
		/// popped from the queue, and it will only be visited if it has still not yet been visited.
		/// This can be useful when <see cref="RevisitNeighbor"/> is sometimes called on vertices, but this
		/// particular visitation could take precedence depending on the queue pop order.</para></remarks>
		public void VisitNeighbor(Topology.Vertex vertex, bool includeVisited = false)
		{
			if (includeVisited || !_visitedVertices[vertex.index])
			{
				_queue.Push(new QueueItem(vertex.index, _depth + 1));
			}
		}

		/// <summary>
		/// Addes the specified vertex to the queue of vertices to visit, and marks it as not yet visited.
		/// Its visitation depth will be one greater than the depth of the vertex currently being visited.
		/// </summary>
		/// <param name="vertex">The vertex to be visited.</param>
		/// <remarks><note type="caution">One must be be cautious with this function, as it can lead to an infinite
		/// visitation cycle that never exits.</note></remarks>
		public void RevisitNeighbor(Topology.Vertex vertex)
		{
			_visitedVertices[vertex.index] = false;
			_queue.Push(new QueueItem(vertex.index, _depth + 1));
		}

		/// <summary>
		/// Adds all neighbor vertices of the vertex currently being visited to the visitation queue.
		/// </summary>
		/// <param name="includeVisited">Indicates if the vertex should be added to the queue even if it has already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Vertex.edges"/>
		public void VisitAllNeighbors(bool includeVisited = false)
		{
			foreach (var edge in _vertex.edges)
			{
				if (includeVisited || !_visitedVertices[edge.vertex.index])
				{
					VisitNeighbor(edge.vertex);
				}
			}
		}

		/// <summary>
		/// Adds all neighbor vertices of the vertex currently being visited to the visitation queue, except for the vertex specified.
		/// </summary>
		/// <param name="excludedVertex">The neighbor vertex that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the vertices should be added to the queue even if they have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Vertex.edges"/>
		public void VisitAllNeighborsExcept(Topology.Vertex excludedVertex, bool includeVisited = false)
		{
			foreach (var edge in _vertex.edges)
			{
				if (edge.vertex != excludedVertex && (includeVisited || !_visitedVertices[edge.vertex.index]))
				{
					VisitNeighbor(edge.vertex);
				}
			}
		}

		/// <summary>
		/// Adds all outer neighbor vertices of the vertex currently being visited to the visitation queue.
		/// </summary>
		/// <param name="includeVisited">Indicates if the vertices should be added to the queue even if they have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Vertex.outerEdges"/>
		public void VisitAllOuterNeighbors(bool includeVisited = false)
		{
			foreach (var edge in _vertex.outerEdges)
			{
				if (includeVisited || !_visitedVertices[edge.vertex.index])
				{
					VisitNeighbor(edge.vertex);
				}
			}
		}

		/// <summary>
		/// Adds all outer neighbor vertices of the vertex currently being visited to the visitation queue, except for the vertex specified.
		/// </summary>
		/// <param name="excludedVertex">The outer neighbor vertex that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the vertices should be added to the queue even if they have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Vertex.outerEdges"/>
		public void VisitAllOuterNeighborsExcept(Topology.Vertex excludedVertex, bool includeVisited = false)
		{
			foreach (var edge in _vertex.outerEdges)
			{
				if (edge.vertex != excludedVertex && (includeVisited || !_visitedVertices[edge.vertex.index]))
				{
					VisitNeighbor(edge.vertex);
				}
			}
		}

		/// <summary>
		/// Checks whether the two queue items are in the proper order for a breadth-first visitation order, based on visitation depth.
		/// </summary>
		/// <param name="lhs">The first queue item to compare.</param>
		/// <param name="rhs">The second queue item to compare.</param>
		/// <returns>True if the first queue item should indeed be visited first, and false if the second queue item should instead be visited first.</returns>
		public static bool AreOrderedBreadthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth <= rhs.depth;
		}

		/// <summary>
		/// Checks whether the two queue items are in the proper order for a depth-first visitation order, based on visitation depth.
		/// </summary>
		/// <param name="lhs">The first queue item to compare.</param>
		/// <param name="rhs">The second queue item to compare.</param>
		/// <returns>True if the first queue item should indeed be visited first, and false if the second queue item should instead be visited first.</returns>
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

		/// <summary>
		/// Visits adjacent topology vertices in the order determined by the provided queue, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which vertices are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		public static void Visit(Topology.Vertex rootVertex, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Visit(InitializeQueue(rootVertex, queue), queue, visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in the order determined by the provided queue, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which vertices are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		public static void Visit(IEnumerable<Topology.Vertex> rootVertices, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Visit(InitializeQueue(rootVertices, queue), queue, visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in the order determined by the provided queue, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which vertices are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		public static void Visit<TState>(Topology.Vertex rootVertex, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Visit(InitializeQueue(rootVertex, queue), queue, visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in the order determined by the provided queue, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which vertices are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		public static void Visit<TState>(IEnumerable<Topology.Vertex> rootVertices, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Visit(InitializeQueue(rootVertices, queue), queue, visitDelegate, state);
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

		private static void Visit(Topology topology, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			if (topology == null) return;
			var visitor = new VertexVisitor(topology, queue);
			visitor.Visit(visitDelegate);
		}

		private static void Visit<TState>(Topology topology, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			if (topology == null) return;
			var visitor = new VertexVisitor(topology, queue);
			visitor.Visit(visitDelegate, state);
		}

		private void Visit(VisitDelegate visitDelegate)
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

		private void Visit<TState>(VisitDelegate<TState> visitDelegate, TState state)
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

	/// <summary>
	/// Class for visiting the vertices of a topology, one at a time in some connected order,
	/// while keeping track of a custom distance metric along the way.
	/// </summary>
	/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
	/// <remarks><para>A visitor is an object that maintains a queue of elements ready to
	/// be visited, and a memory of which elements have already been visited.  When a
	/// visitation process is executed, the visitor repeatedly pops one element at a time
	/// and calls a supplied visit delegate with itself as a parameter.  The element just
	/// popped is then accessible through the visitor within the visit delegate.  It is
	/// then up to the visit delegate to push any new elements into the visitors queue,
	/// typically all or some of the immediate neighbors of the element currently being
	/// visited.</para></remarks>
	public class VertexVisitor<TDistance> : TopologyVisitor
	{
		/// <summary>
		/// The data that needs to be stored in a queue for the visitor to know which
		/// vertex to visit next.  It also keeps track of informational data such as
		/// visit depth even if it does not affect visitation order.
		/// </summary>
		public struct QueueItem : IEquatable<QueueItem>
		{
			/// <summary>
			/// The index of the vertex to be visited.
			/// </summary>
			public int vertexIndex;

			/// <summary>
			/// The depth of visitation, which is the number of edges that had to be
			/// traversed to reach the vertex of this queue item.
			/// </summary>
			public int depth;

			/// <summary>
			/// The total distance accumulated during visitation from the root vertex
			/// to the vertex of this queue item.
			/// </summary>
			public TDistance distance;

			/// <summary>
			/// Constructs a queue item with the given vertex index, visitation depth, and visitation distance.
			/// </summary>
			/// <param name="vertexIndex">The index of the vertex to be visited.</param>
			/// <param name="depth">The depth of visitation.</param>
			/// <param name="distance">The distance of visitation.</param>
			public QueueItem(int vertexIndex, int depth, TDistance distance)
			{
				this.vertexIndex = vertexIndex;
				this.depth = depth;
				this.distance = distance;
			}

			/// <summary>
			/// Determines if the current queue item references the same vertex as the specified queue item.
			/// </summary>
			/// <param name="other">The other queue item to compare.</param>
			/// <returns>True if both queue items refer to the same vertex, and false otherwise.</returns>
			public bool Equals(QueueItem other)
			{
				return vertexIndex == other.vertexIndex;
			}
		}

		/// <summary>
		/// A delegate signature which the visitor uses to inform the consumer of each vertex visited.
		/// </summary>
		/// <param name="visitor">The visitor executing the visitation, which stores the current state of visitation.</param>
		public delegate void VisitDelegate(VertexVisitor<TDistance> visitor);

		/// <summary>
		/// A delegate signature which the visitor uses to inform the consumer of each vertex visited.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="visitor">The visitor executing the visitation, which stores the current state of visitation.</param>
		/// <param name="state">The custom state object which was supplied at the beginning of visitation.  Ideally a class or immutable struct.</param>
		public delegate void VisitDelegate<TState>(VertexVisitor<TDistance> visitor, TState state);

		private IQueue<QueueItem> _queue;
		private BitArray _visitedVertices;

		private Topology.Vertex _vertex;
		private TDistance _distance;

		/// <summary>
		/// The vertex currently being visited.
		/// </summary>
		public Topology.Vertex vertex { get { return _vertex; } }

		/// <summary>
		/// The total distance accumulated during visitation from the root vertex
		/// to the vertex currently being visited.
		/// </summary>
		public TDistance distance { get { return _distance; } }

		/// <summary>
		/// Checks if the specified vertex has already been visited.
		/// </summary>
		/// <param name="vertex">The vertex whose visitation status is to be checked.</param>
		/// <returns>True if the vertex has already been visited earlier in the visitation process, and false
		/// if either it has not yet been visited or if the visitor has been told to revisit the it.</returns>
		public bool HasBeenVisited(Topology.Vertex vertex)
		{
			return _visitedVertices[vertex.index];
		}

		/// <summary>
		/// Addes the specified vertex to the queue of vertices to visit.  Its visitation depth will be one
		/// greater than the depth of the vertex currently being visited.
		/// </summary>
		/// <param name="vertex">The vertex to be visited.</param>
		/// <param name="distance">The total visitation distance to the specified vertex, as computed by the visitor consumer.</param>
		/// <param name="includeVisited">Indicates if the vertex should be added to the queue even if it has already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <remarks><para>If <paramref name="includeVisited"/> is true, the vertex is added to the queue
		/// even if it has already been visited.  Its visitation status will only be checked once it is
		/// popped from the queue, and it will only be visited if it has still not yet been visited.
		/// This can be useful when <see cref="RevisitNeighbor"/> is sometimes called on vertices, but this
		/// particular visitation could take precedence depending on the queue pop order.</para></remarks>
		public void VisitNeighbor(Topology.Vertex vertex, TDistance distance, bool includeVisited = false)
		{
			if (includeVisited || !_visitedVertices[vertex.index])
			{
				_queue.Push(new QueueItem(vertex.index, _depth + 1, distance));
			}
		}

		/// <summary>
		/// Addes the specified vertex to the queue of vertices to visit, and marks it as not yet visited.
		/// Its visitation depth will be one greater than the depth of the vertex currently being visited.
		/// </summary>
		/// <param name="vertex">The vertex to be visited.</param>
		/// <param name="distance">The total visitation distance to the specified vertex, as computed by the visitor consumer.</param>
		/// <remarks><note type="caution">One must be be cautious with this function, as it can lead to an infinite
		/// visitation cycle that never exits.</note></remarks>
		public void RevisitNeighbor(Topology.Vertex vertex, TDistance distance)
		{
			_visitedVertices[vertex.index] = false;
			_queue.Push(new QueueItem(vertex.index, _depth + 1, distance));
		}

		/// <summary>
		/// Checks whether the two queue items are in the proper order for a breadth-first visitation order, based on visitation depth.
		/// </summary>
		/// <param name="lhs">The first queue item to compare.</param>
		/// <param name="rhs">The second queue item to compare.</param>
		/// <returns>True if the first queue item should indeed be visited first, and false if the second queue item should instead be visited first.</returns>
		public static bool AreOrderedBreadthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth <= rhs.depth;
		}

		/// <summary>
		/// Checks whether the two queue items are in the proper order for a depth-first visitation order, based on visitation depth.
		/// </summary>
		/// <param name="lhs">The first queue item to compare.</param>
		/// <param name="rhs">The second queue item to compare.</param>
		/// <returns>True if the first queue item should indeed be visited first, and false if the second queue item should instead be visited first.</returns>
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

		/// <summary>
		/// Visits adjacent topology vertices in the order determined by the provided queue, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which vertices are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		public static void Visit(Topology.Vertex rootVertex, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Visit(InitializeQueue(rootVertex, queue), queue, visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in the order determined by the provided queue, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which vertices are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		public static void Visit(IEnumerable<Topology.Vertex> rootVertices, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Visit(InitializeQueue(rootVertices, queue), queue, visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertices in the order determined by the provided queue, starting with the specified root vertex, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertex">The vertex to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which vertices are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		public static void Visit<TState>(Topology.Vertex rootVertex, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Visit(InitializeQueue(rootVertex, queue), queue, visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertices in the order determined by the provided queue, starting with the specified root vertices, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootVertices">The collection of vertices to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which vertices are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		public static void Visit<TState>(IEnumerable<Topology.Vertex> rootVertices, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Visit(InitializeQueue(rootVertices, queue), queue, visitDelegate, state);
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

		private static void Visit(Topology topology, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			if (topology == null) return;
			var visitor = new VertexVisitor<TDistance>(topology, queue);
			visitor.Visit(visitDelegate);
		}

		private static void Visit<TState>(Topology topology, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			if (topology == null) return;
			var visitor = new VertexVisitor<TDistance>(topology, queue);
			visitor.Visit(visitDelegate, state);
		}

		private void Visit(VisitDelegate visitDelegate)
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

		private void Visit<TState>(VisitDelegate<TState> visitDelegate, TState state)
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

	/// <summary>
	/// Class for visiting the vertices of a topology, one at a time in some connected order,
	/// explicitly indicating the edge that was traversed to reach each one.
	/// </summary>
	/// <remarks><para>A visitor is an object that maintains a queue of elements ready to
	/// be visited, and a memory of which elements have already been visited.  When a
	/// visitation process is executed, the visitor repeatedly pops one element at a time
	/// and calls a supplied visit delegate with itself as a parameter.  The element just
	/// popped is then accessible through the visitor within the visit delegate.  It is
	/// then up to the visit delegate to push any new elements into the visitors queue,
	/// typically all or some of the immediate neighbors of the element currently being
	/// visited.</para></remarks>
	public sealed class VertexEdgeVisitor : TopologyVisitor
	{
		/// <summary>
		/// The data that needs to be stored in a queue for the visitor to know which
		/// vertex to visit next, and through which vertex edge.  It also keeps track
		/// of informational data such as visit depth even if it does not affect
		/// visitation order.
		/// </summary>
		public struct QueueItem : IEquatable<QueueItem>
		{
			/// <summary>
			/// The index of the vertex edge to be traversed to visit the far vertex.
			/// </summary>
			public int edgeIndex;

			/// <summary>
			/// The depth of visitation, which is the number of edges that had to be
			/// traversed to reach the vertex edge of this queue item, not counting
			/// the root edge.
			/// </summary>
			public int depth;

			/// <summary>
			/// Constructs a queue item with the given vertex edge index and visitation depth.
			/// </summary>
			/// <param name="edgeIndex">The index of the vertex edge to be visited.</param>
			/// <param name="depth">The depth of visitation.</param>
			public QueueItem(int edgeIndex, int depth)
			{
				this.edgeIndex = edgeIndex;
				this.depth = depth;
			}

			/// <summary>
			/// Determines if the current queue item references the same vertex edge as the specified queue item.
			/// </summary>
			/// <param name="other">The other queue item to compare.</param>
			/// <returns>True if both queue items refer to the same vertex edge, and false otherwise.</returns>
			public bool Equals(QueueItem other)
			{
				return edgeIndex == other.edgeIndex;
			}
		}

		/// <summary>
		/// A delegate signature which the visitor uses to inform the consumer of each vertex visited.
		/// </summary>
		/// <param name="visitor">The visitor executing the visitation, which stores the current state of visitation.</param>
		public delegate void VisitDelegate(VertexEdgeVisitor visitor);

		/// <summary>
		/// A delegate signature which the visitor uses to inform the consumer of each vertex visited.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="visitor">The visitor executing the visitation, which stores the current state of visitation.</param>
		/// <param name="state">The custom state object which was supplied at the beginning of visitation.  Ideally a class or immutable struct.</param>
		public delegate void VisitDelegate<TState>(VertexEdgeVisitor visitor, TState state);

		private IQueue<QueueItem> _queue;
		private BitArray _visitedVertices;

		private Topology.VertexEdge _edge;

		/// <summary>
		/// The vertex edge whose far vertex is currently being visited.
		/// </summary>
		public Topology.VertexEdge edge { get { return _edge; } }

		/// <summary>
		/// The vertex currently being visited.
		/// </summary>
		public Topology.Vertex vertex { get { return _edge.vertex; } }

		/// <summary>
		/// Checks if the specified vertex has already been visited.
		/// </summary>
		/// <param name="vertex">The vertex whose visitation status is to be checked.</param>
		/// <returns>True if the vertex has already been visited earlier in the visitation process, and false
		/// if either it has not yet been visited or if the visitor has been told to revisit the it.</returns>
		public bool HasBeenVisited(Topology.Vertex vertex)
		{
			return _visitedVertices[vertex.index];
		}

		/// <summary>
		/// Addes the specified vertex edge to the queue of edges to visit.  Its visitation depth will be one
		/// greater than the depth of the vertex currently being visited.
		/// </summary>
		/// <param name="edge">The edge whose far vertex is to be visited.</param>
		/// <param name="includeVisited">Indicates if the edge should be added to the queue even if its far vertex has already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <remarks><para>If <paramref name="includeVisited"/> is true, the edge is added to the queue
		/// even if its far vertex has already been visited.  Its far vertex visitation status will only
		/// be checked once it is popped from the queue, and it will only be visited if it has still not
		/// yet been visited. This can be useful when <see cref="RevisitNeighbor"/> is sometimes called
		/// on vertices, but this particular visitation could take precedence depending on the queue pop
		/// order.</para></remarks>
		public void VisitNeighbor(Topology.VertexEdge edge, bool includeVisited = false)
		{
			if (includeVisited || !_visitedVertices[edge.vertex.index])
			{
				_queue.Push(new QueueItem(edge.index, _depth + 1));
			}
		}

		/// <summary>
		/// Addes the specified vertex edge to the queue of edges to visit, and marks it as not yet visited.
		/// Its visitation depth will be one greater than the depth of the vertex currently being visited.
		/// </summary>
		/// <param name="edge">The edge whose far vertex is to be visited.</param>
		/// <remarks><note type="caution">One must be be cautious with this function, as it can lead to an infinite
		/// visitation cycle that never exits.</note></remarks>
		public void RevisitNeighbor(Topology.VertexEdge edge)
		{
			_visitedVertices[edge.vertex.index] = false;
			_queue.Push(new QueueItem(edge.index, _depth + 1));
		}

		/// <summary>
		/// Adds all neighbor edges of the vertex currently being visited to the visitation queue.
		/// </summary>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far vertices have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Vertex.edges"/>
		public void VisitAllNeighbors(bool includeVisited = false)
		{
			foreach (var edge in _edge.vertex.edges)
			{
				if (includeVisited || !_visitedVertices[edge.vertex.index])
				{
					VisitNeighbor(edge);
				}
			}
		}

		/// <summary>
		/// Adds all neighbor edges of the vertex currently being visited to the visitation queue, except for any whose far vertex is the vertex specified.
		/// </summary>
		/// <param name="excludedVertex">The neighbor vertex that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far vertices have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Vertex.edges"/>
		public void VisitAllNeighborsExcept(Topology.Vertex excludedVertex, bool includeVisited = false)
		{
			foreach (var edge in _edge.vertex.edges)
			{
				if (edge.vertex != excludedVertex && (includeVisited || !_visitedVertices[edge.vertex.index]))
				{
					VisitNeighbor(edge);
				}
			}
		}

		/// <summary>
		/// Adds all neighbor edges of the vertex currently being visited to the visitation queue, except for the edge specified.
		/// </summary>
		/// <param name="excludedEdge">The neighbor edge that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far vertices have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Vertex.edges"/>
		public void VisitAllNeighborsExcept(Topology.VertexEdge excludedEdge, bool includeVisited = false)
		{
			foreach (var edge in _edge.vertex.edges)
			{
				if (edge != excludedEdge && (includeVisited || !_visitedVertices[edge.vertex.index]))
				{
					VisitNeighbor(edge);
				}
			}
		}

		/// <summary>
		/// Adds all neighbor edges of the vertex currently being visited to the visitation queue, except for the twin of the edge currently being visited that leads back to the source vertex previously visited.
		/// </summary>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far vertices have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Vertex.edges"/>
		/// <remarks><para>This function is useful in most cases when there is no point in following the
		/// twin edge back to the vertex that was visited before visting the current vertex.</para>
		/// <note type="important">When depth is zero, the twin is included anyway, because its source
		/// vertex was not yet actually visited.</note></remarks>
		public void VisitAllNeighborsExceptSource(bool includeVisited = false)
		{
			if (_depth > 0)
			{
				VisitAllNeighborsExcept(_edge.twin, includeVisited);
			}
			else
			{
				VisitAllNeighbors(includeVisited);
			}
		}

		/// <summary>
		/// Adds all outer neighbor edges of the vertex currently being visited to the visitation queue.
		/// </summary>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far vertices have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Vertex.outerEdges"/>
		public void VisitAllOuterNeighbors(bool includeVisited = false)
		{
			foreach (var edge in _edge.vertex.outerEdges)
			{
				if (includeVisited || !_visitedVertices[edge.vertex.index])
				{
					VisitNeighbor(edge);
				}
			}
		}

		/// <summary>
		/// Adds all outer neighbor edges of the vertex currently being visited to the visitation queue, except for any whose far vertex is the vertex specified.
		/// </summary>
		/// <param name="excludedVertex">The outer neighbor vertex that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far vertices have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Vertex.outerEdges"/>
		public void VisitAllOuterNeighborsExcept(Topology.Vertex excludedVertex, bool includeVisited = false)
		{
			foreach (var edge in _edge.vertex.outerEdges)
			{
				if (edge.vertex != excludedVertex && (includeVisited || !_visitedVertices[edge.vertex.index]))
				{
					VisitNeighbor(edge);
				}
			}
		}

		/// <summary>
		/// Adds all outer neighbor edges of the vertex currently being visited to the visitation queue, except for the edge specified.
		/// </summary>
		/// <param name="excludedEdge">The outer neighbor edge that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far vertices have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Vertex.outerEdges"/>
		public void VisitAllOuterNeighborsExcept(Topology.VertexEdge excludedEdge, bool includeVisited = false)
		{
			foreach (var edge in _edge.vertex.outerEdges)
			{
				if (edge != excludedEdge && (includeVisited || !_visitedVertices[edge.vertex.index]))
				{
					VisitNeighbor(edge);
				}
			}
		}

		/// <summary>
		/// Adds all outer neighbor edges of the vertex currently being visited to the visitation queue, except for the twin of the edge currently being visited that leads back to the source vertex previously visited.
		/// </summary>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far vertices have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Vertex.outerEdges"/>
		/// <remarks><para>This function is useful in most cases when there is no point in following the
		/// twin edge back to the vertex that was visited before visting the current vertex.</para>
		/// <note type="important">When depth is zero, the twin is included anyway, because its source
		/// vertex was not yet actually visited.</note></remarks>
		public void VisitAllOuterNeighborsExceptSource(bool includeVisited = false)
		{
			if (_depth > 0)
			{
				VisitAllOuterNeighborsExcept(_edge.twin, includeVisited);
			}
			else
			{
				VisitAllOuterNeighbors();
			}
		}

		/// <summary>
		/// Checks whether the two queue items are in the proper order for a breadth-first visitation order, based on visitation depth.
		/// </summary>
		/// <param name="lhs">The first queue item to compare.</param>
		/// <param name="rhs">The second queue item to compare.</param>
		/// <returns>True if the first queue item should indeed be visited first, and false if the second queue item should instead be visited first.</returns>
		public static bool AreOrderedBreadthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth <= rhs.depth;
		}

		/// <summary>
		/// Checks whether the two queue items are in the proper order for a depth-first visitation order, based on visitation depth.
		/// </summary>
		/// <param name="lhs">The first queue item to compare.</param>
		/// <param name="rhs">The second queue item to compare.</param>
		/// <returns>True if the first queue item should indeed be visited first, and false if the second queue item should instead be visited first.</returns>
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

		/// <summary>
		/// Visits adjacent topology vertex edges in the order determined by the provided queue, starting with the specified root vertex edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The vertex edge to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which vertex edges are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex edge that is visited.</param>
		public static void Visit(Topology.VertexEdge rootEdge, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Topology topology;
			BitArray visitedVertices;
			if (!Prepare(rootEdge, queue, out topology, out visitedVertices)) return;
			var visitor = new VertexEdgeVisitor(topology, queue, visitedVertices);
			visitor.Visit(visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in the order determined by the provided queue, starting with the specified root vertex edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of vertex edges to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which vertex edges are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex edge that is visited.</param>
		public static void Visit(IEnumerable<Topology.VertexEdge> rootEdges, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Topology topology;
			BitArray visitedVertices;
			if (!Prepare(rootEdges, queue, out topology, out visitedVertices)) return;
			var visitor = new VertexEdgeVisitor(topology, queue, visitedVertices);
			visitor.Visit(visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in the order determined by the provided queue, starting with the specified root vertex edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The vertex edge to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which vertex edges are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		public static void Visit<TState>(Topology.VertexEdge rootEdge, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Topology topology;
			BitArray visitedVertices;
			if (!Prepare(rootEdge, queue, out topology, out visitedVertices)) return;
			var visitor = new VertexEdgeVisitor(topology, queue, visitedVertices);
			visitor.Visit(visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in the order determined by the provided queue, starting with the specified root vertex edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of vertex edges to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which vertex edges are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		public static void Visit<TState>(IEnumerable<Topology.VertexEdge> rootEdges, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Topology topology;
			BitArray visitedVertices;
			if (!Prepare(rootEdges, queue, out topology, out visitedVertices)) return;
			var visitor = new VertexEdgeVisitor(topology, queue, visitedVertices);
			visitor.Visit(visitDelegate, state);
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

		private void Visit(VisitDelegate visitDelegate)
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

		private void Visit<TState>(VisitDelegate<TState> visitDelegate, TState state)
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

	/// <summary>
	/// Class for visiting the vertices of a topology, one at a time in some connected order,
	/// explicitly indicating the edge that was traversed to reach each one, while keeping
	/// track of a custom distance metric along the way.
	/// </summary>
	/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
	/// <remarks><para>A visitor is an object that maintains a queue of elements ready to
	/// be visited, and a memory of which elements have already been visited.  When a
	/// visitation process is executed, the visitor repeatedly pops one element at a time
	/// and calls a supplied visit delegate with itself as a parameter.  The element just
	/// popped is then accessible through the visitor within the visit delegate.  It is
	/// then up to the visit delegate to push any new elements into the visitors queue,
	/// typically all or some of the immediate neighbors of the element currently being
	/// visited.</para></remarks>
	public class VertexEdgeVisitor<TDistance> : TopologyVisitor
	{
		/// <summary>
		/// The data that needs to be stored in a queue for the visitor to know which
		/// vertex to visit next, and through which vertex edge.  It also keeps track
		/// of informational data such as visit depth even if it does not affect
		/// visitation order.
		/// </summary>
		public struct QueueItem : IEquatable<QueueItem>
		{
			/// <summary>
			/// The index of the vertex edge to be traversed to visit the far vertex.
			/// </summary>
			public int edgeIndex;

			/// <summary>
			/// The depth of visitation, which is the number of edges that had to be
			/// traversed to reach the vertex edge of this queue item, not counting
			/// the root edge.
			/// </summary>
			public int depth;

			/// <summary>
			/// The total distance accumulated during visitation from the root vertex
			/// to the vertex of this queue item.
			/// </summary>
			public TDistance distance;

			/// <summary>
			/// Constructs a queue item with the given vertex edge index, visitation depth, and visitation distance.
			/// </summary>
			/// <param name="edgeIndex">The index of the vertex edge to be visited.</param>
			/// <param name="depth">The depth of visitation.</param>
			/// <param name="distance">The distance of visitation.</param>
			public QueueItem(int edgeIndex, int depth, TDistance distance)
			{
				this.edgeIndex = edgeIndex;
				this.depth = depth;
				this.distance = distance;
			}

			/// <summary>
			/// Determines if the current queue item references the same vertex edge as the specified queue item.
			/// </summary>
			/// <param name="other">The other queue item to compare.</param>
			/// <returns>True if both queue items refer to the same vertex edge, and false otherwise.</returns>
			public bool Equals(QueueItem other)
			{
				return edgeIndex == other.edgeIndex;
			}
		}

		/// <summary>
		/// A delegate signature which the visitor uses to inform the consumer of each vertex visited.
		/// </summary>
		/// <param name="visitor">The visitor executing the visitation, which stores the current state of visitation.</param>
		public delegate void VisitDelegate(VertexEdgeVisitor<TDistance> visitor);

		/// <summary>
		/// A delegate signature which the visitor uses to inform the consumer of each vertex visited.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="visitor">The visitor executing the visitation, which stores the current state of visitation.</param>
		/// <param name="state">The custom state object which was supplied at the beginning of visitation.  Ideally a class or immutable struct.</param>
		public delegate void VisitDelegate<TState>(VertexEdgeVisitor<TDistance> visitor, TState state);

		private IQueue<QueueItem> _queue;
		private BitArray _visitedVertices;

		private Topology.VertexEdge _edge;
		private TDistance _distance;

		/// <summary>
		/// The vertex edge whose far vertex is currently being visited.
		/// </summary>
		public Topology.VertexEdge edge { get { return _edge; } }

		/// <summary>
		/// The vertex currently being visited.
		/// </summary>
		public Topology.Vertex vertex { get { return _edge.vertex; } }

		/// <summary>
		/// The total distance accumulated during visitation from the root vertex
		/// to the vertex currently being visited.
		/// </summary>
		public TDistance distance { get { return _distance; } }

		/// <summary>
		/// Checks if the specified vertex has already been visited.
		/// </summary>
		/// <param name="vertex">The vertex whose visitation status is to be checked.</param>
		/// <returns>True if the vertex has already been visited earlier in the visitation process, and false
		/// if either it has not yet been visited or if the visitor has been told to revisit the it.</returns>
		public bool HasBeenVisited(Topology.Vertex vertex)
		{
			return _visitedVertices[vertex.index];
		}

		/// <summary>
		/// Addes the specified vertex edge to the queue of edges to visit.  Its visitation depth will be one
		/// greater than the depth of the vertex currently being visited.
		/// </summary>
		/// <param name="edge">The edge whose far vertex is to be visited.</param>
		/// <param name="distance">The total visitation distance to the specified vertex, as computed by the visitor consumer.</param>
		/// <param name="includeVisited">Indicates if the edge should be added to the queue even if its far vertex has already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <remarks><para>If <paramref name="includeVisited"/> is true, the edge is added to the queue
		/// even if its far vertex has already been visited.  Its far vertex visitation status will only
		/// be checked once it is popped from the queue, and it will only be visited if it has still not
		/// yet been visited. This can be useful when <see cref="RevisitNeighbor"/> is sometimes called
		/// on vertices, but this particular visitation could take precedence depending on the queue pop
		/// order.</para></remarks>
		public void VisitNeighbor(Topology.VertexEdge edge, TDistance distance, bool includeVisited = false)
		{
			if (includeVisited || !_visitedVertices[edge.vertex.index])
			{
				_queue.Push(new QueueItem(edge.index, _depth + 1, distance));
			}
		}

		/// <summary>
		/// Addes the specified vertex edge to the queue of edges to visit, and marks it as not yet visited.
		/// Its visitation depth will be one greater than the depth of the vertex currently being visited.
		/// </summary>
		/// <param name="edge">The edge whose far vertex is to be visited.</param>
		/// <param name="distance">The total visitation distance to the specified vertex, as computed by the visitor consumer.</param>
		/// <remarks><note type="caution">One must be be cautious with this function, as it can lead to an infinite
		/// visitation cycle that never exits.</note></remarks>
		public void RevisitNeighbor(Topology.VertexEdge edge, TDistance distance)
		{
			_visitedVertices[edge.vertex.index] = false;
			_queue.Push(new QueueItem(edge.index, _depth + 1, distance));
		}

		/// <summary>
		/// Checks whether the two queue items are in the proper order for a breadth-first visitation order, based on visitation depth.
		/// </summary>
		/// <param name="lhs">The first queue item to compare.</param>
		/// <param name="rhs">The second queue item to compare.</param>
		/// <returns>True if the first queue item should indeed be visited first, and false if the second queue item should instead be visited first.</returns>
		public static bool AreOrderedBreadthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth <= rhs.depth;
		}

		/// <summary>
		/// Checks whether the two queue items are in the proper order for a depth-first visitation order, based on visitation depth.
		/// </summary>
		/// <param name="lhs">The first queue item to compare.</param>
		/// <param name="rhs">The second queue item to compare.</param>
		/// <returns>True if the first queue item should indeed be visited first, and false if the second queue item should instead be visited first.</returns>
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

		/// <summary>
		/// Visits adjacent topology vertex edges in the order determined by the provided queue, starting with the specified root vertex edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The vertex edge to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which vertex edges are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex edge that is visited.</param>
		public static void Visit(Topology.VertexEdge rootEdge, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Topology topology;
			BitArray visitedVertices;
			if (!Prepare(rootEdge, queue, out topology, out visitedVertices)) return;
			var visitor = new VertexEdgeVisitor<TDistance>(topology, queue, visitedVertices);
			visitor.Visit(visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in the order determined by the provided queue, starting with the specified root vertex edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of vertex edges to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which vertex edges are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex edge that is visited.</param>
		public static void Visit(IEnumerable<Topology.VertexEdge> rootEdges, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Topology topology;
			BitArray visitedVertices;
			if (!Prepare(rootEdges, queue, out topology, out visitedVertices)) return;
			var visitor = new VertexEdgeVisitor<TDistance>(topology, queue, visitedVertices);
			visitor.Visit(visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in the order determined by the provided queue, starting with the specified root vertex edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The vertex edge to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which vertex edges are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		public static void Visit<TState>(Topology.VertexEdge rootEdge, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Topology topology;
			BitArray visitedVertices;
			if (!Prepare(rootEdge, queue, out topology, out visitedVertices)) return;
			var visitor = new VertexEdgeVisitor<TDistance>(topology, queue, visitedVertices);
			visitor.Visit(visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology vertex edges in the order determined by the provided queue, starting with the specified root vertex edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of vertex edges to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which vertex edges are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each vertex edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		public static void Visit<TState>(IEnumerable<Topology.VertexEdge> rootEdges, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Topology topology;
			BitArray visitedVertices;
			if (!Prepare(rootEdges, queue, out topology, out visitedVertices)) return;
			var visitor = new VertexEdgeVisitor<TDistance>(topology, queue, visitedVertices);
			visitor.Visit(visitDelegate, state);
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

		private void Visit(VisitDelegate visitDelegate)
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

		private void Visit<TState>(VisitDelegate<TState> visitDelegate, TState state)
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

	/// <summary>
	/// Class for visiting the faces of a topology, one at a time in some connected order.
	/// </summary>
	/// <remarks><para>A visitor is an object that maintains a queue of elements ready to
	/// be visited, and a memory of which elements have already been visited.  When a
	/// visitation process is executed, the visitor repeatedly pops one element at a time
	/// and calls a supplied visit delegate with itself as a parameter.  The element just
	/// popped is then accessible through the visitor within the visit delegate.  It is
	/// then up to the visit delegate to push any new elements into the visitors queue,
	/// typically all or some of the immediate neighbors of the element currently being
	/// visited.</para></remarks>
	public sealed class FaceVisitor : TopologyVisitor
	{
		/// <summary>
		/// The data that needs to be stored in a queue for the visitor to know which
		/// face to visit next.  It also keeps track of informational data such as
		/// visit depth even if it does not affect visitation order.
		/// </summary>
		public struct QueueItem : IEquatable<QueueItem>
		{
			/// <summary>
			/// The index of the face to be visited.
			/// </summary>
			public int faceIndex;

			/// <summary>
			/// The depth of visitation, which is the number of edges that had to be
			/// traversed to reach the face of this queue item.
			/// </summary>
			public int depth;

			/// <summary>
			/// Constructs a queue item with the given face index and visitation depth.
			/// </summary>
			/// <param name="faceIndex">The index of the face to be visited.</param>
			/// <param name="depth">The depth of visitation.</param>
			public QueueItem(int faceIndex, int depth)
			{
				this.faceIndex = faceIndex;
				this.depth = depth;
			}

			/// <summary>
			/// Determines if the current queue item references the same face as the specified queue item.
			/// </summary>
			/// <param name="other">The other queue item to compare.</param>
			/// <returns>True if both queue items refer to the same face, and false otherwise.</returns>
			public bool Equals(QueueItem other)
			{
				return faceIndex == other.faceIndex;
			}
		}

		/// <summary>
		/// A delegate signature which the visitor uses to inform the consumer of each face visited.
		/// </summary>
		/// <param name="visitor">The visitor executing the visitation, which stores the current state of visitation.</param>
		public delegate void VisitDelegate(FaceVisitor visitor);

		/// <summary>
		/// A delegate signature which the visitor uses to inform the consumer of each face visited.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="visitor">The visitor executing the visitation, which stores the current state of visitation.</param>
		/// <param name="state">The custom state object which was supplied at the beginning of visitation.  Ideally a class or immutable struct.</param>
		public delegate void VisitDelegate<TState>(FaceVisitor visitor, TState state);

		private IQueue<QueueItem> _queue;
		private BitArray _visitedFaces;

		private Topology.Face _face;

		/// <summary>
		/// The face currently being visited.
		/// </summary>
		public Topology.Face face { get { return _face; } }

		/// <summary>
		/// Checks if the specified face has already been visited.
		/// </summary>
		/// <param name="face">The face whose visitation status is to be checked.</param>
		/// <returns>True if the face has already been visited earlier in the visitation process, and false
		/// if either it has not yet been visited or if the visitor has been told to revisit the it.</returns>
		public bool HasBeenVisited(Topology.Face face)
		{
			return _visitedFaces[face.index];
		}

		/// <summary>
		/// Addes the specified face to the queue of faces to visit.  Its visitation depth will be one
		/// greater than the depth of the face currently being visited.
		/// </summary>
		/// <param name="face">The face to be visited.</param>
		/// <param name="includeVisited">Indicates if the face should be added to the queue even if it has already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <remarks><para>If <paramref name="includeVisited"/> is true, the face is added to the queue
		/// even if it has already been visited.  Its visitation status will only be checked once it is
		/// popped from the queue, and it will only be visited if it has still not yet been visited.
		/// This can be useful when <see cref="RevisitNeighbor"/> is sometimes called on faces, but this
		/// particular visitation could take precedence depending on the queue pop order.</para></remarks>
		public void VisitNeighbor(Topology.Face face, bool includeVisited = false)
		{
			if (includeVisited || !_visitedFaces[face.index])
			{
				_queue.Push(new QueueItem(face.index, _depth + 1));
			}
		}

		/// <summary>
		/// Addes the specified face to the queue of faces to visit, and marks it as not yet visited.
		/// Its visitation depth will be one greater than the depth of the face currently being visited.
		/// </summary>
		/// <param name="face">The face to be visited.</param>
		/// <remarks><note type="caution">One must be be cautious with this function, as it can lead to an infinite
		/// visitation cycle that never exits.</note></remarks>
		public void RevisitNeighbor(Topology.Face face)
		{
			_visitedFaces[face.index] = false;
			_queue.Push(new QueueItem(face.index, _depth + 1));
		}

		/// <summary>
		/// Adds all neighbor faces of the face currently being visited to the visitation queue.
		/// </summary>
		/// <param name="includeVisited">Indicates if the face should be added to the queue even if it has already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.edges"/>
		public void VisitAllNeighbors(bool includeVisited = false)
		{
			foreach (var edge in _face.edges)
			{
				if (includeVisited || !_visitedFaces[edge.face.index])
				{
					VisitNeighbor(edge.face);
				}
			}
		}

		/// <summary>
		/// Adds all neighbor faces of the face currently being visited to the visitation queue, except for the face specified.
		/// </summary>
		/// <param name="excludedFace">The neighbor face that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the faces should be added to the queue even if they have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.edges"/>
		public void VisitAllNeighborsExcept(Topology.Face excludedFace, bool includeVisited = false)
		{
			foreach (var edge in _face.edges)
			{
				if (edge.face != excludedFace && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge.face);
				}
			}
		}

		/// <summary>
		/// Adds all internal neighbor faces of the face currently being visited to the visitation queue.
		/// </summary>
		/// <param name="includeVisited">Indicates if the face should be added to the queue even if it has already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.edges"/>
		/// <seealso cref="Topology.Face.isInternal"/>
		public void VisitInternalNeighbors(bool includeVisited = false)
		{
			foreach (var edge in _face.edges)
			{
				if (!edge.isExternal && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge.face);
				}
			}
		}

		/// <summary>
		/// Adds all internal neighbor faces of the face currently being visited to the visitation queue, except for the face specified.
		/// </summary>
		/// <param name="excludedFace">The neighbor face that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the faces should be added to the queue even if they have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.edges"/>
		/// <seealso cref="Topology.Face.isInternal"/>
		public void VisitInternalNeighborsExcept(Topology.Face excludedFace, bool includeVisited = false)
		{
			foreach (var edge in _face.edges)
			{
				if (!edge.isExternal && edge.face != excludedFace && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge.face);
				}
			}
		}

		/// <summary>
		/// Adds all outer neighbor faces of the face currently being visited to the visitation queue.
		/// </summary>
		/// <param name="includeVisited">Indicates if the faces should be added to the queue even if they have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.outerEdges"/>
		public void VisitAllOuterNeighbors(bool includeVisited = false)
		{
			foreach (var edge in _face.outerEdges)
			{
				if (includeVisited || !_visitedFaces[edge.face.index])
				{
					VisitNeighbor(edge.face);
				}
			}
		}

		/// <summary>
		/// Adds all outer neighbor faces of the face currently being visited to the visitation queue, except for the face specified.
		/// </summary>
		/// <param name="excludedFace">The outer neighbor face that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the faces should be added to the queue even if they have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.outerEdges"/>
		public void VisitAllOuterNeighborsExcept(Topology.Face excludedFace, bool includeVisited = false)
		{
			foreach (var edge in _face.outerEdges)
			{
				if (edge.face != excludedFace && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge.face);
				}
			}
		}

		/// <summary>
		/// Adds all internal outer neighbor faces of the face currently being visited to the visitation queue.
		/// </summary>
		/// <param name="includeVisited">Indicates if the faces should be added to the queue even if they have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.outerEdges"/>
		/// <seealso cref="Topology.Face.isInternal"/>
		public void VisitInternalOuterNeighbors(bool includeVisited = false)
		{
			foreach (var edge in _face.outerEdges)
			{
				if (!edge.isExternal && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge.face);
				}
			}
		}

		/// <summary>
		/// Adds all internal outer neighbor faces of the face currently being visited to the visitation queue, except for the face specified.
		/// </summary>
		/// <param name="excludedFace">The outer neighbor face that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the faces should be added to the queue even if they have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.outerEdges"/>
		/// <seealso cref="Topology.Face.isInternal"/>
		public void VisitInternalOuterNeighbors(Topology.Face excludedFace, bool includeVisited = false)
		{
			foreach (var edge in _face.outerEdges)
			{
				if (!edge.isExternal && edge.face != excludedFace && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge.face);
				}
			}
		}

		/// <summary>
		/// Checks whether the two queue items are in the proper order for a breadth-first visitation order, based on visitation depth.
		/// </summary>
		/// <param name="lhs">The first queue item to compare.</param>
		/// <param name="rhs">The second queue item to compare.</param>
		/// <returns>True if the first queue item should indeed be visited first, and false if the second queue item should instead be visited first.</returns>
		public static bool AreOrderedBreadthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth <= rhs.depth;
		}

		/// <summary>
		/// Checks whether the two queue items are in the proper order for a depth-first visitation order, based on visitation depth.
		/// </summary>
		/// <param name="lhs">The first queue item to compare.</param>
		/// <param name="rhs">The second queue item to compare.</param>
		/// <returns>True if the first queue item should indeed be visited first, and false if the second queue item should instead be visited first.</returns>
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

		/// <summary>
		/// Visits adjacent topology faces in the order determined by the provided queue, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which faces are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		public static void Visit(Topology.Face rootFace, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Visit(InitializeQueue(rootFace, queue), queue, visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in the order determined by the provided queue, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which faces are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		public static void Visit(IEnumerable<Topology.Face> rootFaces, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Visit(InitializeQueue(rootFaces, queue), queue, visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in the order determined by the provided queue, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which faces are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		public static void Visit<TState>(Topology.Face rootFace, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Visit(InitializeQueue(rootFace, queue), queue, visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology faces in the order determined by the provided queue, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which faces are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		public static void Visit<TState>(IEnumerable<Topology.Face> rootFaces, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Visit(InitializeQueue(rootFaces, queue), queue, visitDelegate, state);
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

		private static void Visit(Topology topology, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			if (topology == null) return;
			var visitor = new FaceVisitor(topology, queue);
			visitor.Visit(visitDelegate);
		}

		private static void Visit<TState>(Topology topology, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			if (topology == null) return;
			var visitor = new FaceVisitor(topology, queue);
			visitor.Visit(visitDelegate, state);
		}

		private void Visit(VisitDelegate visitDelegate)
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

		private void Visit<TState>(VisitDelegate<TState> visitDelegate, TState state)
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

	/// <summary>
	/// Class for visiting the faces of a topology, one at a time in some connected order,
	/// while keeping track of a custom distance metric along the way.
	/// </summary>
	/// <typeparam name="TState">The type of the distance metric.</typeparam>
	/// <remarks><para>A visitor is an object that maintains a queue of elements ready to
	/// be visited, and a memory of which elements have already been visited.  When a
	/// visitation process is executed, the visitor repeatedly pops one element at a time
	/// and calls a supplied visit delegate with itself as a parameter.  The element just
	/// popped is then accessible through the visitor within the visit delegate.  It is
	/// then up to the visit delegate to push any new elements into the visitors queue,
	/// typically all or some of the immediate neighbors of the element currently being
	/// visited.</para></remarks>
	public class FaceVisitor<TState> : TopologyVisitor
	{
		/// <summary>
		/// The data that needs to be stored in a queue for the visitor to know which
		/// face to visit next.  It also keeps track of informational data such as
		/// visit depth even if it does not affect visitation order.
		/// </summary>
		public struct QueueItem : IEquatable<QueueItem>
		{
			/// <summary>
			/// The index of the face to be visited.
			/// </summary>
			public int faceIndex;

			/// <summary>
			/// The depth of visitation, which is the number of edges that had to be
			/// traversed to reach the face of this queue item.
			/// </summary>
			public int depth;

			/// <summary>
			/// The total distance accumulated during visitation from the root face
			/// to the face of this queue item.
			/// </summary>
			public TState state;

			/// <summary>
			/// Constructs a queue item with the given face index, visitation depth, and visitation distance.
			/// </summary>
			/// <param name="faceIndex">The index of the face to be visited.</param>
			/// <param name="depth">The depth of visitation.</param>
			/// <param name="distance">The distance of visitation.</param>
			public QueueItem(int faceIndex, int depth, TState state)
			{
				this.faceIndex = faceIndex;
				this.depth = depth;
				this.state = state;
			}

			/// <summary>
			/// Determines if the current queue item references the same face as the specified queue item.
			/// </summary>
			/// <param name="other">The other queue item to compare.</param>
			/// <returns>True if both queue items refer to the same face, and false otherwise.</returns>
			public bool Equals(QueueItem other)
			{
				return faceIndex == other.faceIndex;
			}
		}

		public struct FaceStatePair
		{
			public Topology.Face face;
			public TState state;

			public FaceStatePair(Topology.Face face)
			{
				this.face = face;
				state = default(TState);
			}

			public FaceStatePair(Topology.Face face, TState state)
			{
				this.face = face;
				this.state = state;
			}
		}

		/// <summary>
		/// A delegate signature which the visitor uses to inform the consumer of each face visited.
		/// </summary>
		/// <param name="visitor">The visitor executing the visitation, which stores the current state of visitation.</param>
		public delegate void VisitDelegate(FaceVisitor<TState> visitor);

		/// <summary>
		/// A delegate signature which the visitor uses to inform the consumer of each face visited.
		/// </summary>
		/// <typeparam name="TSharedState">The type of the custom state object.</typeparam>
		/// <param name="visitor">The visitor executing the visitation, which stores the current state of visitation.</param>
		/// <param name="state">The custom state object which was supplied at the beginning of visitation.  Ideally a class or immutable struct.</param>
		public delegate void VisitDelegate<TSharedState>(FaceVisitor<TState> visitor, TSharedState sharedState);

		private IQueue<QueueItem> _queue;
		private BitArray _visitedFaces;

		private Topology.Face _face;
		private TState _state;

		/// <summary>
		/// The face currently being visited.
		/// </summary>
		public Topology.Face face { get { return _face; } }

		/// <summary>
		/// The total distance accumulated during visitation from the root face
		/// to the face currently being visited.
		/// </summary>
		public TState state { get { return _state; } }

		/// <summary>
		/// Checks if the specified face has already been visited.
		/// </summary>
		/// <param name="face">The face whose visitation status is to be checked.</param>
		/// <returns>True if the face has already been visited earlier in the visitation process, and false
		/// if either it has not yet been visited or if the visitor has been told to revisit the it.</returns>
		public bool HasBeenVisited(Topology.Face face)
		{
			return _visitedFaces[face.index];
		}

		/// <summary>
		/// Addes the specified face to the queue of faces to visit.  Its visitation depth will be one
		/// greater than the depth of the face currently being visited.
		/// </summary>
		/// <param name="face">The face to be visited.</param>
		/// <param name="distance">The total visitation distance to the specified face, as computed by the visitor consumer.</param>
		/// <param name="includeVisited">Indicates if the face should be added to the queue even if it has already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <remarks><para>If <paramref name="includeVisited"/> is true, the face is added to the queue
		/// even if it has already been visited.  Its visitation status will only be checked once it is
		/// popped from the queue, and it will only be visited if it has still not yet been visited.
		/// This can be useful when <see cref="RevisitNeighbor"/> is sometimes called on faces, but this
		/// particular visitation could take precedence depending on the queue pop order.</para></remarks>
		public void VisitNeighbor(Topology.Face face, TState state, bool includeVisited = false)
		{
			if (includeVisited || !_visitedFaces[face.index])
			{
				_queue.Push(new QueueItem(face.index, _depth + 1, state));
			}
		}

		/// <summary>
		/// Addes the specified face to the queue of faces to visit, and marks it as not yet visited.
		/// Its visitation depth will be one greater than the depth of the face currently being visited.
		/// </summary>
		/// <param name="face">The face to be visited.</param>
		/// <param name="distance">The total visitation distance to the specified face, as computed by the visitor consumer.</param>
		/// <remarks><note type="caution">One must be be cautious with this function, as it can lead to an infinite
		/// visitation cycle that never exits.</note></remarks>
		public void RevisitNeighbor(Topology.Face face, TState state)
		{
			_visitedFaces[face.index] = false;
			_queue.Push(new QueueItem(face.index, _depth + 1, state));
		}

		/// <summary>
		/// Adds all neighbor faces of the face currently being visited to the visitation queue.
		/// </summary>
		/// <param name="includeVisited">Indicates if the face should be added to the queue even if it has already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.edges"/>
		public void VisitAllNeighbors(TState state, bool includeVisited = false)
		{
			foreach (var edge in _face.edges)
			{
				if (includeVisited || !_visitedFaces[edge.face.index])
				{
					VisitNeighbor(edge.face, state);
				}
			}
		}

		/// <summary>
		/// Adds all neighbor faces of the face currently being visited to the visitation queue, except for the face specified.
		/// </summary>
		/// <param name="excludedFace">The neighbor face that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the faces should be added to the queue even if they have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.edges"/>
		public void VisitAllNeighborsExcept(TState state, Topology.Face excludedFace, bool includeVisited = false)
		{
			foreach (var edge in _face.edges)
			{
				if (edge.face != excludedFace && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge.face, state);
				}
			}
		}

		/// <summary>
		/// Adds all internal neighbor faces of the face currently being visited to the visitation queue.
		/// </summary>
		/// <param name="includeVisited">Indicates if the face should be added to the queue even if it has already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.edges"/>
		/// <seealso cref="Topology.Face.isInternal"/>
		public void VisitInternalNeighbors(TState state, bool includeVisited = false)
		{
			foreach (var edge in _face.edges)
			{
				if (!edge.isExternal && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge.face, state);
				}
			}
		}

		/// <summary>
		/// Adds all internal neighbor faces of the face currently being visited to the visitation queue, except for the face specified.
		/// </summary>
		/// <param name="excludedFace">The neighbor face that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the faces should be added to the queue even if they have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.edges"/>
		/// <seealso cref="Topology.Face.isInternal"/>
		public void VisitInternalNeighborsExcept(TState state, Topology.Face excludedFace, bool includeVisited = false)
		{
			foreach (var edge in _face.edges)
			{
				if (!edge.isExternal && edge.face != excludedFace && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge.face, state);
				}
			}
		}

		/// <summary>
		/// Adds all outer neighbor faces of the face currently being visited to the visitation queue.
		/// </summary>
		/// <param name="includeVisited">Indicates if the faces should be added to the queue even if they have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.outerEdges"/>
		public void VisitAllOuterNeighbors(TState state, bool includeVisited = false)
		{
			foreach (var edge in _face.outerEdges)
			{
				if (includeVisited || !_visitedFaces[edge.face.index])
				{
					VisitNeighbor(edge.face, state);
				}
			}
		}

		/// <summary>
		/// Adds all outer neighbor faces of the face currently being visited to the visitation queue, except for the face specified.
		/// </summary>
		/// <param name="excludedFace">The outer neighbor face that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the faces should be added to the queue even if they have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.outerEdges"/>
		public void VisitAllOuterNeighborsExcept(TState state, Topology.Face excludedFace, bool includeVisited = false)
		{
			foreach (var edge in _face.outerEdges)
			{
				if (edge.face != excludedFace && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge.face, state);
				}
			}
		}

		/// <summary>
		/// Adds all internal outer neighbor faces of the face currently being visited to the visitation queue.
		/// </summary>
		/// <param name="includeVisited">Indicates if the faces should be added to the queue even if they have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.outerEdges"/>
		/// <seealso cref="Topology.Face.isInternal"/>
		public void VisitInternalOuterNeighbors(TState state, bool includeVisited = false)
		{
			foreach (var edge in _face.outerEdges)
			{
				if (!edge.isExternal && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge.face, state);
				}
			}
		}

		/// <summary>
		/// Adds all internal outer neighbor faces of the face currently being visited to the visitation queue, except for the face specified.
		/// </summary>
		/// <param name="excludedFace">The outer neighbor face that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the faces should be added to the queue even if they have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.outerEdges"/>
		/// <seealso cref="Topology.Face.isInternal"/>
		public void VisitInternalOuterNeighbors(TState state, Topology.Face excludedFace, bool includeVisited = false)
		{
			foreach (var edge in _face.outerEdges)
			{
				if (!edge.isExternal && edge.face != excludedFace && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge.face, state);
				}
			}
		}

		/// <summary>
		/// Checks whether the two queue items are in the proper order for a breadth-first visitation order, based on visitation depth.
		/// </summary>
		/// <param name="lhs">The first queue item to compare.</param>
		/// <param name="rhs">The second queue item to compare.</param>
		/// <returns>True if the first queue item should indeed be visited first, and false if the second queue item should instead be visited first.</returns>
		public static bool AreOrderedBreadthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth <= rhs.depth;
		}

		/// <summary>
		/// Checks whether the two queue items are in the proper order for a depth-first visitation order, based on visitation depth.
		/// </summary>
		/// <param name="lhs">The first queue item to compare.</param>
		/// <param name="rhs">The second queue item to compare.</param>
		/// <returns>True if the first queue item should indeed be visited first, and false if the second queue item should instead be visited first.</returns>
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

		/// <summary>
		/// Visits adjacent topology faces in the order determined by the provided queue, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which faces are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		public static void Visit(Topology.Face rootFace, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Visit(InitializeQueue(rootFace, queue), queue, visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in the order determined by the provided queue, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which faces are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		public static void Visit(Topology.Face rootFace, TState state, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Visit(InitializeQueue(rootFace, state, queue), queue, visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in the order determined by the provided queue, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which faces are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		public static void Visit(FaceStatePair rootFace, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Visit(InitializeQueue(rootFace.face, rootFace.state, queue), queue, visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in the order determined by the provided queue, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which faces are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		public static void Visit(IEnumerable<Topology.Face> rootFaces, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Visit(InitializeQueue(rootFaces, queue), queue, visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in the order determined by the provided queue, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which faces are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		public static void Visit(IEnumerable<FaceStatePair> rootFaces, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Visit(InitializeQueue(rootFaces, queue), queue, visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology faces in the order determined by the provided queue, starting with the specified root face, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TSharedState">The type of the custom state object.</typeparam>
		/// <param name="rootFace">The face to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which faces are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		public static void Visit<TSharedState>(Topology.Face rootFace, IQueue<QueueItem> queue, VisitDelegate<TSharedState> visitDelegate, TSharedState sharedState)
		{
			Visit(InitializeQueue(rootFace, queue), queue, visitDelegate, sharedState);
		}

		/// <summary>
		/// Visits adjacent topology faces in the order determined by the provided queue, starting with the specified root faces, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TSharedState">The type of the custom state object.</typeparam>
		/// <param name="rootFaces">The collection of faces to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which faces are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		public static void Visit<TSharedState>(IEnumerable<Topology.Face> rootFaces, IQueue<QueueItem> queue, VisitDelegate<TSharedState> visitDelegate, TSharedState sharedState)
		{
			Visit(InitializeQueue(rootFaces, queue), queue, visitDelegate, sharedState);
		}

		private static Topology InitializeQueue(Topology.Face rootFace, IQueue<QueueItem> queue)
		{
			queue.Push(new QueueItem(rootFace.index, 0, default(TState)));
			return rootFace.topology;
		}

		private static Topology InitializeQueue(Topology.Face rootFace, TState state, IQueue<QueueItem> queue)
		{
			queue.Push(new QueueItem(rootFace.index, 0, state));
			return rootFace.topology;
		}

		private static Topology InitializeQueue(IEnumerable<Topology.Face> rootFaces, IQueue<QueueItem> queue)
		{
			var rootFacesEnumerator = rootFaces.GetEnumerator();
			if (!rootFacesEnumerator.MoveNext()) return null;

			var topology = rootFacesEnumerator.Current.topology;

			do
			{
				queue.Push(new QueueItem(rootFacesEnumerator.Current.index, 0, default(TState)));
			} while (rootFacesEnumerator.MoveNext());

			return topology;
		}

		private static Topology InitializeQueue(IEnumerable<FaceStatePair> rootFaces, IQueue<QueueItem> queue)
		{
			var rootFacesEnumerator = rootFaces.GetEnumerator();
			if (!rootFacesEnumerator.MoveNext()) return null;

			var topology = rootFacesEnumerator.Current.face.topology;

			do
			{
				var faceStatePair = rootFacesEnumerator.Current;
				queue.Push(new QueueItem(faceStatePair.face.index, 0, faceStatePair.state));
			} while (rootFacesEnumerator.MoveNext());

			return topology;
		}

		private static void Visit(Topology topology, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			if (topology == null) return;
			var visitor = new FaceVisitor<TState>(topology, queue);
			visitor.Visit(visitDelegate);
		}

		private static void Visit<TSharedState>(Topology topology, IQueue<QueueItem> queue, VisitDelegate<TSharedState> visitDelegate, TSharedState sharedState)
		{
			if (topology == null) return;
			var visitor = new FaceVisitor<TState>(topology, queue);
			visitor.Visit(visitDelegate, sharedState);
		}

		private void Visit(VisitDelegate visitDelegate)
		{
			while (_queue.isEmpty == false)
			{
				var queueItem = _queue.Pop();

				// Move on to the next queue item if we've already visited the target face.
				if (_visitedFaces[queueItem.faceIndex] == true) continue;

				// Visit the face and get the list of adjacent neighbors.
				_face = new Topology.Face(_topology, queueItem.faceIndex);
				_depth = queueItem.depth;
				_state = queueItem.state;
				_break = _ignore = false;
				visitDelegate(this);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedFaces[queueItem.faceIndex] = true;
			}
		}

		private void Visit<TSharedState>(VisitDelegate<TSharedState> visitDelegate, TSharedState sharedState)
		{
			while (_queue.isEmpty == false)
			{
				var queueItem = _queue.Pop();

				// Move on to the next queue item if we've already visited the target face.
				if (_visitedFaces[queueItem.faceIndex] == true) continue;

				// Visit the face and get the list of adjacent neighbors.
				_face = new Topology.Face(_topology, queueItem.faceIndex);
				_depth = queueItem.depth;
				_state = queueItem.state;
				_break = _ignore = false;
				visitDelegate(this, sharedState);

				// Check the visitation action and respond accordingly.
				if (_break) return;
				if (!_ignore) _visitedFaces[queueItem.faceIndex] = true;
			}
		}
	}

	/// <summary>
	/// Class for visiting the faces of a topology, one at a time in some connected order,
	/// explicitly indicating the edge that was traversed to reach each one.
	/// </summary>
	/// <remarks><para>A visitor is an object that maintains a queue of elements ready to
	/// be visited, and a memory of which elements have already been visited.  When a
	/// visitation process is executed, the visitor repeatedly pops one element at a time
	/// and calls a supplied visit delegate with itself as a parameter.  The element just
	/// popped is then accessible through the visitor within the visit delegate.  It is
	/// then up to the visit delegate to push any new elements into the visitors queue,
	/// typically all or some of the immediate neighbors of the element currently being
	/// visited.</para></remarks>
	public sealed class FaceEdgeVisitor : TopologyVisitor
	{
		/// <summary>
		/// The data that needs to be stored in a queue for the visitor to know which
		/// face to visit next, and through which face edge.  It also keeps track
		/// of informational data such as visit depth even if it does not affect
		/// visitation order.
		/// </summary>
		public struct QueueItem : IEquatable<QueueItem>
		{
			/// <summary>
			/// The index of the face edge to be traversed to visit the far face.
			/// </summary>
			public int edgeIndex;

			/// <summary>
			/// The depth of visitation, which is the number of edges that had to be
			/// traversed to reach the face edge of this queue item, not counting
			/// the root edge.
			/// </summary>
			public int depth;

			/// <summary>
			/// Constructs a queue item with the given face edge index and visitation depth.
			/// </summary>
			/// <param name="edgeIndex">The index of the face edge to be visited.</param>
			/// <param name="depth">The depth of visitation.</param>
			public QueueItem(int edgeIndex, int depth)
			{
				this.edgeIndex = edgeIndex;
				this.depth = depth;
			}

			/// <summary>
			/// Determines if the current queue item references the same face edge as the specified queue item.
			/// </summary>
			/// <param name="other">The other queue item to compare.</param>
			/// <returns>True if both queue items refer to the same face edge, and false otherwise.</returns>
			public bool Equals(QueueItem other)
			{
				return edgeIndex == other.edgeIndex;
			}
		}

		/// <summary>
		/// A delegate signature which the visitor uses to inform the consumer of each face visited.
		/// </summary>
		/// <param name="visitor">The visitor executing the visitation, which stores the current state of visitation.</param>
		public delegate void VisitDelegate(FaceEdgeVisitor visitor);

		/// <summary>
		/// A delegate signature which the visitor uses to inform the consumer of each face visited.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="visitor">The visitor executing the visitation, which stores the current state of visitation.</param>
		/// <param name="state">The custom state object which was supplied at the beginning of visitation.  Ideally a class or immutable struct.</param>
		public delegate void VisitDelegate<TState>(FaceEdgeVisitor visitor, TState state);

		private IQueue<QueueItem> _queue;
		private BitArray _visitedFaces;

		private Topology.FaceEdge _edge;

		/// <summary>
		/// The face edge whose far face is currently being visited.
		/// </summary>
		public Topology.FaceEdge edge { get { return _edge; } }

		/// <summary>
		/// The face currently being visited.
		/// </summary>
		public Topology.Face face { get { return _edge.face; } }

		/// <summary>
		/// Checks if the specified face has already been visited.
		/// </summary>
		/// <param name="face">The face whose visitation status is to be checked.</param>
		/// <returns>True if the face has already been visited earlier in the visitation process, and false
		/// if either it has not yet been visited or if the visitor has been told to revisit the it.</returns>
		public bool HasBeenVisited(Topology.Face face)
		{
			return _visitedFaces[face.index];
		}

		/// <summary>
		/// Addes the specified face edge to the queue of edges to visit.  Its visitation depth will be one
		/// greater than the depth of the face currently being visited.
		/// </summary>
		/// <param name="edge">The edge whose far face is to be visited.</param>
		/// <param name="includeVisited">Indicates if the edge should be added to the queue even if its far face has already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <remarks><para>If <paramref name="includeVisited"/> is true, the edge is added to the queue
		/// even if its far face has already been visited.  Its far face visitation status will only
		/// be checked once it is popped from the queue, and it will only be visited if it has still not
		/// yet been visited. This can be useful when <see cref="RevisitNeighbor"/> is sometimes called
		/// on faces, but this particular visitation could take precedence depending on the queue pop
		/// order.</para></remarks>
		public void VisitNeighbor(Topology.FaceEdge edge, bool includeVisited = false)
		{
			if (includeVisited || !_visitedFaces[edge.face.index])
			{
				_queue.Push(new QueueItem(edge.index, _depth + 1));
			}
		}

		/// <summary>
		/// Addes the specified face edge to the queue of edges to visit, and marks it as not yet visited.
		/// Its visitation depth will be one greater than the depth of the face currently being visited.
		/// </summary>
		/// <param name="edge">The edge whose far face is to be visited.</param>
		/// <remarks><note type="caution">One must be be cautious with this function, as it can lead to an infinite
		/// visitation cycle that never exits.</note></remarks>
		public void RevisitNeighbor(Topology.FaceEdge edge)
		{
			_visitedFaces[edge.farFace.index] = false;
			_queue.Push(new QueueItem(edge.index, _depth + 1));
		}

		/// <summary>
		/// Adds all neighbor edges of the face currently being visited to the visitation queue.
		/// </summary>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far faces have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.edges"/>
		public void VisitAllNeighbors(bool includeVisited = false)
		{
			foreach (var edge in _edge.face.edges)
			{
				if (includeVisited || !_visitedFaces[edge.face.index])
				{
					VisitNeighbor(edge);
				}
			}
		}

		/// <summary>
		/// Adds all neighbor edges of the face currently being visited to the visitation queue, except for any whose far face is the face specified.
		/// </summary>
		/// <param name="excludedFace">The neighbor face that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far faces have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.edges"/>
		public void VisitAllNeighborsExcept(Topology.Face excludedFace, bool includeVisited = false)
		{
			foreach (var edge in _edge.face.edges)
			{
				if (edge.face != excludedFace && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge);
				}
			}
		}

		/// <summary>
		/// Adds all neighbor edges of the face currently being visited to the visitation queue, except for the edge specified.
		/// </summary>
		/// <param name="excludedEdge">The neighbor edge that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far faces have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.edges"/>
		public void VisitAllNeighborsExcept(Topology.FaceEdge excludedEdge, bool includeVisited = false)
		{
			foreach (var edge in _edge.face.edges)
			{
				if (edge != excludedEdge && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge);
				}
			}
		}

		/// <summary>
		/// Adds all neighbor edges of the face currently being visited to the visitation queue, except for the twin of the edge currently being visited that leads back to the source face previously visited.
		/// </summary>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far faces have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.edges"/>
		/// <remarks><para>This function is useful in most cases when there is no point in following the
		/// twin edge back to the face that was visited before visting the current face.</para>
		/// <note type="important">When depth is zero, the twin is included anyway, because its source
		/// face was not yet actually visited.</note></remarks>
		public void VisitAllNeighborsExceptSource(bool includeVisited = false)
		{
			if (_depth > 0)
			{
				VisitAllNeighborsExcept(_edge.twin, includeVisited);
			}
			else
			{
				VisitAllNeighbors();
			}
		}

		/// <summary>
		/// Adds all internal neighbor edges of the face currently being visited to the visitation queue.
		/// </summary>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far faces have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.edges"/>
		public void VisitInternalNeighbors(bool includeVisited = false)
		{
			foreach (var edge in _edge.face.edges)
			{
				if (!edge.isExternal && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge);
				}
			}
		}

		/// <summary>
		/// Adds all internal neighbor edges of the face currently being visited to the visitation queue, except for any whose far face is the face specified.
		/// </summary>
		/// <param name="excludedFace">The neighbor face that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far faces have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.edges"/>
		public void VisitInternalNeighborsExcept(Topology.Face excludedFace, bool includeVisited = true)
		{
			foreach (var edge in _edge.face.edges)
			{
				if (!edge.isExternal && edge.face != excludedFace)
				{
					VisitNeighbor(edge);
				}
			}
		}

		/// <summary>
		/// Adds all internal neighbor edges of the face currently being visited to the visitation queue, except for the edge specified.
		/// </summary>
		/// <param name="excludedEdge">The neighbor edge that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far faces have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.edges"/>
		public void VisitInternalNeighborsExcept(Topology.FaceEdge excludedEdge, bool includeVisited = true)
		{
			foreach (var edge in _edge.face.edges)
			{
				if (!edge.isExternal && edge != excludedEdge && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge);
				}
			}
		}

		/// <summary>
		/// Adds all internal neighbor edges of the face currently being visited to the visitation queue, except for the twin of the edge currently being visited that leads back to the source face previously visited.
		/// </summary>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far faces have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.edges"/>
		/// <remarks><para>This function is useful in most cases when there is no point in following the
		/// twin edge back to the face that was visited before visting the current face.</para>
		/// <note type="important">When depth is zero, the twin is included anyway, because its source
		/// face was not yet actually visited.</note></remarks>
		public void VisitInternalNeighborsExceptSource(bool includeVisited = false)
		{
			if (_depth > 0)
			{
				VisitInternalNeighborsExcept(_edge.twin, includeVisited);
			}
			else
			{
				VisitInternalNeighbors();
			}
		}

		/// <summary>
		/// Adds all outer neighbor edges of the face currently being visited to the visitation queue.
		/// </summary>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far faces have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.outerEdges"/>
		public void VisitAllOuterNeighbors(bool includeVisited = false)
		{
			foreach (var edge in _edge.face.outerEdges)
			{
				if (includeVisited || !_visitedFaces[edge.face.index])
				{
					VisitNeighbor(edge);
				}
			}
		}

		/// <summary>
		/// Adds all outer neighbor edges of the face currently being visited to the visitation queue, except for any whose far face is the face specified.
		/// </summary>
		/// <param name="excludedFace">The outer neighbor face that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far faces have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.outerEdges"/>
		public void VisitAllOuterNeighborsExcept(Topology.Face excludedFace, bool includeVisited = false)
		{
			foreach (var edge in _edge.face.outerEdges)
			{
				if (edge.face != excludedFace && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge);
				}
			}
		}

		/// <summary>
		/// Adds all outer neighbor edges of the face currently being visited to the visitation queue, except for the edge specified.
		/// </summary>
		/// <param name="excludedEdge">The outer neighbor edge that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far faces have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.outerEdges"/>
		public void VisitAllOuterNeighborsExcept(Topology.FaceEdge excludedEdge, bool includeVisited = false)
		{
			foreach (var edge in _edge.face.outerEdges)
			{
				if (edge != excludedEdge && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge);
				}
			}
		}

		/// <summary>
		/// Adds all outer neighbor edges of the face currently being visited to the visitation queue, except for the twin of the edge currently being visited that leads back to the source face previously visited.
		/// </summary>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far faces have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.outerEdges"/>
		/// <remarks><para>This function is useful in most cases when there is no point in following the
		/// twin edge back to the face that was visited before visting the current face.</para>
		/// <note type="important">When depth is zero, the twin is included anyway, because its source
		/// face was not yet actually visited.</note></remarks>
		public void VisitAllOuterNeighborsExceptSource(bool includeVisited = false)
		{
			if (_depth > 0)
			{
				VisitAllOuterNeighborsExcept(_edge.twin, includeVisited);
			}
			else
			{
				VisitAllOuterNeighbors();
			}
		}

		/// <summary>
		/// Adds all internal outer neighbor edges of the face currently being visited to the visitation queue.
		/// </summary>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far faces have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.outerEdges"/>
		public void VisitInternalOuterNeighbors(bool includeVisited = false)
		{
			foreach (var edge in _edge.face.outerEdges)
			{
				if (!edge.isExternal && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge);
				}
			}
		}

		/// <summary>
		/// Adds all internal outer neighbor edges of the face currently being visited to the visitation queue, except for any whose far face is the face specified.
		/// </summary>
		/// <param name="excludedFace">The outer neighbor face that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far faces have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.outerEdges"/>
		public void VisitInternalOuterNeighborsExcept(Topology.Face excludedFace, bool includeVisited = false)
		{
			foreach (var edge in _edge.face.outerEdges)
			{
				if (!edge.isExternal && edge.face != excludedFace && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge);
				}
			}
		}

		/// <summary>
		/// Adds all internal outer neighbor edges of the face currently being visited to the visitation queue, except for the edge specified.
		/// </summary>
		/// <param name="excludedEdge">The outer neighbor edge that should not be added to the visitation queue.</param>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far faces have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.outerEdges"/>
		public void VisitInternalOuterNeighborsExcept(Topology.FaceEdge excludedEdge, bool includeVisited = false)
		{
			foreach (var edge in _edge.face.outerEdges)
			{
				if (!edge.isExternal && edge != excludedEdge && (includeVisited || !_visitedFaces[edge.face.index]))
				{
					VisitNeighbor(edge);
				}
			}
		}

		/// <summary>
		/// Adds all internal outer neighbor edges of the face currently being visited to the visitation queue, except for the twin of the edge currently being visited that leads back to the source face previously visited.
		/// </summary>
		/// <param name="includeVisited">Indicates if the edges should be added to the queue even if their far faces have already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <seealso cref="Topology.Face.outerEdges"/>
		/// <remarks><para>This function is useful in most cases when there is no point in following the
		/// twin edge back to the face that was visited before visting the current face.</para>
		/// <note type="important">When depth is zero, the twin is included anyway, because its source
		/// face was not yet actually visited.</note></remarks>
		public void VisitInternalOuterNeighborsExceptSource(bool includeVisited = false)
		{
			if (_depth > 0)
			{
				VisitInternalOuterNeighborsExcept(_edge.twin, includeVisited);
			}
			else
			{
				VisitInternalOuterNeighbors();
			}
		}

		/// <summary>
		/// Checks whether the two queue items are in the proper order for a breadth-first visitation order, based on visitation depth.
		/// </summary>
		/// <param name="lhs">The first queue item to compare.</param>
		/// <param name="rhs">The second queue item to compare.</param>
		/// <returns>True if the first queue item should indeed be visited first, and false if the second queue item should instead be visited first.</returns>
		public static bool AreOrderedBreadthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth <= rhs.depth;
		}

		/// <summary>
		/// Checks whether the two queue items are in the proper order for a depth-first visitation order, based on visitation depth.
		/// </summary>
		/// <param name="lhs">The first queue item to compare.</param>
		/// <param name="rhs">The second queue item to compare.</param>
		/// <returns>True if the first queue item should indeed be visited first, and false if the second queue item should instead be visited first.</returns>
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

		/// <summary>
		/// Visits adjacent topology face edges in the order determined by the provided queue, starting with the specified root face edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The face edge to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which face edges are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face edge that is visited.</param>
		public static void Visit(Topology.FaceEdge rootEdge, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Topology topology;
			BitArray visitedFaces;
			if (!Prepare(rootEdge, queue, out topology, out visitedFaces)) return;
			var visitor = new FaceEdgeVisitor(topology, queue, visitedFaces);
			visitor.Visit(visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in the order determined by the provided queue, starting with the specified root face edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of face edges to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which face edges are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face edge that is visited.</param>
		public static void Visit(IEnumerable<Topology.FaceEdge> rootEdges, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Topology topology;
			BitArray visitedFaces;
			if (!Prepare(rootEdges, queue, out topology, out visitedFaces)) return;
			var visitor = new FaceEdgeVisitor(topology, queue, visitedFaces);
			visitor.Visit(visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in the order determined by the provided queue, starting with the specified root face edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The face edge to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which face edges are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		public static void Visit<TState>(Topology.FaceEdge rootEdge, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Topology topology;
			BitArray visitedFaces;
			if (!Prepare(rootEdge, queue, out topology, out visitedFaces)) return;
			var visitor = new FaceEdgeVisitor(topology, queue, visitedFaces);
			visitor.Visit(visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in the order determined by the provided queue, starting with the specified root face edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of face edges to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which face edges are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		public static void Visit<TState>(IEnumerable<Topology.FaceEdge> rootEdges, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Topology topology;
			BitArray visitedFaces;
			if (!Prepare(rootEdges, queue, out topology, out visitedFaces)) return;
			var visitor = new FaceEdgeVisitor(topology, queue, visitedFaces);
			visitor.Visit(visitDelegate, state);
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

		private void Visit(VisitDelegate visitDelegate)
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

		private void Visit<TState>(VisitDelegate<TState> visitDelegate, TState state)
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

	/// <summary>
	/// Class for visiting the faces of a topology, one at a time in some connected order,
	/// explicitly indicating the edge that was traversed to reach each one, while keeping
	/// track of a custom distance metric along the way.
	/// </summary>
	/// <typeparam name="TDistance">The type of the distance metric.</typeparam>
	/// <remarks><para>A visitor is an object that maintains a queue of elements ready to
	/// be visited, and a memory of which elements have already been visited.  When a
	/// visitation process is executed, the visitor repeatedly pops one element at a time
	/// and calls a supplied visit delegate with itself as a parameter.  The element just
	/// popped is then accessible through the visitor within the visit delegate.  It is
	/// then up to the visit delegate to push any new elements into the visitors queue,
	/// typically all or some of the immediate neighbors of the element currently being
	/// visited.</para></remarks>
	public class FaceEdgeVisitor<TDistance> : TopologyVisitor
	{
		/// <summary>
		/// The data that needs to be stored in a queue for the visitor to know which
		/// face to visit next, and through which face edge.  It also keeps track
		/// of informational data such as visit depth even if it does not affect
		/// visitation order.
		/// </summary>
		public struct QueueItem : IEquatable<QueueItem>
		{
			/// <summary>
			/// The index of the face edge to be traversed to visit the far face.
			/// </summary>
			public int edgeIndex;

			/// <summary>
			/// The depth of visitation, which is the number of edges that had to be
			/// traversed to reach the face edge of this queue item, not counting
			/// the root edge.
			/// </summary>
			public int depth;

			/// <summary>
			/// The total distance accumulated during visitation from the root face
			/// to the face of this queue item.
			/// </summary>
			public TDistance distance;

			/// <summary>
			/// Constructs a queue item with the given face edge index, visitation depth, and visitation distance.
			/// </summary>
			/// <param name="edgeIndex">The index of the face edge to be visited.</param>
			/// <param name="depth">The depth of visitation.</param>
			/// <param name="distance">The distance of visitation.</param>
			public QueueItem(int edgeIndex, int depth, TDistance distance)
			{
				this.edgeIndex = edgeIndex;
				this.depth = depth;
				this.distance = distance;
			}

			/// <summary>
			/// Determines if the current queue item references the same face edge as the specified queue item.
			/// </summary>
			/// <param name="other">The other queue item to compare.</param>
			/// <returns>True if both queue items refer to the same face edge, and false otherwise.</returns>
			public bool Equals(QueueItem other)
			{
				return edgeIndex == other.edgeIndex;
			}
		}

		/// <summary>
		/// A delegate signature which the visitor uses to inform the consumer of each face visited.
		/// </summary>
		/// <param name="visitor">The visitor executing the visitation, which stores the current state of visitation.</param>
		public delegate void VisitDelegate(FaceEdgeVisitor<TDistance> visitor);

		/// <summary>
		/// A delegate signature which the visitor uses to inform the consumer of each face visited.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="visitor">The visitor executing the visitation, which stores the current state of visitation.</param>
		/// <param name="state">The custom state object which was supplied at the beginning of visitation.  Ideally a class or immutable struct.</param>
		public delegate void VisitDelegate<TState>(FaceEdgeVisitor<TDistance> visitor, TState state);

		private IQueue<QueueItem> _queue;
		private BitArray _visitedFaces;

		private Topology.FaceEdge _edge;
		private TDistance _distance;

		/// <summary>
		/// The face edge whose far face is currently being visited.
		/// </summary>
		public Topology.FaceEdge edge { get { return _edge; } }

		/// <summary>
		/// The face currently being visited.
		/// </summary>
		public Topology.Face face { get { return _edge.face; } }

		/// <summary>
		/// The total distance accumulated during visitation from the root face
		/// to the face currently being visited.
		/// </summary>
		public TDistance distance { get { return _distance; } }

		/// <summary>
		/// Checks if the specified face has already been visited.
		/// </summary>
		/// <param name="face">The face whose visitation status is to be checked.</param>
		/// <returns>True if the face has already been visited earlier in the visitation process, and false
		/// if either it has not yet been visited or if the visitor has been told to revisit the it.</returns>
		public bool HasBeenVisited(Topology.Face face)
		{
			return _visitedFaces[face.index];
		}

		/// <summary>
		/// Addes the specified face edge to the queue of edges to visit.  Its visitation depth will be one
		/// greater than the depth of the face currently being visited.
		/// </summary>
		/// <param name="edge">The edge whose far face is to be visited.</param>
		/// <param name="distance">The total visitation distance to the specified face, as computed by the visitor consumer.</param>
		/// <param name="includeVisited">Indicates if the edge should be added to the queue even if its far face has already been visited.  Possibly useful in the presence of calls to <see cref="RevisitNeighbor"/>.</param>
		/// <remarks><para>If <paramref name="includeVisited"/> is true, the edge is added to the queue
		/// even if its far face has already been visited.  Its far face visitation status will only
		/// be checked once it is popped from the queue, and it will only be visited if it has still not
		/// yet been visited. This can be useful when <see cref="RevisitNeighbor"/> is sometimes called
		/// on faces, but this particular visitation could take precedence depending on the queue pop
		/// order.</para></remarks>
		public void VisitNeighbor(Topology.FaceEdge edge, TDistance distance, bool includeVisited = false)
		{
			if (includeVisited || !_visitedFaces[edge.face.index])
			{
				_queue.Push(new QueueItem(edge.index, _depth + 1, distance));
			}
		}

		/// <summary>
		/// Addes the specified face edge to the queue of edges to visit, and marks it as not yet visited.
		/// Its visitation depth will be one greater than the depth of the face currently being visited.
		/// </summary>
		/// <param name="edge">The edge whose far face is to be visited.</param>
		/// <param name="distance">The total visitation distance to the specified face, as computed by the visitor consumer.</param>
		/// <remarks><note type="caution">One must be be cautious with this function, as it can lead to an infinite
		/// visitation cycle that never exits.</note></remarks>
		public void RevisitNeighbor(Topology.FaceEdge edge, TDistance distance)
		{
			_visitedFaces[edge.farFace.index] = false;
			_queue.Push(new QueueItem(edge.index, _depth + 1, distance));
		}

		/// <summary>
		/// Checks whether the two queue items are in the proper order for a breadth-first visitation order, based on visitation depth.
		/// </summary>
		/// <param name="lhs">The first queue item to compare.</param>
		/// <param name="rhs">The second queue item to compare.</param>
		/// <returns>True if the first queue item should indeed be visited first, and false if the second queue item should instead be visited first.</returns>
		public static bool AreOrderedBreadthFirst(QueueItem lhs, QueueItem rhs)
		{
			return lhs.depth <= rhs.depth;
		}

		/// <summary>
		/// Checks whether the two queue items are in the proper order for a depth-first visitation order, based on visitation depth.
		/// </summary>
		/// <param name="lhs">The first queue item to compare.</param>
		/// <param name="rhs">The second queue item to compare.</param>
		/// <returns>True if the first queue item should indeed be visited first, and false if the second queue item should instead be visited first.</returns>
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

		/// <summary>
		/// Visits adjacent topology face edges in the order determined by the provided queue, starting with the specified root face edge, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdge">The face edge to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which face edges are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face edge that is visited.</param>
		public static void Visit(Topology.FaceEdge rootEdge, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Topology topology;
			BitArray visitedFaces;
			if (!Prepare(rootEdge, queue, out topology, out visitedFaces)) return;
			var visitor = new FaceEdgeVisitor<TDistance>(topology, queue, visitedFaces);
			visitor.Visit(visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in the order determined by the provided queue, starting with the specified root face edges, using the supplied visit delegate.
		/// </summary>
		/// <param name="rootEdges">The collection of face edges to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which face edges are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face edge that is visited.</param>
		public static void Visit(IEnumerable<Topology.FaceEdge> rootEdges, IQueue<QueueItem> queue, VisitDelegate visitDelegate)
		{
			Topology topology;
			BitArray visitedFaces;
			if (!Prepare(rootEdges, queue, out topology, out visitedFaces)) return;
			var visitor = new FaceEdgeVisitor<TDistance>(topology, queue, visitedFaces);
			visitor.Visit(visitDelegate);
		}

		/// <summary>
		/// Visits adjacent topology face edges in the order determined by the provided queue, starting with the specified root face edge, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdge">The face edge to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which face edges are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		public static void Visit<TState>(Topology.FaceEdge rootEdge, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Topology topology;
			BitArray visitedFaces;
			if (!Prepare(rootEdge, queue, out topology, out visitedFaces)) return;
			var visitor = new FaceEdgeVisitor<TDistance>(topology, queue, visitedFaces);
			visitor.Visit(visitDelegate, state);
		}

		/// <summary>
		/// Visits adjacent topology face edges in the order determined by the provided queue, starting with the specified root face edges, using the supplied visit delegate.
		/// </summary>
		/// <typeparam name="TState">The type of the custom state object.</typeparam>
		/// <param name="rootEdges">The collection of face edges to visit first.</param>
		/// <param name="queue">The queue that will determine the order in which face edges are popped and visited.</param>
		/// <param name="visitDelegate">The delegate that will be called for each face edge that is visited.</param>
		/// <param name="state">The custom state object which will be passed to the delegate along with the visitor.  Ideally a class or immutable struct.</param>
		public static void Visit<TState>(IEnumerable<Topology.FaceEdge> rootEdges, IQueue<QueueItem> queue, VisitDelegate<TState> visitDelegate, TState state)
		{
			Topology topology;
			BitArray visitedFaces;
			if (!Prepare(rootEdges, queue, out topology, out visitedFaces)) return;
			var visitor = new FaceEdgeVisitor<TDistance>(topology, queue, visitedFaces);
			visitor.Visit(visitDelegate, state);
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

		private void Visit(VisitDelegate visitDelegate)
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

		private void Visit<TState>(VisitDelegate<TState> visitDelegate, TState state)
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
#endif
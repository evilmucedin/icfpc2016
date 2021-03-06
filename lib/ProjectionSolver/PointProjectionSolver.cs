﻿using System.Collections.Generic;
using System.Linq;
using lib.Graphs;
using lib.ProjectionSolver;

namespace lib
{
	public class EdgeInfo
	{
		public Segment segment;
		public Rational length;

		public override string ToString()
		{
			return segment.ToString();
		}
	}

	public class NodeInfo
	{
		public Vector Location;
		public List<Vector> Projections = new List<Vector>();

		public override string ToString()
		{
			return new string('*', Projections.Count);
		}
	}

	public class PPath
	{
		public PPath() { }

		public PPath(PPath other)
		{
			edges = other.edges.ToList();
			length = other.length;
			originalityByVertices = other.originalityByVertices;
			straightness = other.straightness;
			metric = other.metric;
		}

		public List<Edge<EdgeInfo, NodeInfo>> edges;

		public Edge<EdgeInfo, NodeInfo> LastEdge
		{
			get { return edges[edges.Count - 1]; }
		}

		public Edge<EdgeInfo, NodeInfo> FirstEdge
		{
			get { return edges[0]; }
		}

		public Rational length;
        public double originalityByVertices, originalityByEdges;
        public double straightness;
        public double metric;

		public IEnumerable<int> NodeNumbers => edges
			.Select(edge => edge.From.NodeNumber)
			.Concat(new[] { LastEdge.To.NodeNumber });

		public override string ToString()
		{
			return $"{metric} {length} : {string.Join(" ", NodeNumbers)}";
		}
	}

	public class PointProjectionSolver
	{
		#region data classes 

		public class ProjectedNodeInfo
		{
			public Node<EdgeInfo, NodeInfo> Original;
			public Vector Projection;
		}

		public class ProjectedEdgeInfo
		{
		}

		#endregion

		public Graph<ProjectedEdgeInfo, ProjectedNodeInfo> Projection;
		public ProblemSpec spec;
		public List<Segment> AllSegments;
		public List<Vector> vectors;
		public Graph<EdgeInfo, NodeInfo> Graph;
        public List<SegmentFamily> SegmentFamilies;
		public Projection ProjectionScheme;
	}
}
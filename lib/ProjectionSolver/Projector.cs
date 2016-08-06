﻿using lib;
using lib.Graphs;
using Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{

    public class NodeProjection
    {
        public Node<EdgeInfo, NodeInfo> Original;
        public Vector Projection;
    }

    public class EdgeProjection
    {
        public NodeProjection begin;
        public NodeProjection end;
        public List<Segment> Segments;
    }

    public class ProjectionStage
    {
        public List<NodeProjection> Nodes = new List<NodeProjection>();
        public List<EdgeProjection> Edges = new List<EdgeProjection>();
    }

     public partial class Projection
     {
        public Graph<EdgeInfo, NodeInfo> Graph;
        public List<Segment> AllSegments;
        public Stack<ProjectionStage> Stages = new Stack<ProjectionStage>();
        internal List<SegmentFamily> SegmentsFamily;

        public IEnumerable<NodeProjection> AllNodeProjections
        {
            get
            {

                return Stages.SelectMany(z => z.Nodes);
            }
        }

        public IEnumerable<EdgeProjection> AllEdgeProjections
        {
            get
            {
                return Stages.SelectMany(z => z.Edges);
            }
        }

        public Dictionary<Node<EdgeInfo, NodeInfo>, List<NodeProjection>> GetNodeFunction()
        {
            var dict = Graph.Nodes.ToDictionary(z => z, z => new List<NodeProjection>());
            foreach (var e in AllNodeProjections)
                dict[e.Original].Add(e);
            return dict;
        }

        public Dictionary<Segment, List<EdgeProjection>> GetEdgeFunction()
        {
            var dict = AllSegments.ToDictionary(z => z, z => new List<EdgeProjection>());
            foreach (var e in AllEdgeProjections)
                foreach (var ee in e.Segments)
                    dict[ee].Add(e);
            return dict;
        }



        public bool IsCompleteProjection()
        {
            var goodNodes = AllNodeProjections.Select(z => z.Original).Distinct().Count();
            if (goodNodes != Graph.NodesCount) return false;
            var goodEdges = AllEdgeProjections.SelectMany(z => z.Segments).Distinct().Count();
            if (goodEdges != AllSegments.Count) return false;
            return true;
        }
    }



    public static class Projector
    {
        public static Projection CreateProjection(List<SegmentFamily> families, List<Segment> allSegments, Graph<EdgeInfo, NodeInfo> graph)
        {
            var p = new Projection();
            p.AllSegments = allSegments.ToList();
            p.Graph = graph;
            p.SegmentsFamily = families.ToList();
            return p;
        }


        public static ProjectionStage CreateInitialProjection(List<PPath> square, Projection p)
        {
            ProjectionStage stage = new ProjectionStage();

            var corners = new[] { new Vector(0, 0), new Vector(0, 1), new Vector(1, 1), new Vector(1, 0) };
            var direction = new[] { new Vector(0, 1), new Vector(1, 0), new Vector(0, -1), new Vector(-1, 0) };


            for (int i = 0; i < 4; i++)
            {
                Rational len = 0;
                for (int k = 0; k < square[i].edges.Count; k++)
                {
                    var location = corners[i] + direction[i] * len;
                    var node = square[i].edges[k].From;

                    var pr = new NodeProjection { Original = node, Projection = location };
                    stage.Nodes.Add(pr);
                    len += square[i].edges[k].Data.length;
                }
            }

            int ptr = 0;
            foreach (var s in square)
                foreach (var e in s.edges)
                {
                    Segment seg = e.Data.segment;
                    if (!p.AllSegments.Contains(seg))
                    {
                        seg = new Segment(e.Data.segment.End, e.Data.segment.Start);
                        if (!p.AllSegments.Contains(seg))
                            throw new Exception();
                    }
                    var begin = stage.Nodes[ptr];
                    var end = stage.Nodes[(ptr + 1) % stage.Nodes.Count];
                    ptr++;

                    stage.Edges.Add(new EdgeProjection { begin = begin, end = end, Segments = new List<Segment> { e.Data.segment } });
                }
            return stage;
        }

        public class SegmentFamilySubset
        {
            public SegmentFamily family;
            public int start;
            public int count;
            public Segment[] Insides { get; private set; }
            public Rational QuadratLength { get; private set; }

            public SegmentFamilySubset(SegmentFamily family, int start, int count)
            {
                this.family = family;
                this.start = start;
                this.count = count;
                Insides = family.Segments.Skip(start).Take(count).ToArray();
                QuadratLength = new Segment(Begin, End).QuadratOfLength;
                Segment = new Segment(Begin, End);
            }



            public Vector Begin
            {
                get { return family.Points[start]; }
            }

            public Vector End
            {
                get { return family.Points[start+count]; }
            }

            public Segment Segment { get; internal set; }
        }

        public class CurrentState
        {
            public Dictionary<Node<EdgeInfo, NodeInfo>, List<NodeProjection>> nodesMap;
            public Dictionary<Segment, List<EdgeProjection>> edgesMap;


            public Node<EdgeInfo, NodeInfo> GetNode(Vector p)
            {
                return nodesMap.Keys.Where(z => z.Data.Location.Equals(p)).FirstOrDefault();
            }

            public bool SegmentIsCovered(SegmentFamilySubset subs)
            {
                return subs.Insides.All(z => edgesMap[z].Count != 0);
            }

            public bool IsMapped(Node<EdgeInfo,NodeInfo> node)
            {
                return node != null && nodesMap[node].Count != 0;
            }
        }



        public static CurrentState GetCurrentState(Projection p)
        {
            var ep = new CurrentState();
            ep.edgesMap = p.GetEdgeFunction();
            ep.nodesMap = p.GetNodeFunction();
            return ep;
        }



        public static EdgeProjection TryInsertFamily(SegmentFamilySubset subset, CurrentState state)
        {

            if (state.SegmentIsCovered(subset)) return null;
            var start = state.GetNode(subset.Begin);
            if (start == null) return null;
            var end = state.GetNode(subset.End);
            if (end == null) return null;

            foreach (var s in state.nodesMap[start])
                foreach (var f in state.nodesMap[end])
                {
                    var length = new Segment(s.Projection, f.Projection).QuadratOfLength;
                    if (length != new Segment(subset.Begin, subset.End).QuadratOfLength) continue;
                    var result = new EdgeProjection { begin = s, end = f, Segments = subset.Insides.ToList() };
                    return result;
                }
            return null;
        }




        public static IEnumerable<SegmentFamilySubset> GetAllPossibleSegments(List<SegmentFamily> family)
        {
            foreach (var f in family)
                for (int size = 1; size < f.Segments.Length + 1; size++)
                    for (int start = 0; start <= f.Segments.Length - size; start++)
                        yield return new SegmentFamilySubset(f, start, size);
        }

        public static ProjectionStage AddVeryGoodEdges(Projection p)
        {

            var stage = new ProjectionStage();

            var ep = GetCurrentState(p);

            foreach (var subs in GetAllPossibleSegments(p.SegmentsFamily))
            {
                var res = TryInsertFamily(subs, ep);
                if (res != null)
                {
                    stage.Edges.Add(res);
                    return stage;
                }
            }
            return null;
        }


        public class AdjoinedSegmentFamilySubset
        {
            public SegmentFamilySubset family;
            public Node<EdgeInfo, NodeInfo> ProjectedNode;
            public Node<EdgeInfo, NodeInfo> NonProjectedNode;

        }

        public static IEnumerable<int[]> GetCounting(int[] sizes)
        {
            var result = new int[sizes.Length];
            while(true)
            {
                bool ok = false;
                for(int position=0;position<sizes.Length;position++)
                {
                    result[position]++;
                    if (result[position]>=sizes[position])
                    {
                        result[position] = 0;
                        continue;
                    }
                    else
                    {
                        ok = true;
                        break;
                    }
                }
                if (!ok) break;
                yield return result;
            }
        }


        public static IEnumerable<ProjectionStage> TrySquashPoint(CurrentState state, List<AdjoinedSegmentFamilySubset> edges)
        {
            var sizes = edges.Select(z => state.nodesMap[z.ProjectedNode].Count).ToArray();
            foreach (var p in GetCounting(sizes))
            {
                var proj = new NodeProjection[sizes.Length];
                for (int i = 0; i < sizes.Length; i++)
                    proj[i] = state.nodesMap[edges[i].ProjectedNode][p[i]];
                var vars = Arithmetic.RationalTriangulate(edges[0].family.Segment, edges[1].family.Segment, proj[0].Projection, proj[1].Projection);
                if (vars == null) continue;
                foreach(var v in vars)
                {
                    if (v.X < 0 || v.X > 1 || v.Y < 0 || v.Y > 1) continue;

                    var stage = new ProjectionStage();
                    var sq= new NodeProjection { Original = edges[0].NonProjectedNode, Projection = v };
                    stage.Nodes.Add(sq);
                    bool ok = true;
                    for (int k=0;k<sizes.Length;k++)
                    {
                        var len = new Segment(v, proj[k].Projection);
                        if (len.QuadratOfLength != edges[k].family.Segment.QuadratOfLength)
                        {
                            ok = false;
                            break;
                        }
                        stage.Edges.Add(new EdgeProjection { begin = sq, end = proj[k], Segments = edges[k].family.Insides.ToList() });
                    }
                    if (ok)
                        yield return stage;
                }
            }
        }

        public static IEnumerable<ProjectionStage> FindSquashPoint(Projection p)
        {
            var state = GetCurrentState(p);
            var store = new Dictionary<Node<EdgeInfo,NodeInfo>, List<AdjoinedSegmentFamilySubset>>();
            foreach(var e in GetAllPossibleSegments(p.SegmentsFamily))
            {
                var start = state.GetNode(e.Begin);
                var end = state.GetNode(e.End);

                var startFound = state.IsMapped(start);
                var endFound = state.IsMapped(end);

                if (startFound && endFound) continue;
                if (!startFound && !endFound) continue;

                var a = new AdjoinedSegmentFamilySubset {  family = e };
                a.ProjectedNode = startFound ? start : end;
                a.NonProjectedNode = startFound ? end : start;
                if (!store.ContainsKey(a.NonProjectedNode)) store[a.NonProjectedNode] = new List<AdjoinedSegmentFamilySubset>();
                store[a.NonProjectedNode].Add(a);
            }

            store = store.Where(z => z.Value.Count >= 2).ToDictionary(z => z.Key, z => z.Value);

            foreach (var e in store)
                foreach (var st in TrySquashPoint(state, e.Value))
                    yield return st;
        }
    }
}
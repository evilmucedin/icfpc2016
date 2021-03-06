using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace lib
{
	public class Polygon : IEquatable<Polygon>
	{
		public int Id;
		public readonly Vector[] Vertices;
		public readonly Segment[] Segments;

		public bool IsReflected = false;

		public Polygon(params Vector[] vertices)
		{
			Vertices = vertices;
			Segments = BuildSegments(vertices).ToArray();
		}

		public bool Equals(Polygon other)
		{
			if (other.Vertices.Length != Vertices.Length)
				return false;
			return !Vertices.Where((t, i) => !other.Vertices[i].Equals(t)).Any();
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Polygon) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = 0;
				foreach (var vector in Vertices)
					hashCode = hashCode * 397 ^ vector.GetHashCode();
				return hashCode;
			}
		}

		private static List<Segment> BuildSegments(Vector[] vertices)
		{
			var segments = new List<Segment>();
			for (int i = 0; i < vertices.Length; i++)
			{
				var vertex1 = vertices[i];
				var vertex2 = vertices[(i + 1)%vertices.Length];
				segments.Add(new Segment(vertex1, vertex2));
			}
			return segments;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendLine(Vertices.Length.ToString());
			sb.Append(Vertices.StrJoin(Environment.NewLine));
			return sb.ToString();
		}

		public static Polygon Parse(StringReader reader)
		{
			var vCount = int.Parse(reader.ReadLine() ?? "0");
			var ps = Enumerable.Range(0, vCount)
				.Select(i => reader.ReadLine())
				.Select(Vector.Parse)
				.ToArray();
			return new Polygon(ps);
		}

		public Polygon Move(Rational shiftX, Rational shiftY)
		{
			return new Polygon(Vertices.Select(p => new Vector(p.X + shiftX, p.Y + shiftY)).ToArray());
		}

		public Polygon Reflect(Segment mirror)
		{
			var polygon = new Polygon(Vertices.Select(v => v.Reflect(mirror)).ToArray()) { IsReflected = !IsReflected, Id = Id };
			for (int i = 0; i < Segments.Length; i++)
			{
				polygon.Segments[i].Id = Segments[i].Id;
			}
			return polygon;
		}

		public Rational GetUnsignedSquare()
		{
			var s = GetSignedSquare();
			return s > 0 ? s : -s;
		}

		public Rational GetSignedSquare() => GetSignedSquare(Vertices);

		public static Rational GetSignedSquare(IList<Vector> vertices)
		{
			Rational sum = 0;
			for (int i = 0; i < vertices.Count; i++)
			{
				var p1 = vertices[i];
				var p2 = vertices[(i + 1) % vertices.Count];
				sum += (p1.X - p2.X) * (p1.Y + p2.Y) / 2;
			}
			return sum;
		}

		public bool IsConvex()
		{
			var signedSq = GetSignedSquare();
			for (int i = 0; i < Segments.Length; i++)
			{
				var thisEdge = Segments[i];
				var nextEdge = Segments[(i + 1)%Segments.Length];
				var prod = thisEdge.ToVector().VectorProdLength(nextEdge.ToVector());
				if ((signedSq > 0 && prod <= 0) || (signedSq < 0 && prod >= 0))
					return false;
			}
			return true;
		}

		public Polygon RemoveExtraVertices()
		{
			var vertices = new List<Vector>(Vertices);
			while (true)
			{
				var changed = false;
				for (int i = 1; i < vertices.Count + 1; i++)
				{
					var thisVertex = vertices[i % vertices.Count];
					var thisEdge = new Segment(vertices[(i - 1) % vertices.Count], thisVertex);
					var nextEdge = new Segment(thisVertex, vertices[(i + 1) % vertices.Count]);
					var prod = thisEdge.ToVector().VectorProdLength(nextEdge.ToVector());
					if (prod == 0)
					{
						vertices.Remove(thisVertex);
						changed = true;
						break;
					}
				}
				if (!changed)
					break;
			}
			return new Polygon(vertices.ToArray());
		}

		public Polygon GetConvexBoundary()
		{
			var vertices = Vertices.ToList();
			var zero = vertices.OrderBy(v => v.Y).ThenBy(v => v.X).First();
			vertices = vertices.OrderByDescending(v => (v - zero).ScalarProd(new Vector(1, 0))/(v - zero).Length).ToList();
			var signedSq = GetSignedSquare(vertices);
			while (true)
			{
				var changed = false;
				for (int i = 1; i < vertices.Count + 1; i++)
				{
					var thisVertex = vertices[i%vertices.Count];
					var thisEdge = new Segment(vertices[(i - 1)% vertices.Count], thisVertex);
					var nextEdge = new Segment(thisVertex, vertices[(i + 1)% vertices.Count]);
					var prod = thisEdge.ToVector().VectorProdLength(nextEdge.ToVector());
					if ((signedSq > 0 && prod <= 0) || (signedSq < 0 && prod >= 0))
					{
						vertices.Remove(thisVertex);
						changed = true;
						break;
					}
				}
				if (!changed)
					break;
			}
			return new Polygon(vertices.ToArray());
		}
	}

	[TestFixture]
	public class Polygon_Should
	{
		[TestCase("0,0 1,0 1,1 0,1", "1")]
		[TestCase("0,0 1,0 1,1", "1/2")]
		[TestCase("0,0 1000000000000000000,0 1000000000000000000,1000000000000000000", "500000000000000000000000000000000000")]
		[TestCase("0,0 0,1 1,1 1,0", "-1")]
		public void CalcSquare(string poly, string expectedSquare)
		{
			var polygon = new Polygon(poly.Split(' ').Select(Vector.Parse).ToArray());
			var s = polygon.GetSignedSquare();
			s.Should().Be(Rational.Parse(expectedSquare));
		}

		[TestCase("0,0 0,1 1,1 1,0", "1,0 1,1 0,1 0,0")]
		[TestCase("0,0 1/2,1/2 0,1 1,1 1,0", "1,0 1,1 0,1 0,0")]
		[TestCase("0,0 0,1 1/2,1/2 1,1 1,0", "1,0 1,1 0,1 0,0")]
		[TestCase("0,0 0,1 1,1 1/2,1/2 1,0", "1,0 1,1 0,1 0,0")]
		[TestCase("1,0 1,1 0,1 0,0", "1,0 1,1 0,1 0,0")]
		[TestCase("0,0 1/2,1/2 1,0 1,1 0,1", "1,0 1,1 0,1 0,0")]
		[TestCase("0,0 1,0 1,1 3/4,1/2 1/2,3/4 1/4,1/2 0,1", "1,0 1,1 0,1 0,0")]
		[TestCase("0,0 0,1 1/2,1 1,1 1,0", "1,0 1,1 0,1 0,0")]
		[TestCase("0,0 1,0 1/2,1 1,1 0,1", "1,0 1,1 0,1 0,0")]
		public void DoSomething_GetConvexBoundary(string poly, string expectedBoundary)
		{
			var polygon = new Polygon(poly.Split(' ').Select(Vector.Parse).ToArray());
			var expectedPoly= new Polygon(expectedBoundary.Split(' ').Select(Vector.Parse).ToArray());
			polygon.GetConvexBoundary().Vertices.Should().Equal(expectedPoly.Vertices);
		}
	}
}
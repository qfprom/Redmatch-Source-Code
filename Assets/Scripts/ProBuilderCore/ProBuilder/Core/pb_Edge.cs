using System;
using System.Collections.Generic;

namespace ProBuilder.Core
{
	[Serializable]
	public struct pb_Edge : IEquatable<pb_Edge>
	{
		public int x;

		public int y;

		public static readonly pb_Edge Empty = new pb_Edge(-1, -1);

		public pb_Edge(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public bool IsValid()
		{
			return x > -1 && y > -1 && x != y;
		}

		public override string ToString()
		{
			return "[" + x + ", " + y + "]";
		}

		public bool Equals(pb_Edge edge)
		{
			return (x == edge.x && y == edge.y) || (x == edge.y && y == edge.x);
		}

		public override bool Equals(object b)
		{
			return b is pb_Edge && Equals((pb_Edge)b);
		}

		public override int GetHashCode()
		{
			int num = 27;
			num = num * 29 + ((x >= y) ? y : x);
			return num * 29 + ((x >= y) ? x : y);
		}

		public static pb_Edge operator +(pb_Edge a, pb_Edge b)
		{
			return new pb_Edge(a.x + b.x, a.y + b.y);
		}

		public static pb_Edge operator -(pb_Edge a, pb_Edge b)
		{
			return new pb_Edge(a.x - b.x, a.y - b.y);
		}

		public static pb_Edge operator +(pb_Edge a, int b)
		{
			return new pb_Edge(a.x + b, a.y + b);
		}

		public static pb_Edge operator -(pb_Edge a, int b)
		{
			return new pb_Edge(a.x - b, a.y - b);
		}

		public static bool operator ==(pb_Edge a, pb_Edge b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(pb_Edge a, pb_Edge b)
		{
			return !(a == b);
		}

		public int[] ToArray()
		{
			return new int[2] { x, y };
		}

		public bool Equals(pb_Edge b, Dictionary<int, int> lookup)
		{
			int num = lookup[x];
			int num2 = lookup[y];
			int num3 = lookup[b.x];
			int num4 = lookup[b.y];
			return (num == num3 && num2 == num4) || (num == num4 && num2 == num3);
		}

		public bool Contains(int a)
		{
			return x == a || y == a;
		}

		public bool Contains(pb_Edge b)
		{
			return x == b.x || y == b.x || x == b.y || y == b.x;
		}

		internal bool Contains(int a, pb_IntArray[] sharedIndices)
		{
			int num = sharedIndices.IndexOf(a);
			return Array.IndexOf(sharedIndices[num], x) > -1 || Array.IndexOf(sharedIndices[num], y) > -1;
		}
	}
}

using System;
using System.Collections.Generic;

public class Tilenode
{
    public struct Point : IEquatable<Point>
    {
        public int X, Y;
        public Point(int x , int y)
        {
            this.X = x;
            this.Y = y;
        }

        public override int GetHashCode() => (X, Y).GetHashCode();
        public override bool Equals(object obj) => base.Equals(obj);
        public bool Equals(Point other) => this.X == other.X && this.Y == other.Y;
        public static bool operator ==(Point first, Point other) => Equals(first, other);

        public static bool operator !=(Point first, Point other) => !Equals(first, other);
    }

    public Point Position;
    public int X { get { return Position.X; } }
    public int Y { get { return Position.Y; } }
    public IList<Tilenode> Neighbors { get; private set; }
    public Tilenode PreviousNode { get; set; }
    public int GCost { get; set; }
    public int HCost { get; set; }
    public int FCost { get { return GCost + HCost; } }

    public void SetNeighbors(IList<Tilenode> tilenodes)
    {
        if (tilenodes == null || tilenodes.Count <= 0)
            throw new NullReferenceException("List is cannot be empty or null");
        Neighbors = tilenodes;
    }
    public Tilenode(int x, int y)
    {
        Position = new Point(x, y);
        Neighbors = new List<Tilenode>();
    }
}


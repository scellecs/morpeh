namespace Tests;

public struct Point2D : IEquatable<Point2D>, IComparable<Point2D>, IComparable {
    public int x;
    public int y;

    public Point2D(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public bool Equals(Point2D other) => this.x == other.x && this.y == other.y;
    public override bool Equals(object? obj) => obj is Point2D other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(this.x, this.y);

    public int CompareTo(Point2D other)
    {
        var xComparison = this.x.CompareTo(other.x);
        return xComparison != 0 ? xComparison : this.y.CompareTo(other.y);
    }

    public int CompareTo(object? obj) {
        if (obj is null) {
            return 1;
        }
        if (obj is not Point2D other) {
            throw new ArgumentException("Object must be of type Point2D");
        }

        return CompareTo(other);
    }

    public static bool operator ==(Point2D left, Point2D right) => left.Equals(right);
    public static bool operator !=(Point2D left, Point2D right) => !(left == right);
    public static bool operator <(Point2D left, Point2D right) => left.CompareTo(right) < 0;
    public static bool operator >(Point2D left, Point2D right) => left.CompareTo(right) > 0;
    public static bool operator <=(Point2D left, Point2D right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Point2D left, Point2D right) => left.CompareTo(right) >= 0;
}

public sealed class Person : IEquatable<Person>, IComparable<Person>, IComparable {
    public string Name { get; }
    public int Age { get; }

    public Person(string name, int age) {
        this.Name = name;
        this.Age = age;
    }

    public bool Equals(Person? other)  {
        if (other is null) return false;
        return this.Name == other.Name && this.Age == other.Age;
    }

    public override bool Equals(object? obj) => Equals(obj as Person);

    public override int GetHashCode() => HashCode.Combine(this.Name, this.Age);

    public int CompareTo(Person? other) {
        if (other is null) return 1;
        var nameComparison = string.Compare(this.Name, other.Name, StringComparison.Ordinal);
        return nameComparison != 0 ? nameComparison : this.Age.CompareTo(other.Age);
    }

    public int CompareTo(object? obj) {
        if (obj is null) {
            return 1;
        }
        if (obj is not Person other) {
            throw new ArgumentException("Object must be of type Person");
        }

        return CompareTo(other);
    }
}

public struct SimpleColor {
    public byte r;
    public byte g;
    public byte b;

    public SimpleColor(byte r, byte g, byte b) {
        this.r = r;
        this.g = g;
        this.b = b;
    }
}

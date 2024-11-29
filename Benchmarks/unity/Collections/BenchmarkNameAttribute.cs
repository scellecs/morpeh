using System;

[AttributeUsage(AttributeTargets.Class)]
public class BenchmarkNameAttribute : Attribute
{
    public string BCL { get; }
    public string Morpeh { get; }

    public BenchmarkNameAttribute(string bcl, string morpeh)
    {
        BCL = bcl;
        Morpeh = morpeh;
    }
}

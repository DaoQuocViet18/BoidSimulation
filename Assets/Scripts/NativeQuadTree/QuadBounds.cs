using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public struct QuadBounds 
{
    public float2 center;
    public float2 extents;
    public float2 Size => extents * 2f;
    public float2 Half => extents * 2f;
    public float2 Min => center - extents;
    public float2 Max => center + extents;
    public float2 Radius => math.length(extents);

    public QuadBounds(float2 center, float2 extents) => (this.center, this.extents) = (center, extents);
    public bool Contains(float2 point) => math.all(point >= Min) && math.all(point <= Max);
    public bool Contains(QuadBounds bounds) => math.all(bounds.Min >= Min) && math.all(bounds.Max <= Max);
    public bool Intersects(QuadBounds bounds) =>
        math.abs(center.x - bounds.center.x) <= (extents.x + bounds.extents.x) &&
        math.abs(center.y - bounds.center.y) <= (extents.y + bounds.extents.y);
    public bool ContainsCircle(float2 point) => math.lengthsq(point - center) <= math.lengthsq(Radius);
    public bool ContainsCircle(QuadBounds bounds) =>
        ContainsCircle(bounds.GetCorner(0)) && ContainsCircle(bounds.GetCorner(1)) &&
        ContainsCircle(bounds.GetCorner(2)) && ContainsCircle(bounds.GetCorner(3));
    public bool IntersectsCircle(QuadBounds bounds)
    {
        float2 closesPoint = math.clamp(center, bounds.Min, bounds.Max);
        float distanceSquared = math.lengthsq(center - closesPoint);
        return distanceSquared <= math.length(Radius);
    }
    public float2 GetCorner(int zInderChild)
    {
        return zInderChild switch
        {
            0 => new float2(Min.x, Max.y),
            1 => Max,
            2 => Min,
            3 => new float2(Max.x, Min.y),
            _ => throw new ArgumentOutOfRangeException(nameof(zInderChild)),
        };
    }
    public QuadBounds GetBoundsChild(int zInderChild)
    {
        return zInderChild switch
        {
            0 => new QuadBounds(new float2(center.x - Half.x, center.x + Half.y), Half),
            1 => new QuadBounds(center + Half, Half),
            2 => new QuadBounds(center - Half, Half),
            3 => new QuadBounds(new float2(center.x + Half.x, center.x - Half.y), Half),
            _ => throw new ArgumentOutOfRangeException(nameof(zInderChild)),
        };
    }
}

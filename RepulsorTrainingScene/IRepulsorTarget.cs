using UnityEngine;

/// <summary>
/// Interface for objects that can be hit by repulsor beams.
/// Implement this interface on any GameObject that would react to repulsor hits.
/// </summary>
public interface IRepulsorTarget
{
    /// <summary>
    /// Called when this object is hit by a repulsor beam
    /// </summary>
    /// <param name="power">The power of the repulsor beam</param>
    /// <param name="hitPoint">The point in world space where the repulsor hit</param>
    /// <param name="direction">The direction the repulsor beam was traveling</param>
    void OnHitByRepulsor(float power, Vector3 hitPoint, Vector3 direction);
}
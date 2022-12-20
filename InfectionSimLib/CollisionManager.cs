using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static InfectionSimLib.Settings;

namespace InfectionSim;

public class CollisionManager2D : CollisionManager<CollisionManager2D, Vector2D, Circle2D> { }

public class CollisionManager3D : CollisionManager<CollisionManager3D, Vector3D, Circle3D> { }

public class CollisionManager<TThis, TVector, TCircle>
    where TThis : CollisionManager<TThis, TVector, TCircle>
    where TCircle : Circle<TCircle, TVector>
    where TVector : Vector<TVector>, new()
{
    public bool paused;
    public bool spawnSick;

    public List<TCircle> circles = new();

    private List<TCircle> _activeInterval = new();

    public void Update(double dt)
    {
        var modDt = dt;

        modDt *= paused ? 0 : SimSpeed;

        circles.ForEach(c => c.Update(modDt));

        // meta collision (broad phase) (sweep and prune)
        double xLeft;
        foreach (var circle in circles.OrderBy(c => c.Position.FirstComponent()))
        {
            if (!_activeInterval.Any())
            {
                _activeInterval.Add(circle);
                continue;
            }

            xLeft = circle.Position.FirstComponent() - circle.Radius;
            _activeInterval.RemoveAll(c => c.Position.FirstComponent() + c.Radius < xLeft);
            if (_activeInterval.Any())
            {
                foreach (var circleCollide in _activeInterval.Where(circleCollide => CollisionCheck(circle, circleCollide)))
                {
                    if (circle.Infected ^ circleCollide.Infected)
                    {
                        if (circle.Infected) circleCollide.GetInfected();
                        else circle.GetInfected();
                    }

                    VelocityUpdate(circle, circleCollide);
                }
            }

            _activeInterval.Add(circle);
        }

        _activeInterval.Clear();
    }

    public static bool CollisionCheck(TCircle c1, TCircle c2) // narrow phase
    {
        var rad = c1.Radius + c2.Radius;
        return c1.Position.DistanceSquareTo(c2.Position) <= Math.Pow(rad, c1.Position.Arity());
    }

    public static void VelocityUpdate(TCircle c1, TCircle c2)
    {
        var newVel1 = CalculateVelocity(c1.Velocity, c2.Velocity, c1.Position, c2.Position);
        var newVel2 = CalculateVelocity(c2.Velocity, c1.Velocity, c2.Position, c1.Position);
        c1.Velocity = newVel1;
        c2.Velocity = newVel2;
    }

    public static TVector CalculateVelocity(TVector vel1, TVector vel2, TVector pos1, TVector pos2)
    {
        var pos1MinusPos2 = pos1.Subtract(pos2);
        var dot = vel1.Subtract(vel2).DotProduct(pos1MinusPos2);
        var linalgNorm = pos1MinusPos2.Norm();
        pos1MinusPos2.ModifyTimes(dot / (linalgNorm * linalgNorm));
        return vel1.Subtract(pos1MinusPos2);
    }
}
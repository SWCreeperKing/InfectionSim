using System.Numerics;

namespace InfectionSim;

public class CollisionManager
{
    public bool paused;
    public bool spawnSick;
    public float maxInfectTimer = 40;
    public float simSpeed = 1;
    public float radius = 15;
    public float infectChance = .5f;
    public List<Circle> circles = new();

    private List<Circle> _activeInterval = new();

    public void Update(float dt)
    {
        var modDt = dt;

        if (paused) modDt *= 0;
        else modDt *= simSpeed;

        circles.ForEach(c => c.Update(modDt));

        // meta collision (broad phase) (sweep and prune)
        float xLeft;
        foreach (var cirlce in circles.OrderBy(c => c.Position.X))
        {
            if (!_activeInterval.Any())
            {
                _activeInterval.Add(cirlce);
                continue;
            }

            xLeft = cirlce.Position.X - cirlce.Radius;
            _activeInterval.RemoveAll(c => c.Position.X + c.Radius < xLeft);
            if (_activeInterval.Any())
            {
                foreach (var circleCollide in _activeInterval.Where(circleCollide =>
                             CollisionCheck(cirlce, circleCollide)))
                {
                    if (cirlce.Infected ^ circleCollide.Infected)
                    {
                        if (cirlce.Infected) circleCollide.GetInfected();
                        else cirlce.GetInfected();
                    }

                    VelocityUpdate(cirlce, circleCollide);
                }
            }

            _activeInterval.Add(cirlce);
        }

        _activeInterval.Clear();
    }

    public static bool CollisionCheck(Circle c1, Circle c2) // narrow phase
    {
        var deltaX = c2.Position.X - c1.Position.X;
        var deltaY = c2.Position.Y - c1.Position.Y;
        var rad = c1.Radius + c2.Radius;
        return deltaX * deltaX + deltaY * deltaY <= rad * rad;
    }

    public static void VelocityUpdate(Circle c1, Circle c2)
    {
        var newVel1 = CalculateVelocity(c1.Velocity, c2.Velocity, c1.Position, c2.Position);
        var newVel2 = CalculateVelocity(c2.Velocity, c1.Velocity, c2.Position, c1.Position);
        c1.Velocity = newVel1;
        c2.Velocity = newVel2;
    }

    public static Vector2 CalculateVelocity(Vector2 vel1, Vector2 vel2, Vector2 pos1, Vector2 pos2)
    {
        var dot = Vector2.Dot(vel1 - vel2, pos1 - pos2);
        var linalgNorm = (pos1 - pos2).Length();

        return vel1 - dot / (linalgNorm * linalgNorm) * (pos1 - pos2);
    }
}
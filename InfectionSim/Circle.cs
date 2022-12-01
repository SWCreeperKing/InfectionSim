using System.Numerics;
using Raylib_CsLo;
using RayWrapper.Base.GameBox;

namespace InfectionSim;

public class Circle
{
    public static int Healthy;
    public static int Sick;
    public static Random r = new();

    public bool Infected
    {
        get => _infected;
        set
        {
            if (_infected = value)
            {
                Sick++;
                _infectedTimer = Program.maxInfectTimer;
            }
            else Healthy++;
        }
    }

    public float Radius { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }

    private bool _infected;
    private float _infectedTimer;
    
    public Circle(float radius, float velocityAllocation = 125)
    {
        var phi = 2 * Math.PI * r.NextDouble();
        var vx = .25f * Math.Cos(phi);
        var vy = .25f * Math.Sin(phi);
        
        Radius = radius;

        var windowSize = GameBox.WindowSize;
        Position = new Vector2(r.Next((int) windowSize.X), r.Next((int) windowSize.Y));
        Velocity = new Vector2((float) vx, (float) vy) * velocityAllocation;
    }

    public void Update(float dt)
    {
        if (Infected)
        {
            _infectedTimer -= dt;
            if (_infectedTimer <= 0) Cured();
        }
        
        Position += Velocity * dt;

        var windowSize = GameBox.WindowSize;
        if (Position.X < 0 || Position.X > windowSize.X)
        {
            Position = Position with { X = Position.X < 0 ? windowSize.X : 0 };
        }
        
        if (Position.Y < 0 || Position.Y > windowSize.Y)
        {
            Position = Position with { Y = Position.Y < 0 ? windowSize.Y : 0 };
        }
    }

    public void Render()
    {
        Raylib.DrawCircle((int) Position.X, (int) Position.Y, Radius, Infected ? Raylib.RED : Raylib.GREEN);
    }

    public void GetInfected()
    {
        if (r.NextDouble() < Program.infectChance)
        {
            Healthy--;
            Infected = true;
        }
    }

    public void Cured()
    {
        Sick--;
        Infected = false;
    }
}
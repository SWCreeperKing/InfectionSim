using InfectionSimLib;
using System;
using System.Numerics;
using static InfectionSimLib.Settings;

namespace InfectionSim;

public class Circle2D : Circle<Circle2D, Vector2D>
{
    public override void ClampPosition() => Position.ModifyClamp(Settings.Range2D);

    public override Vector2D CreateInitialVelocity(double velocityAllocation)
    {
        var phi = 2 * Math.PI * Random.NextDouble();
        var vx = .25f * Math.Cos(phi);
        var vy = .25f * Math.Sin(phi);
        var result = new Vector2D() { X = vx, Y = vy };
        result.ModifyTimes(velocityAllocation);
        return result;
    }

    public override Vector2D GetInitialPosition() => new()
    {
        X = Random.NextDouble() * Range2D.X,
        Y = Random.NextDouble() * Range2D.Y,
    };
}

public class Circle3D : Circle<Circle3D, Vector3D>
{
    public override void ClampPosition() => Position.ModifyClamp(Settings.Range3D);

    public override Vector3D CreateInitialVelocity(double velocityAllocation)
    {
        var phi = 2 * Math.PI * Random.NextDouble();
        var vx = .25f * Math.Cos(phi);
        var vy = .25f * Math.Sin(phi);
        var vz = .25f * Math.Sin(phi) * Math.Cos(phi);
        var result = new Vector3D() { X = vx, Y = vy, Z = vz };
        result.ModifyTimes(velocityAllocation);
        return result;
    }

    public override Vector3D GetInitialPosition() => new()
    {
        X = Random.NextDouble() * Range3D.X,
        Y = Random.NextDouble() * Range3D.Y,
    };
}

public abstract class Circle
{
    public static int Healthy { get; protected set; }
    public static int Sick { get; protected set; }
}

public abstract class Circle<TThis , TVector> : Circle
    where TThis : Circle<TThis, TVector>
    where TVector : Vector<TVector>, new()
{
    protected static Random Random = new();

    private bool _Infected;
    public bool Infected
    {
        get => _Infected;
        set
        {
            if (_Infected = value)
            {
                Sick++;
                InfectedTimer = MaxInfectTimer;
            }
            else
            {
                Healthy++;
            }
        }
    }

    private double InfectedTimer;

    public double Radius { get; set; }
    public TVector Position { get; set; }
    public TVector Velocity { get; set; }

    public TThis Init(double radius, double velocityAllocation = DefaultVelocityAllocation)
    {
        Radius = radius;

        Position = GetInitialPosition();
        Velocity = CreateInitialVelocity(velocityAllocation);
        return (TThis)this;
    }

    public abstract TVector GetInitialPosition();
    public abstract TVector CreateInitialVelocity(double velocityAllocation);

    public abstract void ClampPosition();
    
    public void Update(double deltaTime)
    {
        if (Infected)
        {
            InfectedTimer -= deltaTime;
            if (InfectedTimer <= 0)
            {
                Cure();
            }
        }

        Position.ModifyPlusTimes(Velocity, deltaTime);
        ClampPosition();
    }

    public void GetInfected()
    {
        if (!(Random.NextDouble() < InfectChance)) return;
        Healthy--;
        Infected = true;
    }

    public void Cure()
    {
        Sick--;
        Infected = false;
    }
}

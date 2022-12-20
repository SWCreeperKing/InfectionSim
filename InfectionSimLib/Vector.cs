using System;

namespace InfectionSim;

public abstract class Vector<TThis>
    where TThis : Vector<TThis>, new()
{
    public abstract double ComponentSum();

    public abstract void ModifyTimes(double factor);
    public abstract void ModifyPlusTimes(TThis plus, double times);

    public abstract void ModifyClamp(TThis max);

    public double Norm() => Math.Sqrt(NormSquared());
    public abstract double NormSquared();
    public abstract double DistanceSquareTo(TThis position);

    public abstract TThis Subtract(TThis subtrahend);
    public abstract double DotProduct(TThis factor);
    public abstract double FirstComponent();

    public abstract int Arity();
}


public class Vector2D : Vector2D<Vector2D> { }

public class Vector2D<TThis> : Vector<TThis>
    where TThis : Vector2D<TThis>, new()
{
    public double X { get; set; }
    public double Y { get; set; }
    public override void ModifyTimes(double factor)
    {
        X *= factor;
        Y *= factor;
    }

    public override void ModifyPlusTimes(TThis plus, double times)
    {
        X += plus.X * times;
        Y += plus.Y * times;
    }

    public override void ModifyClamp(TThis max)
    {
        while (X < 0) { X += max.X; }
        while (X > max.X) { X -= max.X; }

        while (Y < 0) { Y += max.Y; }
        while (Y > max.Y) { Y -= max.Y; }
    }

    public override double DistanceSquareTo(TThis v2)
    {
        double dx = v2.X - X;
        dx *= dx;

        double dy = v2.Y - Y;
        dy *= dy;
        return dx + dy;
    }

    public override double ComponentSum() => X + Y;

    public override TThis Subtract(TThis subtrahend) => new()
    {
        X = X - subtrahend.X,
        Y = Y - subtrahend.Y,
    };

    public override double NormSquared() => X * X + Y * Y;

    public override double DotProduct(TThis factor) => X * factor.X + Y * factor.Y;

    public override double FirstComponent() => X;

    public override int Arity() => 2;
}

public class Vector3D : Vector2D<Vector3D>
{
    public double Z { get; set; }
    public override void ModifyTimes(double factor)
    {
        base.ModifyTimes(factor);
        Z *= factor;
    }

    public override void ModifyPlusTimes(Vector3D plus, double times)
    {
        base.ModifyPlusTimes(plus, times);
        Z += plus.Z * times;
    }

    public override void ModifyClamp(Vector3D max)
    {
        base.ModifyClamp(max);

        while (Y < 0) { Y += max.Y; }
        while (Y > max.Y) { Y -= max.Y; }
    }

    public override double ComponentSum() => base.ComponentSum() + Z;

    public override double DistanceSquareTo(Vector3D v2)
    {
        var dxy = base.DistanceSquareTo(v2);
        double dz = v2.Z - Z;
        dz *= dz;
        return dxy + dz;
    }

    public override Vector3D Subtract(Vector3D subtrahend)
    {
        var result = base.Subtract(subtrahend);
        result.Z = Z - subtrahend.Z;
        return result;
    }

    public override double NormSquared() => base.NormSquared() + Z * Z;

    public override double DotProduct(Vector3D factor) => base.DotProduct(factor) + Z * factor.Z;

    public override int Arity() => 3;
}
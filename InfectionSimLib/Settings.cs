using InfectionSim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfectionSimLib;

public static class Settings
{
    public static float MaxInfectTimer = 40;
    public static float Radius = 15;
    public static float InfectChance = .5f;

    public static float SimSpeed { get; set; } = 1;

    public const double DefaultVelocityAllocation = 125;

    public static Vector2D Range2D { get; set; } = new() { X = 1, Y = 1 };
    public static Vector3D Range3D { get; set; } = new() { X = 1, Y = 1, Z = 1 };

}

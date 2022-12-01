using System.Numerics;
using ImGuiNET;
using InfectionSim;
using Raylib_CsLo;
using RayWrapper.Base.GameBox;
using RayWrapper.Imgui.Widgets;

new GameBox(new Program(), new Vector2(1280, 720), "Infection simulator");

// challenge: write from 'scratch' only given render/update loop + imgui for gui
public partial class Program : GameLoop
{
    public static bool paused;
    public static bool spawnSick;
    public static float maxInfectTimer = 40;
    public static float simSpeed = 1;
    public static float radius = 15;
    public static float infectChance = .5f;
    public static List<Circle> circles = new();

    private Func<bool> _onPause;
    private Func<bool> _onSick;
    private List<Circle> _activeInterval = new();

    public override void Init()
    {
        var modWindow = new CompoundWidgetBuilder()
            .AddCheckBox("Pause Sim", out _onPause)
            .AddSlider("Sim Speed", new SliderFloat(simSpeed, f => simSpeed = f, .5f, 2))
            .AddSlider("Infect%", new SliderFloat(infectChance, f => infectChance = f, .01f, 1))
            .AddNonWidget(() =>
            {
                if (!ImGui.InputFloat("Infect time (sec)", ref maxInfectTimer, 5, 10)) return;
                if (maxInfectTimer <= 0) maxInfectTimer = 1;
            })
            .AddNonWidget(ImGui.Separator)
            .AddCheckBox("Is Sick?", out _onSick)
            .AddSlider("Ball Radius", new SliderFloat(radius, f => radius = f, 5, 30))
            .AddButton("Spawn Ball", () => circles.Add(new Circle(radius) { Infected = spawnSick }));

        RegisterGameObj(modWindow.ToWindow("Sim Modification Window"));
    }

    public override void UpdateLoop(float dt)
    {
        var modDt = dt;

        if (_onPause()) paused = !paused;
        if (_onSick()) spawnSick = !spawnSick;
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

    public override void RenderLoop()
    {
        Raylib.DrawFPS(10, 10);
        Raylib.DrawText($"Healthy (G): {Circle.Healthy}", 10, 50, 24, Raylib.GREEN);
        Raylib.DrawText($"Sick (K): {Circle.Sick}", 10, 80, 24, Raylib.RED);
        circles.ForEach(c => c.Render());
    }

    public bool CollisionCheck(Circle c1, Circle c2) // narrow phase
    {
        var deltaX = c2.Position.X - c1.Position.X;
        var deltaY = c2.Position.Y - c1.Position.Y;
        var rad = c1.Radius + c2.Radius;
        return deltaX * deltaX + deltaY * deltaY <= rad * rad;
    }

    public void VelocityUpdate(Circle c1, Circle c2)
    {
        var newVel1 = CalculateVelocity(c1.Velocity, c2.Velocity, c1.Position, c2.Position);
        var newVel2 = CalculateVelocity(c2.Velocity, c1.Velocity, c2.Position, c1.Position);
        c1.Velocity = newVel1;
        c2.Velocity = newVel2;
    }

    public Vector2 CalculateVelocity(Vector2 vel1, Vector2 vel2, Vector2 pos1, Vector2 pos2)
    {
        var dot = Vector2.Dot(vel1 - vel2, pos1 - pos2);
        var linalgNorm = (pos1 - pos2).Length();

        return vel1 - dot / (linalgNorm * linalgNorm) * (pos1 - pos2);
    }
}
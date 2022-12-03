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
    private CollisionManager cm = new();
    private Func<bool> _onPause;
    private Func<bool> _onSick;

    public override void Init()
    {
        var modWindow = new CompoundWidgetBuilder()
            .AddCheckBox("Pause Sim", out _onPause)
            .AddSlider("Sim Speed", new SliderFloat(cm.simSpeed, f => cm.simSpeed = f, .5f, 2))
            .AddSlider("Infect%", new SliderFloat(cm.infectChance, f => cm.infectChance = f, .01f, 1))
            .AddNonWidget(() =>
            {
                if (!ImGui.InputFloat("Infect time (sec)", ref cm.maxInfectTimer, 5, 10)) return;
                if (cm.maxInfectTimer <= 0) cm.maxInfectTimer = 1;
            })
            .AddNonWidget(ImGui.Separator)
            .AddCheckBox("Is Sick?", out _onSick)
            .AddSlider("Ball Radius", new SliderFloat(cm.radius, f => cm.radius = f, 5, 30))
            .AddButton("Spawn Ball", () => cm.circles.Add(new Circle(cm, cm.radius) { Infected = cm.spawnSick }));

        RegisterGameObj(modWindow.ToWindow("Sim Modification Window"));
    }

    public override void UpdateLoop(float dt)
    {
        if (_onPause()) cm.paused = !cm.paused;
        if (_onSick()) cm.spawnSick = !cm.spawnSick;
        cm.Update(dt);
    }

    public override void RenderLoop()
    {
        Raylib.DrawFPS(10, 10);
        Raylib.DrawText($"Healthy (G): {Circle.Healthy}", 10, 50, 24, Raylib.GREEN);
        Raylib.DrawText($"Sick (K): {Circle.Sick}", 10, 80, 24, Raylib.RED);
        cm.circles.ForEach(c =>
            Raylib.DrawCircle((int) c.Position.X, (int) c.Position.Y, c.Radius,
                c.Infected ? Raylib.RED : Raylib.GREEN));
    }
}
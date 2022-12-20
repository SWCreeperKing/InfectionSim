using System.Numerics;
using ImGuiNET;
using InfectionSim;
using InfectionSimLib;
using Raylib_CsLo;
using RayWrapper.Base.GameBox;
using RayWrapper.Imgui.Widgets;
using static InfectionSimLib.Settings;

new GameBox(new Program(), new Vector2(1280, 720), "Infection simulator");

// challenge: write from 'scratch' only given render/update loop + imgui for gui
public partial class Program : GameLoop
{
    private CollisionManager2D cm = new();
    private Func<bool> _onPause;
    private Func<bool> _onSick;

    public override void Init()
    {
        Settings.Range2D = new() { X = GameBox.WindowSize.X, Y = GameBox.WindowSize.Y };
        var modWindow = new CompoundWidgetBuilder()
            .AddCheckBox("Pause Sim", out _onPause)
            .AddSlider("Sim Speed", new SliderFloat(SimSpeed, f => SimSpeed = f, .5f, 2))
            .AddSlider("Infect%", new SliderFloat(InfectChance, f => InfectChance = f, .01f, 1))
            .AddNonWidget(() =>
            {
                if (!ImGui.InputFloat("Infect time (sec)", ref MaxInfectTimer, 5, 10)) return;
                if (MaxInfectTimer <= 0) MaxInfectTimer = 1;
            })
            .AddNonWidget(ImGui.Separator)
            .AddCheckBox("Is Sick?", out _onSick)
            .AddSlider("Ball Radius", new SliderFloat(Radius, f => Radius = f, 5, 30))
            .AddButton("Spawn Ball", () => cm.circles.Add(new Circle2D() { Infected = cm.spawnSick }.Init(Radius) ));

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
            Raylib.DrawCircle((int) c.Position.X, (int) c.Position.Y, (float)c.Radius,
                c.Infected ? Raylib.RED : Raylib.GREEN));
    }
}
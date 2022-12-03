using System.Numerics;
using ImGuiNET;
using InfectionSim;
using Raylib_CsLo;
using RayWrapper.Base.GameBox;
using RayWrapper.Imgui.Widgets;
using static InfectionSim.CollisionManager;

new GameBox(new Program(), new Vector2(1280, 720), "Infection simulator");

// challenge: write from 'scratch' only given render/update loop + imgui for gui
public partial class Program : GameLoop
{
    private Func<bool> _onPause;
    private Func<bool> _onSick;

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
        if (_onPause()) paused = !paused;
        if (_onSick()) spawnSick = !spawnSick;
        CollisionManager.Update(dt);
    }

    public override void RenderLoop()
    {
        Raylib.DrawFPS(10, 10);
        Raylib.DrawText($"Healthy (G): {Circle.Healthy}", 10, 50, 24, Raylib.GREEN);
        Raylib.DrawText($"Sick (K): {Circle.Sick}", 10, 80, 24, Raylib.RED);
        circles.ForEach(c => c.Render());
    }
}
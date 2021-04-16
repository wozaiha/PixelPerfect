using System;
using Dalamud.Plugin;
using ImGuiNET;
using ImGuiScene;
using Dalamud.Configuration;
using Num = System.Numerics;
using Dalamud.Game.Command;
using Dalamud.Interface;

namespace PixelPerfect
{
    public class PixelPerfect : IDalamudPlugin
    {
        public string Name => "Pixel Perfect";
        private DalamudPluginInterface _pluginInterface;
        private Config _configuration;
        private bool _enabled = true;
        private bool _config;
        private bool _combat = true;
        private bool _circle;
        private bool _instance;
        private Num.Vector4 _col = new Num.Vector4(1f, 1f, 1f, 1f);
        private Num.Vector4 _col2 = new Num.Vector4(0.4f, 0.4f, 0.4f, 1f);
        private bool _ring;
        private Num.Vector4 _colRing = new Num.Vector4(0.4f, 0.4f, 0.4f, 0.5f);
        private float _radius = 10f;
        private int _segments = 100;
        private float _thickness = 10f;

        private bool _targetRing = false;
        //ptivate Num.Vector4 col_ring = new Num.Vector4(0.4f, 0.4f, 0.4f, 0.5f);
        private float _targetRadius = 10f;
        private int _targetSegments = 100;
        private float _targetThickness = 2f;
        
        public void Initialize(DalamudPluginInterface pI)
        {
            _pluginInterface = pI;
            _configuration = _pluginInterface.GetPluginConfig() as Config ?? new Config();
            _ring = _configuration.Ring;
            _thickness = _configuration.Thickness;
            _colRing = _configuration.ColRing;
            _segments = _configuration.Segments;
            _radius = _configuration.Radius;
            _enabled = _configuration.Enabled;
            _combat = _configuration.Combat;
            _circle = _configuration.Circle;
            _instance = _configuration.Instance;
            _col = _configuration.Col;
            _col2 = _configuration.Col2;

            _targetRing = _configuration.TargetRing;
            _targetThickness = _configuration.TargetThickness;
            _targetSegments = _configuration.TargetSegments;
            _targetRadius = _configuration.TargetRadius;

            _pluginInterface.UiBuilder.OnBuildUi += DrawWindow;
            _pluginInterface.UiBuilder.OnOpenConfigUi += ConfigWindow;
            _pluginInterface.CommandManager.AddHandler("/pp", new CommandInfo(Command)
            {
                HelpMessage = "Pixel Perfect config."
            });
        }

        public void Dispose()
        {
            _pluginInterface.UiBuilder.OnBuildUi -= DrawWindow;
            _pluginInterface.UiBuilder.OnOpenConfigUi -= ConfigWindow;
            _pluginInterface.CommandManager.RemoveHandler("/pp");
        }

        private void DrawWindow()
        {
            if (_config)
            {
                ImGui.SetNextWindowSize(new Num.Vector2(300, 500), ImGuiCond.FirstUseEver);
                ImGui.Begin("Pixel Perfect Config", ref _config);
                ImGui.Checkbox("Hitbox", ref _enabled);
                ImGui.Checkbox("Outer Ring", ref _circle);
                ImGui.Checkbox("Combat Only", ref _combat);
                ImGui.Checkbox("Instance Only", ref _instance);
                ImGui.ColorEdit4("Colour", ref _col, ImGuiColorEditFlags.NoInputs);
                ImGui.ColorEdit4("Outer Colour", ref _col2, ImGuiColorEditFlags.NoInputs);
                ImGui.Separator();

                ImGui.Checkbox("Ring", ref _ring);
                ImGui.DragFloat("Yalms", ref _radius, 1f, 0f, 30f);
                ImGui.DragFloat("Thickness", ref _thickness, 0.5f, 0f, 50f);
                ImGui.DragInt("Smoothness", ref _segments, 1f, 0, 200);

                ImGui.Checkbox("Target Ring", ref _targetRing);
                ImGui.DragFloat("Target Yalms", ref _targetRadius, 1f, 0f, 30f);
                ImGui.DragFloat("Target Thickness", ref _targetThickness, 0.5f, 0f, 50f);
                ImGui.DragInt("Target Smoothness", ref _targetSegments, 1f, 0, 200);

                ImGui.ColorEdit4("Ring Colour", ref _colRing, ImGuiColorEditFlags.NoInputs);

                if (ImGui.Button("Save and Close Config"))
                {
                    SaveConfig();
                    _config = false;
                }
                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);

                if (ImGui.Button("Buy Haplo a Hot Chocolate"))
                {
                    System.Diagnostics.Process.Start("https://ko-fi.com/haplo");
                }
                ImGui.PopStyleColor(3);
                ImGui.End();
            }

            if (!_enabled || _pluginInterface.ClientState.LocalPlayer == null) return;
            if(_combat)
            {
                if (!_pluginInterface.ClientState.Condition[Dalamud.Game.ClientState.ConditionFlag.InCombat]) { return; }
            }

            if (_instance)
            {
                if (!_pluginInterface.ClientState.Condition[Dalamud.Game.ClientState.ConditionFlag.BoundByDuty]) { return; }
            }

            var actor = _pluginInterface.ClientState.LocalPlayer;
            if (!_pluginInterface.Framework.Gui.WorldToScreen(
                new SharpDX.Vector3(actor.Position.X, actor.Position.Z, actor.Position.Y),
                out var pos)) return;
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Num.Vector2(pos.X - 10 - ImGuiHelpers.MainViewport.Pos.X, pos.Y - 10- ImGuiHelpers.MainViewport.Pos.Y));
            ImGui.Begin("Pixel Perfect", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoBackground);
            ImGui.GetWindowDrawList().AddCircleFilled(
                new Num.Vector2(pos.X, pos.Y),
                2f,
                ImGui.GetColorU32(_col),
                100);
            if(_circle)
            {
                ImGui.GetWindowDrawList().AddCircle(
                    new Num.Vector2(pos.X, pos.Y),
                    2.2f,
                    ImGui.GetColorU32(_col2),
                    100);
            }
            ImGui.End();

            if (_ring) 
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Num.Vector2(0, 0));
                ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Num.Vector2(0, 0));
                ImGui.Begin("Ring", ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground);
                ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);
                DrawRingWorld(_pluginInterface.ClientState.LocalPlayer, _radius, _segments, _thickness, ImGui.GetColorU32(_colRing));
                ImGui.End();
                ImGui.PopStyleVar();
            }

            if ((_targetRing) & (_pluginInterface.ClientState.Targets.CurrentTarget != null)) {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Num.Vector2(0, 0));
                ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Num.Vector2(0, 0));
                ImGui.Begin("Target_Ring", ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground);
                ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);
                DrawRingWorld(_pluginInterface.ClientState.Targets.CurrentTarget, _targetRadius, _targetSegments, _targetThickness, ImGui.GetColorU32(_colRing));
                ImGui.End();
                ImGui.PopStyleVar();
            }
        }
        private void ConfigWindow(object sender, EventArgs args)
        {
            _config = true;
        }

        private void Command(string command, string arguments)
        {
            if (arguments.Trim() == "") _config = true;
            else
            {
                if (arguments.IndexOf(' ') > 0)
                {
                    if (arguments.Substring(0, arguments.IndexOf(' ')).ToLower().Trim() != "me") return;
                    else
                    {
                        if (float.TryParse(arguments.Substring(arguments.IndexOf(' ')).Trim(), out var me))
                        {
                            _ring = true;
                            _radius = me;
                        }
                        else
                        {
                            _ring = false;
                            _radius = _configuration.Radius;
                        }
                    }
                }

                if (float.TryParse(arguments, out var target))
                {
                    _targetRing = true;
                    _targetRadius = target;
                }
                else
                {
                    _targetRing = false;
                    _targetRadius = _configuration.TargetRadius;
                }
            }
        }

        private void SaveConfig()
        {
            _configuration.Enabled = _enabled;
            _configuration.Combat = _combat;
            _configuration.Circle = _circle;
            _configuration.Instance = _instance;
            _configuration.Col = _col;
            _configuration.Col2 = _col2;
            _configuration.ColRing = _colRing;
            _configuration.Thickness = _thickness;
            _configuration.Segments = _segments;
            _configuration.Ring = _ring;
            _configuration.Radius = _radius;
            _pluginInterface.SavePluginConfig(_configuration);

            _configuration.TargetThickness = _targetThickness;
            _configuration.TargetSegments = _targetSegments;
            _configuration.TargetRing = _targetRing;
            _configuration.TargetRadius = _targetRadius;
        }

        private void DrawRingWorld(Dalamud.Game.ClientState.Actors.Types.Actor actor, float radius, int numSegments, float thicc, uint colour)
        {
            var seg = numSegments / 2;
            for (var i = 0; i <= numSegments; i++)
            {
                _pluginInterface.Framework.Gui.WorldToScreen(new SharpDX.Vector3(actor.Position.X + (radius * (float)Math.Sin((Math.PI / seg) * i)), actor.Position.Z, actor.Position.Y + (radius * (float)Math.Cos((Math.PI / seg) * i))), out SharpDX.Vector2 pos);
                ImGui.GetWindowDrawList().PathLineTo(new Num.Vector2(pos.X, pos.Y));
            }
            ImGui.GetWindowDrawList().PathStroke(colour, ImDrawFlags.Closed, thicc);
        }
    }

    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool Enabled { get; set; } = true;
        public bool Combat { get; set; } = true;
        public bool Circle { get; set; }
        public bool Instance { get; set; }
        public Num.Vector4 Col { get; set; } = new Num.Vector4(1f, 1f, 1f, 1f);
        public Num.Vector4 Col2 { get; set; } = new Num.Vector4(0.4f, 0.4f, 0.4f, 1f);
        public Num.Vector4 ColRing { get; set; } = new Num.Vector4(0.4f, 0.4f, 0.4f, 0.5f);
        public int Segments { get; set; } = 100;
        public float Thickness { get; set; } = 10f;
        public bool Ring { get; set; }
        public float Radius { get; set; } = 2f;
        public int TargetSegments { get; set; } = 100;
        public float TargetThickness { get; set; } = 10f;
        public bool TargetRing { get; set; } = false;
        public float TargetRadius { get; set; } = 2f;
    }
}
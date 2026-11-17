using Godot;
using TheLastGate.World;

namespace TheLastGate.Game;

// The current client multiplied every pixel it drew, the HUD included, by its gamma; this covers the screen and does
// the same to whatever has been drawn beneath it.
public partial class ScreenGammaLayer : CanvasLayer
{
    private const int AboveTheGame = 100;

    private const string ShaderCode = """
        shader_type canvas_item;

        uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;
        uniform float gamma = 1.0;

        void fragment() {
            COLOR = vec4(texture(screen_texture, SCREEN_UV).rgb * gamma, 1.0);
        }
        """;

    private readonly ShaderMaterial material = new() { Shader = new Shader { Code = ShaderCode } };

    public override void _Ready()
    {
        Layer = AboveTheGame;
        var cover = new ColorRect { Material = material, MouseFilter = Control.MouseFilterEnum.Ignore };
        cover.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(cover);
    }

    // At normal gamma nothing changes, so the layer is skipped rather than copying the screen for nothing.
    public void ShowGamma(float gamma)
    {
        material.SetShaderParameter("gamma", gamma);
        Visible = gamma > Gamma.Normal;
    }
}

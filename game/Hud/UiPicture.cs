using Godot;
using Godot.Collections;

namespace TheLastGate.Hud;

[Tool]
public partial class UiPicture : TextureRect
{
    private int darkPictureNumber;
    private int pictureNumber;

    [Export]
    public int PictureNumber
    {
        get => pictureNumber;
        set
        {
            pictureNumber = value;
            LoadPicture();
        }
    }

    [Export]
    public int DarkPictureNumber
    {
        get => darkPictureNumber;
        set
        {
            darkPictureNumber = value;
            LoadPicture();
        }
    }

    public override void _EnterTree() => LoadPicture();

    public override void _ValidateProperty(Dictionary property)
    {
        if (property["name"].AsStringName() != TextureRect.PropertyName.Texture)
        {
            return;
        }

        var usage = property["usage"].As<PropertyUsageFlags>() & ~PropertyUsageFlags.Storage;
        property["usage"] = (int)(usage | PropertyUsageFlags.ReadOnly);
    }

    public void LoadPicture()
    {
        if (!IsInsideTree() || HudView.Containing(this) is not { Sprites: { } sprites } hud)
        {
            return;
        }

        Texture = sprites.GetUiPicture(hud.DarkMode && darkPictureNumber != 0 ? darkPictureNumber : pictureNumber);
    }
}

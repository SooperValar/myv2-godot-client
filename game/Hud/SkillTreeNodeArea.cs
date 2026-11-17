using Godot;

namespace TheLastGate.Hud;

// A tree node answers the mouse within 14 pixels of its centre, measured from whole pixels as the current client does.
public partial class SkillTreeNodeArea : ClickArea
{
    private const int Radius = 14;

    public override bool _HasPoint(Vector2 point) => Contains(point);

    protected override bool Contains(Vector2 point)
    {
        var fromCentre = point.Floor() - new Vector2(Radius, Radius);
        return fromCentre.LengthSquared() < Radius * Radius;
    }
}

namespace TheLastGate.Assets;

public interface IFontSet
{
    bool CentresLikeTheCurrentClient { get; }

    GameFont For(TextColour colour);
}

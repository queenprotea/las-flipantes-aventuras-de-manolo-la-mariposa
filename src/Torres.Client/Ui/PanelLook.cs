using Microsoft.Xna.Framework;

namespace Torres.Client.Ui
{
    internal readonly record struct PanelLook(Color TopColor, Color BottomColor, Color LineColor, int CornerRadius, bool HasLine, bool HasShadow);
}

using Nexus.Extensions.Controls;

namespace Nexus;

public static class StaticResources
{
    public static class Colors
    {
        // Text tiers from the Figma redesign: 90% primary, 60% secondary, 30% muted.
        public static Color ForegroundColor { get; } = "#ffffff".ToColor().WithAlpha(0.9f);
        public static Color SecondaryForegroundColor { get; } = "#ffffff".ToColor().WithAlpha(0.6f);
        public static Color MutedForegroundColor { get; } = "#ffffff".ToColor().WithAlpha(0.3f);

        // Toolbar/button glyphs sit on their own tier in the Figma redesign: idle icons are
        // white @ 70% (Vector fo=0.7), brightening to full white only when the tool/toggle is active.
        public static Color IconForegroundColor { get; } = "#ffffff".ToColor().WithAlpha(0.7f);
        public static Color SceneBackgroundColor { get; set; } = "#1C1C1E".ToColor();
        public static Color PanelsBorderColor { get; } = "#2C2C2E".ToColor();

        // Figma redesign: blur-panel "Base" frames have a 1px white @ 7% outline.
        public static Color PanelStrokeColor { get; } = "#FFFFFF".ToColor().WithAlpha(0.07f);
        public static Color PanelsBackgroundColor { get; } = "#060608".ToColor().WithAlpha(0.95f);
        public static Color MainBackgroundColor { get; } = "#2d2d2f".ToColor();
        public static Color MainMenuBackgroundColor { get; } = "#1C1C1E".ToColor();
        public static Color ModalOverlayColor { get; } = "#882d2d2f".ToColor();
        public static Color TitleBarBackgroundColor { get; } = "#1d1d1f".ToColor();
        public static Color SelectedItemColor { get; } = "#FFFFFF".ToColor().WithAlpha(0.2f);
        public static Color MyAccentColor { get; } = "#ff4384de".ToColor();
        public static Color ButtonBackgroundColor { get; } = "#060608".ToColor().WithAlpha(0.8f);
        public static Color ButtonHoverColor { get; } = "#ffffff".ToColor().WithAlpha(0.3f);
        public static Color ButtonActiveColor { get; } = "#5883bf".ToColor();
        public static Color MyLinkHighlightColor { get; } = "#ff6ba7f9".ToColor();
        public static Color BrushButtonColor { get; } = "#FFFFFF".ToColor().WithAlpha(0.2f);
        public static Color BrushItemColor { get; } = "#FFFFFF".ToColor().WithAlpha(0.1f);
        public static Color InnerPanelBackgroundColor { get; } = "#3a3f46".ToColor();

        // Popups & flyouts share the docked panels' near-black "Base" frame color from the Figma
        // redesign — not a lighter gray. (Was #2A2A2A, which read as gray against the black panels.)
        public static Color PopupBackgroundColor { get; } = PanelsBackgroundColor;
    }

    public static class Measures
    {
        public static double PipkaCornerRadius = 3;
        public static double ToolItemCornerRadius = 12;
        public static double ButtonCornerRadius = 16;
        public static double PanelCornerRadius = 16;
        public static double PanelMargin = 12;
        public static double SmallButtonCornerRadius = 10;
        public static double SmallButtonSize = 36;
    }
}

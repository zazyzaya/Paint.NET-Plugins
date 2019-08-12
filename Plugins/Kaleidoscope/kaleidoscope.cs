// Name: Kaleidoscope
// Submenu: my tools
// Author: Zaya
// Title:
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
AngleControl angle = 45; // [1,180] Slices
#endregion

private double radius = null;
private double rads = null;

// The slope of a "slice" of the pie
private double slope = Math.tan(0.5 - rads);

// Check if x,y fall between the outline of the circle and the line slicing it
private boolean isInArc(int x, int y) {
    return ((slope * x < y) && (Math.cos(x) * radius > y)
}

// Set up globals
void PreRender(Surface dst, Surface src) {
    // Clean this up later
    if (radius == null) {
        radius = src.Height / 2
        if (radius < src.Width / 2) 
            radius = src.Width / 2;
    }

    if (rads == null) {
        rads = (Math.PI * angle) / 180;
    }
}

void Render(Surface dst, Surface src, Rectangle rect)
{
    // Delete any of these lines you don't need
    Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();
    int CenterX = ((selection.Right - selection.Left) / 2) + selection.Left;
    int CenterY = ((selection.Bottom - selection.Top) / 2) + selection.Top;
    ColorBgra PrimaryColor = EnvironmentParameters.PrimaryColor;
    ColorBgra SecondaryColor = EnvironmentParameters.SecondaryColor;
    int BrushWidth = (int)EnvironmentParameters.BrushWidth;

    ColorBgra CurrentPixel;
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {
            CurrentPixel = src[x,y];
            // TODO: Add pixel processing code here
            // Access RGBA values this way, for example:
            // CurrentPixel.R = PrimaryColor.R;
            // CurrentPixel.G = PrimaryColor.G;
            // CurrentPixel.B = PrimaryColor.B;
            // CurrentPixel.A = PrimaryColor.A;
            dst[x,y] = CurrentPixel;
        }
    }
}

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
IntSliderControl angle = 8; // [4,36] Slices
#endregion

private double radius = 0;
private double rads = 1337;

// The slope of a "slice" of the pie
private double slope = double.PositiveInfinity;
private int centerX = 0;
private int centerY = 0;

private bool isUnderAngle(int x, int y) {
    return (x * slope <= y) && (y >= 0);
}

private bool isInArc(int x, int y) {
    return Math.Sqrt(x*x + y*y) < radius;
}

// Set up globals
void PreRender(Surface dst, Surface src) {
    Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();

    if (radius == 0) {
        radius = Math.Min(selection.Height / 2, selection.Width / 2);
    }

    if (rads == 1337) {
        rads = (Math.PI * 2) / angle;
    }

    if (slope == double.PositiveInfinity) {
        slope = Math.Tan(rads);
    }

    if (centerX == 0) {
        centerX = (selection.Right - selection.Left) / 2;
        centerX += selection.Left;
    }

    if (centerY == 0) {
        centerY = (selection.Bottom - selection.Top) / 2;
        centerY += selection.Top;
    }
}

void Render(Surface dst, Surface src, Rectangle rect)
{
    ColorBgra CurrentPixel;
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {
            CurrentPixel = src[x,y];
            
            if (!(isUnderAngle(x-centerX, y-centerY) || !(isInArc(x-centerX,y-centerY)) )) {
                CurrentPixel.A = 0;
            }

            dst[x,y] = CurrentPixel;
        }
    }
}

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

private bool isInArc(int x, int y) {
    // Adjust so we are working in reference to the center of the circle
    x -= centerX;
    y -= centerY;

    // Easy to tell if radius too large
    if (Math.Abs(x) > radius || Math.Abs(y) > radius) 
        return false;

    if (Math.Sqrt(x*x + y*y) > radius)
        return false;

    // We already know y is in range; this avoids div by 0 errors
    if (x < 0) return false;

    #if DEBUG
    Debug.WriteLine("X,Y: " + x + ", " + y);
    Debug.WriteLine("Atan(y/x): " + Math.Atan((double)y/(double)x));
    Debug.WriteLine("Rads: " + rads);
    #endif

    // Make sure the point is between the angle we want and the 90* line
    double point_ang = Math.Atan((double)y/(double)x);
    return (point_ang <= 0.5 && point_ang >= 0.5 - rads);
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
            
            if (!isInArc(x,y)) {
                CurrentPixel.A = 0;
            }

            dst[x,y] = CurrentPixel;
        }
    }
}

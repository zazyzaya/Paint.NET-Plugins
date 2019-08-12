// Name:
// Submenu:
// Author:
// Title:
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
IntSliderControl Amount1 = 0; // [0,100] Slider 1 Description
IntSliderControl Amount2 = 0; // [0,100] Slider 2 Description
IntSliderControl Amount3 = 0; // [0,100] Slider 3 Description
#endregion

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

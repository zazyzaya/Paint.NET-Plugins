// Name: Pixelated Abstraction
// Submenu: Stylize
// Author: Zaya
// Title:
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
IntSliderControl scale = 1; // [1,100] Scale Change
IntSliderControl num_colors = 2; // [2, 255] Colors
#endregion

double M, N;
double newW, newH;
double[] dist_arr;
double[,] super_pixels; 

// Use KNN to assign superpixels O(n^3) must be done once
void PreRender(Surface dst, Surface src) {
    newW = Math.Floor(src.Width / scale);
    newH = Math.Floor(src.Height / scale);
    
    M = src.Height * src.Width;
    N = newW * newH;

    ColorBgra[] pallette = new ColorBgra[num_colors];
    super_pixels = new double[src.Height, src.Width];
    
    // Initialize pallette
    int[] sum = [0,0,0];
    for (int x=0; x<src.Height; x++) {
        for (int y=0; y<src.Width; x++) {
            sum[0] += (int)src[x,y].B;
            sum[1] += (int)src[x,y].G;
            sum[2] += (int)src[x,y].R;
        }
    }

    sum[0] /= M;
    sum[1] /= M;
    sum[2] /= M;
    pallette[0] = ColorBgra.FromBgr((byte)sum[0], (byte)sum[1], (byte)sum[2]);

    
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

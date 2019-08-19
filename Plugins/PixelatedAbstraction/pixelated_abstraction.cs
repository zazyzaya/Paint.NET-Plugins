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
IntSliderControl weight = 10; // [0, 20] Weight
#endregion

double M, N;
double newW, newH;
double[] super_pixels; 

// Distance function to determine how "close" two pixels are   __
// Because this is called a zillion times, precalculate     m\/N/M
double distance(ColorBgra c1, int x1, int y1, ColorBgra c2, int x2, int y2, double coef) {
    double dc = Math.Sqrt(
        Math.Pow((c1.R - c2.R), 2) + 
        Math.Pow((c1.G - c2.G), 2) +
        Math.Pow((c1.B - c2.B), 2)
    );

    double dp = Math.Sqrt(
        Math.Pow((x1-x2), 2) +
        Math.Pow((y1-y2), 2)
    );

    return dc + coef*dp;
}

Pair<int, int> findNearest(ColorBgra input, int ix, int iy, ColorBgra[,] superPixels, int sy, int sx) {
    Pair<int, int> nearest = new Pair<int, int>(0,0);
    double nearest_val = 10000;

    double coef = (double)weight * Math.Sqrt((double)(N) / (double)(M))
    for (int y=0; i<sy; y++) {
        for (int x=0; x<sx; x++) {
            double dst = distance(input, ix, iy, superPixels[x,y], x, y, coef);
            
            if (dst < nearest_val) {
                nearest_val = dst;
                nearest = new Pair<int, int>(x,y);
            }
        }
    }

    return nearest;
}

// Use KNN to assign superpixels O(n^3) must be done once
// Technically this should all be done in LAB color space, but 
// PDN doesn't have an easy way to convert, so we're using sad ol RGB
void PreRender(Surface dst, Surface src) {
    newW = Math.Floor(src.Width / scale);
    newH = Math.Floor(src.Height / scale);
    
    M = src.Height * src.Width;
    N = newW * newH;

    int pallette_size = 1;
    ColorBgra[] pallette = new ColorBgra[num_colors];
    super_pixels = new ColorBgra[newW, newH];
    
    // Initialize pallette
    int[] sum = [0,0,0];
    for (int y=0; y<src.Height; y++) {
        for (int x=0; x<src.Width; x++) {
            sum[0] += (int)src[x,y].B;
            sum[1] += (int)src[x,y].G;
            sum[2] += (int)src[x,y].R;
        }
    }

    sum[0] /= M;
    sum[1] /= M;
    sum[2] /= M;
    pallette[0] = ColorBgra.FromBgr((byte)sum[0], (byte)sum[1], (byte)sum[2]);

    // All superpixels have mean color first iteration
    for (int y=0; y<newH; y++) {
        for (int x=0; x<newW; x++) {
            super_pixels[x,y] = pallette[0];
        }
    }

    Pair<int, int>[,] assigned_sp = new Pair<int, int>[src.Height, src.Width];
    int superH = src.Height, superW = src.Width;

    while (pallette_size < num_colors) {
        // find nearest neighbor
        for (int y=0; y<src.Height; y++) {
            for (int x=0; x<src.Width; x++) {
                assigned_sp[x,y] = findNearest(src[x,y], x, y, super_pixels, superH, superW);
            }
        }

        // move superpixels

        // adjust color pallette
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

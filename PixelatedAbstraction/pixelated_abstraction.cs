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

int N, M;
int sp_width, sp_height, sel_width, sel_height;
double sp_unit_width, sp_unit_height;

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

double[,] findNearest(Rectangle selection, Surface src, ColorBgra me, int sx, int sy) {
    double[,] distances = new double[sel_width, sel_height];
    for (int y=selection.Top; y<selection.Bottom; y++) {
        for (int x=selection.Left; x<selection.Right; x++) {
            distances[x,y] = distance(src[x,y], x, y, me, sx, sy)
        }
    }

    return distances;
}

// Use KNN to assign superpixels O(n^3) must be done once
// Technically this should all be done in LAB color space, but 
// PDN doesn't have an easy way to convert, so we're using sad ol RGB
void PreRender(Surface dst, Surface src) {
    Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();
    sel_width = (selection.Right - selection.Left);
    sel_height = (selection.Top - selection.Bottom);
    M = sel_height * sel_width;

    // How many super pixels per row/col
    sp_width = (selection.Right - selection.Left) / scale;
    sp_height = (selection.Top - selection.Bottom) / scale;
    N = sp_width * sp_height;

    // How large is one superpixel
    sp_unit_width = (selection.Right - selection.Left) / sp_width;
    sp_unit_height = (selection.Top - selection.Bottom) / sp_height;

    int pallette_size = 1;
    ColorBgra[] pallette = new ColorBgra[num_colors];
    
    // Initialize pallette
    int[] sum = [0,0,0];
    for (int y=selection.Top; y<selection.Bottom; y++) {
        for (int x=selection.Left; x<selection.Right; x++) {
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
    for (int y=0; y<sp_height; y++) {
        for (int x=0; x<sp_width; x++) {
            super_pixels[x,y] = pallette[0];
        }
    }

    Pair<int, int>[ , , ] neighbors = new Pair<int, int>[sp_width, sp_height, M];
    while (pallette_size < num_colors) {
        // find nearest neighbors
        for (int y=0; y<sp_height; y++) {
            for (int x=0; x<sp_width; x++) {

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

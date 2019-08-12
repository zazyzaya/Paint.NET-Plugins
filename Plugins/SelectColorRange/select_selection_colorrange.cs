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
int tolerance=5; // [0, 100] Tolerance
ColorBgra color = ColorBgra.FromBgr(0,0,0); // Color
bool invert = false; // [X] Invert
#endregion

// The distance between [0,0,0] and [255,255,255]
private static double MAX_DIST = 441.6729; 

private double vect_abs(double b, double g, double r) {
    double dist = (double)Math.Sqrt(b*b + g*g + r*r);

    #if DEBUG
    Debug.WriteLine(dist);
    #endif

    return (dist / MAX_DIST) * 100;
}

void Render(Surface dst, Surface src, Rectangle rect)
{
    PdnRegion srcSelect = EnvironmentParameters.GetSelection(src.Bounds);

    ColorBgra curPx;
    double difB, difG, difR;
    for (int y = rect.Top; y<rect.Bottom; y++) {
        for (int x = rect.Left; x<rect.Right; x++) {
    
            curPx = src[x, y];
            difB = curPx.B - color.B;
            difG = curPx.G - color.G;
            difR = curPx.R - color.R;

            if (vect_abs(difB, difG, difR) > tolerance) {
                if (invert) curPx.A = 0;
            }
            else {
                if (!invert) curPx.A = 0;
            }
            
            dst[x, y] = curPx;
        }
    }    
}

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
int addR=0;   //[-100,100]Cyan - Red
int addG=0;   //[-100,100]Magenta - Green
int addB=0;   //[-100,100]Yellow - Blue
#endregion

private byte clamp2byte(int val) {
    if (val < 0) return 0;
    if (val > 255) return 255;
    return (byte)val;
}

void Render(Surface dst, Surface src, Rectangle rect)
{
    ColorBgra CurrentPixel;
    int r, g, b;
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {
            CurrentPixel = src[x,y];
            
            r = (int)CurrentPixel.R;
            g = (int)CurrentPixel.G;
            b = (int)CurrentPixel.B;

            // Adjust red
            r += addR;
            g -= addR << 2;
            b -= addR << 2;

            // Adjust green
            r -= addG << 2;
            g += addG;
            b -= addG << 2;

            // Adjust blue
            r -= addB << 2;
            g -= addB << 2;
            b += addB;

            CurrentPixel = ColorBgra.FromBgra(clamp2byte(b), clamp2byte(g), clamp2byte(r), CurrentPixel.A);
            dst[x,y] = CurrentPixel;
        }
    }
}

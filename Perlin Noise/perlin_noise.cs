// Name: Perlin Noise
// Submenu: Noise
// Author: Zaya
// Title: Perlin Noise Generator
// Version: 1.0
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
DoubleSliderControl p_width = 15; // [0,100] Gradient Width
DoubleSliderControl p_height = 15; // [0,100] Gradient Height
ColorWheelControl white = ColorBgra.FromBgr(255,255,255); // [White] Primary Color
ColorWheelControl black = ColorBgra.FromBgr(0,0,0); // [Black] Secondary Color
ListBoxControl ang_range = 0; // Angle Range|90|45|22.5|full
CheckboxControl not_smooth = false; // [X] Disable Smoothing
CheckboxControl rings = false; // [0,1] Rings
TextboxControl seed = ""; // [0,255] Seed
#endregion

struct Vector {
    public double x;
    public double y;

    public Vector (double xx, double yy) {
        x = xx; y = yy;
    }
}

struct Coordinate {
    public int x;
    public int y;

    public Coordinate (int xx, int yy) {
        x = xx; y = yy;
    }
}

struct RGBVector {
    public int r;
    public int g;
    public int b;

    public RGBVector(int rr, int gg, int bb) {
        r = rr; g = gg; b = bb;
    }

    public RGBVector ScalarMult(double multiplier) {
        return new RGBVector(
            (int)(r * multiplier),
            (int)(g * multiplier),
            (int)(b * multiplier)
        );
    }

    public RGBVector Add(RGBVector other) {
        return new RGBVector(
            r + other.r,
            g + other.g,
            b + other.b
        );
    }

    public RGBVector FromBgra(ColorBgra c) {
        return new RGBVector(
            c.R,
            c.G,
            c.B
        );
    }

    public ColorBgra ToBgra() {
        return ColorBgra.FromBgr(
            (byte) b, 
            (byte) g,
            (byte) r
        );
    }
}

Random rnd;
Vector[] possible_angles;
int[ , ] gradient;          // Can't use pointers, but can use index of possible angles 
Vector[ , ] slowGradient;   // If all angles selected, have to do this the slow way
int g_width, g_height;
double g_unit_width, g_unit_height;

RGBVector usr_w, usr_b;

// Perlin Simplex method; faster to use pre allowed vectors
int GetRandVector() {
    int i = rnd.Next(8);
    
    // So 90 will only pick 0 or 4, 45 will only pick evens and 22.5 picks any
    i %= 1 << (ang_range + 1);
    i *= 1 << (2 - ang_range);
    
    // Decide randomly if negative or positive
    if (rnd.Next(2) % 2 == 0)
        return i;
    else    
        return i + 8;
}

// Original Perlin noise; slower but more varied
Vector GetRandVectorSlow() {
    double ang = 2*Math.PI*rnd.NextDouble();
    return new Vector(Math.Cos(ang), Math.Sin(ang));
}

unsafe void BuildGradient() {
    // Seed random generator (this should not be run concurrently)
    if (String.Equals(seed, "")) {
        rnd = new Random();
    }
    else {
        rnd = new Random(seed.GetHashCode());
    }

    // Predefined angles
    if (ang_range < 3) {
        gradient = new int[g_width+3, g_height+3];
        for (int y=0; y<=g_height+2; y++) {
            for (int x=0; x<=g_width+2; x++) {
                gradient[x,y] = GetRandVector();
            }
        }
        return;
    }

    // Pseudo-random angles
    else {
        slowGradient = new Vector[g_width+3, g_height+3];
        for (int y=0; y<=g_height+2; y++) {
            for (int x=0; x<=g_width+2; x++) {
                slowGradient[x,y] = GetRandVectorSlow();
            }
        }
        return;
    }
}

// Returns a list of the 4 gradient vectors that surround a given coord
// Assumes the input coordinate is normalized (e.g. 0,0 means the top, left of the selection)
Coordinate[] FindCellCorners(int x, int y) {
    int gx = (int)(x / g_unit_width);
    int gy = (int)(y / g_unit_height);

    // Call any point on the right/bottom-most boarder a member of the 
    // Grid to the left/bottom
    if (gx == g_width+1) gx--;
    if (gy == g_height+1) gy--;

    return new Coordinate[] {
        new Coordinate(gx,gy), new Coordinate(gx+1, gy), 
        new Coordinate(gx, gy+1), new Coordinate(gx+1, gy+1)
    };
}

// Perform linear interpolation on 2 vectors and a weight
double lerp(double a, double b, double x) {
    return a + x*(b-a);
}

// Perform smoothing on an input value to make contrast less harsh
double smooth(double x) {
    return 6*Math.Pow(x, 5) - 15*Math.Pow(x, 4) + 10*Math.Pow(x, 3);
}

// Returns the Perlin noise value for a given point
// Assumes the input coordinate is normalized (e.g. 0,0 means the top, left of the selection)
double Perlin(int ox, int oy) {
    Coordinate[] neighbors = FindCellCorners(ox,oy);
    
    // Find coord in reference to top left of grid box
    double x = ((double) ox / g_unit_width);
    double y = ((double) oy / g_unit_height);

    // Find the dot product of the vector pointing to the corner and
    // That corner's vector
    double[] dps = new double[4]; 
    for (int i=0; i<4; i++) {
        double dx, dy;                                          // Distance between me and neighbor
        Vector angle;                                           // Neighbor's vector
        int gx=neighbors[i].x, gy=neighbors[i].y;               // Neighbor's position

        // Neighbor's angle
        if (ang_range < 3)
            angle = possible_angles[gradient[gx,gy]];
        else 
            angle = slowGradient[gx, gy];
        

        // Find distance vector
        if (!rings) {
            dx = x - (double)gx;
            dy = y - (double)gy; // May need to flip this over (make +)
        }
        // Was originally a bug, but I think it makes a cool effect
        else {
            dx = x - angle.x;
            dy = x - angle.y;
        }

        // Take dot product w gradient vector
        dps[i] = angle.x * dx + angle.y * dy;
    }

    // No longer care about the decimal part
    x -= Math.Truncate(x);
    y -= Math.Truncate(y);

    if (!not_smooth) {
        x = smooth(x);
        y = smooth(y);
    }

    // Linear interpolation of dotproducts
    double x1 = lerp(dps[0], dps[1], x);
    double x2 = lerp(dps[2], dps[3], x);

    double ret = lerp(x1, x2, y);
    if (rings) return ret;

    // Rounding errors sometimes cause weird rings
    if (ret > 1) return 1.0;
    if (ret < -1) return -1.0;
    return ret;
}


void PreRender(Surface dst, Surface src) {
    // Also the negative of all of these angles
    possible_angles = new Vector[] {
        new Vector(1.0, 0.0), new Vector(0.92388, 0.382683), new Vector(0.707107, 0.707107), new Vector(0.382683, 0.92388),
        new Vector(0.0, 1.0), new Vector(-0.382683, 0.92388), new Vector(-0.707107, 0.707107), new Vector(-0.92388, 0.382683),
        new Vector(-1.0, -0.0), new Vector(-0.92388, -0.382683), new Vector(-0.707107, -0.707107), new Vector(-0.382683, -0.92388),
        new Vector(-0.0, -1.0), new Vector(0.382683, -0.92388), new Vector(0.707107, -0.707107), new Vector(0.92388, -0.382683)
    };

    Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();

    // How wide/tall the gradient blocks are
    g_unit_width = (selection.Right - selection.Left) * (p_width / 100);
    g_unit_height = (selection.Bottom - selection.Top) * (p_height / 100);

    // Prevent gradient squares from being smaller than pixels
    if (g_unit_width < 2) {
        g_unit_width = 2;
        g_width = (selection.Right - selection.Left) / 2;
    } else {
        g_width = (int) Math.Ceiling((100 / p_width));
    }

    if (g_unit_height < 2) {
        g_unit_height = 2;
        g_height = (selection.Bottom - selection.Top)/2;
    } else {
        g_height = (int) Math.Ceiling((100 / p_height));
    }

    BuildGradient();
}

unsafe void Render(Surface dst, Surface src, Rectangle rect)
{
    Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();
    RGBVector colorDifference = new RGBVector(
        black.R - white.R,
        black.G - white.G,
        black.B - white.B
    );

    RGBVector tmp = new RGBVector();
    usr_w = tmp.FromBgra(white);

    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);
        for (int x = rect.Left; x < rect.Right; x++)
        {
            int adjY = y - selection.Top;
            int adjX = x - selection.Left;

            double noise = Perlin(adjX, adjY);
            noise = (noise + 1) / 2; // Normalize it to be [0-1]
            tmp = colorDifference.ScalarMult(noise);
            tmp = tmp.Add(usr_w);            

            *dstPtr = tmp.ToBgra();
            dstPtr++;
        }
    }
}
// Name: Perlin Noise
// Submenu: Noise
// Author: Zaya
// Title: Perlin Noise Generator
// Version: 1.2
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
DoubleSliderControl p_width = 10; // [0,100] Gradient Width
DoubleSliderControl p_height = 10; // [0,100] Gradient Height
IntSliderControl octaves = 1; // [1,10] Octaves
DoubleSliderControl persistance = 0.25; // [0.0001,1] 
ColorWheelControl white = ColorBgra.FromBgr(255,255,255); // [White] Primary Color
ColorWheelControl black = ColorBgra.FromBgr(0,0,0); // [Black] Secondary Color
ListBoxControl ang_range = 3; // Angle Range|90|45|22.5|full
ListBoxControl coloring = 0; // Coloring Options|Default|Islands|Dalmation
CheckboxControl not_smooth = false; // [0,1] Disable Smoothing
CheckboxControl rings = false; // [0,1] Rings
TextboxControl seed = ""; // [0,255] Seed
#endregion


/************** Structs  ****************/

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

    public RGBVector Sub(RGBVector other) {
        return new RGBVector(
            r - other.r,
            g - other.g,
            b - other.b
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


/**************** Globals  **************/

Random rnd;
Vector[] possible_angles;
int[ , ] gradient;          // Can't use pointers, but can use index of possible angles 
Vector[ , ] slowGradient;   // If all angles selected, have to do this the slow way
int g_width, g_height;
double g_unit_width, g_unit_height;
int num_x, num_y;
RGBVector usr_w, usr_b, colorDifference;


/***************** Perlin Noise Math ************/


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
        // Need larger gradient as more octaves added (gradient doubles in size each time)
        // Otherwise there is an undesired "tiling" effect
        gradient = new int[ num_x, num_y ];
        for (int y=0; y<num_y; y++) {
            for (int x=0; x<num_x; x++) {
                gradient[x,y] = GetRandVector();
            }
        }
        return;
    }

    // Pseudo-random angles
    else {
        slowGradient = new Vector[ num_x, num_y];
        for (int y=0; y<num_y; y++) {
            for (int x=0; x<num_x; x++) {
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
    if (gx >= num_x) gx--;
    if (gy >= num_x) gy--;

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
        // Fixed up a bit to make rings same size across image
        else {
            dx = (double)gx % p_width - angle.x;
            dy = (double)gy % p_height - angle.y;
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

double PerlinOctaves(int x, int y) {
    double total=0, amplitude=1, max=0, frequency=1;

    for (int i=0; i<octaves; i++) {
        total += Perlin(x * (int)frequency, y * (int)frequency) * amplitude;
        max += amplitude;

        amplitude *= persistance;
        frequency *= 1.9;
    }

    // Normalize to be between [-1, 1]
    return total/max;
}


/******************* Coloring Functions *****************/

ColorBgra NormalColor(double noise) {
    RGBVector tmp = colorDifference.ScalarMult(noise);
    tmp = tmp.Add(usr_w); 
    return tmp.ToBgra();
}

ColorBgra Dalmation(double noise) {
    if (noise >= 0.5)   return usr_w.ToBgra();
    else                return usr_b.ToBgra();
}

ColorBgra Islands(double noise) {
    double  wco=0.50,   // Water cut-off
            sco=0.52,    // Sand cut-off
            ico=0.60,   // Island cut-off 
            mco=0.70;   // Mountain cut-off

    // Water (amount of greenness determined by closeness to land)
    // G in {0, 225}
    if (noise <= wco) {
        int r=0,b=255,g;
        
        double f_g = (noise/wco);                               // Put it between 0 and 1
        f_g = 225*Math.Pow((f_g-1), 2) + 450*(f_g-1) + 225;    // Run it through parabolic function with max 225, min 0
        g = (int)f_g;

        return ColorBgra.FromBgr((byte)b, (byte)g, (byte)r);
    }

    // Sand { (255, 255, 209) - (220, 220, 179)}
    else if (noise < sco) {
        double darken = (noise - wco) / (sco - wco);
        darken *= 35;

        RGBVector tmp = new RGBVector((int)(255-darken), (int)(255-darken), (int)(209-darken));
        return tmp.ToBgra();
    }

    // Land R in {60 - 160}, G=145, B=48
    else if (noise < ico){
        int r_val, g_val=145, b_val=48;
        double redness = (noise-sco) / (ico-sco);
        redness = 100*(redness) + 75 + rnd.Next(5);
        r_val = (int)(redness);

        return ColorBgra.FromBgr((byte)b_val, (byte)g_val, (byte)r_val);
    }

    else if (noise < mco){
        RGBVector min = new RGBVector(180, 145, 48);
        RGBVector max = new RGBVector(70, 29, 12);
        RGBVector dif = max.Sub(min);

        double noise_amt = (noise-ico) / (mco-ico);
        noise_amt = Math.Pow(noise_amt, 0.5);
        dif = dif.ScalarMult(noise_amt);

        dif = min.Add(dif);
        return dif.ToBgra();
    }
    
    else {
        RGBVector min = new RGBVector(230, 231, 232);
        RGBVector max = new RGBVector(255, 255, 255);
        RGBVector dif = max.Sub(min);

        double noise_amt = (noise-mco) / (1-mco);
        noise_amt = Math.Pow(noise_amt, 0.5);
        dif = dif.ScalarMult(noise_amt);

        dif = min.Add(dif);
        return dif.ToBgra();
    }

}

// A number of cool coloring options for the noise
// Coloring Options|Default|Islands|Dalmation
ColorBgra ColorPx(double noise) {
    ColorBgra ret;
    switch(coloring) {
        case 1: 
            ret = Islands(noise);
            break;
        case 2:
            ret = Dalmation(noise);
            break;

        default: 
            ret = NormalColor(noise);
            break;
    }

    return ret;
}


/******************** Rendering Methods ********************/

void PreRender(Surface dst, Surface src) {
    // Also the negative of all of these angles
    possible_angles = new Vector[] {
        new Vector(1.0, 0.0), new Vector(0.92388, 0.382683), new Vector(0.707107, 0.707107), new Vector(0.382683, 0.92388),
        new Vector(0.0, 1.0), new Vector(-0.382683, 0.92388), new Vector(-0.707107, 0.707107), new Vector(-0.92388, 0.382683),
        new Vector(-1.0, -0.0), new Vector(-0.92388, -0.382683), new Vector(-0.707107, -0.707107), new Vector(-0.382683, -0.92388),
        new Vector(-0.0, -1.0), new Vector(0.382683, -0.92388), new Vector(0.707107, -0.707107), new Vector(0.92388, -0.382683)
    };

    Rectangle selection = EnvironmentParameters.GetSelectionAsPdnRegion().GetBoundsInt();

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

    num_x = (int) Math.Pow(2, octaves-1) * g_width + 3;
    num_y = (int) Math.Pow(2, octaves-1) * g_height + 3;

    BuildGradient();
}

unsafe void Render(Surface dst, Surface src, Rectangle rect)
{
    Rectangle selection = EnvironmentParameters.GetSelectionAsPdnRegion().GetBoundsInt();
    
    // Set up globals for coloring functions
    RGBVector tmp = new RGBVector();
    usr_w = tmp.FromBgra(white);
    usr_b = tmp.FromBgra(black);

    colorDifference = new RGBVector(
        black.R - white.R,
        black.G - white.G,
        black.B - white.B
    );

    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);
        for (int x = rect.Left; x < rect.Right; x++)
        {
            int adjY = y - selection.Top;
            int adjX = x - selection.Left;

            double noise = PerlinOctaves(adjX, adjY);
            noise = (noise + 1) / 2; // Normalize it to be [0-1]           

            *dstPtr = ColorPx(noise);
            dstPtr++;
        }
    }
}
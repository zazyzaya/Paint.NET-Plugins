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
DoubleSliderControl p_width = 50; // [1,100] Gradient Width
DoubleSliderControl p_height = 50; // [1,100] Gradient Height
ListBoxControl ang_range = 0; // Angle Range|90|45|22.5|full
CheckboxControl not_smooth = false; // [X] Disable Smoothing
CheckboxControl rings = false; // [0,1] Rings
TextboxControl seed = ""; // [0,255] Seed
#endregion

//AngleControl ang_start = 0; // [-180,180] Rotation

Random rnd;
Tuple<double, double>[] possible_angles;
Tuple<double, double>[ , ] gradient = null;
int g_width, g_height;
double g_unit_width, g_unit_height;

Tuple<double, double> GetRandVector() {
    // Much faster way of doing this; Perlin simplex noise method
    if (ang_range < 3) {
        int i = rnd.Next(8);
        
        // So 90 will only pick 0 or 4, 45 will only pick evens and 22.5 picks any
        i %= 1 << (ang_range + 1);
        i *= 1 << (2 - ang_range);
        
        if (rnd.Next(2) != 0) {
            return possible_angles[i];
        }
        // Negate it half the time
        else {
            return new Tuple<double, double>(-possible_angles[i].Item1, -possible_angles[i].Item2);
        }
    }

    // Generate random angle of dist 1
    // Slower way of doing this; Perlin original noise method
    else {
        double ang = 2*Math.PI*rnd.NextDouble();
        return new Tuple<double, double>(Math.Cos(ang), Math.Sin(ang));
    }
}

void BuildGradient() {
    // Seed random generator (this should not be run concurrently)
    if (String.Equals(seed, "")) {
        rnd = new Random();
    }
    else {
        rnd = new Random(seed.GetHashCode());
    }

    gradient = new Tuple<double, double>[g_width+3, g_height+3];
    for (int y=0; y<=g_height+2; y++) {
        for (int x=0; x<=g_width+2; x++) {
            gradient[x,y] = GetRandVector();
        }
    }
}

// Returns a list of the 4 gradient vectors that surround a given coord
// Assumes the input coordinate is normalized (e.g. 0,0 means the top, left of the selection)
Tuple<int, int>[] FindCellCorners(int x, int y) {
    int gx = (int)(x / g_unit_width);
    int gy = (int)(y / g_unit_height);

    // Call any point on the right/bottom-most boarder a member of the 
    // Grid to the left/bottom
    if (gx == g_width+1) gx--;
    if (gy == g_height+1) gy--;

    return new Tuple<int, int>[] {
        Tuple.Create(gx,gy), Tuple.Create(gx+1, gy), 
        Tuple.Create(gx, gy+1), Tuple.Create(gx+1, gy+1)
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
    Tuple<int, int>[] neighbors = FindCellCorners(ox,oy);
    
    // Find coord in reference to top left of grid box
    double x = ((double) ox / g_unit_width);
    double y = ((double) oy / g_unit_height);

    // Find the dot product of the vector pointing to the corner and
    // That corner's vector
    double[] dps = new double[4]; 
    for (int i=0; i<4; i++) {
        int gx=neighbors[i].Item1, gy=neighbors[i].Item2;
        double sx=gradient[gx, gy].Item1, sy=gradient[gx, gy].Item2;
        double dx, dy;

        // Find distance vector
        if (!rings) {
            dx = x - (double)gx;
            dy = y - (double)gy; // May need to flip this over (make +)
        }
        // Was originally a bug, but I think it makes a cool effect
        else {
            dx = x - sx;
            dy = x - sy;
        }

        // Take dot product w gradient vector
        dps[i] = sx * dx + sy + dy;
    }

    // No longer care about non-decimal part TODO add smoothing here
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
    possible_angles = new Tuple<double, double>[] {
        Tuple.Create(1.0, 0.0), Tuple.Create(0.92388, 0.382683), Tuple.Create(0.707107, 0.707107), Tuple.Create(0.382683, 0.92388),
        Tuple.Create(0.0, 1.0), Tuple.Create(-0.382683, 0.92388), Tuple.Create(-0.707107, 0.707107), Tuple.Create(-0.92388, 0.382683),
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
        g_height = (selection.Bottom - selection.Top) / 2;
    } else {
        g_height = (int) Math.Ceiling((100 / p_height));
    }

    BuildGradient();
}

void Render(Surface dst, Surface src, Rectangle rect)
{
    Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();

    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {
            int adjY = y - selection.Top;
            int adjX = x - selection.Left;

            double noise = Perlin(adjX, adjY);
            noise = (noise + 1) / 2; // Normalize it to be [0-1]
            Debug.WriteLine("Noise: " + noise);

            byte val = (byte)(255 * noise);
            Debug.WriteLine("Val: " + val);
            dst[x,y] = ColorBgra.FromBgr(val, val, val);
        }
    }
}

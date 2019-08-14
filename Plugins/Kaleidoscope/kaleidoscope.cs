// Name: Kaleidoscope
// Submenu: Zaya's tools
// Author: Zaya
// Title:
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
IntSliderControl slices = 4; // [2,36] Slices
CheckboxControl square = false; // [0,1] Square
AngleControl offset = 90; // [-180,180] Sample Angle 
#endregion

// Based on selection; defined during PreRender
private int centerX = 0;
private int centerY = 0;
private double radius = 0;

// Statics (prevent from computing over and over)
private static double piOver2 = Math.PI / 2;
private static double threePiOver2 = 3*piOver2;
private static double twoPi = 2*Math.PI;

// Based on usr input; defined during Render
private double theta;
private double slope;

// TODO 
private double phi_slope;
private double phi;

// Returns a coordinates position if it were rotated theta radians
private Pair<int, int> getRotatedCoords(double theta, int x, int y) {
    double sin = Math.Sin(theta);
    double cos = Math.Cos(theta);

    Pair<int, int> ret = new Pair<int, int>(
        (int)((double)x*cos - (double)y*sin),
        (int)((double)x*sin + (double)y*cos)
    );

    return ret;
}

// Safely convert angle to slope of line made by that angle
private double getSlope(double angle) {
    if (angle != piOver2 && angle != 3*piOver2) {
        return Math.Tan(angle);
    }

    if (angle == piOver2) {
        return double.PositiveInfinity;
    }

    else {
        return double.NegativeInfinity;
    }
}

private bool isInRange(int x, int y) {
    bool tCheck, gCheck;

    // Set theta slope tests
    if (slope == double.PositiveInfinity) {
        tCheck = x < 0;
    }
    else if (slope == double.NegativeInfinity) {
        tCheck = x > 0;
    }
    else if (slope > 0) {
        tCheck = x*slope < y;
    }
    else if (slope < 0) {
        tCheck = x*slope > y;
    }
    else { // Slope is 0 means it depends where the phi slope is pointing, up or down
        tCheck = phi_slope > slope ? slope*x < y : slope*x > y;
    }

    // Skip the phi tests if the theta test fails
    if (!tCheck)
        return false;

    // Set phi slope tests
    if (phi_slope == double.PositiveInfinity) {
        gCheck = x >= 0;
    }
    else if (phi_slope == double.NegativeInfinity) {
        gCheck = x <= 0;
    }
    else if (phi_slope > 0) {
        gCheck = phi_slope * x >= y;
    }
    else if (phi_slope < 0) {
        gCheck = phi_slope * x <= y;
    }
    else { // Same deal as theta slope; if it's zero, it depends where the other is at
        gCheck = phi_slope > slope ? phi_slope*x > y : phi_slope*x < y;
    }

    // By now we know tCheck is true, so true && gCheck = gCheck
    return gCheck;
}

private bool isInArc(int x, int y) {
    if (!square)
        return Math.Sqrt(x*x + y*y) < radius;
    
    else 
        return (Math.Abs(x) <= radius && Math.Abs(y) <= radius);
}

// Uses coordinates to determine exact angle within range 0 - 2Pi
private double smartAtan(double angle, double x, double y) {
    if (x == 0) {
        if (y == 0) return phi; // Easier to just let origin be in initial slice
        return y > 0 ? piOver2 : threePiOver2;
    }

    double atan = Math.Atan(y/x);
    if (x > 0) {
        return y > 0 ? atan : atan + twoPi;
    }
    // For readability. I know it's not necessary
    else {
        return Math.PI + atan;
    }

}

// Set up globals
void PreRender(Surface dst, Surface src) {
    Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();

    if (radius == 0) {
        radius = Math.Min(selection.Height / 2, selection.Width / 2);
    }

    if (centerX == 0) {
        centerX = ((selection.Right - selection.Left) / 2) + selection.Left;
    }

    if (centerY == 0) {
        centerY = ((selection.Bottom - selection.Top) / 2) + selection.Top;
    }
}

void Render(Surface dst, Surface src, Rectangle rect)
{
    Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();
    
    phi = (Math.PI / 180) * offset;
    theta = offset_ang + (Math.PI / slices);

    slope = getSlope(theta);
    phi_slope = getSlope(phi);
    
    ColorBgra CurrentPixel;
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {
            CurrentPixel = src[x,y];
            
            int adjX=x-centerX, adjY=centerY-y;
            // Check if inside circle
            if (isInArc(adjX, adjY)) {

                // Check if in angle we are keeping; otherwise, find the appropriate region in original slice
                if ( !isInRange(adjX, adjY) ) {
                    double rotations;
                    double gamma;   // Distance between this angle and theta

                    gamma = smartAtan((gamma, (double)adjX, (double)adjY);
                    rotations = Math.Floor((phi - gamma) / theta);
                    
                    // Angle is the same on either horizontal line of symmetry of the circle
                    // This is to compensate
                    // NOTE: Only works for even numbers right now
                    if (adjX < 0) {
                        rotations += slices;
                    }
                    
                    Pair<int, int> rotatedCoords = getRotatedCoords(rotations * theta, adjX, adjY);
                    CurrentPixel = src[rotatedCoords.First + centerX, centerY-rotatedCoords.Second];
                }
            }
            else {
                CurrentPixel.A = 0;    
            }
            
            dst[x,y] = CurrentPixel;
        }
    }
}

// Name: zKaleidoscope
// Submenu: Distort
// Author: Zaya
// Title:
// Version: 1.2
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
IntSliderControl slices = 4; // [2,180] Slices
CheckboxControl square = false; // [0,1] Square
AngleControl offset = 45; // [0,360] Sample Angle 
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
private double angle;
private double theta;
private double theta_slope;
private double phi_slope;
private double phi;
private int tQuad, pQuad;
private bool sameQuadrant;

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
    bool tCheck, pCheck;

    // Theta edge cases
    if (theta_slope == double.PositiveInfinity) {
        tCheck = x < 0;
    }
    else if (theta_slope == double.NegativeInfinity) {
        tCheck = x > 0;
    }
    else {
        switch (tQuad) {
            case 0: // Angle is on the right of the circle 
            case 3: // This means it is a lower boundary
                tCheck = y > x*theta_slope;
                break;
            
            // Cases 1 and 2, left of circle, angle is upper boundary
            default:
                tCheck = y < x*theta_slope;
                break;
        }
    }

    if (!tCheck) return false;

    // Phi edge cases
    if (phi_slope == double.PositiveInfinity) {
        pCheck = x >= 0;
    }
    else if (phi_slope == double.NegativeInfinity) {
        pCheck = x <= 0;
    }
    else {
        switch(pQuad) {
            case 0: // Angle is on the right of the circle
            case 3: // This means it is an upper boundary
                pCheck = y <= x*phi_slope;
                break;
            
            // Cases 1 and 2, left of circle, angle is lower boundary
            default: 
                pCheck = y >= x*phi_slope;
                break;
        }
    }

    // We already know tCheck is true, so True && pCheck == pCheck
    return pCheck;

}

private bool isInArc(int x, int y) {
    if (!square)
        return Math.Sqrt(x*x + y*y) < radius;
    
    else 
        return (Math.Abs(x) <= radius && Math.Abs(y) <= radius);
}

// Uses coordinates to determine exact angle within range 0 - 2Pi
private double smartAtan(double x, double y) {
    if (x == 0) {
        //if (y == 0) return phi; // Easier to just let origin be in initial slice
        return y >= 0 ? piOver2 : threePiOver2;
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
    
    phi = offset * (Math.PI / 180);
    angle = Math.PI / slices;
    theta = phi - angle;
    theta = theta < 0 ? theta + twoPi : theta;

    // Determine the quadrant both angles are in 
    // Useful to have this calculated here (once) rather than in the loops
    tQuad = (int)(theta / piOver2); 
    pQuad = (int)(phi / piOver2);
    sameQuadrant = tQuad == pQuad;

    theta_slope = getSlope(theta);
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

                    gamma = smartAtan((double)adjX, (double)adjY);
                    
                    // Find positive distance between two angles, then rotate however many units of theta
                    // Are needed to put it in original slice
                    double dist = phi > gamma ? phi - gamma : (phi + twoPi) - gamma;

                    rotations = Math.Floor(dist / angle);
                    Pair<int, int> rotatedCoords = getRotatedCoords(rotations * angle, adjX, adjY);
                    
                    CurrentPixel = src[rotatedCoords.First + centerX, centerY-rotatedCoords.Second];
                    //CurrentPixel.A = 0;
                }
            }
            else {
                CurrentPixel.A = 0;    
            }
            
            dst[x,y] = CurrentPixel;
        }
    }
}

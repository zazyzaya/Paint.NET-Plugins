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
//AngleControl offset = 90; // [-180,180] Sample Angle 
#endregion

// Based on selection; defined during PreRender
private int centerX = 0;
private int centerY = 0;
private double radius = 0;

// Statics
private double piOver2 = Math.PI / 2;

// Based on usr input; defined during Render
private double theta;
private double slope;

// TODO 
//private double offset_slope;
//private double offset_ang;

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

private bool isInRange(int x, int y) {
    //if (offset_ang == piOver2)
    return (x >= 0 && y >= 0);

    //else 
    //    return (x * offset_slope <= y);
}

private bool isOverAngle(int x, int y) {
    return (x * slope <= y);
}

private bool isInArc(int x, int y) {
    if (!square)
        return Math.Sqrt(x*x + y*y) < radius;
    
    else 
        return (Math.Abs(x) <= radius && Math.Abs(y) <= radius);
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
    
    theta = Math.PI / slices;
    //offset_ang = (Math.PI / 180) * offset;
    slope = Math.Tan(piOver2 - theta);
    //offset_slope = Math.Tan(piOver2);
    
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

                // Check if in angle we are keeping
                if ( !(isInRange(adjX, adjY) && isOverAngle(adjX, adjY)) ) {
                    double rotations;
                    double phi;

                    // We know its in the bottom-most slice
                    if (adjX == 0) {
                        rotations = slices - 1;
                    }
                    else {
                        phi = (double)(adjY) / (double)(adjX);
                        phi = Math.Atan(phi);
                        rotations = Math.Floor((piOver2 - phi) / theta);
                    }
                    
                    // Angle is the same on either horizontal line of symmetry of the circle
                    // This is to compensate
                    // NOTE: Only works for even numbers right now
                    if (adjX < 0) {
                        rotations += slices;
                    }
                    
                    Pair<int, int> rotatedCoords = getRotatedCoords(rotations * theta, adjX, adjY);

                    /* // I think something's wrong with the rotate method, this catches out of bounds rotations though
                    if (centerX - Math.Abs(rotatedCoords.First) < 0 || centerY - Math.Abs(rotatedCoords.Second) < 0) {
                        CurrentPixel.A = 0;
                    }
                    else { */
                    CurrentPixel = src[rotatedCoords.First + centerX, centerY-rotatedCoords.Second];
                    //}
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

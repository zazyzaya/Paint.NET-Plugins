// Name: Make 3D
// Submenu: Stylize
// Author: Zaya
// Title: Make 3D
// Version: 1.0
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
DoubleSliderControl density = 1; // [0,1] Fog Density
ColorWheelControl black = ColorBgra.FromBgr(0,0,0); // [Black] Secondary Color
AngleControl zrot = 0; // [0,45] Sample Angle 
#endregion

// Quick Matrix struct. Makes life easier
struct Vector3D {
    public double x, y, z;

    public Vector3D(double x, double y, double z) {
        this.x=x; this.y=y; this.z=z;
    }

    public Vector3D(Vector3D other) {
        this.x = other.x;
        this.y = other.y;
        this.z = other.z;
    }

    public double Euclidian() {
        return Math.Sqrt(x*x + y*y + z*z);
    }

    public double Euclidian(Vector3D other) {
        return Math.Sqrt(
            Math.Pow(x-other.x, 2) +
            Math.Pow(y-other.y, 2) +
            Math.Pow(z-other.z, 2)
        );
    }

    public Vector3D ScalarMult(double s) {
        return new Vector3D(s*x, s*y, s*z);
    }

    public Vector3D Add(Vector3D other) {
        return new Vector3D(x+other.x, y+other.y, z+other.z);
    }

    public Vector3D MultBy3x3(Vector3D[] other) {
        Vector3D mx = other[0].ScalarMult(x);
        Vector3D my = other[1].ScalarMult(y);
        Vector3D mz = other[2].ScalarMult(z);

        return mx.Add(my).Add(mz);
    }

    public Vector3D[] GetRotX(double theta) {
        double sin=Math.Sin(theta), cos=Math.Cos(theta);
        
        return new Vector3D[] {
            new Vector3D(1, 0, 0),
            new Vector3D(0, cos, sin),
            new Vector3D(0, -sin, cos)
        };
    }

    public Vector3D[] GetRotY(double theta) {
        double sin=Math.Sin(theta), cos=Math.Cos(theta);
        
        return new Vector3D[] {
            new Vector3D(cos, 0, -sin),
            new Vector3D(0, 1, 0),
            new Vector3D(sin, 0, cos)
        };
    }

    public Vector3D[] GetRotZ(double theta) {
        double sin=Math.Sin(theta), cos=Math.Cos(theta);
        
        return new Vector3D[] {
            new Vector3D(cos, sin, 0),
            new Vector3D(-sin, cos, 0),
            new Vector3D(0, 0, 1)
        };
    }
}

Object matrix_lock = new Object();
Vector3D unit_vector = new Vector3D(1,1,1);
Vector3D[,] matrix=null;  
const double MaxDistance = 441.6729559300637; // dist from White to

// Based on Direct3D 9's D3DFOG_EXP function
double FogAmount(double amt) {
    return 1 / Math.Exp(amt * density);
}

// Sloppy conversion of color into distance for z axis
double getDistance(ColorBgra c) {
    return Math.Sqrt(c.R*c.R + c.G*c.G + c.B*c.B)/MaxDistance;
}

void PreRender(Surface dst, Surface src, Rectangle rect) {
    Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();
    
    // I'm not entirely sure if this is a multithreaded function, but just in case...
    if (matrix==null) {
        Monitor.Enter(matrix_lock);
        try {
            if (matrix==null) {
                matrix = new Vector3D[selection.Width, selection.Height];

                // Build 3D representation of image
                for (int y=0; y<selection.Height; y++) {
                    for (int x=0; x<selection.Width; x++) {
                        matrix[x,y] = new Vector3D((double)x, (double)y, getDistance(src[x,y])*selection.Height);
                    }
                }
            }
        } finally {
            Monitor.Exit(matrix_lock);
        }
    }

    // Rotate all parts of the matrix in the ROI
    Vector3D[] RotMatrix = unit_vector.GetRotZ(zrot);
    Vector3D me;
    
    for (int y=rect.Top; y<rect.Bottom; y++) {
        for (int x=rect.Left; x<rect.Right; x++) {
            me = matrix[x,y];
            matrix[x,y] = me.MultBy3x3(RotMatrix);        
        }
    }
}

void Render(Surface dst, Surface src, Rectangle rect)
{
    // Delete any of these lines you don't need
    Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();

    ColorBgra px;
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {
            Vector3D v = new Vector3D(0,0,0);
            Pair<int, int> bestCoords = new Pair<int, int>(-1,-1);
            int y_val=selection.Height;
            double min_height = (double) y / (double) y_val;

            for (int my=0; my<selection.Height; my++) {
                for (int mx=0; mx<selection.Width; mx++) {
                    v = matrix[mx,my];
                    
                    if ((int)v.x != x) {
                        continue;
                    }
                    
                    if (v.y < y_val) {
                        if (v.z < min_height) {
                            bestCoords = new Pair<int, int>(my, mx);
                            y_val = (int)v.y;
                        }
                    }
                }
            }

            if (bestCoords.First == -1) {
                dst[x,y] = ColorBgra.FromBgr(0,0,0);
            }

            else {
                v = matrix[bestCoords.First, bestCoords.Second];
                double dist = (v.y-(double)selection.Height) / (double)(selection.Height);
                double fog = FogAmount(dist);
                
                px = src[bestCoords.First+selection.Top, bestCoords.Second+selection.Left];
                px.R = (byte)((int) px.R*fog);
                px.G = (byte)((int) px.G*fog);
                px.B = (byte)((int) px.B*fog);

                dst[x,y] = px;
            }
        }
    }
}

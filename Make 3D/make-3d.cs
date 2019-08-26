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
DoubleSliderControl density = 0.5; // [0,1] Fog Density
AngleControl zrot = 0; // [0,90] Viewing Angle 
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

struct Coord {
    public int x, y;
    public Coord(int x, int y) {
        this.x = x; this.y = y;
    }
}

struct VectorLocation {
    public Vector3D v;
    public Coord c;

    public VectorLocation (Vector3D v, Coord c){
        this.v = v; this.c = c;
    }
}

Vector3D unit_vector = new Vector3D(1,1,1);
Vector3D[,] matrix=null;  
Dictionary<int, List<VectorLocation>> sortedMatrix;
const double MaxDistance = 441.6729559300637; // dist from White to

// Based on Direct3D 9's D3DFOG_EXP function
double FogAmount(double amt) {
    return 1 / Math.Exp(amt * density);
}

// Sloppy conversion of color into distance for z axis
double getDistance(ColorBgra c) {
    return Math.Sqrt(c.R*c.R + c.G*c.G + c.B*c.B)/MaxDistance;
}

void PreRender(Surface dst, Surface src) {
    Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();
    
    matrix = new Vector3D[selection.Width, selection.Height];

    // Build 3D representation of image
    for (int y=selection.Top; y<selection.Bottom; y++) {
        for (int x=selection.Left; x<selection.Right; x++) {
            Vector3D v = new Vector3D((double)x, getDistance(src[x,y])*selection.Height + selection.Top, (double)y);
            
            // Adjust to be between 0 and height/width
            int adjx = x - selection.Left;
            int adjy = y - selection.Top;

            matrix[adjx, adjy] = v;
        }
    }
            
    // Rotate all parts of the matrix
    double rads = ((double)zrot / 180) * Math.PI;
    Vector3D[] RotMatrix = unit_vector.GetRotX(rads);
    Vector3D me;
    sortedMatrix = new Dictionary<int, List<VectorLocation>>();
    
    for (int y=0; y<selection.Height; y++) {
        for (int x=0; x<selection.Width; x++) {
            me = matrix[x,y];
            
            Debug.WriteLine("Before: " + me.x + ", " + me.y+ ", " + me.z);

            if (zrot != 0)
                me = me.MultBy3x3(RotMatrix);    

            Debug.WriteLine("After: " + me.x + ", " + me.y+ ", " + me.z);
            
            VectorLocation mePair = new VectorLocation(me, new Coord(x+selection.Left, y+selection.Top));

            if (sortedMatrix.ContainsKey((int)me.x)) {
                sortedMatrix[(int) me.x].Add(mePair);
            }    
            else {
                List<VectorLocation> l = new List<VectorLocation>();
                l.Add(mePair);
                sortedMatrix.Add((int) me.x, l);
            }
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
            Coord bestCoords = new Coord(0,0);
            double bestZ=double.PositiveInfinity;
            Vector3D bestV= new Vector3D(0,0,0);

            if (sortedMatrix.ContainsKey(x)) {
                foreach (VectorLocation vl in sortedMatrix[x]) {
                    Vector3D v = vl.v;

                    if (v.z < bestZ && v.y < y) {
                        bestCoords=vl.c;
                        bestV = v;
                        bestZ = v.z;
                    }
                }

                if (bestZ == double.PositiveInfinity) {
                    dst[x,y] = ColorBgra.FromBgr(75,75,75);
                }
                else {
                    double dist = (bestV.z - selection.Top) / (double)(selection.Height);
                    double fog = FogAmount(dist);
                    
                    //Debug.WriteLine("Dst Amt: " + dist);
                    //Debug.WriteLine("Fog Amt: " + fog);

                    px = src[bestCoords.x, bestCoords.y];
                    px.R = (byte)((int) px.R*fog);
                    px.G = (byte)((int) px.G*fog);
                    px.B = (byte)((int) px.B*fog);

                    dst[x,y] = px;
                }
            }
            else {
                Debug.WriteLine("Matrix did not contain key " + x);
                dst[x,y] = ColorBgra.FromBgr(75,75,75);
            }
        }
    }
}
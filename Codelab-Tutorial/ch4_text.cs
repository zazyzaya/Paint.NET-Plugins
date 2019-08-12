#region UICode
string str = "Write your text here"; // [1, 32767] Text
FontFamily font = new FontFamily("Arial"); // Font
int fontsize = 12; // [10, 72] Size
byte smoothing = 1; // [1] Smoothing|None|Anti-alias|ClearType
ColorBgra color = ColorBgra.FromBgr(0,0,0); // Color
Pair<double, double> where = Pair.Create( 0.0, 0.0 ); // Location
#endregion

void Render(Surface dst, Surface src, Rectangle rect) {
    Rectangle selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();
    int CenterX = ((selection.Right - selection.Left) / 2) + selection.Left;
    int CenterY = ((selection.Bottom - selection.Top) / 2) + selection.Top;

    // Copy src to dst
    dst.CopySurface(src, rect.Location, rect);    

    // Determine where text will go
    int column = (int)Math.Round(((where.First + 1) / 2) * (selection.Right - selection.Left));
    int row = (int)Math.Round(((where.Second + 1) / 2) * (selection.Bottom - selection.Top));

    // Make a new brush of some color and a graphics object to draw on
    SolidBrush brush = new SolidBrush(color.ToColor());
    Graphics g = new RenderArgs(dst).Graphics; 

    switch(smoothing) {
        case 0: // None
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            break;
        
        case 1: // anti-alias
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            break;

        case 2: // ClearType
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            break;
    }

    g.Clip = new Region(rect);  // Make sure we are only in the rectangle currently being rendered
    
    // Safely set up font
    Font usrFont;
    try {
        usrFont = new Font(font.Name, fontsize);
    }
    // Catch errors if font creation fails
    catch {
        usrFont = new Font("Arial", fontsize);
    }

    // Write some string on dst
    g.DrawString(str, usrFont, brush, column, row);
}
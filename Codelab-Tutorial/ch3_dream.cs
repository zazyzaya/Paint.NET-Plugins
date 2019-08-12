// Name: Dream
// Submenu: codelab tutorial
// Author:
// Title: 
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
IntSliderControl rad = 1; // [1,20] Radius
#endregion

// Not sure why this is a field, but here we are
private UserBlendOp darkenOp = new UserBlendOps.DarkenBlendOp();

void Render(Surface dst, Surface src, Rectangle rect)
{
    // Call gausian blur effect
    GaussianBlurEffect blur = new GaussianBlurEffect();
    PropertyCollection bProps = blur.CreatePropertyCollection();
    PropertyBasedEffectConfigToken bParams = new PropertyBasedEffectConfigToken(bProps);
    
    // Set gausian blur radius
    bParams.SetPropertyValue(GaussianBlurEffect.PropertyNames.Radius, rad);
    
    // Tell it what to blur, and where to put the result, then render it
    blur.SetRenderInfo(bParams, new RenderArgs(dst), new RenderArgs(src));
    blur.Render(new Rectangle[1] {rect}, 0, 1); // Not sure what the arguments here are doing

    ColorBgra CurrentPixel;
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {
            CurrentPixel = src[x,y];
            
            // Combine with blurred destination pixel, using darken blend operation
            CurrentPixel = darkenOp.Apply(CurrentPixel, dst[x,y]);

            dst[x,y] = CurrentPixel;
        }
    }
}

using System;

namespace Microsoft.Msagl.Drawing
{


  /// <summary>
  /// Size structure
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes"), Serializable]
  public struct Size
  {
    double width;
    /// <summary>
    /// width
    /// </summary>
    public double Width
    {
      get { return width; }
      set { width = value; }
    }
    double height;
    /// <summary>
    /// Height
    /// </summary>
    public double Height
    {
      get { return height; }
      set { height = value; }
    }

    /// <summary>
    /// constructor
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public Size(double width, double height)
    {
      this.width = width;


      this.height = height;

    }
  }


}

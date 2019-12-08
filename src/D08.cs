namespace src08
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Drawing;
  using System.Drawing.Imaging;

  public class Layer
  {
    public Layer(IEnumerable<char> data) => Data = data.ToList();
    public readonly IList<char> Data;
    public static Layer operator +(Layer a, Layer b) =>
      new Layer(a.Data.Zip(b.Data).Select(x => x.First=='2' ? x.Second : x.First));
    public int NumberOf(char digit) => Data.Count(c => c==digit);
    public static IEnumerable<Layer> Read(string data, int layerSize) => 
      data.Chunks(layerSize).Select(c => new Layer(c));

    public Layer WithSpace(int charWidth, int spaceWidth) =>
      new Layer( Data.Chunks(charWidth)
                  .Select(chunk => chunk.Concat(Enumerable.Repeat('0',spaceWidth)))
                  .Aggregate((x,y) => x.Concat(y)));

    public void SaveAsImage(string fileName, int rowWidth, int charWidth)
    {
      var height = Data.Count() / rowWidth;
      var charPerRow = rowWidth / charWidth;
      var ws = WithSpace(charWidth,1);
      var charWidthWithSpace = charWidth+1;
      var width = charPerRow*charWidthWithSpace;
      using (var bitmap = new Bitmap(width,height,PixelFormat.Format32bppRgb))
      {
        var bitmapFill = from y in Enumerable.Range(0, height)
                from x in Enumerable.Range(0, width)
                let color = ws.Data[y*width+x] == '0' ? Color.White : Color.Black
                select (Action)(() => bitmap.SetPixel(x,y,color));
        bitmapFill.ToList().ForEach(a => a.Invoke());
        bitmap.Save(fileName);
      }
    }
  }
  public static class SomeExtensions
  {
    public static Layer Stack(this IEnumerable<Layer> layers) => layers.Aggregate((a,b) => a+b);
    public static IEnumerable<IEnumerable<T>> Chunks<T>(this IEnumerable<T> data, int size) =>
      Enumerable.Range(0, data.Count()/size).Select(i => data.Skip(i*size).Take(size));
  }
}

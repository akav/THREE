using SkiaSharp;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace THREE
{
    [Serializable]
    public struct RGBA
    {
        public byte B;
        public byte G;
        public byte R;
        public byte A;
    }
    [Serializable]
    public static class ImageExtension
    {
        public static byte[] ToByteArray(this float[] floatArray)
        {
            byte[] byteArray = new byte[floatArray.Length];
            for (int i = 0; i < floatArray.Length; i++)
                byteArray[i] = (byte)(floatArray[i] * 255.0f);

            return byteArray;
        }
        public static SKBitmap ToSKBitMap(this byte[] byteArray, int width, int height)
        {
            SKBitmap bitmap = new SKBitmap();

            // pin the managed array so that the GC doesn't move it
            var gcHandle = GCHandle.Alloc(byteArray, GCHandleType.Pinned);

            // install the pixels with the color type of the pixel data
            var info = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Unpremul);
            bitmap.InstallPixels(info, gcHandle.AddrOfPinnedObject(), info.RowBytes, (addr, ctx) => gcHandle.Free());

            return bitmap;
        }        
    }
}

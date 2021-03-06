﻿using WindowsAPI;
using System.Drawing;
using System.Drawing.Imaging;
using System;
using System.Threading.Tasks;

namespace Gotchya
{

    /// <summary>
    /// Performs an effect that makes the desktop appear that it is bleeding from a hole.
    /// </summary>
    public static class ScreenBleedHoleOptimised
    {
        public static void Run(int frames) {
            Bitmap screenshot = Desktop.Screenshot();
            Bitmap canvas = Desktop.Screenshot();
            Mask mask = new Mask(screenshot);

            unsafe {
                BitmapData screenshotData = screenshot.LockBits(new Rectangle(0, 0, screenshot.Width, screenshot.Height), ImageLockMode.ReadWrite, screenshot.PixelFormat);
                int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(screenshot.PixelFormat) / 8;
                int heightInPixels = screenshotData.Height;
                int widthInBytes = screenshotData.Width * bytesPerPixel;
                byte* ptrFirstPixelScreenshot = (byte*)screenshotData.Scan0;


                int lastBlue = 255;
                int lastGreen = 255;
                int lastRed = 255;
                int h2 = heightInPixels / 2;
                int w2 = widthInBytes / bytesPerPixel / 2;
                for (int time = 1; time < frames; time++) {
                    float scale = time * 10000.0f;
                    BitmapData canvasData = canvas.LockBits(new Rectangle(0, 0, canvas.Width, canvas.Height), ImageLockMode.ReadWrite, canvas.PixelFormat);
                    byte* ptrFirstPixelCanvas = (byte*)canvasData.Scan0;
                    Parallel.For(0, heightInPixels, (y) => {
                        int yy = y - h2;
                        byte* currentLineScreenshot = ptrFirstPixelScreenshot + (y * screenshotData.Stride);
                        byte* currentLineCanvas = ptrFirstPixelCanvas + (y * canvasData.Stride);
                        for (int xBits = 0; xBits < widthInBytes; xBits = xBits + bytesPerPixel) {
                            int xx = xBits / bytesPerPixel - w2;
                            float factor = exp(-(xx * xx / scale) - (yy * yy / scale));

                            currentLineCanvas[xBits] = (byte)(currentLineScreenshot[xBits] - (int)(lastBlue * factor));
                            currentLineCanvas[xBits + 1] = (byte)(currentLineScreenshot[xBits + 1] - (int)(lastGreen * factor));
                            currentLineCanvas[xBits + 2] = (byte)(currentLineScreenshot[xBits + 2] - (int)(lastRed * factor));

                            lastBlue = currentLineScreenshot[xBits];
                            lastGreen = currentLineScreenshot[xBits + 1];
                            lastRed = currentLineScreenshot[xBits + 2];
                        }
                    });
                    canvas.UnlockBits(canvasData);
                    mask.Picture.Image = canvas;
                    mask.Picture.Update();
                }
            }

            mask.Close();

            Mouse.Move(15, Desktop.GetWidth() - 15);
        }

        //Math.Exp() approximate
        private static float exp(float x) {
            x = 1.0f + x / 256.0f;
            x *= x; x *= x; x *= x; x *= x;
            x *= x; x *= x; x *= x; x *= x;
            return x;
        }

        private static int Flattern(int oldColor, int newColor, double factor) {
            return newColor - (int)(oldColor * factor);
        }
    }
}
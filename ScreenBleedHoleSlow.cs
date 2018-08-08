using WindowsAPI;
using System.Drawing;
using System.Drawing.Imaging;
using System;
using System.Threading.Tasks;

namespace Gotchya
{

    /// <summary>
    /// Performs an effect that makes the desktop appear that it is glitching from a hole. Unsafe factor array access.
    /// </summary>
    public static class ScreenBleedHoleSlow
    {
        public static void Run(int frames) {
            Bitmap screenshot = Desktop.Screenshot();
            Bitmap canvas = Desktop.Screenshot();
            Mask mask = new Mask(screenshot);
            //mask.Hide();

            unsafe {
                BitmapData screenshotData = screenshot.LockBits(new Rectangle(0, 0, screenshot.Width, screenshot.Height), ImageLockMode.ReadWrite, screenshot.PixelFormat);
                int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(screenshot.PixelFormat) / 8;
                int heightInPixels = screenshotData.Height;
                int widthInBytes = screenshotData.Width * bytesPerPixel;
                byte* ptrFirstPixelScreenshot = (byte*)screenshotData.Scan0;

                int widthInPixels = widthInBytes / bytesPerPixel;
                float[][] factorArray = new float[widthInPixels][];
                for (int i = 0; i < widthInPixels; i++) {
                    factorArray[i] = new float[heightInPixels];
                }
         
                int lastBlue = 255;
                int lastGreen = 255;
                int lastRed = 255;
                int h2 = heightInPixels / 2;
                int w2 = widthInPixels / 2;
                for (int time = 1; time < frames; time++) {
                    float scale = time * 10000.0f;
                    BitmapData canvasData = canvas.LockBits(new Rectangle(0, 0, canvas.Width, canvas.Height), ImageLockMode.ReadWrite, canvas.PixelFormat);
                    byte* ptrFirstPixelCanvas = (byte*)canvasData.Scan0;

                    Task t = Task.Run(() => {
                        Parallel.For(0, heightInPixels, (y) => {
                            int yy = y - h2;
                            for (int x = 0; x < widthInPixels; x++) {
                                int xx = x - w2;
                                factorArray[x][y] = (float)Math.Exp(-(xx * xx / scale) - (yy * yy / scale));
                            }
                        });
                    });

                    Parallel.For(0, heightInPixels, (y) => {
                        byte* currentLineScreenshot = ptrFirstPixelScreenshot + (y * screenshotData.Stride);
                        byte* currentLineCanvas = ptrFirstPixelCanvas + (y * canvasData.Stride);
                        for (int xBits = 0; xBits < widthInBytes; xBits = xBits + bytesPerPixel) {
                            int thisBlue = currentLineScreenshot[xBits];
                            int thisGreen = currentLineScreenshot[xBits + 1];
                            int thisRed = currentLineScreenshot[xBits + 2];

                            int x = xBits / bytesPerPixel;
                            float factor = factorArray[x][y];

                            currentLineCanvas[xBits] = (byte)Flattern(lastBlue, thisBlue, factor);
                            currentLineCanvas[xBits + 1] = (byte)Flattern(lastGreen, thisGreen, factor);
                            currentLineCanvas[xBits + 2] = (byte)Flattern(lastRed, thisRed, factor);

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

        private static int Flattern(int oldColor, int newColor, double factor) {
            return newColor - (int)(oldColor * factor);
        }
    }
}
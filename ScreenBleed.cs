using WindowsAPI;
using System.Drawing;
using System.Drawing.Imaging;

namespace Gotchya
{

    /// <summary>
    /// Performs an effect that makes the desktop appear that it is melting.
    /// </summary>
    public static class ScreenBleed
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


                int lastBlue = 255;
                int lastGreen = 255;
                int lastRed = 255;
                int thisBlue = 0;
                int thisGreen = 0;
                int thisRed = 0;
                for (int time = 0; time < frames; time++) {
                    BitmapData canvasData = canvas.LockBits(new Rectangle(0, 0, canvas.Width, canvas.Height), ImageLockMode.ReadWrite, canvas.PixelFormat);
                    byte* ptrFirstPixelCanvas = (byte*)canvasData.Scan0;
                    for (int y = 0; y < heightInPixels; y++) {
                        byte* currentLineScreenshot = ptrFirstPixelScreenshot + (y * screenshotData.Stride);
                        byte* currentLineCanvas = ptrFirstPixelCanvas + (y * canvasData.Stride);
                        for (int x = 0; x < widthInBytes; x = x + bytesPerPixel) {
                            thisBlue = currentLineScreenshot[x];
                            thisGreen = currentLineScreenshot[x + 1];
                            thisRed = currentLineScreenshot[x + 2];

                            int xx = x / bytesPerPixel;
                            float factor = time * time * 7f / 255.0f;

                            currentLineCanvas[x] = (byte)Flattern(lastBlue, thisBlue, factor);
                            currentLineCanvas[x + 1] = (byte)Flattern(lastGreen, thisGreen, factor);
                            currentLineCanvas[x + 2] = (byte)Flattern(lastRed, thisRed, factor);

                            lastBlue = currentLineScreenshot[x];
                            lastGreen = currentLineScreenshot[x + 1];
                            lastRed = currentLineScreenshot[x + 2];
                        }
                    }
                    canvas.UnlockBits(canvasData);
                    mask.Picture.Image = canvas;
                    mask.Picture.Update();
                }
            }

            mask.Close();

            Mouse.Move(15, Desktop.GetWidth() - 15);
        }

        private static int Flattern(int oldColor, int newColor, float factor) {
            return newColor - (int)(oldColor * factor);
        }
    }
}
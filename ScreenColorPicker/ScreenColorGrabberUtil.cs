/*
 * ScreenColorPicker
 * Copyright (C) 2017  Federico Zanco [federico.zanco (at) gmail.com]
 * 
 * This file is part of ScreenColorPicker.
 * 
 * ScreenColorPicker is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with ScreenColorPicker; if not, If not, see<http://www.gnu.org/licenses/> .
 */

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ScreenColorPicker
{
    /// <summary>
    /// ScreenColorGrabberUtil
    /// </summary>
    public static class ScreenColorGrabberUtil
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Point lpPoint);
        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        private static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
        // P/Invoke declarations
        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);
        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteDC(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteObject(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr ptr);

        private static Bitmap screenPixel = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        /// <summary>
        /// Gets the color under mouse pointer.
        /// </summary>
        /// <returns></returns>
        public static System.Windows.Media.Color GetColorUnderMousePointer(out System.Windows.Point mousePointerPosition)
        {
            var cursorPosition = new Point();

            GetCursorPos(ref cursorPosition);

            mousePointerPosition = new System.Windows.Point(cursorPosition.X, cursorPosition.Y);

            using (var gdest = Graphics.FromImage(screenPixel))
            {
                using (var gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    var hSrcDC = gsrc.GetHdc();
                    var hDC = gdest.GetHdc();

                    BitBlt(hDC, 0, 0, 1, 1, hSrcDC, cursorPosition.X, cursorPosition.Y, (int)CopyPixelOperation.SourceCopy);

                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }

            var c = screenPixel.GetPixel(0, 0);

            return System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        /// <summary>
        /// Gets the color under mouse pointer2.
        /// </summary>
        /// <param name="mousePointerPosition">The mouse pointer position.</param>
        /// <returns></returns>
        public static System.Windows.Media.Color GetColorUnderMousePointer2(out System.Windows.Point mousePointerPosition)
        {
            var cursorPosition = new Point();

            GetCursorPos(ref cursorPosition);

            mousePointerPosition = new System.Windows.Point(cursorPosition.X, cursorPosition.Y);

            var hDesk = GetDesktopWindow();
            var hSrce = GetWindowDC(hDesk);
            var hDest = CreateCompatibleDC(hSrce);
            var hBmp = CreateCompatibleBitmap(hSrce, 1, 1);
            var hOldBmp = SelectObject(hDest, hBmp);
            var b = BitBlt(hDest, 0, 0, 1, 1, hSrce, cursorPosition.X, cursorPosition.Y, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
            var bmp = Image.FromHbitmap(hBmp);

            SelectObject(hDest, hOldBmp);
            DeleteObject(hBmp);
            DeleteDC(hDest);
            ReleaseDC(hDesk, hSrce);

            var c = bmp.GetPixel(0, 0);

            bmp.Dispose();

            return System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
        }

        /// <summary>
        /// Gets the screen area.
        /// </summary>
        /// <param name="center">The center.</param>
        /// <param name="roiWidth">Width of the roi.</param>
        /// <param name="roiHeight">Height of the roi.</param>
        /// <returns></returns>
        public static Bitmap GetScreenArea(System.Windows.Point center, int roiWidth, int roiHeight)
        {
            var origin = new Point((int)center.X - roiWidth / 2, (int)center.Y - roiHeight / 2);

            var hDesk = GetDesktopWindow();
            var hSrce = GetWindowDC(hDesk);
            var hDest = CreateCompatibleDC(hSrce);
            var hBmp = CreateCompatibleBitmap(hSrce, roiWidth, roiHeight);
            var hOldBmp = SelectObject(hDest, hBmp);
            var b = BitBlt(hDest, 0, 0, roiWidth, roiHeight, hSrce, origin.X, origin.Y, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
            var bmp = Image.FromHbitmap(hBmp);

            SelectObject(hDest, hOldBmp);
            DeleteObject(hBmp);
            DeleteDC(hDest);
            ReleaseDC(hDesk, hSrce);

            return bmp;
        }

        /// <summary>
        /// Draws the center.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <returns></returns>
        public static Bitmap DrawCenter(Bitmap bitmap)
        {
            bitmap.SetPixel(bitmap.Size.Width / 2 - 1, bitmap.Size.Height / 2 - 1, Color.White);
            bitmap.SetPixel(bitmap.Size.Width / 2, bitmap.Size.Height / 2, Color.White);
            bitmap.SetPixel(bitmap.Size.Width / 2, bitmap.Size.Height / 2 - 1, Color.Black);
            bitmap.SetPixel(bitmap.Size.Width / 2 -1, bitmap.Size.Height / 2, Color.Black);

            return bitmap;
        }
    }
}

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
using System.ComponentModel;
using System.IO;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ScreenColorPicker
{
    /// <summary>
    /// ViewModel
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class ViewModel : INotifyPropertyChanged
    {
        #region Constants
        private const int RoiWidth = 8;
        private const int RoiHeight = 8;
        #endregion

        #region Private fields
        private DispatcherTimer timer = new DispatcherTimer();

        private Point origin = new Point(0.0, 0.0);
        private Point position = new Point(0.0, 0.0);
        private Color screenPixelColor = Colors.White;
        private BitmapImage imageSource = null;
        #endregion

        #region Public properties
        /// <summary>
        /// Gets or sets the color of the screen pixel.
        /// </summary>
        /// <value>
        /// The color of the screen pixel.
        /// </value>
        public Color ScreenPixelColor
        {
            get { return screenPixelColor; }
            set
            {
                screenPixelColor = value;

                NotifyPropertyChanged(() => ScreenPixelColor);
                NotifyPropertyChanged(() => Brush);
                NotifyPropertyChanged(() => X);
                NotifyPropertyChanged(() => Y);
                NotifyPropertyChanged(() => A);
                NotifyPropertyChanged(() => R);
                NotifyPropertyChanged(() => G);
                NotifyPropertyChanged(() => B);
                NotifyPropertyChanged(() => Hex);
            }
        }

        /// <summary>
        /// Gets the brush.
        /// </summary>
        /// <value>
        /// The brush.
        /// </value>
        public SolidColorBrush Brush { get { return new SolidColorBrush(screenPixelColor); } }

        /// <summary>
        /// Gets the x.
        /// </summary>
        /// <value>
        /// The x.
        /// </value>
        public int X { get { return (int)(position.X - origin.X); } }

        /// <summary>
        /// Gets the y.
        /// </summary>
        /// <value>
        /// The y.
        /// </value>
        public int Y { get { return (int)(position.Y - origin.Y); } }

        /// <summary>
        /// Gets a.
        /// </summary>
        /// <value>
        /// a.
        /// </value>
        public int A { get { return screenPixelColor.A; } }

        /// <summary>
        /// Gets the r.
        /// </summary>
        /// <value>
        /// The r.
        /// </value>
        public int R { get { return screenPixelColor.R; } }

        /// <summary>
        /// Gets the g.
        /// </summary>
        /// <value>
        /// The g.
        /// </value>
        public int G { get { return screenPixelColor.G; } }

        /// <summary>
        /// Gets the b.
        /// </summary>
        /// <value>
        /// The b.
        /// </value>
        public int B { get { return screenPixelColor.B; } }

        /// <summary>
        /// Gets the hexadecimal.
        /// </summary>
        /// <value>
        /// The hexadecimal.
        /// </value>
        public string Hex
        {
            get
            {
                return "#"
                    + screenPixelColor.A.ToString("X2")
                    + screenPixelColor.R.ToString("X2")
                    + screenPixelColor.G.ToString("X2")
                    + screenPixelColor.B.ToString("X2");
            }
        }

        /// <summary>
        /// Gets the start stop command label.
        /// </summary>
        /// <value>
        /// The start stop command label.
        /// </value>
        public string StartStopCommandLabel { get { return timer.IsEnabled ? "Stop" : "Start"; } }

        /// <summary>
        /// Gets or sets the image source.
        /// </summary>
        /// <value>
        /// The image source.
        /// </value>
        public BitmapImage ImageSource
        {
            get { return imageSource; }
            set
            {
                imageSource = value;

                NotifyPropertyChanged(() => ImageSource);
            }
        }
        #endregion

        #region Delegate commands
        /// <summary>
        /// Gets or sets the start stop command.
        /// </summary>
        /// <value>
        /// The start stop command.
        /// </value>
        public SimpleDelegateCommand StartStopCommand { get; set; }

        /// <summary>
        /// Gets or sets the copy command.
        /// </summary>
        /// <value>
        /// The copy command.
        /// </value>
        public SimpleDelegateCommand CopyCommand { get; set; }

        /// <summary>
        /// Gets or sets the change origin command.
        /// </summary>
        /// <value>
        /// The change origin command.
        /// </value>
        public SimpleDelegateCommand ChangeOriginCommand { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModel"/> class.
        /// </summary>
        public ViewModel()
        {
            StartStopCommand = new SimpleDelegateCommand(StartStopCommand_Execute);
            CopyCommand = new SimpleDelegateCommand(CopyCommand_Execute);
            ChangeOriginCommand = new SimpleDelegateCommand(ChangeOriginCommand_Execute);

            timer.Tick += MouseMoveTimer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Start();
        }
        #endregion

        #region MouseMoveTimer_Tick
        private void MouseMoveTimer_Tick(object sender, EventArgs e)
        {
            ScreenPixelColor = ScreenColorGrabberUtil.GetColorUnderMousePointer2(out position);

            ImageSource = BitmapToBitmapImage(
                ScreenColorGrabberUtil.DrawCenter(
                    ScreenColorGrabberUtil.GetScreenArea(position, 16, 16)));
        }
        #endregion

        #region BitmapToBitmapImage
        private static BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
        #endregion

        #region Commands
        #region StartStop
        private void StartStopCommand_Execute(object o)
        {
            if (timer.IsEnabled)
                timer.Stop();
            else
                timer.Start();

            NotifyPropertyChanged(() => StartStopCommandLabel);
        }
        #endregion

        #region CopyCommand_Execute
        private void CopyCommand_Execute(object parameter)
        {
            Clipboard.SetDataObject(
                new DataObject(
                    DataFormats.Text,
                    Hex,
                    true), true);
        }
        #endregion

        #region CopyCommand_Execute
        private void ChangeOriginCommand_Execute(object parameter)
        {
            origin = (string)parameter == "Set" ? position : new Point(0.0, 0.0);
        }
        #endregion
        #endregion

        #region INotifyPropertyChanged
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        private void NotifyPropertyChanged<T>(Expression<Func<T>> expr)
        {
            NotifyPropertyChanged((expr.Body as MemberExpression).Member.Name);
        }
        #endregion
    }
}

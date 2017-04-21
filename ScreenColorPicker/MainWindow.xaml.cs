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
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ScreenColorPicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new ViewModel();

            var b = (SolidColorBrush)Background;

            Background = new SolidColorBrush(Color.FromArgb(0, b.Color.R, b.Color.G, b.Color.B));

            RenderOptions.SetBitmapScalingMode(Zoom, BitmapScalingMode.NearestNeighbor); // reduce quality during resize to improve performance (or not :()
        }

        private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Raises the <see cref="E:Closed" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            var model = DataContext as ViewModel;
            if (model == null)
                return;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}

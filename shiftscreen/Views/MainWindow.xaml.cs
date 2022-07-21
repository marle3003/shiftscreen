using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Extensions.Options;
using ShiftScreen.Settings;
using ShiftScreen.Utils;
using Point = ShiftScreen.Settings.Point;

namespace ShiftScreen.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private DispatcherTimer? _timer;
        private WriteableBitmap? _bitmap;
        private readonly CaptureSettings _settings;
        private readonly Screen _screen;

        public MainWindow(IOptions<CaptureSettings> settings)
        {
            _settings = settings.Value;
            _screen = new Screen(settings.Value);

            InitializeComponent();

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ShiftScreen.share.png");
            var icon = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            Icon = icon;

            Update();
            _screen.Show();
        }

        private void Update()
        {
            _timer?.Stop();

            Width = _settings.Width;
            Height = _settings.Height;

            _bitmap = new WriteableBitmap(_settings.Width, _settings.Height, 96, 96, PixelFormats.Pbgra32, null);
            viewer.Source = _bitmap;

            _timer = new DispatcherTimer();
            _timer.Tick += CopyScreen!;
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / _settings.Fps);
            _timer.Start();

            _screen.Update();
        }

        private void CopyScreen(object sender, EventArgs e)
        {
            _bitmap.Lock();
                using var screenBmp = new Bitmap(
                    _bitmap.PixelWidth, _bitmap.PixelHeight, _bitmap.BackBufferStride,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb, _bitmap.BackBuffer);

                using var g = Graphics.FromImage(screenBmp);
                g.CopyFromScreen(_settings.Start.X, _settings.Start.Y, 0, 0,
                    new System.Drawing.Size()
                    {
                        Width = _settings.Width,
                        Height = _settings.Height
                    }, CopyPixelOperation.SourceCopy);
                drawCursor(g);

                _bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight));
                _bitmap.Unlock();
        }

        public void Dispose()
        {
            _timer?.Stop();
        }

        private void drawCursor(Graphics g)
        {
            User32.CURSORINFO cursorInfo;
            cursorInfo.cbSize = Marshal.SizeOf(typeof(User32.CURSORINFO));

            if (User32.GetCursorInfo(out cursorInfo))
            {
                if (cursorInfo.flags == User32.CURSOR_SHOWING && IsPointInScreen(cursorInfo.ptScreenPos.x, cursorInfo.ptScreenPos.y))
                {
                    var iconPointer = User32.CopyIcon(cursorInfo.hCursor);
                    User32.ICONINFO iconInfo;
                    int iconX, iconY;

                    if (User32.GetIconInfo(iconPointer, out iconInfo))
                    {
                        // calculate the correct position of the cursor
                        iconX = cursorInfo.ptScreenPos.x - iconInfo.xHotspot - _settings.Start.X;
                        iconY = cursorInfo.ptScreenPos.y - iconInfo.yHotspot - _settings.Start.Y;

                        // draw the cursor icon on top of the captured screen image
                        User32.DrawIcon(g.GetHdc(), iconX, iconY, cursorInfo.hCursor);

                        // release the handle created by call to g.GetHdc()
                        g.ReleaseHdc();
                    }
                }
            }
        }

        private void Viewer_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void SelectScreen_OnClick(object sender, RoutedEventArgs e)
        {
            _timer?.Stop();

            var dialog = new SelectScreen();
            if (dialog.ShowDialog() == true)
            {
                var screen = dialog.GetSceen();
                dialog.Close();
                _settings.Start = new Point { X = (int)screen.X, Y = (int)screen.Y };
                _settings.Width = (int)screen.Width;
                _settings.Height = (int)screen.Height;
                Update();
            }
        }

        private bool IsPointInScreen(int x, int y)
        {
            if (x < _settings.Start.X || x > _settings.Start.X + _settings.Width)
            {
                return false;
            }

            if (y < _settings.Start.Y || y > _settings.Start.Y + _settings.Height)
            {
                return false;
            }

            return true;
        }

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ControlStack.Orientation = Width < 350 ? Orientation.Vertical : Orientation.Horizontal;
            ((DoubleAnimation)storyShow.Children[0]).To = Width < 350 ? 150 : 66;
            ((DoubleAnimation)storyHide.Children[0]).From = Width < 350 ? 150 : 66;
        }

        private void Help_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/marle3003/shiftscreen",
                UseShellExecute = true
            });
        }
    }
}
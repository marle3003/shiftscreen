using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using ShiftScreen.Settings;
using ShiftScreen.Utils;
using Window = System.Windows.Window;

namespace ShiftScreen;

public partial class Screen : Window
{
    private readonly CaptureSettings _settings;

    public Screen(CaptureSettings settings)
    {
        _settings = settings;

        InitializeComponent();

        Left = 0;
        Top = 0;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;
    }

    public void Update()
    {
        double borderSpace = Math.Ceiling(_settings.Style.BorderWidth / 2.0);
        double space = Math.Floor(_settings.Style.BorderWidth % 2.0);
        var rect = new Rect(
            _settings.Start.X - borderSpace,
            _settings.Start.Y - borderSpace,
            _settings.Width + _settings.Style.BorderWidth + space,
            _settings.Height + _settings.Style.BorderWidth + space);
        Presentation.Rect = rect;

        Path.StrokeThickness = _settings.Style.BorderWidth > 0 ? _settings.Style.BorderWidth : 0;

        if (!string.IsNullOrWhiteSpace(_settings.Style.BorderColor))
        {
            Path.Stroke = (SolidColorBrush)new BrushConverter().ConvertFrom(_settings.Style.BorderColor)!;
        }
        else
        {
            Path.Stroke = null;
        }
    }

    // hide window from <alt><tab>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        WindowInteropHelper wndHelper = new WindowInteropHelper(this);

        int exStyle = (int)User32.GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);

        exStyle |= (int)ExtendedWindowStyles.ToolWindow;
        Window32.SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
    }
}
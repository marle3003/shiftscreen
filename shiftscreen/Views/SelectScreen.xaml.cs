﻿using System.Windows;
using System.Windows.Input;

namespace ShiftScreen.Views;

public partial class SelectScreen : Window
{
    private bool _selectionMode; // Set to 'true' when mouse is held down.
    private Point _firstPosition; // The point where the mouse button was clicked down.

    public SelectScreen()
    {
        InitializeComponent();

        Left = 0;
        Top = 0;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;
    }

    public Rect GetSceen()
    {
        return Selection.Rect;
    }

    private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // Capture and track the mouse.
        _selectionMode = true;
        _firstPosition = e.GetPosition(theGrid);
        theGrid.CaptureMouse();

        // Initial placement of the drag selection box.
        Selection.Rect = new Rect(_firstPosition.X, _firstPosition.Y, 1, 1);
    }

    private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
    {
        // Release the mouse capture and stop tracking it.
        _selectionMode = false;
        theGrid.ReleaseMouseCapture();

        DialogResult = true;
    }

    private void Grid_MouseMove(object sender, MouseEventArgs e)
    {
        if (_selectionMode)
        {
            // When the mouse is held down, reposition the drag selection box.
            var mousePos = e.GetPosition(theGrid);
            var rect = new Rect();

            if (_firstPosition.X < mousePos.X)
            {
                rect.X = _firstPosition.X;
                rect.Width = mousePos.X - _firstPosition.X;
            }
            else
            {
                rect.X = mousePos.X;
                rect.Width = _firstPosition.X - mousePos.X;
            }

            if (_firstPosition.Y < mousePos.Y)
            {
                rect.Y = _firstPosition.Y;
                rect.Height = mousePos.Y - _firstPosition.Y;
            }
            else
            {
                rect.Y = mousePos.Y;
                rect.Height = _firstPosition.Y - mousePos.Y;
            }

            Selection.Rect = rect;
        }
    }
}
using ImageEditor.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace ImageEditor.Views
{
    public partial class KeyboardShortcutsWindow : Window
    {
        public KeyboardShortcutsWindow()
        {
            InitializeComponent();

            ToolsGrid.ItemsSource = new List<ShortcutItem>
            {
                new ShortcutItem("B",  "Brush tool"),
                new ShortcutItem("S",  "Selection tool"),
                new ShortcutItem("L",  "Line tool"),
                new ShortcutItem("T",  "Text tool"),
                new ShortcutItem("E",  "Eyedropper tool"),
                new ShortcutItem("C",  "Toggle color picker"),
            };

            FileGrid.ItemsSource = new List<ShortcutItem>
            {
                new ShortcutItem("Ctrl + N", "New image"),
                new ShortcutItem("Ctrl + O", "Open image"),
                new ShortcutItem("Ctrl + S", "Save"),
                new ShortcutItem("Ctrl + I", "Image info"),
            };

            EditGrid.ItemsSource = new List<ShortcutItem>
            {
                new ShortcutItem("Ctrl + Z",         "Undo"),
                new ShortcutItem("Ctrl + Shift + Z", "Redo"),
                new ShortcutItem("Ctrl + C",         "Copy selection"),
                new ShortcutItem("Ctrl + X",         "Cut selection"),
                new ShortcutItem("Ctrl + V",         "Paste"),
                new ShortcutItem("Ctrl + A",         "Select all"),
                new ShortcutItem("Escape",           "Clear selection"),
            };

            CanvasGrid.ItemsSource = new List<ShortcutItem>
            {
                new ShortcutItem("Scroll",              "Zoom in / out"),
                new ShortcutItem("Middle click + drag", "Pan canvas"),
                new ShortcutItem("Shift + resize",      "Keep aspect ratio"),
                new ShortcutItem("Ctrl + Enter",        "Commit text"),
            };
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
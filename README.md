# MonoFrame - WPF Image Editor

MonoFrame is an open-source, lightweight image editor built with WPF and C#. Designed with a clean dark UI and an MVVM architecture, it gives you the essential tools to edit images without the bloat of heavy software.

---

## 📝 License

This project is licensed under the **MIT License**.
You are free to use, modify, and distribute the code for any purpose.

---

## 📦 Features

* **Multi-tab Editing** - Work on multiple images simultaneously with a clean tab interface.
* **Memory/CPU-Efficient Processing** - All editing operations (brush, line, text, filters) use partial 
  dirty-region snapshots instead of full image copies.
* **Brush Tool** - Freehand drawing with adjustable size and hardness. Supports smooth stroke interpolation.
* **Selection Tool** - Rectangular selection with copy, cut, paste, and floating paste with resize handles.
* **Line Tool** - Draw straight lines and cubic Bézier curves with configurable width.
* **Text Tool** - Place text anywhere on the canvas with full font, size, bold, italic, and alignment control.
* **Eyedropper Tool** - Pick any color from the canvas with a live hex preview.
* **Color Picker** - Floating color picker window for choosing your active drawing color.
* **Image Filters** - Gaussian blur, sharpen, brightness, grayscale, Sobel edge detection, invert, pixelate, gamma correction.
* **Rotate & Flip** - Rotate 90°/180° and flip horizontally or vertically.
* **Undo / Redo** - Full undo/redo history with memory-efficient partial snapshots (dirty region only).
* **Zoom & Pan** - Scroll to zoom, middle-click to pan, and a zoom slider in the status bar.
* **Status Bar** - Live display of mouse coordinates, selection size, image dimensions, and zoom level.
* **New / Open / Save / Save As** - PNG and JPEG support with unsaved changes dialog.

---

## 🖼️ Screenshots

<img width="1920" height="1080" alt="{E3065FDA-735C-4EAF-A550-1A560E029615}" src="https://github.com/user-attachments/assets/a27a5de4-c22d-457b-b369-f3f3477ba50a" />

<img width="1576" height="885" alt="{2B7BB541-6ED6-493C-A845-BEEA1D04DA54}" src="https://github.com/user-attachments/assets/a582d3d0-5fe5-4813-9b76-88298d6ab0b3" />

<img width="1920" height="1080" alt="{1C7619AD-EA1F-45A6-8767-53F3B771FC4D}" src="https://github.com/user-attachments/assets/334f6420-88fc-4eb5-9439-dde1180ed466" />

---

## ⌨️ Keyboard Shortcuts

| Shortcut | Action |
|---|---|
| `Ctrl+N` | New image |
| `Ctrl+O` | Open image |
| `Ctrl+S` | Save |
| `Ctrl+Z` | Undo |
| `Ctrl+Shift+Z` | Redo |
| `Ctrl+C` | Copy selection |
| `Ctrl+X` | Cut selection |
| `Ctrl+V` | Paste |
| `Ctrl+A` | Select all |
| `B` | Brush tool |
| `S` | Selection tool |
| `L` | Line tool |
| `T` | Text tool |
| `E` | Eyedropper tool |
| `C` | Toggle color picker |
| `Escape` | Clear selection |

---

## 🛠️ Tech Stack

* **Framework:** WPF (.NET 8.0+)
* **Language:** C#
* **Architecture:** MVVM
* **Image Processing:** WriteableBitmap, RenderTargetBitmap (WPF native)

---

## Installation

### Prerequisites

* [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
* Windows 10 or later
* Visual Studio 2022 or newer

### Run locally

1. **Clone the repository:**

```bash
git clone https://github.com/XBambinoX/ImageEditor.git
cd ImageEditor
```

2. **Run:** 
```bash
dotnet run
```
---

## 🏗️ Project Structure

```
ImageEditor/
├── Commands/             # RelayCommand implementation
├── Models/               # ImageTab, ImageSnapshot
├── Services/             # Drawing, selection, filters, logging
│   └── ImageProcessing/  # Blur, sharpen, brightness, etc.
├── ViewModels/           # MainViewModel and filter ViewModels
└── Views/                # MainWindow, tool windows, dialogs
```

---

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

---

## About

MonoFrame was built as a learning project to explore WPF, MVVM, and low-level pixel manipulation in C#. It started as a simple image viewer and grew into a fully featured editor.

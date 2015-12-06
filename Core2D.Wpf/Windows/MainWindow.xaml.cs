﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace Core2D.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for <see cref="MainWindow"/> xaml.
    /// </summary>
    public partial class MainWindow : Window, IView
    {
        private string _resourceLayoutRoot = "Core2D.Wpf.Layouts.";
        private string _resourceLayoutFileName = "Core2D.Wpf.layout";
        private string _defaultLayoutFileName = "Core2D.layout";

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes the zoom border control.
        /// </summary>
        /// <param name="context">The editor context instance.</param>
        public void InitializeZoom(EditorContext context)
        {
            border.InvalidateChild =
                (z, x, y) =>
                {
                    bool invalidate = context.Editor.Renderers[0].State.Zoom != z;
                    context.Editor.Renderers[0].State.Zoom = z;
                    context.Editor.Renderers[0].State.PanX = x;
                    context.Editor.Renderers[0].State.PanY = y;
                    if (invalidate)
                    {
                        context.InvalidateCache(isZooming: true);
                    }
                };

            border.AutoFitChild =
                (width, height) =>
                {
                    if (border != null
                        && context != null
                        && context.Editor.Project != null
                        && context.Editor.Project.CurrentContainer != null)
                    {
                        var container = context.Editor.Project.CurrentContainer;
                        if (container.Template == null)
                        {
                            border.FitTo(
                                width,
                                height,
                                container.Width,
                                container.Height);
                        }
                        else
                        {
                            border.FitTo(
                                width,
                                height,
                                container.Template.Width,
                                container.Template.Height);
                        }
                    }
                };

            border.MouseDown +=
                (s, e) =>
                {
                    if (e.ChangedButton == MouseButton.Middle && e.ClickCount == 2)
                    {
                        panAndZoomGrid.AutoFit();
                    }

                    if (e.ChangedButton == MouseButton.Middle && e.ClickCount == 3)
                    {
                        panAndZoomGrid.ResetZoomAndPan();
                    }
                };
        }

        /// <summary>
        /// Initializes canvas control drag and drop handler.
        /// </summary>
        /// <param name="context">The editor context instance.</param>
        public void InitializeDrop(EditorContext context)
        {
            drawableControl.AllowDrop = true;

            drawableControl.DragEnter +=
                (s, e) =>
                {
                    if (!e.Data.GetDataPresent(DataFormats.FileDrop)
                        && !e.Data.GetDataPresent(typeof(XGroup))
                        && !e.Data.GetDataPresent(typeof(Record))
                        && !e.Data.GetDataPresent(typeof(ShapeStyle)))
                    {
                        e.Effects = DragDropEffects.None;
                        e.Handled = true;
                    }
                };

            drawableControl.Drop +=
                (s, e) =>
                {
                    if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    {
                        try
                        {
                            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                            if (context.Drop(files))
                            {
                                e.Handled = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (context.Editor.Log != null)
                            {
                                context.Editor.Log.LogError("{0}{1}{2}",
                                    ex.Message,
                                    Environment.NewLine,
                                    ex.StackTrace);
                            }
                        }
                    }

                    if (e.Data.GetDataPresent(typeof(XGroup)))
                    {
                        try
                        {
                            var group = e.Data.GetData(typeof(XGroup)) as XGroup;
                            if (group != null)
                            {
                                var p = e.GetPosition(drawableControl);
                                context.DropAsClone(group, p.X, p.Y);
                                e.Handled = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (context.Editor.Log != null)
                            {
                                context.Editor.Log.LogError("{0}{1}{2}",
                                    ex.Message,
                                    Environment.NewLine,
                                    ex.StackTrace);
                            }
                        }
                    }

                    if (e.Data.GetDataPresent(typeof(Record)))
                    {
                        try
                        {
                            var record = e.Data.GetData(typeof(Record)) as Record;
                            if (record != null)
                            {
                                var p = e.GetPosition(drawableControl);
                                context.Drop(record, p.X, p.Y);
                                e.Handled = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (context.Editor.Log != null)
                            {
                                context.Editor.Log.LogError("{0}{1}{2}",
                                    ex.Message,
                                    Environment.NewLine,
                                    ex.StackTrace);
                            }
                        }
                    }

                    if (e.Data.GetDataPresent(typeof(ShapeStyle)))
                    {
                        try
                        {
                            var style = e.Data.GetData(typeof(ShapeStyle)) as ShapeStyle;
                            if (style != null)
                            {
                                var p = e.GetPosition(drawableControl);
                                context.Drop(style, p.X, p.Y);
                                e.Handled = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (context.Editor.Log != null)
                            {
                                context.Editor.Log.LogError("{0}{1}{2}",
                                    ex.Message,
                                    Environment.NewLine,
                                    ex.StackTrace);
                            }
                        }
                    }
                };
        }

        /// <summary>
        /// Load docking manager layout from resource.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="context"></param>
        private void LoadLayoutFromResource(string path, object context)
        {
            var serializer = new XmlLayoutSerializer(dockingManager);

            serializer.LayoutSerializationCallback +=
                (s, e) =>
                {
                    var element = e.Content as FrameworkElement;
                    if (element != null)
                    {
                        element.DataContext = context;
                    }
                };

            var assembly = this.GetType().Assembly;
            using (var stream = assembly.GetManifestResourceStream(path))
            {
                using (var reader = new System.IO.StreamReader(stream))
                {
                    serializer.Deserialize(reader);
                }
            }
        }

        /// <summary>
        /// Load docking manager layout.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="context"></param>
        private void LoadLayout(string path, object context)
        {
            if (!System.IO.File.Exists(path))
                return;

            var serializer = new XmlLayoutSerializer(dockingManager);

            serializer.LayoutSerializationCallback +=
                (s, e) =>
                {
                    var element = e.Content as FrameworkElement;
                    if (element != null)
                    {
                        element.DataContext = context;
                    }
                };

            using (var reader = new System.IO.StreamReader(path))
            {
                serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Save docking manager layout.
        /// </summary>
        /// <param name="path"></param>
        private void SaveLayout(string path)
        {
            var serializer = new XmlLayoutSerializer(dockingManager);
            using (var writer = new System.IO.StreamWriter(path))
            {
                serializer.Serialize(writer);
            }
        }

        /// <summary>
        /// Auto load docking manager layout.
        /// </summary>
        /// <param name="context"></param>
        public void AutoLoadLayout(EditorContext context)
        {
            try
            {
                LoadLayout(_defaultLayoutFileName, context);
            }
            catch (Exception ex)
            {
                if (context.Editor.Log != null)
                {
                    context.Editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// Auto save docking manager layout.
        /// </summary>
        /// <param name="context"></param>
        public void AutoSaveLayout(EditorContext context)
        {
            try
            {
                SaveLayout(_defaultLayoutFileName);
            }
            catch (Exception ex)
            {
                if (context.Editor.Log != null)
                {
                    context.Editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// Load docking manager layout.
        /// </summary>
        public void OnLoadLayout()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            var dlg = new OpenFileDialog()
            {
                Filter = "Layout (*.layout)|*.layout|All (*.*)|*.*",
                FilterIndex = 0,
                FileName = ""
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    LoadLayout(dlg.FileName, context);
                }
                catch (Exception ex)
                {
                    if (context.Editor.Log != null)
                    {
                        context.Editor.Log.LogError("{0}{1}{2}",
                            ex.Message,
                            Environment.NewLine,
                            ex.StackTrace);
                    }
                }
            }
        }

        /// <summary>
        /// Save docking manager layout.
        /// </summary>
        public void OnSaveLayout()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            var dlg = new SaveFileDialog()
            {
                Filter = "Layout (*.layout)|*.layout|All (*.*)|*.*",
                FilterIndex = 0,
                FileName = _defaultLayoutFileName
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    SaveLayout(dlg.FileName);
                }
                catch (Exception ex)
                {
                    if (context.Editor.Log != null)
                    {
                        context.Editor.Log.LogError("{0}{1}{2}",
                            ex.Message,
                            Environment.NewLine,
                            ex.StackTrace);
                    }
                }
            }
        }

        /// <summary>
        /// Reset docking manager layout.
        /// </summary>
        public void OnResetLayout()
        {
            var context = DataContext as EditorContext;
            if (context == null)
                return;

            try
            {
                LoadLayoutFromResource(_resourceLayoutRoot + _resourceLayoutFileName, context);
            }
            catch (Exception ex)
            {
                if (context.Editor.Log != null)
                {
                    context.Editor.Log.LogError("{0}{1}{2}",
                        ex.Message,
                        Environment.NewLine,
                        ex.StackTrace);
                }
            }
        }

        /// <summary>
        /// Reset pan and zoom to default state.
        /// </summary>
        public void OnZoomReset()
        {
            panAndZoomGrid.ResetZoomAndPan();
        }

        /// <summary>
        /// Stretch view to the available extents.
        /// </summary>
        public void OnZoomExtent()
        {
            panAndZoomGrid.AutoFit();
        }
    }
}
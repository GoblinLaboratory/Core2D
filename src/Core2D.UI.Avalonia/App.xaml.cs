﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Core2D.UI.Avalonia.Converters;
using Core2D.UI.Avalonia.Modules;
using Core2D.UI.Avalonia.Views;
using Core2D.Editor;
using Core2D.Editor.Designer;
using Core2D.Interfaces;
using Dock.Model;

namespace Core2D.UI.Avalonia
{
    /// <summary>
    /// Encapsulates an Avalonia application.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes static data.
        /// </summary>
        static App()
        {
            InitializeDesigner();
        }

        /// <summary>
        /// Initializes designer.
        /// </summary>
        public static void InitializeDesigner()
        {
            if (Design.IsDesignMode)
            {
                var builder = new ContainerBuilder();

                builder.RegisterModule<LocatorModule>();
                builder.RegisterModule<CoreModule>();
                builder.RegisterModule<DesignerModule>();
                builder.RegisterModule<AppModule>();
                builder.RegisterModule<ViewModule>();

                var container = builder.Build();

                DesignerContext.InitializeContext(container.Resolve<IServiceProvider>());
            }
        }

        /// <summary>
        /// Initializes converters.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public static void InitializeConverters(IServiceProvider serviceProvider)
        {
            ObjectToXamlStringConverter.XamlSerializer = serviceProvider.GetServiceLazily<IXamlSerializer>();
            ObjectToJsonStringConverter.JsonSerializer = serviceProvider.GetServiceLazily<IJsonSerializer>();
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Initialize application about information.
        /// </summary>
        /// <param name="runtimeInfo">The runtime info.</param>
        /// <param name="windowingSubsystem">The windowing subsystem.</param>
        /// <param name="renderingSubsystem">The rendering subsystem.</param>
        /// <returns>The about information.</returns>
        public AboutInfo CreateAboutInfo(RuntimePlatformInfo runtimeInfo, string windowingSubsystem, string renderingSubsystem)
        {
            return new AboutInfo()
            {
                Title = "Core2D",
                Version = $"{GetType().GetTypeInfo().Assembly.GetName().Version}",
                Description = "A multi-platform data driven 2D diagram editor.",
                Copyright = "Copyright (c) Wiesław Šoltés. All rights reserved.",
                License = "Licensed under the MIT license. See LICENSE file in the project root for full license information.",
                OperatingSystem = $"{runtimeInfo.OperatingSystem}",
                IsDesktop = runtimeInfo.IsDesktop,
                IsMobile = runtimeInfo.IsMobile,
                IsCoreClr = runtimeInfo.IsCoreClr,
                IsMono = runtimeInfo.IsMono,
                IsDotNetFramework = runtimeInfo.IsDotNetFramework,
                IsUnix = runtimeInfo.IsUnix,
                WindowingSubsystemName = windowingSubsystem,
                RenderingSubsystemName = renderingSubsystem
            };
        }

        /// <summary>
        /// Initialize application context and displays main window.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="aboutInfo">The about information.</param>
        public void Start(IServiceProvider serviceProvider, AboutInfo aboutInfo)
        {
            InitializeConverters(serviceProvider);

            var log = serviceProvider.GetService<ILog>();
            var fileIO = serviceProvider.GetService<IFileSystem>();

            log?.Initialize(System.IO.Path.Combine(fileIO?.GetBaseDirectory(), "Core2D.log"));

            try
            {
                var editor = serviceProvider.GetService<ProjectEditor>();

                var layoutPath = System.IO.Path.Combine(fileIO.GetBaseDirectory(), "Core2D.layout");
                if (fileIO.Exists(layoutPath))
                {
                    editor.OnLoadLayout(layoutPath);
                }

                var dockFactory = serviceProvider.GetService<IDockFactory>();
                editor.Layout = editor.Layout ?? dockFactory.CreateLayout();
                dockFactory.InitLayout(editor.Layout);

                var recentPath = System.IO.Path.Combine(fileIO.GetBaseDirectory(), "Core2D.recent");
                if (fileIO.Exists(recentPath))
                {
                    editor.OnLoadRecent(recentPath);
                }

                editor.CurrentTool = editor.Tools.FirstOrDefault(t => t.Title == "Selection");
                editor.CurrentPathTool = editor.PathTools.FirstOrDefault(t => t.Title == "Line");
                editor.IsToolIdle = true;
                editor.AboutInfo = aboutInfo;

                var window = serviceProvider.GetService<Windows.MainWindow>();
                window.Closing += (sender, e) =>
                {
                    editor.Layout.Close();
                    editor.OnSaveLayout(layoutPath);
                    editor.OnSaveRecent(recentPath);
                };
                window.DataContext = editor;
                window.Show();
                Run(window);
            }
            catch (Exception ex)
            {
                log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    log?.LogError($"{ex.InnerException.Message}{Environment.NewLine}{ex.InnerException.StackTrace}");
                }
            }
        }

        /// <summary>
        /// Initialize application context and returns main view.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>The main view.</returns>
        public UserControl CreateView(IServiceProvider serviceProvider)
        {
            InitializeConverters(serviceProvider);

            var log = serviceProvider.GetService<ILog>();
            var fileIO = serviceProvider.GetService<IFileSystem>();

            log?.Initialize(System.IO.Path.Combine(fileIO?.GetBaseDirectory(), "Core2D.log"));

            var editor = serviceProvider.GetService<ProjectEditor>();

            var dockFactory = serviceProvider.GetService<IDockFactory>();
            editor.Layout = editor.Layout ?? dockFactory.CreateLayout();
            dockFactory.InitLayout(editor.Layout);

            editor.CurrentTool = editor.Tools.FirstOrDefault(t => t.Title == "Selection");
            editor.CurrentPathTool = editor.PathTools.FirstOrDefault(t => t.Title == "Line");
            editor.IsToolIdle = true;

            return new MainControl()
            {
                DataContext = editor
            };
        }
    }
}

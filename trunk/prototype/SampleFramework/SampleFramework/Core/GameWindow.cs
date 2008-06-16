﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Resources;

namespace SampleFramework
{
    /// <summary>
    /// Implements a specialized window for games and rendering.
    /// </summary>
    public class GameWindow : Form
    {
        // constants
        const int DefaultWidth = 800;
        const int DefaultHeight = 600;
        const string DefaultTitle = "Game";
        const string DefaultIconName = "Game.ico";

        // variables
        bool minimized;
        bool maximized;
        bool inSizeMove;

        /// <summary>
        /// Occurs when the application is suspended.
        /// </summary>
        public event EventHandler Suspend;

        /// <summary>
        /// Occurs when the application is resumed.
        /// </summary>
        public event EventHandler Resume;

        /// <summary>
        /// Occurs when the user resizes the window.
        /// </summary>
        public event EventHandler UserResized;

        /// <summary>
        /// Occurs when the screen on which the window resides is changed.
        /// </summary>
        public event EventHandler ScreenChanged;

        /// <summary>
        /// Occurs when the application is activated.
        /// </summary>
        public event EventHandler ApplicationActivated;

        /// <summary>
        /// Occurs when the application is deactivated.
        /// </summary>
        public event EventHandler ApplicationDeactivated;

        /// <summary>
        /// Occurs when the system is suspended.
        /// </summary>
        public event EventHandler SystemSuspend;

        /// <summary>
        /// Occurs when the system is resumed.
        /// </summary>
        public event EventHandler SystemResume;

        /// <summary>
        /// Occurs when a screen saver is about to be activated.
        /// </summary>
        public event CancelEventHandler ScreenSaver;

        /// <summary>
        /// Gets a value indicating whether the window is in the middle of sizing operations.
        /// </summary>
        /// <value><c>true</c> if the window is in the middle of sizing operations; otherwise, <c>false</c>.</value>
        internal bool InSizeMove
        {
            get { return inSizeMove; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is minimized.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is minimized; otherwise, <c>false</c>.
        /// </value>
        public bool IsMinimized
        {
            get { return minimized; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is maximized.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is maximized; otherwise, <c>false</c>.
        /// </value>
        public bool IsMaximized
        {
            get { return maximized; }
        }

        /// <summary>
        /// Gets the screen on which the window resides.
        /// </summary>
        /// <value>The screen.</value>
        public Screen Screen
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameWindow"/> class.
        /// </summary>
        public GameWindow()
        {
            // set the minimum size
            MinimumSize = new Size(200, 200);

            // initialize the screen
            Screen = ScreenFromHandle(Handle);

            // set a default title and icon
            Icon = GetDefaultIcon();
            Text = GetDefaultTitle();
        }

        /// <summary>
        /// Raises the <see cref="E:Suspend"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnSuspend(EventArgs e)
        {
            // raise the event
            if (Suspend != null)
                Suspend(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:Resume"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnResume(EventArgs e)
        {
            // raise the event
            if (Resume != null)
                Resume(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:UserResized"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnUserResized(EventArgs e)
        {
            // raise the event
            if (UserResized != null)
                UserResized(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:ScreenChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnScreenChanged(EventArgs e)
        {
            // raise the event
            if (ScreenChanged != null)
                ScreenChanged(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:ApplicationActivated"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnApplicationActivated(EventArgs e)
        {
            // raise the event
            if (ApplicationActivated != null)
                ApplicationActivated(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:ApplicationDeactivated"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnApplicationDeactivated(EventArgs e)
        {
            // raise the event
            if (ApplicationDeactivated != null)
                ApplicationDeactivated(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:SystemSuspend"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnSystemSuspend(EventArgs e)
        {
            // raise the event
            if (SystemSuspend != null)
                SystemSuspend(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:SystemResume"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnSystemResume(EventArgs e)
        {
            // raise the event
            if (SystemResume != null)
                SystemResume(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:ScreenSaver"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        protected virtual void OnScreenSaver(CancelEventArgs e)
        {
            // raise the event
            if (ScreenSaver != null)
                ScreenSaver(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.ResizeBegin"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnResizeBegin(EventArgs e)
        {
            // call the base method
            base.OnResizeBegin(e);

            // suspend any processing until we are done being minimized
            inSizeMove = true;
            OnSuspend(EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.ResizeEnd"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnResizeEnd(EventArgs e)
        {
            // call the base method
            base.OnResizeEnd(e);

            // check for screen and size changes
            OnUserResized(EventArgs.Empty);
            UpdateScreen();
            inSizeMove = false;

            // resume application processing
            OnResume(EventArgs.Empty);
        }

        /// <summary>
        /// Handles raw window messages.
        /// </summary>
        /// <param name="m">The raw message.</param>
        protected override void WndProc(ref Message m)
        {
            // check if we should handle the message
            if (m.Msg == WindowConstants.WM_SIZE)
            {
                // check the type of sizing that is occurring
                if (m.WParam == WindowConstants.SIZE_MINIMIZED)
                {
                    // we are now minimized
                    minimized = true;
                    maximized = false;

                    // suspend any processing until we are done being minimized
                    OnSuspend(EventArgs.Empty);
                }
                else
                {
                    // get the raw client bounds
                    Rectangle client = NativeMethods.GetClientRectangle(m.HWnd);
                    if (client.Height == 0)
                    {
                        // rapidly clicking the task bar to minimize and restore a window
                        // can cause a WM_SIZE message with SIZE_RESTORED when 
                        // the window has actually become minimized due to rapid change
                        // so just ignore this message
                    }
                    else if (m.WParam == WindowConstants.SIZE_MAXIMIZED)
                    {
                        // if we were previously minimized, resume the application
                        if (minimized)
                            OnResume(EventArgs.Empty);

                        // we are now maximized
                        minimized = false;
                        maximized = true;

                        // check for screen and size changes
                        OnUserResized(EventArgs.Empty);
                        UpdateScreen();
                    }
                    else if (m.WParam == WindowConstants.SIZE_RESTORED)
                    {
                        // if we were previously minimized, resume the application
                        if (minimized)
                            OnResume(EventArgs.Empty);

                        // we are now restored
                        minimized = false;
                        maximized = false;

                        // check for screen and size changes
                        if (!inSizeMove)
                        {
                            OnUserResized(EventArgs.Empty);
                            UpdateScreen();
                        }
                    }
                }
            }
            else if (m.Msg == WindowConstants.WM_ACTIVATEAPP)
            {
                // check if we are active or inactive
                if (m.WParam != IntPtr.Zero)
                    OnApplicationActivated(EventArgs.Empty);
                else
                    OnApplicationDeactivated(EventArgs.Empty);
            }
            else if (m.Msg == WindowConstants.WM_POWERBROADCAST)
            {
                // check the type of power message we are receiving
                if (m.WParam == WindowConstants.PBT_APMQUERYSUSPEND)
                {
                    // allow the application to close up shop
                    OnSystemSuspend(EventArgs.Empty);
                    m.Result = (IntPtr)1;
                    return;
                }
                else if (m.WParam == WindowConstants.PBT_APMRESUMESUSPEND)
                {
                    // allow the application to resume working
                    OnSystemResume(EventArgs.Empty);
                    m.Result = (IntPtr)1;
                    return;
                }
            }
            else if (m.Msg == WindowConstants.WM_SYSCOMMAND)
            {
                // process the parameter
                long wparam = m.WParam.ToInt64() & 0xFFF0;
                if (wparam == WindowConstants.SC_MONITORPOWER || wparam == WindowConstants.SC_SCREENSAVE )
                {
                    // allow the command to be canceled
                    CancelEventArgs e = new CancelEventArgs();
                    OnScreenSaver(e);
                    if (e.Cancel)
                    {
                        // cancel the command
                        m.Result = IntPtr.Zero;
                        return;
                    }
                }
            }

            // call the base method
            base.WndProc(ref m);
        }

        /// <summary>
        /// Updates the screen.
        /// </summary>
        void UpdateScreen()
        {
            // grab the current screen
            Screen current = Screen.FromHandle(Handle);
            if (Screen == null || Screen != current)
            {
                // update the value
                Screen = current;
                if (Screen != null)
                    OnScreenChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets the screen's device name.
        /// </summary>
        /// <param name="screen">The screen.</param>
        /// <returns>The screen's device name.</returns>
        static string DeviceNameFromScreen(Screen screen)
        {
            // remove any garbage characters
            string deviceName = screen.DeviceName;
            int index = screen.DeviceName.IndexOf('\0');
            if (index != -1)
                deviceName = screen.DeviceName.Substring(0, index);
            return deviceName;
        }

        /// <summary>
        /// Retrieves the screen on which the specified window resides.
        /// </summary>
        /// <param name="windowHandle">The window handle.</param>
        /// <returns>The screen containing the window.</returns>
        static Screen ScreenFromHandle(IntPtr windowHandle)
        {
            // get the real window rectangle
            Rectangle rectangle = NativeMethods.GetWindowRectangle(windowHandle);

            // find the screen that contains the most area of the window
            Screen screen = Screen.AllScreens.BestMatch(new Func<Screen, int>(delegate(Screen s)
                { Rectangle r = Rectangle.Intersect(rectangle, s.Bounds); return r.Width * r.Height; }));

            // if we didn't find a screen, return the default one
            if (screen == null)
                screen = Screen.PrimaryScreen;
            return screen;
        }

        /// <summary>
        /// Finds a screen based upon its given device name.
        /// </summary>
        /// <param name="screenDeviceName">Name of the screen device.</param>
        /// <returns>The desired screen.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="screenDeviceName"/> is <c>null</c>.</exception>
        static Screen ScreenFromDeviceName(string screenDeviceName)
        {
            // error checking
            if (string.IsNullOrEmpty(screenDeviceName))
                throw new ArgumentNullException("screenDeviceName");

            // return the appropriate screen
            return Screen.AllScreens.First(s => DeviceNameFromScreen(s) == screenDeviceName);
        }

        /// <summary>
        /// Gets the assembly title.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The title of the assembly, or <c>null</c> if one cannot be found.</returns>
        static string GetAssemblyTitle(Assembly assembly)
        {
            // make sure we have an assembly
            if (assembly == null)
                return null;

            // check if there is a title attribute
            AssemblyTitleAttribute[] customAttributes = (AssemblyTitleAttribute[])assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), true);
            if (customAttributes != null && customAttributes.Length > 0)
                return customAttributes[0].Title;

            // otherwise, no title found
            return null;
        }

        /// <summary>
        /// Gets the default application title.
        /// </summary>
        /// <returns>The default application title.</returns>
        static string GetDefaultTitle()
        {
            // check if we have an assembly title
            string assemblyTitle = GetAssemblyTitle(Assembly.GetEntryAssembly());
            if (!string.IsNullOrEmpty(assemblyTitle))
                return assemblyTitle;

            try
            {
                // get the name of the executable
                Uri uri = new Uri(Application.ExecutablePath);
                return Path.GetFileNameWithoutExtension(uri.LocalPath);
            }
            catch (ArgumentNullException)
            {
                // swallow the exception
            }
            catch (UriFormatException)
            {
                // swallow the exception
            }

            // otherwise, use the default title
            return DefaultTitle;
        }

        /// <summary>
        /// Gets the default application icon.
        /// </summary>
        /// <returns>The default application icon, or <c>null</c> if it cannot be found.</returns>
        static Icon GetDefaultIcon()
        {
            // try to get the entry assembly
            Icon icon;
            Assembly entry = Assembly.GetEntryAssembly();
            if (entry != null)
            {
                try
                {
                    // extract the icon
                    icon = Icon.ExtractAssociatedIcon(entry.Location);
                    if (icon != null)
                        return icon;
                }
                catch (ArgumentException)
                {
                    // swallow the exception
                }
            }

            // if we haven't found a default icon yet, search for the first icon in the assembly
            icon = FindFirstIcon(entry);
            if (icon != null)
                return icon;

            // if there are no icons, create a default one for use with the game
            return new Icon(typeof(Game), DefaultIconName);
        }

        /// <summary>
        /// Finds the first icon in an assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The first icon resource in the assembly, or <c>null</c> if one cannot be found.</returns>
        static Icon FindFirstIcon(Assembly assembly)
        {
            // make sure we have an assembly
            if (assembly == null)
                return null;

            // check each resource
            foreach (string name in assembly.GetManifestResourceNames())
            {
                try
                {
                    // try to load the resource stream as an icon
                    return new Icon(assembly.GetManifestResourceStream(name));
                }
                catch
                {
                    try
                    {
                        // it didn't work, so try loading different parts of the resource
                        IDictionaryEnumerator enumerator = new ResourceReader(assembly.GetManifestResourceStream(name)).GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            // try to get the icon
                            Icon icon = enumerator.Value as Icon;
                            if (icon != null)
                                return icon;
                        }
                    }
                    catch
                    {
                        // swallow the exception
                    }
                }
            }

            // if we got to this point, no icon was found
            return null;
        }
    }
}

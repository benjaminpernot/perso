//------------------------------------------------------------------------------
// <copyright file="ToolWindow1Control.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/* TODO:
- Using debugger to get memory access rather than ReadProcessMemory
- Make it work for 32 and 64 bit process
- Remove update buttons
- Save settings
*/

namespace Image_Viewer_for_Visual_Studio
{
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.VisualStudio.Shell;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System;
    using Microsoft.VisualStudio.Shell.Interop;
    using System.Windows.Media.Imaging;
    using System.Windows.Media;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Interaction logic for ToolWindow1Control.
    /// </summary>
    public partial class ToolWindow1Control : UserControl
    {
        private DTE2 m_dte;
        private Debugger m_debugger;
        private DebuggerEvents m_debuggerEvents;
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolWindow1Control"/> class.
        /// </summary>
        public ToolWindow1Control()
        {
            m_dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));
            m_debugger = m_dte.Debugger;
            m_debuggerEvents = m_dte.Events.DebuggerEvents;
            m_debuggerEvents.OnEnterBreakMode += DebuggerEvents_OnEnterBreakMode;

            IVsDebugger debugService = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsShellDebugger)) as IVsDebugger;
            if (debugService != null)
            {
                // Register for debug events.
                // Assumes the current class implements IDebugEventCallback2.
                debugService.AdviseDebugEventCallback(this);
            }
            this.InitializeComponent();
        }
        /*public static BitmapSource FromArray(byte[] data, int w, int h, int ch)
        {
            PixelFormat format = PixelFormats.Default;

            if (ch == 1) format = PixelFormats.Gray8; //grey scale image 0-255
            if (ch == 3) format = PixelFormats.Bgr24; //RGB
            if (ch == 4) format = PixelFormats.Bgr32; //RGB + alpha


            WriteableBitmap wbm = new WriteableBitmap(w, h, 96, 96, format, null);
            wbm.WritePixels(new Int32Rect(0, 0, w, h), data, ch * w, 0);

            return wbm;
        }*/

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess,
          int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        private byte[] ReadMemoryFromProcess(Process process, IntPtr address, UInt32 nbBytes)
        {
            const int PROCESS_WM_READ = 0x0010;
            IntPtr processHandle = OpenProcess(PROCESS_WM_READ, false, process.ProcessID);
            int bytesRead = 0;
            byte[] buffer = new byte[nbBytes];
            bool ret = ReadProcessMemory((int)processHandle, (int)address, buffer, buffer.Length, ref bytesRead);
            return buffer;
        }

        private void DebuggerEvents_OnEnterBreakMode(dbgEventReason Reason, ref dbgExecutionAction ExecutionAction)
        {
            /*string addressText = textboxAddress.Text;
            EnvDTE.Expression evaluated = m_debugger.GetExpression(addressText);
            IntPtr address = (IntPtr)Convert.ToUInt64(evaluated.Value, 16);
            var buffer = ReadMemoryFromProcess(m_debugger.CurrentProcess, address);
            BitmapSource bitmapSource = BitmapSource.Create(2, 2, 1, 1, PixelFormats.Indexed8, BitmapPalettes.Gray256, buffer, 2);
            image.Source = bitmapSource;*/
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void ButtonUpdateOnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string addressText = textboxAddress.Text;
                EnvDTE.Expression evaluated = m_debugger.GetExpression(addressText);
                Int64 address64 = (Int64)new System.ComponentModel.Int64Converter().ConvertFromString(evaluated.Value);
                IntPtr address = (IntPtr)address64;

                string widthText = textboxWidth.Text;
                evaluated = m_debugger.GetExpression(widthText);
                UInt32 width = (UInt32)new System.ComponentModel.UInt32Converter().ConvertFromString(evaluated.Value);

                string heightText = textboxHeight.Text;
                evaluated = m_debugger.GetExpression(heightText);
                UInt32 height = (UInt32)new System.ComponentModel.UInt32Converter().ConvertFromString(evaluated.Value);

                var buffer = ReadMemoryFromProcess(m_debugger.CurrentProcess, address, width * height * 3);
                BitmapSource bitmapSource = BitmapSource.Create((int)width, (int)height, 1, 1, PixelFormats.Rgb24, null, buffer, (int)(width * 3));
                image.Source = bitmapSource;
            }
            catch (Exception ex)
            {
                buttonError.Text = ex.ToString();
            }
            
        }
    }
}
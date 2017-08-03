//------------------------------------------------------------------------------
// <copyright file="ToolWindow1Control.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace VSIXProject2
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.Debugger.Interop;
    using EnvDTE;
    using EnvDTE80;
    using EnvDTE90a;
    using System;
    using Microsoft.VisualStudio;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Windows.Media.Imaging;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for ToolWindow1Control.
    /// </summary>
    public partial class ToolWindow1Control : UserControl, IDebugEventCallback2
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

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess,
          int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        private byte[] ReadMemoryFromProcess(Process process, IntPtr address)
        {
            const int PROCESS_WM_READ = 0x0010;
            IntPtr processHandle = OpenProcess(PROCESS_WM_READ, false, process.ProcessID);
            int bytesRead = 0;
            byte[] buffer = new byte[24];
            bool ret = ReadProcessMemory((int)processHandle, (int)address, buffer, buffer.Length, ref bytesRead);
            return buffer;
        }

        private void DebuggerEvents_OnEnterBreakMode(dbgEventReason Reason, ref dbgExecutionAction ExecutionAction)
        {
            string addressText = textbox_address.Text;
            EnvDTE.Expression evaluated = m_debugger.GetExpression(addressText);
            IntPtr address = (IntPtr)Convert.ToUInt64(evaluated.Value, 16);
            var buffer = ReadMemoryFromProcess(m_debugger.CurrentProcess, address);
            BitmapSource bitmapSource = BitmapSource.Create(2, 2, 1, 1, PixelFormats.Indexed8, BitmapPalettes.Gray256, buffer, 2);
            image.Source = bitmapSource;
            /*EnvDTE.Expression exp = m_debugger.GetExpression(textbox_address.Text);
            textBox.AppendText(exp.Name + "=" + exp.Value);*/
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "ToolWindow1");
        }

        public int Event(IDebugEngine2 pEngine, IDebugProcess2 pProcess, IDebugProgram2 pProgram, IDebugThread2 thread, IDebugEvent2 pEvent, ref Guid riidEvent, uint dwAttrib)
        {
#if false
            // Get the automation API DTE object.
            EnvDTE.DTE DTE = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SDTE)) as EnvDTE.DTE;
            // Cast to StackFrame2, as it contains the Depth property that we need.
            StackFrame2 currentFrame2 = DTE.Debugger.CurrentStackFrame as StackFrame2;
            // Depth property is 1-based.
            uint currentFrameDepth = currentFrame2.Depth - 1;

            // Get frame info enum interface.
            IEnumDebugFrameInfo2 enumDebugFrameInfo2;
            thread.EnumFrameInfo((uint)enum_FRAMEINFO_FLAGS.FIF_FRAME, 0, out enumDebugFrameInfo2);

            // Skip frames above the current one.
            enumDebugFrameInfo2.Reset();
            enumDebugFrameInfo2.Skip(currentFrameDepth);

            // Get the current frame.
            FRAMEINFO[] frameInfo = new FRAMEINFO[1];
            uint fetched = 0;
            int hr = enumDebugFrameInfo2.Next(1, frameInfo, ref fetched);
            IDebugStackFrame2 stackFrame = frameInfo[0].m_pFrame;

            // Get a context for evaluating expressions.
            IDebugExpressionContext2 expressionContext;
            stackFrame.GetExpressionContext(out expressionContext)):

            // Parse the expression string.
            IDebugExpression2 expression;
            string error;
            uint errorCharIndex;
            expressionContext.ParseText(addressExpressionString,
                enum_PARSEFLAGS.PARSE_EXPRESSION, 10, out expression, out error, out errorCharIndex);
                        
            // Evaluate the parsed expression.
            IDebugProperty2 debugProperty = null;
            expression.EvaluateSync((uint)enum_EVALFLAGS.EVAL_NOSIDEEFFECTS,
                unchecked((uint)Timeout.Infinite), null, out debugProperty);

            // Get memory context for the property.
            IDebugMemoryContext2 memoryContext;
            debugProperty.GetMemoryContext(out memoryContext);

            // Get memory bytes interface.
            IDebugMemoryBytes2 memoryBytes;
            debugProperty.GetMemoryBytes(out memoryBytes);

            // The number of bytes to read.
            uint dataSize = GetMemorySizeToRead();

            // Allocate space for the result.
            byte[] data = new byte[dataSize];
            uint writtenBytes = 0;

            // Read data from the debuggee.
            uint unreadable = 0;
            int hr2 = memoryBytes.ReadAt(memoryContext, dataSize, data, out writtenBytes, ref unreadable);

            if (hr2 != VSConstants.S_OK)
            {
                // Read failed.
            }
            else if (writtenBytes < dataSize)
            {
                // Read partially succeeded.
            }
            else
            {
                // Read successful.
            }
#endif
            return 0;
        }
    }
}
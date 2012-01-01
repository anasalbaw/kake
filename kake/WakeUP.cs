// Copyright © Daniele Di Sarli

using System;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Threading;

namespace WakeUPTimer
{
    class WakeUP
    {
        private SafeWaitHandle handle = null;
        private EventWaitHandle wh = null;
        [DllImport("kernel32.dll")]
        public static extern SafeWaitHandle CreateWaitableTimer(IntPtr lpTimerAttributes, 
                                                                  bool bManualReset,
                                                                string lpTimerName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWaitableTimer(SafeWaitHandle hTimer, 
                                                    [In] ref long pDueTime, 
                                                              int lPeriod,
                                                           IntPtr pfnCompletionRoutine, 
                                                           IntPtr lpArgToCompletionRoutine,
                                                             bool fResume);
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CancelWaitableTimer(SafeWaitHandle hTimer);

        public event EventHandler Woken;

        public object Tag = null;

        private BackgroundWorker bgWorker = new BackgroundWorker();

        public WakeUP()
        {
            bgWorker.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorker_RunWorkerCompleted);
        }

        public void SetWakeUpTime(DateTime time)
        {
            bgWorker.RunWorkerAsync(time.ToFileTime());
        }

        public void CancelWakeUp()
        {
            if (handle != null && !handle.IsClosed)
            {
                CancelWaitableTimer(handle);
                handle.Close();
            }
            handle = null;
        }

        void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Woken != null && handle!=null)
            {
                Woken(this, new EventArgs());
                handle = null;
            }
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e) 
        {
            long waketime = (long)e.Argument;

            using (handle = CreateWaitableTimer(IntPtr.Zero, true, this.GetType().Assembly.GetName().Name.ToString() + "Timer"))
            {
                if (SetWaitableTimer(handle, ref waketime, 0, IntPtr.Zero, IntPtr.Zero, true))
                {
                    using (wh = new EventWaitHandle(false, EventResetMode.AutoReset))
                    {
                        wh.SafeWaitHandle = handle;
                        wh.WaitOne();
                    }
                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            wh = null;
        }

    }
}

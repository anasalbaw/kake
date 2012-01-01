using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
//   clsHook
//
//   This code allows you to hook into (subclass) external programs to 
//   intercept their windows messages.  This allows you to do things like monitor 
//   button presses and keyboard input as well as respond to new menu items you '
//   can add to other programs which you don't have the source code for.
//
//   Copyright (C) 2006  William Moeur
//   This program is free software; you can redistribute it and/or modify it
//
//Methods
//   GetWindowHandle(Caption, ClassName) - returns a handle to a top-level window
//   GetChildHandle(Caption, ClassName, TopLevelHandle) - returns a handle to a child window
//   AddMessage(NewMessage, strMessage) - Adds a windows message to the list of messages we want to monitor
//   SetHook - Sets the hook into the external program.
//   RemoveHook - Removes the hook from the external program.

//Properties
//   isHooked - Returns True if the hook is set.
//   TargethWnd - Sets or returns the handle of the target window that is to be hooked

//Events
//   SentMessage(ByRef m As Message) - Sent messages arrive here
//   SentRETMessage(ByRef m As Message) - Sent messages after processing arrive here
//   PostedMessage(ByRef m As Message) - Posted messages arrive here
//   HookError(Number, LocalMsg, APIMsg) - Raised on certain non-fatal error conditions
//   UnHook() - Raised when the hook is removed
//
public class clsHook : System.Windows.Forms.NativeWindow
{
	[DllImport("MainHook.Dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

	#region "Declare MainHook.dll routines"
	private static extern int InstallFilterDLL(int HookType, int dwThread, IntPtr hWndDLL);
	[DllImport("MainHook.Dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

	private static extern int UnInstallFilterDLL(int dwThread, IntPtr hWndDLL, IntPtr hWndVB);
	[DllImport("MainHook.Dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

	private static extern void SetSharedData(int uMsg, int wParam, int lParam);
	#endregion

	#region " Events "
	public event SentMessageEventHandler SentMessage;
	public delegate void SentMessageEventHandler(ref Message m);
	public event SentRETMessageEventHandler SentRETMessage;
	public delegate void SentRETMessageEventHandler(ref Message m);
	public event PostedMessageEventHandler PostedMessage;
	public delegate void PostedMessageEventHandler(ref Message m);
	public event HookErrorEventHandler HookError;
	public delegate void HookErrorEventHandler(int Number, string LocalMsg, string APIMsg);
	public event UnHookEventHandler UnHook;
	public delegate void UnHookEventHandler();
	#endregion

	#region "Constants"
	//Private Window Messages
	private const int WM_USER = 0x400;
	//sent from VBApp to tell DLL to monitor a message
	private const int UM_FLAGHOOK = WM_USER + 0x100;
	//sent from VBApp to tell DLL to stop monitoring a message
	private const int UM_CLEARFLAG = WM_USER + 0x101;
	//sent from DLL to tell VBApp that we received a monitored message
	private const int UM_CALLBACK = WM_USER + 0x102;
	//sent from VBApp to tell DLL to monitor a message
	private const int UM_ADDMESSAGE = WM_USER + 0x100;
	//sent from dll when we're shutting down

	private const int UM_CLOSE = WM_USER + 0x107;
	private const int WH_CALLWNDPROC = 4;
	private const int WH_CALLWNDPROCRET = 12;

	private const int WH_GETMESSAGE = 3;
	#endregion

	#region " Private Variables "
	private IntPtr m_Targethwnd;
	private int m_ThreadID;
		#endregion
	private bool m_isHooked;
	[DllImport("kernel32", EntryPoint = "FormatMessageA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

	#region " Windows API Routines "
	//used for GetAPIErrorText
	private static extern int FormatMessage(int dwFlags, int lpSource, int dwMessageId, int dwLanguageId, string lpBuffer, int nSize, int Arguments);
	private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;

    private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
	[DllImport("user32", EntryPoint = "SendMessageA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
	private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
	[DllImport("user32", EntryPoint = "FindWindowA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

	private static extern int FindWindow(string lpClassName, string lpWindowName);
	[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

	private static extern int GetWindowThreadProcessId(IntPtr hWnd, int lpdwProcessId);
	[DllImport("user32", EntryPoint = "FindWindowExA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]

	private static extern int FindWindowEx(int hWnd1, int hWnd2, string lpsz1, string lpsz2);

	#endregion


	public Dictionary<IntPtr, string> Messages = new Dictionary<IntPtr, string>();
	public clsHook()
	{
		this.CreateHandle(new CreateParams());
	}

	//UM_CALLBACK is our special message.  It tells us that 
	// someting has happened in the DLL
	IntPtr static_WndProc_MyMessage;
	IntPtr static_WndProc_MyMessageType;
	protected override void WndProc(ref Message m)
	{
		int old_uMsg = 0;
		int old_wParam = 0;
		int old_lParam = 0;

		
		if(m.Msg==UM_CALLBACK)
        {
			static_WndProc_MyMessageType = m.LParam;
			static_WndProc_MyMessage = m.WParam;

        }else if(m.Msg==UM_CLOSE)
        {
		    //RemoveAllHooks() 'so shut them down
		    if (UnHook != null) {
			    UnHook();
		    }

		}else if(m.Msg==static_WndProc_MyMessage.ToInt32())
        {
				switch (static_WndProc_MyMessageType.ToInt32()) {

					//================== Sent message notification ============
					case  // ERROR: Case labels with binary operators are unsupported : Equality
WH_CALLWNDPROC:
						// sent message notification
						if (SentMessage != null) {
							SentMessage(ref m);
						}


						break;
					//================== Sent message notification ========
					case  // ERROR: Case labels with binary operators are unsupported : Equality
WH_CALLWNDPROCRET:
						// sent message notification
						if (SentRETMessage != null) {
							SentRETMessage(ref m);
						}


						break;
					//================== Posted message notification =======
					case  // ERROR: Case labels with binary operators are unsupported : Equality
WH_GETMESSAGE:
						//posted message notification
						//parameters are passed by reference so that user can change them
						//Save old values
						old_uMsg = m.Msg;
						old_wParam = m.WParam.ToInt32();
						old_lParam = m.LParam.ToInt32();
						if (PostedMessage != null) {
							PostedMessage(ref m);
						}

						//if user changed any value then pass it on to dll
						if ((old_uMsg != m.Msg) | (old_wParam != m.WParam.ToInt32()) | (old_lParam != m.LParam.ToInt32()))
							SetSharedData(m.Msg, m.WParam.ToInt32(), m.LParam.ToInt32());

						break;
				}

				static_WndProc_MyMessageType = IntPtr.Zero;
				static_WndProc_MyMessage = IntPtr.Zero;

                } else {
				//pass the message on
				base.WndProc(ref m);
		}

	}
    /*
	protected override void Finalize()
	{
		RemoveHook();
	}
    */
	public int GetChildHandle(string Caption, string ClassName, int TopLevelHandle)
	{
		//Returns the handle to a child window
		int i = 0;

		i = FindWindowEx(TopLevelHandle, 0, ClassName, Caption);
		if (i == 0) {
			if (HookError != null) {
				throw new Exception("Error getting child window.");
			}
		}
		return i;

	}

	public int GetWindowHandle(string Caption, string ClassName = (string)null)
	{
		//Returns the Handle of a top-level Window that has the caption and/or classname specified
		int HandleToWindow = 0;
		HandleToWindow = FindWindow(ClassName, Caption);
		if (HandleToWindow == 0) {
			if (HookError != null) {
                throw new Exception("Error Getting Top Level Handle");
			}
		}
		return HandleToWindow;
	}

    public void AddMessage(int NewMessage, string strMessage)
    {
        AddMessage((IntPtr)NewMessage, strMessage);
    }
	public void AddMessage(IntPtr NewMessage, string strMessage)
	{
		//Add new message to monitor
		//here we want to...
		//1. see if hook has been set
		//2. if so, then send message to tell dll to monitor new message

		Messages.Add(NewMessage, strMessage);
		if (m_isHooked)
			SendMessage(m_Targethwnd, UM_ADDMESSAGE, NewMessage, this.Handle);
	}

	public bool SetHook()
	{
		bool functionReturnValue = false;
		//Sets both hooks
		//Before calling this routine you must set Target Handle
		int ErrorNumber = 0;


		functionReturnValue = false;

		if (m_isHooked) {
			if (HookError != null) {
				HookError(0, "Hook Already Set", "");
			}
			return functionReturnValue;
		}

		if (m_Targethwnd == IntPtr.Zero)
			throw new Exception("You must set handle before setting hook.");

		//get thread ID of process running window m_Targethwnd
		m_ThreadID = GetWindowThreadProcessId(m_Targethwnd, 0);
		if (m_ThreadID == 0) {
			throw new Exception("Error Getting Thread ID. " + GetAPIErrorText(ErrorNumber));
		}

		//install the msg dll hook into that thread
		ErrorNumber = InstallFilterDLL(WH_GETMESSAGE, m_ThreadID, m_Targethwnd);
		if (ErrorNumber != 0)
			throw new Exception("Error Setting DLL Filter. " + GetAPIErrorText(ErrorNumber));

		ErrorNumber = InstallFilterDLL(WH_CALLWNDPROC, m_ThreadID, m_Targethwnd);
		if (ErrorNumber != 0)
			throw new Exception("Error Setting DLL Filter. " + GetAPIErrorText(ErrorNumber));

		ErrorNumber = InstallFilterDLL(WH_CALLWNDPROCRET, m_ThreadID, m_Targethwnd);
		if (ErrorNumber != 0)
            throw new Exception("Error Setting DLL Filter. " + GetAPIErrorText(ErrorNumber));

		//send all queued messages
		foreach (KeyValuePair<IntPtr, string> message_loopVariable in Messages) {
            SendMessage(m_Targethwnd, UM_ADDMESSAGE, message_loopVariable.Key, this.Handle);
		}


		m_isHooked = true;
		functionReturnValue = true;
		return functionReturnValue;


	}

	private string GetAPIErrorText(int lError)
	{
		string functionReturnValue = null;
		//Return the text of the API error denoted by lError
		string sOut = null;
		string sMsg = null;
		int lret = 0;
		functionReturnValue = "";
		sMsg = new string(' ',256);
		lret = FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS, 0, lError, 0, sMsg, sMsg.Length, 0);

        sOut = lError + " (&H" + lError.ToString("x") +"): ";

		if (lret != 0) {
			sMsg = sMsg.Trim();
			if (sMsg.EndsWith("\r\n"))
				sMsg = sMsg.Substring(0,sMsg.Length-2);
			sOut = sOut + sMsg.Trim();
		} else {
			sOut = sOut + "<no such error> ";
		}

		functionReturnValue = sOut;
		return functionReturnValue;

	}

	public bool isHooked()
	{
		//Checks to see if a hook type has already been set by this control
		return m_isHooked;
	}

	public bool RemoveHook()
	{
		bool functionReturnValue = false;
		//Removes all the hooks that this class set
		int retval = 0;
		functionReturnValue = false;
		if (!m_isHooked)
			return functionReturnValue;
		retval = UnInstallFilterDLL(m_ThreadID, m_Targethwnd, this.Handle);
		if (retval != 0) {
			if (HookError != null) {
				HookError(retval, "Error removing hook", GetAPIErrorText(retval));
			}
			functionReturnValue = false;
		} else {
			functionReturnValue = true;
		}
		m_isHooked = false;
		return functionReturnValue;
	}

	public IntPtr TargethWnd {
		get { return m_Targethwnd; }
		set { m_Targethwnd = value; }
	}

}

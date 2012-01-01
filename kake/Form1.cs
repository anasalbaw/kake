using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CoreAudioApi;
using WakeUPTimer;
using SpotiKat.Spotify.Entity;
using SpotiKat.Spotify.Manager;
using SpotiKat.Spotify.Core;
using System.Web;

namespace kake
{
    public partial class Form1 : Form
    {
        delegate void stringFunctionDelegate(string text);
        delegate void boolFunctionDelegate(bool value);
        delegate void intFunctionDelegate(int value);
        delegate void intFunctionDelegateList(ListViewItem value);
        delegate string intFunctionDelegateList2(ListViewItem value);
        delegate string voidFunctionDelegate();
        string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Spotify\\";
        int lastMuted = 0;
        List<Filter> songFilters = null;
        string lastSpotifyTitle = "";
        bool adPlaying = false;
        string userpath = "", user = "", lastSongTitle = "", lastSongUrl="";
        MMDeviceEnumerator devEnum = new MMDeviceEnumerator();
        const string USERFILE = "guistate";
        const string SETTINGS = "settings";
        const string ADFILE = "ad.bnk";
        private int volume;
        private bool muted = false;
        private TcpListener tcpListener;
        private Thread listenThread;
        private bool playingQueue = false;
        private string[] resources;
        private string currentSongArtist;
        private string currentSongTitle;
        private List<string> songQueue = new List<string>();
        private List<TcpClient> tcpClients = new List<TcpClient>();
        private System.Reflection.Assembly thisExe = System.Reflection.Assembly.GetExecutingAssembly();
        IntPtr spotifyHandle = IntPtr.Zero;
        clsHook sysHook = new clsHook();
        bool disconnect = false;
        public Form1()
        {
            InitializeComponent();
            resources = thisExe.GetManifestResourceNames();
            try
            {
                JObject o = JObject.Parse(File.ReadAllText(path + SETTINGS));
                try
                {
                    user = o["autologin_canonical_username"].Value<string>().ToLower();
                }
                catch { }
                if (string.IsNullOrEmpty(user))
                {
                    try
                    {
                        user = o["autologin_username"].Value<string>().ToLower();
                    }
                    catch { }
                }

                lstUsers.Items.Clear();
                foreach (string usrp in Directory.GetDirectories(path + "Users\\"))
                {
                    string usr = usrp.Substring(usrp.LastIndexOf('\\') + 1);
                    usr = usr.Substring(0, usr.Length - 5);
                    if(File.Exists(path + "Users\\" + usr + "-user\\guistate"))
                        lstUsers.Items.Add(usr);
                }
                lstUsers.Text = user;
                string local = getLocalIp();
                if (local != "127.0.0.1")
                {
                    txtIPs.Text = local.Substring(0,local.LastIndexOf('.')+1);
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("Nyt tuli virhe! :(\r\n\r\n"+e.Message);
            }
        }

        private string httpDateTime(DateTime dt)
        {
            return dt.AddHours(-int.Parse(dt.ToString("z ").Trim())).ToString("ddd, dd MMM yyyy H:mm:ss \\G\\M\\T", new System.Globalization.CultureInfo("en-US"));
        }

        private bool isAd(string title)
        {
            if (adPlaying == false && title == "nothing") return false;
            bool isAd = !title.Contains('–');
            if (!isAd)
            {
                JToken tracks = JObject.Parse(File.ReadAllText(userpath + USERFILE))["Recent Tracks"];
                string str = title.Substring(title.IndexOf('–') + 2);
                bool found = false;
                foreach (JToken jt in tracks)
                {
                    if (str == jt["name"].Value<string>())
                    {
                        lastSongUrl = jt["uri"].Value<string>();
                        found = true;
                        break;
                    }
                }
                isAd = !found;
            }
            return isAd;
        }

        private void processMsg(ref Message m)
        {
            string title = Marshal.PtrToStringAuto(m.LParam);
            if (title == "Spotify")
            {
                lastSpotifyTitle = "nothing";
                if (Environment.TickCount - 1000 < lastMuted)
                {
                    play(true, lastSpotifyTitle);
                }
                else
                {
                    lblSong.Text = lastSpotifyTitle;
                }
                return;
            }
            lastSpotifyTitle = title;
            title = title.Substring(10);
            string song = title.Split('–')[1];
            bool ad = isAd(title);
            if (ad)
            {
                lastSongUrl = "ad";
                onAdvertisement();
            }
            else
            {
                onSongChange(title);
            }
            //Debug.WriteLine(title + "\r\nmainos = " + isAd.ToString() + "\r\n");
            /*
            try
            {
                Debug.WriteLine("\"" + title.Split('–')[1].Substring(1) + "\"");
            }
            catch { }
            */
        }
        delegate void launchSpotifyUriDelegate(object uri);
        private void onSongChange(string title = "")
        {
            if (songQueue.Count > 0)
            {
                if (playingQueue == false)
                {
                    playingQueue = true;
                    play(false);
                    prevSong();
                    play(false);
                    new Thread(new ParameterizedThreadStart(new launchSpotifyUriDelegate(launchSpotifyUri))).Start(songQueue[0]);
                    songQueue.RemoveAt(0);
                    return;
                }
            }
            playingQueue = false;
            string songArtist = "";
            string songTitle = "";
            if (title.Contains('–'))
            {
                songArtist = title.Substring(0, title.IndexOf('–')).Trim();
                songTitle = title.Substring(title.IndexOf('–') + 2).Trim();
            }
            else
            {
                songArtist = "";
                songTitle = title;
            }
            setMute(false);
            if (adPlaying)
            {
                adPlaying = false;
            }
            bool success = true;
            if (songFilters != null && songFilters.Count > 0)
            {
                Debug.WriteLine("filters set ("+songFilters.Count+")");
                foreach (Filter f in songFilters)
                    if (f.test(songArtist, songTitle, title)) { success = false; break; }
                //Debug.WriteLine(f.fVars[0].ToString() + " " + f.fOps[0].ToString() + " " + f.fCmp[0] + " = " + f.test(currentSongArtist, currentSongTitle, lastSongTitle));
            }
            lastSongTitle = title;
            if (success)
            {
                lblSong.Text = lastSongTitle;
                currentSongArtist = songArtist;
                currentSongTitle = songTitle;
            }
            else
            {
                new Thread(new ThreadStart(nextSongDelayed)).Start();
            }
        }

        private void nextSongDelayed()
        {
            Thread.Sleep(20);
            nextSong();
        }

        private void sendToClients(string str)
        {
            foreach (TcpClient tc in tcpClients)
            {
                try
                {
                    if (!tc.Connected) tcpClients.Remove(tc);
                    byte[] b = Encoding.UTF8.GetBytes(str + "\r\n");
                    tc.GetStream().Write(b, 0, b.Length);
                }
                catch { }
            }
        }

        private void sendToClient(NetworkStream c, string str, byte[] bytes = null)
        {
            try
            {
                if (bytes == null)
                {
                    byte[] b = Encoding.UTF8.GetBytes(str);
                    c.Write(b, 0, b.Length);
                }
                else
                    c.Write(bytes, 0, bytes.Length);
                //Debug.WriteLine("sent to client: " + str + "\r\n");
            }
            catch { }
        }

        private void sendToClientBytes(NetworkStream c, byte[] b)
        {
            try
            {
                c.Write(b, 0, b.Length);
            }
            catch { }
        }

        private void setMute(bool state)
        {
            try
            {
                getSpotifyAudioSession().SimpleAudioVolume.Mute = state;
                if (state) lastMuted = Environment.TickCount;
            }
            catch { }
        }

        private bool isPlaying()
        {
            return (lastSpotifyTitle != "nothing");
        }

        private void mute(bool state)
        {
            int tmp = volume;
            if (state == true)
            {
                changeVolume(-11);
                volume = tmp;
            }
            else
            {
                volume = 0;
                changeVolume((tmp / 6553));
            }
            muted = state;
        }

        private void changeVolume(int direction)
        {

            volume = volume + Math.Sign(direction) * 6554;
            int key = 0;
            if (direction > 0) key = 0x26; else key = 0x28;
            Win32.keybd_event(0x11, 0x45, Win32.KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);
            Win32.PostMessage(spotifyHandle, Win32.WM_KEYDOWN, key, 0X014C0001);
            Thread.Sleep(30);
            Win32.PostMessage(spotifyHandle, Win32.WM_KEYUP, key, 0XC14C0001);
            Thread.Sleep(20);
            Win32.keybd_event(0x11, 0x45, Win32.KEYEVENTF_EXTENDEDKEY | Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
            if (volume < 0) { volume = 0; return; }
            else if (volume > 65535) { volume = 65535; return; }
            else if (Math.Abs(direction) > 1)
            {
                changeVolume(direction - Math.Sign(direction));
            }
            //writeLog("Volume set to " + Math.Round(((float)volume / 65535.0) * 100.0, 0) + "% (" + volume + ")");
        }

        private void play(bool state = true, string title="")
        {
            if (!string.IsNullOrEmpty(title)) lastSpotifyTitle = title;
            if (!state ^ (lastSpotifyTitle=="nothing"))
            {
                Win32.PostMessage(spotifyHandle, Win32.WM_KEYDOWN, Win32.VkKeyScan(' '), 0);
                //Win32.PostMessage(spotifyHandle, Win32.WM_KEYUP, Win32.VkKeyScan(' '), 0); 
            }
        }
        private void pauseDelayed()
        {
            Thread.Sleep(5);
            play(false);
        }
        private void nextSong()
        {
            Debug.WriteLine("nextSong()");
            if (songQueue.Count > 0)
            {
                playingQueue = true;
                //play(false);
                launchSpotifyUri(songQueue[0]);
                songQueue.RemoveAt(0);
                Thread.Sleep(50);
            }
            else
            {
                Win32.keybd_event(0x11, 0x45, Win32.KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);
                Win32.PostMessage(spotifyHandle, Win32.WM_KEYDOWN, 0X27, 0X014D0001);
                Thread.Sleep(30);
                Win32.PostMessage(spotifyHandle, Win32.WM_KEYUP, 0X27, 0XC14D0001);
                Win32.keybd_event(0x11, 0x45, Win32.KEYEVENTF_EXTENDEDKEY | Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
        }
        private void prevSong()
        {
            Win32.keybd_event(0x11, 0x45, Win32.KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);
            Win32.PostMessage(spotifyHandle, Win32.WM_KEYDOWN, 0X25, 0X014B0001);
            Thread.Sleep(30);
            Win32.PostMessage(spotifyHandle, Win32.WM_KEYUP, 0X25, 0XC14B0001);
            Win32.keybd_event(0x11, 0x45, Win32.KEYEVENTF_EXTENDEDKEY | Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
        private void prevSongDelayed()
        {
            Thread.Sleep(10);
            prevSong();
        }

        private AudioSessionControl getSpotifyAudioSession()
        {
            MMDevice defaultDevice = devEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
            for (int i = 0; i < defaultDevice.AudioSessionManager.Sessions.Count; i++)
            {
                try
                {
                    using (Process p = Process.GetProcessById((int)defaultDevice.AudioSessionManager.Sessions[i].ProcessID))
                    {
                        if (p.ProcessName == "spotify")
                        {
                            return defaultDevice.AudioSessionManager.Sessions[i];
                        }
                    }
                }
                catch { }
            }
            throw new Exception("Audio end point not found!!!");
        }

        private void onAdvertisement()
        {
            adPlaying = true;
            if(chkCommercials.Checked==true)
                setMute(true);
            lblSong.Text = "advertisement";
            lastSongTitle = "advertisement";
        }

        private void processCommandLine()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 0)
            {
                foreach (string arg in args)
                {
                    switch (arg)
                    {
                        case "-server":
                            chkRemote.Checked = true;
                            break;
                        case "-spotify":
                            string[] spotifyReg = { "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Spotify",
                                                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Spotify"};
                            foreach (string reg in spotifyReg)
                            {
                                try
                                {
                                    RegistryKey regKeyAppRoot = Registry.CurrentUser.OpenSubKey(reg);
                                    string loc = (string)regKeyAppRoot.GetValue("InstallLocation") + "\\spotify.exe";
                                    regKeyAppRoot.Close();
                                    if (!File.Exists(loc)) continue;
                                    ProcessStartInfo pI = new ProcessStartInfo(loc);
                                    pI.WindowStyle = ProcessWindowStyle.Normal;
                                    Process sp = Process.Start(pI);

                                    int s = Environment.TickCount + 5000;
                                    spotifyHandle = IntPtr.Zero;
                                    while (Environment.TickCount < s)
                                    {
                                        Thread.Sleep(500);
                                        IntPtr intp = Win32.FindWindow("SpotifyMainWindow", (string)null);
                                        if (intp != IntPtr.Zero)
                                        {
                                            spotifyHandle = intp;
                                            break;
                                        }
                                    }
                                    if (spotifyHandle != IntPtr.Zero) break;
                                }
                                catch { }
                            }
                            if (spotifyHandle == IntPtr.Zero)
                                MessageBox.Show("Can't find Spotify :/");
                            else
                                Win32.SetForegroundWindow(spotifyHandle);
                            break;
                        case "-minimized":
                            this.WindowState = FormWindowState.Minimized;
                            break;
                        case "start":
                            btnStartStop_Click(this, EventArgs.Empty);
                            break;
                        default:
                            if (arg.StartsWith("-key="))
                            {
                                txtKey.Text = arg.Substring(5);
                            }
                            else if (arg.StartsWith("-port="))
                            {
                                txtPort.Text = arg.Substring(6);
                            }
                            else if (arg.StartsWith("-user="))
                            {
                                lstUsers.Text = arg.Substring(6);
                            }
                            else if (arg.StartsWith("-accept="))
                            {
                                txtIPs.Text = arg.Substring(8);
                            }
                            break;
                    }
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                sysHook.RemoveHook();
                sysHook.ReleaseHandle();
            }
            catch { }
            if (chkRemote.Checked)
            {
                closeServer();
            }
        }

        private void closeServer()
        {
            try
            {
                tcpListener.Stop();
                //listenThread.Abort();
                disconnect = true;
            }
            catch { }
            foreach (TcpClient tc in tcpClients)
            {
                try
                {
                    tc.Close();
                }
                catch { }
            }
            tcpClients.Clear();
        }

        private static IntPtr getSpotifyHandle()
        {
            IntPtr hWnd = Win32.FindWindow("SpotifyMainWindow", (string)null);
            if (hWnd.Equals(IntPtr.Zero))
                throw new Exception("Spotify not found :(");
            else
                return hWnd;
        }

        private static Process getSpotifyProcess()
        {
            try
            {
                return getSpotifyProcess(getSpotifyHandle());
            }
            catch
            {
                throw new Exception("Can't find Spotify process.");
            }
        }

        private static Process getSpotifyProcess(IntPtr hWnd)
        {
            try
            {
                int processId = 0;
                Win32.GetWindowThreadProcessId(hWnd, out processId);
                return Process.GetProcessById(processId);
            }
            catch
            {
                throw new Exception("Can't find Spotify process.");
            }
        }

        private static string getSpotifyTitle()
        {
            return getSpotifyTitle(getSpotifyHandle());
        }

        private static string getSpotifyTitle(IntPtr hWnd)
        {
            try
            {
                int length = Win32.GetWindowTextLength(hWnd);
                StringBuilder sb = new StringBuilder(length + 1);
                Win32.GetWindowText(hWnd, sb, sb.Capacity);
                string t = sb.ToString();
                if (t == "Spotify") return "nothing";
                if (t.StartsWith("Spotify - ")) return t.Substring(10);
                return t;
            }
            catch
            {
                throw new Exception("Unable to get Spotify window title.");
            }
        }

        private void btnStartStop_Click(object sender, EventArgs e)
        {
            if (btnStartStop.Text == "Start me!")
            {
                disconnect = false;
                if (string.IsNullOrEmpty(lstUsers.Text)) return;

                try
                {
                    sysHook.RemoveHook();
                    sysHook.Messages.Clear();
                    sysHook.TargethWnd = IntPtr.Zero;
                    sysHook.DestroyHandle();
                    sysHook.ReleaseHandle();
                }
                catch { }
                try
                {

                    user = lstUsers.Text.ToLower();
                    userpath = path + "Users\\" + System.Web.HttpUtility.UrlEncode(user) + "-user\\";
                    if (!Directory.Exists(userpath)) throw new Exception("User '" + user + "' not found!");
                    


                    volume = JObject.Parse(File.ReadAllText(userpath + USERFILE))["volume"].Value<int>();

                    spotifyHandle = getSpotifyHandle();
                    sysHook = new clsHook();
                    sysHook.TargethWnd = spotifyHandle;
                    sysHook.AddMessage(0xC, "WM_SETTEXT");
                    sysHook.SentRETMessage += new clsHook.SentRETMessageEventHandler(processMsg);
                    if (!sysHook.SetHook())
                        throw new Exception("Unable to set a hook!");
                    lastSongTitle = getSpotifyTitle(spotifyHandle);
                    if (isAd(lastSongTitle))
                    {
                        onAdvertisement();
                    }
                    else
                    {
                        onSongChange(lastSongTitle);
                    }
                    lblSong.Text = lastSongTitle;
                    btnStartStop.Text = "Stop me!";
                    lblStatus.Text = "now playing";
                    lstUsers.Enabled = false;
                    chkRemote.Enabled = false;
                    chkCommercials.Enabled = false;
                    txtPort.Enabled = false;
                    if (chkRemote.Checked)
                    {
                        int portti = 80;
                        if (!int.TryParse(txtPort.Text, out portti))
                            throw new Exception("Invalid port!");
                        this.tcpListener = new TcpListener(IPAddress.Any, portti);
                        this.listenThread = new Thread(new ThreadStart(ListenForClients));
                        this.listenThread.Start();
                        string port = (txtPort.Text != "80" ? ":" + int.Parse(txtPort.Text) : "") + "/" + (txtKey.TextLength > 0 ? txtKey.Text + "/" : "");
                        writeLog("Control started at http://" + getLocalIp() + port);
                        writeLog("http://"+System.Environment.MachineName.ToLower()+port+" may work also.");
                        writeLog("If your firewall asks, accept the connection.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message+"\r\n"+ex.StackTrace);
                }
            }
            else
            {
                try
                {
                    sysHook.RemoveHook();
                }
                catch { }
                btnStartStop.Text = "Start me!";
                lblStatus.Text = "not started";
                lblSong.Text = "";
                lstUsers.Enabled = true;
                try
                {
                    if (chkRemote.Checked)
                    {
                        closeServer();
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex )
                {
                    Debug.WriteLine(ex.Message);
                }
                chkRemote.Enabled = true;
                chkCommercials.Enabled = true;
                txtPort.Enabled = true;
            }
        }

        private string getLocalIp()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            return "127.0.0.1";
        }

        private void sendToServer(NetworkStream st, string s)
        {
            byte[] b = Encoding.UTF8.GetBytes(s+"\r\n");
            st.Write(b, 0, b.Length);
        }

        private void setSongText(string text)
        {
            if (lblSong.InvokeRequired)
            {
                lblSong.Invoke(new stringFunctionDelegate(setSongText), text);
                return;
            }
            lblSong.Text = text;
        }
        private void launchSpotifyUri(object o)
        { launchSpotifyUri((string)o); }

        private void launchSpotifyUri(string uri)
        {
            try
            {
                Process p = getSpotifyProcess();
                ProcessStartInfo pInfo = new ProcessStartInfo(p.MainModule.FileName, "/uri " + uri);
                Process.Start(pInfo);
            }
            catch (Exception e) {
                Debug.WriteLine(e.Message);
            }
        }


        private void writeLog(string txt)
        {
            try
            {
                if (this.Disposing) return;
                if (txtLog.InvokeRequired)
                {
                    txtLog.Invoke(new stringFunctionDelegate(writeLog), txt);
                    return;
                }
                txtLog.Text += DateTime.Now.ToString("[H:mm:ss] ") + txt + "\r\n";
                txtLog.SelectionStart = Math.Max(0, txtLog.Text.Length - 2);
                txtLog.ScrollToCaret();
                txtLog.Refresh();
            }
            catch { }
        }

        private void ListenForClients()
        {
            try
            {
                this.tcpListener.Start();
            }
            catch(Exception e) { MessageBox.Show("Unable to start server!\r\n"+e.Message); }
            try
            {
                while (disconnect == false)
                {
                    TcpClient client=null;
                    try
                    {
                        client = this.tcpListener.AcceptTcpClient();
                    }
                    catch (ThreadAbortException) { break; }
                    string ip = client.Client.RemoteEndPoint.ToString();
                    if (ip.Contains(":")) ip = ip.Substring(0, ip.IndexOf(":"));
                    if (ip.Contains(txtIPs.Text) || ip == "127.0.0.1")
                    {
                        Thread clientTh = new Thread(new ParameterizedThreadStart(HandleClientComm));
                        clientTh.Start(client);
                    }
                    else
                    {
                        writeLog("Blocked connection from " + ip);
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception e) {
                Debug.WriteLine(e.Message);
            }
            writeLog("Remote control server stopped");
        }

        private byte[] GetBytes(System.Drawing.Icon icon)
        {
            MemoryStream ms = new MemoryStream();
            icon.Save(ms);
            return ms.GetBuffer();
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;
            try
            {
                tcpClients.Add(tcpClient);
                NetworkStream clientStream = tcpClient.GetStream();
                byte[] message = new byte[4096];
                int bytesRead;
                string request = "";
                while (disconnect == false)
                {
                    bytesRead = 0;
                    bytesRead = clientStream.Read(message, 0, 4096);
                    if (bytesRead == 0)
                        break;
                    string recv = Encoding.UTF8.GetString(message, 0, bytesRead);
                    request += recv;
                    if (request.Contains("\r\n\r\n"))
                    {
                        string[] headers = Regex.Split(Regex.Split(request,"\r\n\r\n")[0], "\r\n");
                        if (headers[0].StartsWith("GET "))
                        {
                            string path = headers[0].Split(' ')[1];
                            string key = txtKey.Text.Trim();
                            if (path == "/favicon.ico")
                            {
                                byte[] b = GetBytes((System.Drawing.Icon)(new System.ComponentModel.ComponentResourceManager(typeof(Form1))).GetObject("$this.Icon"));
                                byte[] b2 = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n"+
                                    "Content-Type: image/x-icon\r\n"+
                                    "Content-Length: " + b.Length + "\r\n"+
                                    "Cache-Control: max-age=29030400, public\r\n\r\n");
                                byte[] b3 = new byte[b.Length + b2.Length];
                                System.Buffer.BlockCopy(b2, 0, b3, 0, b2.Length);
                                System.Buffer.BlockCopy(b, 0, b3,b2.Length, b.Length);
                                sendToClientBytes(clientStream, b3);
                                tcpClients.Remove(tcpClient);
                                tcpClient.Close();
                                break;
                            }
                            string ip = tcpClient.Client.LocalEndPoint.ToString();
                            string content="", contentType="text/plain";
                            byte[] contentBytes = null;
                            if (ip.Contains(":")) ip = ip.Substring(0, ip.IndexOf(":"));
                            if (key.Length > 0)
                            {
                                if (path.StartsWith("/" + key + "/"))
                                {
                                    path = path.Substring(key.Length + 1);
                                }
                                else if(!path.EndsWith("/"+key))
                                {
                                    writeLog(ip + " requested " + path + " (invalid key)");
                                    tcpClients.Remove(tcpClient);
                                    tcpClient.Close();
                                    break;
                                }
                            }
                            string forward = "",cache="";
                            int expires = 0;
                            if (path == "/") path = "/index.html";
                            bool logEvent = false;
                            switch (path)
                            {
                                case "/playpause":
                                    play(!isPlaying());
                                    forward = "/";
                                    break;
                                case "/next":
                                    string tmp = lastSongTitle;
                                    bool queue = songQueue.Count > 0;
                                    for (int i = 0; i < 3; i++)
                                    {
                                        nextSong();
                                        Thread.Sleep(200);
                                        if (tmp != lastSongTitle || queue) break;
                                    }
                                    forward = "/";
                                    break;
                                case "/prev":
                                    tmp = lastSongTitle;
                                    for (int i = 0; i < 3; i++)
                                    {
                                        prevSong();
                                        Thread.Sleep(200);
                                        if (tmp != lastSongTitle) break;
                                    }
                                    forward = "/";
                                    break;
                                case "/volume/up":
                                    changeVolume(1);
                                    forward = "/";
                                    break;
                                case "/volume/down":
                                    changeVolume(-1);
                                    forward = "/";
                                    break;
                                case "/mute":
                                    mute(true);
                                    forward = "/";
                                    break;
                                case "/unmute":
                                    mute(false);
                                    forward = "/";
                                    break;
                                case "/ajax/wait4event":
                                    int waitTime = 60;
                                    bool playing = isPlaying();
                                    int queued = songQueue.Count;
                                    int start = Environment.TickCount;
                                    tmp = lastSongTitle;
                                    while((Environment.TickCount-start)<waitTime*1000)
                                    {
                                        if(tmp!=lastSongTitle || playing!=isPlaying() || queued!=songQueue.Count)
                                            break;
                                        Thread.Sleep(50);
                                    }
                                    break;
                                case "/ajax/playpause":
                                    play(!isPlaying());
                                    Thread.Sleep(100);
                                    break;
                                case "/ajax/play":
                                    play(true);
                                    Thread.Sleep(100);
                                    break;
                                case "/ajax/pause":
                                    play(false);
                                    Thread.Sleep(100);
                                    break;
                                case "/ajax/next":
                                    tmp = lastSongTitle;
                                    queue = songQueue.Count > 0;
                                    for (int i = 0; i < 3; i++)
                                    {
                                        nextSong();
                                        Thread.Sleep(200);
                                        if (tmp != lastSongTitle || queue) break;
                                    }
                                    break;
                                case "/ajax/prev":
                                    tmp = lastSongTitle;
                                    for (int i = 0; i < 3; i++)
                                    {
                                        prevSong();
                                        Thread.Sleep(200);
                                        if (tmp != lastSongTitle) break;
                                    }
                                    break;
                                case "/ajax/volume/up":
                                    changeVolume(1);
                                    break;
                                case "/ajax/volume/down":
                                    changeVolume(-1);
                                    break;
                                case "/ajax/mute":
                                    mute(true);
                                    break;
                                case "/ajax/unmute":
                                    mute(false);
                                    break;
                                case "/song.txt":
                                    //expires = -1;
                                    content = lastSongTitle;
                                    break;
                                default:
                                    string fName = path.Replace(key+"/","").Substring(1);
                                    if (path.EndsWith("/" + key))
                                    {
                                        forward = "/";
                                    }
                                    else if (resources.Contains<string>("kake.html." + fName))
                                    {
                                        switch (fName)
                                        {
                                            case "style.css":
                                                expires = 6;
                                                cache = "Cache-Control: max-age=172800, proxy-revalidate\r\n";
                                                break;
                                            case "index.html":
                                                expires = 0;
                                                break;
                                            default:
                                                expires = 6;
                                                cache = "Cache-Control: max-age=172800, proxy-revalidate\r\n";
                                                break;
                                        }
                                        contentType = MIMEAssistant.GetMIMEType(fName);
                                        if (contentType.StartsWith("text/") || fName.EndsWith(".js"))
                                        {
                                            System.IO.StreamReader file = new System.IO.StreamReader(thisExe.GetManifestResourceStream("kake.html." + fName));
                                            content = file.ReadToEnd();
                                            file.Close();
                                            if (fName == "index.html")
                                            {
                                                content = content
                                                    .Replace("<!-- CURRENT_SONG -->", currentSongTitle)
                                                    .Replace("<!-- CURRENT_ARTIST -->", currentSongArtist)
                                                    .Replace("<!-- COMPUTER_NAME -->", Environment.MachineName);
                                            }
                                            else if (fName == "script.js")
                                            {
                                                content = content
                                                    .Replace("%IS_PLAYING%", Convert.ToInt32(isPlaying()).ToString());
                                            }
                                        }
                                        else
                                        {
                                            System.IO.Stream strm = thisExe.GetManifestResourceStream("kake.html." + fName);
                                            System.IO.StreamReader file = new System.IO.StreamReader(strm);
                                            int totalread = 0;
                                            contentBytes = new Byte[strm.Length];
                                            while (totalread < contentBytes.Length)
                                            {
                                                int read = strm.Read(contentBytes, totalread, (int)strm.Length);
                                                totalread += read;
                                            }
                                            file.Close();
                                        }
                                    }
                                    break;
                            }

                            if (path.StartsWith("/ajax/"))
                            {
                                if (path.StartsWith("/ajax/search?q="))
                                {
                                    string keywords = HttpUtility.UrlDecode(path.Substring("/ajax/search?q=".Length));
                                    try
                                    {
                                        TrackSearchResult result = SearchManager.Instance.SearchTrack(keywords, 0);
                                        if (!result.Success)
                                        {
                                            throw new Exception("Error while trying to search :(");
                                        }
                                        List<object> tracks = new List<object>();
                                        
                                        foreach (Track tr in result.Tracks)
                                        {
                                            if (tr.Available)
                                            {
                                                string artist = "";
                                                foreach(Artist a in tr.Artists)
                                                    artist += ", "+a.Name;
                                                tracks.Add(new Dictionary<string, object> { { "name", tr.Name }, { "artist", artist.Substring(2) }, { "album", tr.Album.Name }, { "length", tr.Length }, { "popularity", tr.Popularity }, { "uri", tr.Href } });
                                            }
                                        }
                                        if (tracks.Count > 0)
                                            content = JsonConvert.SerializeObject(new Dictionary<string, object> { { "results", tracks.GetRange(0,Math.Min(tracks.Count,20))} });
                                        else
                                            throw new Exception("Error while trying to search :(\r\n"+result.TotalResults.ToString());
                                    }
                                    catch (Exception ex) { content = JsonConvert.SerializeObject(new Dictionary<string, object> { { "error", ex.Message }, {"results", null} }); }
                                }
                                else
                                {
                                    if (path.StartsWith("/ajax/queue/spotify:"))
                                    {
                                        songQueue.Add(path.Substring("/ajax/queue/".Length));
                                    }
                                    content = JsonConvert.SerializeObject(new Dictionary<string, object> { { "muted", (Convert.ToInt32(muted)) }, { "volume", Math.Round((float)volume / 65535.0, 2) }, { "songTitle", currentSongTitle }, { "songArtist", currentSongArtist }, { "playing", (Convert.ToInt32(isPlaying())) }, { "queue", songQueue.Count } });
                                }
                                contentType = "application/json";
                                contentBytes = Encoding.UTF8.GetBytes(content);
                            }

                            if (string.IsNullOrEmpty(content) && contentBytes == null && string.IsNullOrEmpty(forward))
                            {
                                tcpClients.Remove(tcpClient);
                                tcpClient.Close();
                                return;
                            }

                            if (logEvent) writeLog(ip + " requested " + path);
                            if (forward.Length == 0)
                            {
                                sendToClient(clientStream, "HTTP/1.1 200 OK\r\n" +
                                                            "Date: " + httpDateTime(DateTime.Now) + "\r\n" +
                                                            "Expires: " + httpDateTime(DateTime.Now.AddHours(expires)) + "\r\n" +
                                                            "Content-Type: " + contentType + (contentType=="application/json" || contentType.StartsWith("text/") ? "; charset=utf-8" : "") + "\r\n" +
                                                            "Content-Length: " + (contentBytes == null ? System.Text.Encoding.UTF8.GetByteCount(content) : contentBytes.LongLength) + "\r\n" +
                                                            cache+
                                                            "\r\n");
                                sendToClient(clientStream, content, contentBytes);
                            }
                            else
                            {
                                string host="";
                                foreach(string h in headers)
                                    if (h.StartsWith("Host: "))
                                    { host = h.Substring(6); }
                                if (string.IsNullOrEmpty(host)) host = getLocalIp();
                                sendToClient(clientStream, "HTTP/1.1 302 Found\r\n" +
                                    "Location: http://"+host+(key.Length>0 ? "/"+key : "")+forward+"\r\n" +
                                                            "Content-Length: 0\r\n" +
                                                            "\r\n");
                            }
                        }
                        tcpClients.Remove(tcpClient);
                        tcpClient.Close();
                        break;
                    }
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception e) { Debug.WriteLine(e.Message); }
            try
            {
                tcpClients.Remove(tcpClient);
                tcpClient.Close();
            }
            catch { }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.ShowInTaskbar = true;
                this.Visible=true;
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {

            if (this.WindowState == FormWindowState.Minimized && this.ShowInTaskbar == true)
            {
                this.ShowInTaskbar = false;
                this.Visible = false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            processCommandLine();
            txtFilter_TextChanged(this, new EventArgs());
        }


        /*
        private void checkTimre(bool state)
        {
            if (chkTimer.InvokeRequired)
            {
                chkTimer.Invoke(new boolFunctionDelegate(checkTimer),state);
                return;
            }
            chkTimer.Checked = state;
        }
        */
        private string getSleepAction()
        {
            if (lstSleepAction.InvokeRequired)
                return (string)lstSleepAction.Invoke(new voidFunctionDelegate(getSleepAction));
            else
                return lstSleepAction.Text;
        }

        private void removeItem(ListViewItem li)
        {
            if (lstSchedule.InvokeRequired)
            {
                lstSchedule.Invoke(new intFunctionDelegateList(removeItem), li);
                return;
            }
            if (li.Text.ToLower() == "wake up from sleep")
            {
                ((WakeUP)li.Tag).CancelWakeUp();
            }
            else
            {
                ((System.Threading.Timer)li.Tag).Dispose();
            }
            li.Remove();
        }

        private string getItemAction(ListViewItem li)
        {
            if (lstSchedule.InvokeRequired)
            {
                return (string)lstSchedule.Invoke(new intFunctionDelegateList2(getItemAction), li);
            }
            return li.SubItems[0].Text;
        }

        private void doSleepAction(object stateInfo)
        {
            
            //AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
            //checkTimer(false);
            ListViewItem li = (ListViewItem)stateInfo;
            string act = getItemAction(li);
            removeItem(li);

            string action = act.ToLower();
            switch (action)
            {
                case "pause playback":
                    try
                    {
                        spotifyHandle = getSpotifyHandle();
                        lastSongTitle = getSpotifyTitle(spotifyHandle);
                        play(false, lastSongTitle);
                    }
                    catch { }
                    break;
                case "resume playback":
                    try
                    {
                        spotifyHandle = getSpotifyHandle();
                        lastSongTitle = getSpotifyTitle(spotifyHandle);
                        play(true, lastSongTitle);
                    }
                    catch { }
                    break;
                case "shutdown":
                    try
                    {
                        ProcessStartInfo pI = new ProcessStartInfo("shutdown", "/s /t 0");
                        pI.CreateNoWindow = true;
                        Process.Start(pI);
                    }
                    catch { }
                    break;
                case "go to sleep":
                    try
                    {
                        /*
                        ProcessStartInfo pI = new ProcessStartInfo("Rundll32.exe", "Powrprof.dll,SetSuspendState");
                        pI.CreateNoWindow = true;
                        Process.Start(pI);
                        */
                        Application.SetSuspendState(PowerState.Suspend, true, false);
                    }
                    catch { }
                    break;
                case "wake up from sleep":
                    try
                    {
                        
                    }
                    catch { }
                    break;
                case "quit spotify":
                    try
                    {
                        getSpotifyProcess().Kill();
                    }
                    catch { }
                    break;
                default:
                    try
                    {
                        Process proc = new Process();
                        proc.StartInfo.FileName = act.Substring(0,(act+" ").IndexOf(' '));
                        if(act.Contains(' ')) proc.StartInfo.Arguments = act.Substring(proc.StartInfo.FileName.Length + 1);
                        proc.StartInfo.UseShellExecute = true;
                        proc.Start();
                        proc.Dispose();
                        
                    }
                    catch { MessageBox.Show("Couldn't execute command \""+act+"\""); }
                    break;
            }
        }
        //delegate void WokenDelegate(object sender, EventArgs e);
        private void Woken(object sender, EventArgs e)
        {
            /*
            if (lstSchedule.InvokeRequired)
            {
                object[] param = {sender,e};
                lstSchedule.Invoke(new WokenDelegate(Woken),param);
                return;
            }
            */
            WakeUP wkup = (WakeUP)sender;
            if (wkup.Tag != null)
            {
                removeItem((ListViewItem)wkup.Tag);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ListViewItem li = new ListViewItem(lstSleepAction.Text);

            bool doRegularTask = true;
            
            //autoEvent = new AutoResetEvent(false);
            long dueTime = (long)dateTimeSleep.Value.Subtract(DateTime.Now).TotalSeconds;
            while (dueTime < 1)
            {
                dueTime += 24 * 60 * 60;
            }
            dateTimeSleep.Value = DateTime.Now.Add(TimeSpan.FromSeconds((double)dueTime));
            li.SubItems.Add(dateTimeSleep.Value.ToString("dddd HH:mm"));
            lstSchedule.Items.Add(li);


            switch (lstSleepAction.Text.ToLower())
            {
                case "wake up from sleep":
                    WakeUP wkup = new WakeUP();
                    wkup.SetWakeUpTime(dateTimeSleep.Value);
                    wkup.Woken += new EventHandler(Woken);
                    wkup.Tag = li;
                    doRegularTask = false;
                    li.Tag = wkup;
                    break;
            }

            if (doRegularTask)
            {
                TimerCallback cb = new TimerCallback(doSleepAction);
                li.Tag = new System.Threading.Timer(cb, li, dueTime * 1000, System.Threading.Timeout.Infinite);
            }

        }

        private void lstSchedule_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lstSchedule.Items.Count <= 0) return;
            if (lstSchedule.SelectedItems.Count != 1) return;
            removeItem(lstSchedule.SelectedItems[0]);
        }

        private void lstSleepAction_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            if (!txtFilter.Text.EndsWith("\n"))
            {
                txtFilter.BackColor = System.Drawing.Color.White;
                return;
            }
            List<Filter> filters = parseFilter(txtFilter.Text);
            if (filters.Count > 0)
            {
                songFilters = filters;
                txtFilter.BackColor = System.Drawing.Color.FromArgb(255, 220, 255, 230);
            }
            else
            {
                txtFilter.BackColor = System.Drawing.Color.FromArgb(255, 255, 220, 230);
                songFilters = null;
            }

        }

        private class Filter
        {
            public List<FilterVar> fVars = new List<FilterVar>();
            public List<FilterCmpOps> fOps = new List<FilterCmpOps>();
            public List<string> fCmp = new List<string>();
            public LogicOps op = LogicOps.AND;

            public Filter(FilterVar fV, FilterCmpOps fO, string cmp)
            {
                addFilter(fV, fO, cmp, LogicOps.AND);
            }

            public void addFilter(FilterVar fV, FilterCmpOps fO, string cmp, LogicOps oper)
            {
                fVars.Add(fV);
                fOps.Add(fO);
                fCmp.Add(cmp.ToLower());
                op = oper;
                if (op == LogicOps.NONE) op = LogicOps.AND;
            }

            public bool test(string artist, string track, string title)
            {
                int c = fOps.Count;
                for (int i = 0; i < c; i++)
                {
                    string cmp = "";
                    bool success = false;
                    switch (fVars[i])
                    {
                        case FilterVar.ARTIST:
                            cmp = artist;
                            break;
                        case FilterVar.TITLE:
                            cmp = title;
                            break;
                        case FilterVar.SONG:
                        case FilterVar.TRACK:
                            cmp = track;
                            break;
                    }
                    cmp = cmp.ToLower();
                    Debug.WriteLine("comparing to " + cmp+", "+fVars[i].ToString() );
                    switch (fOps[i])
                    {
                        case FilterCmpOps.EQ:
                        case FilterCmpOps.EQUALS:
                            success = (cmp == fCmp[i]);
                            break;
                        case FilterCmpOps.CONTAINS:
                            success = (cmp.Contains(fCmp[i]));
                            break;
                        case FilterCmpOps.STARTSWITH:
                            success = (cmp.StartsWith(fCmp[i]));
                            break;
                        case FilterCmpOps.ENDSWITH:
                            success = (cmp.EndsWith(fCmp[i]));
                            break;
                    }
                    if (op == LogicOps.AND && success == false)
                        return false;
                    else if (op == LogicOps.OR && success == true)
                        return true;
                }
                return (op == LogicOps.AND);
            }
        }

        private enum FilterVar
        {
            ARTIST,
            TRACK,
            SONG,
            TITLE,
            NONE
        }

        private enum FilterCmpOps
        {
            EQ,
            EQUALS,
            CONTAINS,
            STARTSWITH,
            ENDSWITH,
            NONE
        }

        private enum LogicOps
        {
            AND,
            OR,
            NONE
        }

        private List<Filter> parseFilter(string f)
        {
            FilterVar var = FilterVar.NONE;
            LogicOps lOp = LogicOps.NONE;
            FilterCmpOps op = FilterCmpOps.NONE;
            List<Filter> filters = new List<Filter>();
            foreach (string r in f.Split('\n'))
            {
                var = FilterVar.NONE;
                lOp = LogicOps.NONE;
                op = FilterCmpOps.NONE;
                string ins = r.TrimEnd('\r');
                int off = -1;
                char[] chars = ins.ToCharArray();
                while (off < ins.Length-1 && (off = ins.IndexOf('"',off+1)) != -1)
                {
                    for (int i = off + 1; i < ins.Length; i++)
                    {
                        if (chars[i] == ' ') chars[i] = '¤';
                        else if (chars[i] == '"') { off = i; break; }
                    }
                }
                ins = new String(chars);
                foreach (string t in ins.Split(' '))
                {
                    if (string.IsNullOrWhiteSpace(t)) continue;
                    string tu = t.ToUpper();
                    if (op != FilterCmpOps.NONE)
                    {
                        if (lOp == LogicOps.NONE)
                            filters.Add(new Filter(var, op, t.Replace('¤',' ').Trim('"')));
                        else
                            filters.Last<Filter>().addFilter(var, op, t.Replace('¤', ' ').Trim('"'), lOp);
                        lOp = LogicOps.NONE;
                    }
                    else
                    {
                        if (var == FilterVar.NONE && Enum.IsDefined(typeof(FilterVar), tu))
                        {
                            var = (FilterVar)Enum.Parse(typeof(FilterVar), tu);
                        }
                        else if (var != FilterVar.NONE && op == FilterCmpOps.NONE && Enum.IsDefined(typeof(FilterCmpOps), tu))
                        {
                            op = (FilterCmpOps)Enum.Parse(typeof(FilterCmpOps), tu);
                        }
                        else if (var != FilterVar.NONE && op != FilterCmpOps.NONE && lOp == LogicOps.NONE && Enum.IsDefined(typeof(LogicOps), tu))
                        {
                            lOp = (LogicOps)Enum.Parse(typeof(LogicOps), tu);
                            var = FilterVar.NONE;
                            op = FilterCmpOps.NONE;
                        }
                        else { filters.Clear(); return filters; }
                    }
                }
            }
            return filters;
        }
    }

    public static class Win32
    {
        public const int KEYEVENTF_EXTENDEDKEY = 0x1;
        public const int KEYEVENTF_KEYUP = 0x2;
        [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);
        [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, uint lParam);

        [DllImport("user32.dll")]
        public static extern byte VkKeyScan(char ch);

        public const uint WM_KEYDOWN = 0x100;
        public const uint WM_KEYUP = 0x101; 

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int ProcessId);


        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern long GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags,
           UIntPtr dwExtraInfo);
    }
}

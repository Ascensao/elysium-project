using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Speech.Recognition;
using System.Threading;
using System.Media;
using System.Diagnostics;
using System.IO;
using WindowsInput;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using IWshRuntimeLibrary;
using Shell32;
using System.Security.Cryptography;


namespace ProjectElysium
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            ico = notifyIcon1.Icon;

            // INTERFACE COLOR

            //backcolor
            this.BackColor = ColorTranslator.FromHtml("#000000");
            this.txtMain.BackColor = ColorTranslator.FromHtml("#000000");
            this.txtCMD.BackColor = ColorTranslator.FromHtml("#000000");
            this.txtNum.BackColor = ColorTranslator.FromHtml("#000000");

            //font color
            this.txtMain.ForeColor = ColorTranslator.FromHtml("#17B3AF");
            this.txtCMD.ForeColor = ColorTranslator.FromHtml("#17B3AF");
            this.txtNum.ForeColor = ColorTranslator.FromHtml("#17B3AF");
           
            //interface settings
            this.Show();
            this.Activate();
            this.CenterToScreen();
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;

            notifyIcon1.Visible = false;

            //check if device use baterry or not
            checkBatterySystem();

            string strName = Environment.UserName;

            txtNum.AppendText("\nLINE");
            txtNum.AppendText("\n\n\n\n");
            txtMain.AppendText("┎━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┒\n");
            txtMain.AppendText("                                                                           WELCOME TO ELYSIUM: "+strName.ToUpper()+"\n");
            txtMain.AppendText("┖━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┚\n");

            txtMain.AppendText("\n\nPress Enter to start or ESC to exit...");
   
            txtCMD.Select();
            updateLines();

            //check default windows folder path
            windir = Environment.GetEnvironmentVariable("WINDIR");

            // Modifier keys codes: Alt = 1, Ctrl = 2, Shift = 4, Win = 8
            // Compute the addition of each combination of the keys you want to be pressed
            // ALT+CTRL = 1 + 2 = 3 , CTRL+SHIFT = 2 + 4 = 6...
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_F5, 0, (int)Keys.F5);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_F6, 0, (int)Keys.F6);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_F7, 0, (int)Keys.F7);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_F8, 0, (int)Keys.F8);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_F9, 0, (int)Keys.F9);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_F10, 0, (int)Keys.F10);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_F11, 0, (int)Keys.F11);
            RegisterHotKey(this.Handle, MYACTION_HOTKEY_ID_F4, 0, (int)Keys.F4);
        }

        [DllImport("user32.dll")] //turn off caps log
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags,
        UIntPtr dwExtraInfo);

        [DllImport("user32")]
        public static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        [DllImport("user32")]
        public static extern void LockWorkStation();

        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        static extern uint SHEmptyRecycleBin(IntPtr hwnd, string rootPath,
                                             BinFlags flags);


        enum BinFlags : uint
        {
            SHERB_NOCONFIRMATION = 0x00000001,
            SHERB_NOPROGRESSUI = 0x00000002,
            SHERB_NOSOUND = 0x00000004
        }

        // Speech recognition variables
        SpeechSynthesizer sSynth = new SpeechSynthesizer();
        PromptBuilder pBuilder = new PromptBuilder();
        SpeechRecognitionEngine sRecognize = new SpeechRecognitionEngine();
        Choices sList = new Choices();

        string windir;

        System.Diagnostics.Process prc = new System.Diagnostics.Process();

        private Icon ico;

        int line = 1000;
        bool speechActiveOrNot = true;
        bool hideOrNot = false;
        bool appClose = false;
        bool batteryWarning = false;
        bool batterySystemYes = false;
        bool login = false;

        //Elysium files path
        string newsPath = @"news.dat";
        string mailPath = @"email.dat";
        string weatherPath = @"weather.dat";
        string userPath = @"primary-commands.txt";
        string desktopPath = @"desktop.dat";
        string musicsPath = @"musics.dat";
        string customCommandsFolderPath = @"custom-commands";
        string defaultPath = @"default-commands.dat";

        //battery vars
        string strBatteryChargeStatus;
        int batteryLifePercent;

        //voice commands lists
        List<string> defaultCommands = new List<string>();

        List<string> generalUserCommands = new List<string>();
        List<string> generalUserPaths = new List<string>();

        List<string> desktopUserCommands = new List<string>();
        List<string> desktopUserPaths = new List<string>();
        string wordToActiveDesktopCommands = "open";

        List<string> musicUserCommands = new List<string>();
        List<string> musicUserPaths = new List<string>();
        string wordToActiveMusicCommands = "play";

        List<string> customUserCommands = new List<string>();
        List<string> customUserPaths = new List<string>();



        // DLL libraries used to manage and control hotkeys
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        const int MYACTION_HOTKEY_ID_F5 = 1;
        const int MYACTION_HOTKEY_ID_F6 = 2;
        const int MYACTION_HOTKEY_ID_F7 = 3;
        const int MYACTION_HOTKEY_ID_F8 = 4;
        const int MYACTION_HOTKEY_ID_F9 = 5;
        const int MYACTION_HOTKEY_ID_F10 = 6;
        const int MYACTION_HOTKEY_ID_F11 = 7;
        const int MYACTION_HOTKEY_ID_F4 = 8;

        //Key Control:
        /*
          F4 -> Hide/Appear Elysium Interface
          F6 -> Previous music track
          F7 -> Play/Pause Media
          F8 -> Next music track
          F9 -> Volume down
          F10 -> volume up
          F11 -> Volume mute
        */
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_F5)
            {
                if (speechActiveOrNot == false)
                {
                    speekEnviromentStart();
                    writeLine("\n" + "ELYSIUM STARTED");
                    updateLines();
                }
                else
                {
                    speekEnviromentStop();
                    writeLine("\n" + "ELYSIUM STOPED");
                    updateLines();
                }
            }
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_F6)
            {
                InputSimulator.SimulateKeyPress(VirtualKeyCode.MEDIA_PREV_TRACK);
            }
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_F7)
            {
                InputSimulator.SimulateKeyPress(VirtualKeyCode.MEDIA_PLAY_PAUSE);
            }
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_F8)
            {
                InputSimulator.SimulateKeyPress(VirtualKeyCode.MEDIA_NEXT_TRACK);
            }
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_F9)
            {
                for (int i = 0; i < 5; i++)
                {
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VOLUME_DOWN);
                }
            }
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_F10)
            {
                for (int i = 0; i < 5; i++)
                {
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VOLUME_UP);
                }
            }
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_F11)
            {
                InputSimulator.SimulateKeyPress(VirtualKeyCode.VOLUME_MUTE);
            }
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID_F4)
            {
                if (hideOrNot == true)
                {
                    hideOrNot = false;
                    this.Show();
                    this.Activate();
                    this.CenterToScreen();
                    notifyIcon1.Visible = false;
                }
                else
                {
                    hideOrNot = true;
                    this.Hide();
                    notifyIcon1.Visible = true;
                }
            }
            base.WndProc(ref m);
        }


        // ============================================== SCROOL RICH TEXT BOX ==============================================================
        // constant definitions / scroll settings
        private const uint WM_HSCROLL = 0x0114;
        private const uint WM_VSCROLL = 0x0115;
        private const uint SB_LINEUP = 0;
        private const uint SB_LINELEFT = 0;
        private const uint SB_LINEDOWN = 1;
        private const uint SB_LINERIGHT = 1;
 
        [DllImport("User32")]
        private static extern uint SendMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

        // ================================================= End Scrool Rich Text Box ==========================

        private void speekEnviroment()
        {
            speechActiveOrNot = true;
            Grammar gr = new Grammar(new GrammarBuilder(sList));
            try
            { 
                sRecognize.RequestRecognizerUpdate();
                sRecognize.LoadGrammar(gr);
                sRecognize.SpeechRecognized += sRecognize_SpeechRecognized;
                sRecognize.SetInputToDefaultAudioDevice();
                sRecognize.RecognizeAsync(RecognizeMode.Multiple);
            }

            catch
            {
                return;
            }

        }

        private void speekEnviromentStop() //stop listen
        {
            speechActiveOrNot = false;
            sRecognize.RecognizeAsyncStop();
        }

        private void speekEnviromentStart() //start listen
        {
            speechActiveOrNot = true;
            sRecognize.RecognizeAsync(RecognizeMode.Multiple);
        }

        //Voice to word recognition function
        private void sRecognize_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            writeLine("\n     ORD " + e.Result.Text.ToString());
            updateLines();
            commandsElysium(e.Result.Text.ToString());
        }


        // word to voice command function
        private void commandsElysium(string command)
        {
            if(searchCommandInFile(command))
            {
                switch(getCommandCode(command))
                {
                    case "AAA1":
                        playSound(@"sounds\AAA1.wav");

                        System.Threading.Thread.Sleep(1000);

                        appClose = true;
                        Application.Exit();
                        break;

                    case "AAA2":
                        playSound(@"sounds\AAA1.wav");
                        System.Threading.Thread.Sleep(1000);
                        appClose = true;
                        Application.Exit();
                        break;

                case "AAA3":
                    playSound(@"sounds\AAA1.wav");

                    System.Threading.Thread.Sleep(1000);
                    appClose = true;
                    Application.Exit();
                    break;

                case "AAA4":
                    txtMain.Clear();
                    txtNum.Clear();
                    txtCMD.Clear();
                    break;

                case "AAA5":
                    txtMain.Clear();
                    txtNum.Clear();
                    txtCMD.Clear();
                    break;

                case "AAA6":
                    hideOrNot = true;
                    this.Hide();
                    notifyIcon1.Visible = true;
                    cmdAcomp(command);
                    break;

                case "AAA7":
                    hideOrNot = false;
                    this.Show();
                    this.Activate();
                    this.CenterToScreen();
                    notifyIcon1.Visible = false;
                    cmdAcomp(command);
                    break;

                case "AAA9":
                    spaceLine(2);
                    writeLine("\n" + "==================   COMMANDS LIST   ==================");
                    spaceLine(1);

                    if (defaultCommands.Count > 0)
                    {
                        writeLine("\n" + "-----------------   Default Commands  -----------------");
                        spaceLine(1);
                    }

                    foreach (string txt in defaultCommands)
                    {
                        writeLine("\n-->" + txt);
                    }

                    if (desktopUserCommands.Count > 0)
                    {
                        spaceLine(1);
                        writeLine("\n" + "-----------------   Desktop Commands  -----------------");
                        spaceLine(1);
                    }

                    foreach (string txt in desktopUserCommands)
                    {
                        writeLine("\n-->" + txt);
                    }

                    if (musicUserCommands.Count > 0)
                    {
                        spaceLine(1);
                        writeLine("\n" + "-----------------   Music Commands  -----------------");
                        spaceLine(1);
                    }

                    foreach (string txt in musicUserCommands)
                    {
                        writeLine("\n-->" + txt);
                    }

                    if (generalUserCommands.Count > 0)
                    {
                        spaceLine(1);
                        writeLine("\n" + "-----------------   Primary Commands  -----------------");
                        spaceLine(1);
                    }

                    foreach (string txt in generalUserCommands)
                    {
                        writeLine("\n-->" + txt);
                    }

                    if (customUserCommands.Count > 0)
                    {
                        spaceLine(1);
                        writeLine("\n" + "-----------------   Custom Commands  -----------------");
                        spaceLine(1);
                    }

                    foreach (string txt in customUserCommands)
                    {
                        writeLine("\n-->" + txt);
                    }

                    spaceLine(2);
                    cmdAcomp(command);
                    playSound(@"sounds\AAA9.wav");
                    break;

                case "AAB3":
                    SetStartup(true);
                    cmdAcomp(command); 
                    break;

                case "AAB4":
                    SetStartup(false);
                    cmdAcomp(command);
                    break;

                case "AAB5":
                    elysiumRestart(5);
                    break;

                case "AAB6":
                activeReference(desktopPath, @"sounds\AAB6.wav");
                cmdAcomp(command);
                break;

                case "AAB7":
                activeReference(musicsPath, @"sounds\AAB7.wav");
                cmdAcomp(command);
                break;

                case "AAB8":
                activeReference(userPath, @"sounds\AAB8.wav");
                cmdAcomp(command);
                break;

                case "AAB9":
                if (Directory.Exists(customCommandsFolderPath))
                {
                    playSound(@"sounds\cmdAlreadyActive.wav");
                }
                else
                {
                    replaceReferenceFolder(customCommandsFolderPath);
                    playSound(@"sounds\AAB9.wav");
                    writeLine("\n" + "you must restart Elysium to apply these changes !");
                }
                cmdAcomp(command);               
                break;

                case "AAC0":
                        /*
                activeReference(defaultPath);
                cmdAcomp(command);
                         * */
                break;

                case "AAC1":
                blockReference(desktopPath, @"sounds\AAC1.wav");
                cmdAcomp(command);
                break;

                case "AAC2":
                blockReference(musicsPath, @"sounds\AAC2.wav");
                cmdAcomp(command);
                break;

                case "AAC3":
                blockReference(userPath, @"sounds\AAC3.wav");
                cmdAcomp(command);
                break;

                case "AAC4":
                blockReferenceFolder(customCommandsFolderPath);
                cmdAcomp(command);
                break;

                case "AAC5":
                /*
                blockReference(defaultPath);
                cmdAcomp(command);
                 */
                break;

                case "AAC6":
                addUserCommand();
                cmdAcomp(command);
                    break;

                case "AAC7":
                    addCustomCommand();
                    cmdAcomp(command);
                    break;

                case "AAC8":
                    changeCommand(userPath, "primary");
                    cmdAcomp(command);
                    break;

                case "AAC9":
                    changeCommand(defaultPath, "default");
                    cmdAcomp(command);
                    break;

                case "AAD0":
                    changeCustomCommand();
                    cmdAcomp(command);
                    break;

                case "AAD1":
                    deleteUserCommand();
                    cmdAcomp(command);
                    break;

                case "AAD2":
                    deleteCustomCommand();
                    cmdAcomp(command);
                    break;
                        
                case "AAD3":
                    try
                    {
                        Process.Start(customCommandsFolderPath);
                    }
                    catch
                    {
                    }
                    cmdAcomp(command);
                    break;

                case "AAD4":
                    try
                    {
                        Process.Start(userPath);
                    }
                    catch
                    {
                    }
                    cmdAcomp(command);
                    break;
                         

                case "ABA2":
                   
                    txtMain.Select();
                    string content;
                    int linesCount = txtMain.Lines.Count();

                    writeLine("\n" + "CMD shutdown in 5 seconds.");

                    updateLines();
                    for (int a = 5; a >= 0; a--)
                    {
                        content = txtMain.Lines[linesCount]; 
                        txtMain.Find(content);
                        txtMain.SelectedText = "CMD shutdown in "+a.ToString() + " seconds.";
                        System.Threading.Thread.Sleep(1000);
                    }
                    Process.Start("shutdown", "/s /t 0");
                    break;

                case "ABA3":
                    Process.Start("shutdown", "/s /t 10");
                    cmdAcomp(command);
                    break;

                case "ABA5":
                    Process.Start("shutdown", "/a");
                    cmdAcomp(command);
                    break;

                case "ABA6":
                    getBatteryPercentage();
                    cmdAcomp(command);
                    break;

                case "ABA7":
                    getBatteryLevel();
                    cmdAcomp(command);
                    break;

                case "ABA0":
                    Process.Start("shutdown", "/r /t 10");
                    cmdAcomp(command);
                    break;

                case "ABA9":
                    SendKeys.Send("%{F4}");
                    cmdAcomp(command);
                    break;

                case "ABB9":
                    try
                    {
                        var result = SHEmptyRecycleBin(IntPtr.Zero, null, 0);
                    }
                    catch//Exception e
                    {
                        //Handle it the way you like it
                    }
                    break;

                case "ACA1":
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_W);
                    cmdAcomp(command);
                    break;

                case "ACA3":
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_X);
                    cmdAcomp(command);
                    break;

                case "ACA5":
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
                    cmdAcomp(command);
                    break;

                case "ACA6":
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V);
                    cmdAcomp(command);
                    break;

                case "ACA2":
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_Z);
                    cmdAcomp(command);
                    break;

                case "ACA7":
                    SendKeys.Send("{ENTER}");
                    cmdAcomp(command);
                    break;

                case "ACC0":
                    InputSimulator.SimulateKeyDown(VirtualKeyCode.CAPITAL);
                    cmdAcomp(command);
                    break;

                case "ACA8":
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.ESCAPE);
                    //InputSimulator.SimulateKeyDown(VirtualKeyCode.LWIN);
                    cmdAcomp(command);
                    break;

                case "ABB0":
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.VK_P);
                    cmdAcomp(command);
                    break;

                case "ABB1":
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.VK_K);
                    cmdAcomp(command);
                    break;

                case "ABB2":
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.VK_I);
                    cmdAcomp(command);
                    break;

                case "ABB3":
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.VK_C);
                    cmdAcomp(command);
                    break;

                case "ADA1":
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.MEDIA_PLAY_PAUSE);
                    cmdAcomp(command);
                    break;

                case "ADA2":
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.MEDIA_PLAY_PAUSE);
                    cmdAcomp(command);
                    break;

                case "ADA3":
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.MEDIA_STOP);
                    cmdAcomp(command);
                    break;

                case "ADA4":
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.MEDIA_NEXT_TRACK);
                    cmdAcomp(command);
                    break;

                case "ADA5":
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.MEDIA_PREV_TRACK);
                    cmdAcomp(command);
                    break;

                case "ACB0":
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.DOWN);
                    cmdAcomp(command);
                    break;

                case "ACB1":
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.UP);
                    cmdAcomp(command);
                    break;

                case "ACB2":
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.RIGHT);
                    cmdAcomp(command);
                    break;

                case "ACB3":
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.LEFT);
                    cmdAcomp(command);
                    break;

                case "ADA6":
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VOLUME_MUTE);
                    cmdAcomp(command);
                    break;

                case "ADA7":
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VOLUME_MUTE);
                    cmdAcomp(command);
                    break;

                case "ADA8":
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.VOLUME_MUTE);
                    cmdAcomp(command);
                    break;

                case "ADA9":
                    for (int i = 0; i < 10; i++)
                    {
                        InputSimulator.SimulateKeyPress(VirtualKeyCode.VOLUME_UP);
                    }
                    cmdAcomp(command);
                    break;

                case "ADB0":
                    for (int i = 0; i < 10; i++)
                    {
                        InputSimulator.SimulateKeyPress(VirtualKeyCode.VOLUME_DOWN);
                    }
                    cmdAcomp(command);
                    break;

                case "ACB4":
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.BROWSER_SEARCH);
                    cmdAcomp(command);
                    break;

                case "ACB5":
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.BROWSER_SEARCH);
                    cmdAcomp(command);
                    break;

                case "ACB6":
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.DOWN);
                    //SendKeys.Send("%( N)");
                    cmdAcomp(command);
                    break;

                case "ACB7":
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.UP);
                    //SendKeys.Send("%( X)");
                    cmdAcomp(command);
                    break;

                case "ACC9":
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.RIGHT);
                    //SendKeys.Send("%( X)");
                    cmdAcomp(command);
                    break;

                case "ACD0":
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.LEFT);
                    //SendKeys.Send("%( X)");
                    cmdAcomp(command);
                    break;

                case "ABB6":
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.VK_R);
                    cmdAcomp(command);
                    break;

                case "ABB7":
                    Process.Start("control.exe", null);
                    cmdAcomp(command);
                    break;

                case "AEB1":
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.VK_E);
                    cmdAcomp(command);
                    break;

                case "ACB9":
                    InputSimulator.SimulateKeyPress(VirtualKeyCode.DELETE);
                    cmdAcomp(command);
                    break;

                case "ABB5":
                    SendKeys.Send("^+{ESC}");
                    cmdAcomp(command);
                    break;

                case "ACB8":
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_S);
                    cmdAcomp(command);
                    break;

                case "ACA4":
                    SendKeys.Send("{ESC}");
                    cmdAcomp(command);
                    break;

                case "AGA3":
                    Process.Start("http://");
                    cmdAcomp(command);
                    break;

                case "ACC1":
                    SendKeys.Send("{PGUP}");
                    cmdAcomp(command);
                    break;

                case "ACC2":
                    SendKeys.Send("{PGDN}");
                    cmdAcomp(command);
                    break;

                case "ACC3":
                    SendKeys.Send("{PGUP}");
                    cmdAcomp(command);
                    break;

                case "ACC4":
                    SendKeys.Send("{PGDN}");
                    cmdAcomp(command);
                    break;

                case "ACC5":
                    SendKeys.Send("{NUMLOCK}");
                    cmdAcomp(command);
                    break;

                case "ACC6":
                    SendKeys.Send("{SCROLLLOCK}");
                    cmdAcomp(command);
                    break;

                case "ACC7":
                    SendKeys.Send("{F5}");
                    cmdAcomp(command);
                    break;

                case "ACC8":
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.LWIN, VirtualKeyCode.VK_D);
                    cmdAcomp(command);
                    break;

                case "AEA1":
                    string startmenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
                    prc.StartInfo.FileName = windir + @"\explorer.exe";
                    prc.StartInfo.Arguments = startmenu;
                    prc.Start();
                    cmdAcomp(command);
                    break;

                case "AEA2":
                    string startup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                    prc.StartInfo.FileName = windir + @"\explorer.exe";
                    prc.StartInfo.Arguments = startup;
                    prc.Start();
                    cmdAcomp(command);
                    break;

                case "AEA3":
                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    prc.StartInfo.FileName = windir + @"\explorer.exe";
                    prc.StartInfo.Arguments = desktop;
                    prc.Start();
                    cmdAcomp(command);
                    break;


                case "AEA4":
                    string internetcache = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);
                    prc.StartInfo.FileName = windir + @"\explorer.exe";
                    prc.StartInfo.Arguments = internetcache;
                    prc.Start();
                    cmdAcomp(command);
                    break;

                case "AEA5":
                    string mycomputer = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
                    prc.StartInfo.FileName = windir + @"\explorer.exe";
                    prc.StartInfo.Arguments = mycomputer;
                    prc.Start();
                    cmdAcomp(command);
                    break;

                case "AEA6":
                    string mydocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    prc.StartInfo.FileName = windir + @"\explorer.exe";
                    prc.StartInfo.Arguments = mydocuments;
                    prc.Start();
                    cmdAcomp(command);
                    break;

                case "AEA7":
                    string mymusic = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                    prc.StartInfo.FileName = windir + @"\explorer.exe";
                    prc.StartInfo.Arguments = mymusic;
                    prc.Start();
                    cmdAcomp(command);
                    break;

                case "AEA8":
                    string mypictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                    prc.StartInfo.FileName = windir + @"\explorer.exe";
                    prc.StartInfo.Arguments = mypictures;
                    prc.Start();
                    cmdAcomp(command);
                    break;

                case "AEA9":
                    string myvideos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                    prc.StartInfo.FileName = windir + @"\explorer.exe";
                    prc.StartInfo.Arguments = myvideos;
                    prc.Start();
                    cmdAcomp(command);
                    break;


                case "AEB0":
                    string programs = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    prc.StartInfo.FileName = windir + @"\explorer.exe";
                    prc.StartInfo.Arguments = programs;
                    prc.Start();
                    cmdAcomp(command);
                    break;

                case "AGA1":
                    Process.Start("calc.exe");
                    cmdAcomp(command);
                    break;

                case "AGA2":
                    Process.Start("mspaint.exe");
                    cmdAcomp(command);
                    break;

                case "AGA4":
                    Process.Start("notepad.exe");
                    cmdAcomp(command);
                    break;

                case "AFA2":
                    string weatherL = null;
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_L);
                    System.Threading.Thread.Sleep(1000);
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
                    System.Threading.Thread.Sleep(1000);
                    weatherL = Clipboard.GetText();
                    weatherL = Clipboard.GetText();
                    if (weatherL != null)
                    {
                        if (System.IO.File.Exists(weatherPath))
                        {
                            StreamWriter wr = new StreamWriter(weatherPath);
                            wr.WriteLine(weatherL);
                            wr.Close();
                            writeLine("\n " + "file: " + weatherPath + " override with " + weatherL);
                        }
                        else
                        {
                            System.IO.File.Create(weatherPath);
                            StreamWriter wr = new StreamWriter(weatherPath);
                            wr.WriteLine(weatherL);
                            wr.Close();
                            writeLine("\n " + "file: " + weatherPath + " created with " + weatherL);
                        }
                        playSound(@"sounds\weatherSaved.wav");
                    }
                    else
                    {
                        writeLine("\n " + "weather save error !");
                    }
                    cmdAcomp(command);
                    break;

                case "AFA3":
                    string newsLink = null;
                    if (System.IO.File.Exists(newsPath))
                    {
                        StreamReader rd = new StreamReader(newsPath);
                        newsLink = rd.ReadLine();
                        rd.Close();
                        writeLine("\n " + "file: " + newsPath + " readed.");
                        if(newsLink != null)
                        {
                            try
                            {
                                Process.Start(newsLink);
                            }
                            catch
                            { }
                        }
                        else
                        {
                            playSound(@"sounds\mustSaveNews.wav");
                            Process.Start("https://www.google.com/");
                        }

                    }else
                    {
                        playSound(@"sounds\mustSaveNews.wav");
                        System.IO.File.Create(newsPath);
                        Process.Start("https://www.google.com/");
                    }
                    cmdAcomp(command);
                    break;

                case "AFA4":
                    string newsL = null;
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_L);
                    System.Threading.Thread.Sleep(1000);
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
                    System.Threading.Thread.Sleep(1000);
                    newsL = Clipboard.GetText();
                    newsL = Clipboard.GetText();
                    if (newsL != null)
                    {
                        if (System.IO.File.Exists(newsPath))
                        {
                            StreamWriter wr = new StreamWriter(newsPath);
                            wr.WriteLine(newsL);
                            wr.Close();
                            writeLine("\n " + "file: " + newsPath + " override with " + newsL);
                        }
                        else
                        {
                            System.IO.File.Create(newsPath);
                            StreamWriter wr = new StreamWriter(newsPath);
                            wr.WriteLine(newsL);
                            wr.Close();
                            writeLine("\n " + "file: " + newsPath + " created with " + newsL);
                        }
                        playSound(@"sounds\newsSaved.wav");
                    }
                    else 
                    {
                        writeLine("\n " + "news save error !");
                    }
                    cmdAcomp(command);
                    break;

                case "AFA6":
                    string mailLink = null;
                    if (System.IO.File.Exists(mailPath))
                    {
                        StreamReader rd = new StreamReader(mailPath);
                        mailLink = rd.ReadLine();
                        rd.Close();
                        writeLine("\n " + "file: " + mailPath + " readed.");
                        if (mailLink != null)
                        {
                            try
                            {
                                Process.Start(mailLink);
                            }
                            catch { }
                        }
                        else
                        {
                            playSound(@"sounds\mustSaveMail.wav");
                            Process.Start("https://www.google.com/");
                        }

                    }
                    else
                    {
                        playSound(@"sounds\mustSaveMail.wav");
                        System.IO.File.Create(mailPath);
                        Process.Start("https://www.google.com/");
                        writeLine("\n " + "file: " + mailPath + " created.");
                    }
                    cmdAcomp(command);
                    break;

                case "AFA7":
                    string mailL = null;
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_L);
                    System.Threading.Thread.Sleep(1000);
                    InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
                    System.Threading.Thread.Sleep(1000);
                    mailL = Clipboard.GetText();
                    mailL = Clipboard.GetText();
                    if (mailL != null)
                    {
                        if (System.IO.File.Exists(mailPath))
                        {
                            StreamWriter wr = new StreamWriter(mailPath);
                            wr.WriteLine(mailL);
                            wr.Close();
                            writeLine("\n " + "file: " + mailPath + " override with " + mailL);
                        }
                        else
                        {
                            System.IO.File.Create(mailPath);
                            StreamWriter wr = new StreamWriter(mailPath);
                            wr.WriteLine(mailL);
                            wr.Close();
                            writeLine("\n " + "file: " + mailPath + " created with " + mailL);
                        }
                        playSound(@"sounds\mailSaved.wav");
                    }
                    else 
                    {
                        writeLine("\n " + "mail save error !");
                    }
                    cmdAcomp(command);
                    break;

                case "AFA1":
                    string weatherLink = null;
                    if (System.IO.File.Exists(weatherPath))
                    {
                        StreamReader rd = new StreamReader(weatherPath);
                        weatherLink = rd.ReadLine();
                        rd.Close();
                        writeLine("\n " + "file: " + weatherPath + " readed.");
                        if (weatherLink != null)
                        {
                            try
                            {
                                Process.Start(weatherLink);
                            }
                            catch { }
                        }
                        else
                        {
                            playSound(@"sounds\mustSaveWeather.wav");
                            Process.Start("https://www.google.com/");
                        }

                    }
                    else
                    {
                        playSound(@"sounds\mustSaveWeather.wav");
                        System.IO.File.Create(weatherPath);
                        Process.Start("https://www.google.com/");
                        writeLine("\n " + "file: " + weatherPath + " created.");
                    }
                    cmdAcomp(command);
                    break;
                        
                case "ADB1":
                    if (Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)).Length != 0)
                    {
                        if (Directory.GetDirectories(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)).Length == 0)
                        {
                            string mymusic2 = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                            prc.StartInfo.FileName = windir + @"\explorer.exe";
                            prc.StartInfo.Arguments = mymusic2;
                            prc.Start();
                            System.Threading.Thread.Sleep(2000);
                            InputSimulator.SimulateModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_A);
                            System.Threading.Thread.Sleep(1000);
                            SendKeys.Send("{ENTER}");
                        }
                        else
                        {

                            string[] musicsMP3 = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic).ToString(), "*.mp3");
                            string[] musicsWAV = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic).ToString(), "*.wav");
                            string mymusic2 = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                            if ((musicsMP3.Length > 1) || musicsWAV.Length > 1)
                            {
                                if (musicsMP3.Length > 1)
                                {
                                    Random random = new Random();
                                    int randomNumber = random.Next(0, musicsMP3.Length);
                                    Process.Start(musicsMP3[randomNumber].ToString());
                                }
                                else
                                {
                                    Random random = new Random();
                                    int randomNumber = random.Next(0, musicsWAV.Length);
                                    Process.Start(musicsMP3[randomNumber].ToString());
                                }
                            }
                            else
                            {
                                playSound(@"sounds\notHaveMusics.wav");
                            }
                        }
                         
                    }
                    else
                    {
                        playSound(@"sounds\notHaveMusics.wav");
                    }
                        
                    cmdAcomp(command);
                    break;

                case "ABB4":
                    ExitWindowsEx(0, 0);
                    cmdAcomp(command);
                    break;

                case "ABA1":
                    LockWorkStation(); 
                    cmdAcomp(command);
                    break;

                case "AAB1":
                    speekEnviromentStop();
                    cmdAcomp(command);
                    playSound(@"sounds\AAB1.wav");
                    break;

                        /* EMPTY COMMAND TO FILL IN FUTURE
                case "AAB2":
                    cmdAcomp(command);
                    break;
                         */

                case "AGA5":
                    if (Control.IsKeyLocked(Keys.CapsLock)) // Checks Capslock is on
                    {
                        const int KEYEVENTF_EXTENDEDKEY = 0x1;
                        const int KEYEVENTF_KEYUP = 0x2;
                        keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
                        keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                        (UIntPtr)0);
                    }
                    if (Control.IsKeyLocked(Keys.CapsLock)) // Checks Capslock is on
                    {
                        const int KEYEVENTF_EXTENDEDKEY = 0x1;
                        const int KEYEVENTF_KEYUP = 0x2;
                        keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
                        keybd_event(0x14, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP,
                        (UIntPtr)0);
                    }
                        cmdAcomp(command);
                    break;

                case "AGA6":
                    SendKeys.Send("{SCROLLLOCK}");
                    cmdAcomp(command);
                    break;

                case  "AGA7":
                    writeLine("\nprogrammed by: Bernardo Ascensão - Version: 1.0");
                    spaceLine(1);
                    updateLines();
                    break;

                default:
                    updateLines();
                    break;
                }
            }
            // END OF WORD TO VOICE COMMAND FUNCTION

            //search voice command in primary commands file
            int currentIndex = 0;
            for (int i = 0; i < generalUserCommands.Count; i++)
            {
                if(generalUserCommands[currentIndex]==command)
                {
                    try
                    {
                        Process.Start(generalUserPaths[currentIndex]);
                    }
                    catch { }
                    cmdAcomp(command);
                }
                currentIndex++;
            }

            //search voice command in desktop (if voicd command == some desktop file name without extencion and especial characters)
            int currentIndex2 = 0;
            for (int i = 0; i < desktopUserCommands.Count; i++)
            {
                if (desktopUserCommands[currentIndex2] == command)
                {
                    try
                    {
                        Process.Start(desktopUserPaths[currentIndex2]);
                    }
                    catch { }
                    cmdAcomp(command);
                }
                currentIndex2++;
            }

            //search voice command in default music folder (if voice command == music name)
            int currentIndex3 = 0;
            for (int i = 0; i < musicUserCommands.Count; i++)
            {
                if (musicUserCommands[currentIndex3] == command)
                {
                    try
                    {
                        Process.Start(musicUserPaths[currentIndex3]);
                    }
                    catch { }
                    cmdAcomp(command);
                }
                currentIndex3++;
            }
            
            //search voice command in custom command file
            int currentIndex4 = 0;
            for (int i = 0; i < customUserCommands.Count; i++)
            {
                if (customUserCommands[currentIndex4] == command)
                {
                    try
                    {
                        using (StreamReader sr = new StreamReader(customUserPaths[currentIndex4]))
                        {
                            string line;
                            int lineCount = 1;
                            while ((line = sr.ReadLine()) != null)
                            {
                                if (lineCount == 1)
                                {
                                    if (line.Contains(".wav"))
                                    {
                                        playSound(line);
                                    }
                                    else
                                    {
                                        try
                                        {
                                            Process.Start(line);
                                        }
                                        catch { }
                                    }
                                }
                            }

                        }
                    }

                    catch (Exception e)
                    {
                        writeLine("Error in reading custom commands: " + e.ToString());
                    }
                    /*
                    Process.Start(musicUserPaths[currentIndex3]);
                    cmdAcomp(command);*/
                }
                currentIndex4++;
            }
            if (command == "core shutdown") //if default commands are deleted or microphone turned of this commands is useful
            {
                appClose = true;
                Application.Exit();
            }

            //scrool text box´s
            SendMessage(this.txtMain.Handle, WM_VSCROLL, SB_LINEDOWN, 0);
            SendMessage(this.txtNum.Handle, WM_VSCROLL, SB_LINEDOWN, 0);
        }

        private void cmdAcomp(string cmd) //notify user when voice command is accomplish
        {
            writeLine("\n          CMD " + cmd + " accomplished");
            spaceLine(1);
            updateLines();
        }

        private void updateLines() // update interface after accomplish command
        {
            txtMain.SelectionStart = txtMain.Text.Length;
            txtMain.ScrollToCaret();
            txtNum.SelectionStart = txtNum.Text.Length;
            txtNum.ScrollToCaret(); 
        }

        private void readUserReference()
        {
            writingDelayEfect("\n" + "loading primary commands...");

            if (System.IO.File.Exists(userPath) == true)
            {
                try
                {
                    int lineCount = System.IO.File.ReadLines(userPath).Count();
                    int lineLocalization = 0;

                    if (lineCount != 0)
                    {
                        if (lineCount % 2 == 0)
                        {

                            int arraySize = lineCount/2;
                            string[] userCommands = new string[arraySize];
                            string[] userPaths = new string[arraySize];
                            string lineContent = null;
                            int localCommand = 0;
                            int localPath = 0;
                            
                            using (StreamReader sr = new StreamReader(userPath))
                            {
                                while (sr.Peek() >= 0)
                                {
                                    lineContent = sr.ReadLine();

                                            if (lineLocalization % 2 == 0)
                                            {
                                                if (localCommand < userCommands.Length)
                                                {
                                                    userCommands[localCommand] = lineContent;
                                                    localCommand++;
                                                }
                                            }
                                            else
                                            {
                                                if (localPath < userPaths.Length)
                                                {
                                                    userPaths[localPath] = lineContent;
                                                    localPath++;
                                                }

                                            }
                                        
                                    lineLocalization++;

                                }
                                sr.Close();
                            }

                            sList.Add(userCommands);
                            generalUserCommands.AddRange(userCommands);
                            generalUserPaths.AddRange(userPaths);
                            writingDelayEfect("\n     primary commands loaded.");
                        }
                        else
                        {
                            writingDelayEfect("\n" + "     file primary-commands.txt corrupted.");
                            writingDelayEfect("\n" + "     you need fix primary-commands.txt and say the command 'delete user commands' and next say 'active user commands.'");
                            try
                            {
                                Process.Start(userPath);
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        writingDelayEfect("\n" + "     file primary-commands.txt empty.");
                    }

                    
                }
                catch (Exception e)
                {
                    writingDelayEfect("\n     " + e.ToString());
                }

            }else
            {
                writingDelayEfect("\n " + "     primary commands not active.");
            }
            spaceLine(2);
        }

        //read name of desktop files and add to elysium voice command list
        private void readDesktopReferences()
        {

            writingDelayEfect("\n" + "loading desktop commands...");

            if (System.IO.File.Exists(desktopPath)) //@"desktop.dat"
            {
                string[] array1 = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory));
                string[] array2 = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

                List<string> desktopItemsPath = new List<string>();
                List<string> desktopItemsCommands = new List<string>();

                StringBuilder stringWork = new StringBuilder();

                desktopItemsPath.AddRange(array1);
                desktopItemsPath.AddRange(array2);

                if ((array1.Length > 0) || (array2.Length > 0))
                {
                }
                else
                {
                    writingDelayEfect("\n" + "     you not have desktop items !");
                }

                foreach (string desktopItems in desktopItemsPath)
                {
                    if (desktopItems.Contains("desktop.ini") != true) //nao mostra o ficheciro desktop.ini
                    {
                        stringWork.Append(desktopItems);
                        desktopUserPaths.Add(stringWork.ToString());


                        stringWork.Remove(desktopItems.Length - 4, 4); //remove file extensions

                        //Get File Name
                        string y = stringWork.ToString();
                        stringWork.Clear();
                        string[] words = y.Split('\\');
                        stringWork.Append(words[words.Length - 1]);

                        desktopUserCommands.Add(wordToActiveDesktopCommands + " " + stringWork.ToString());

                    }
                    stringWork.Clear();
                }

                sList.Add(desktopUserCommands.ToArray());
                writingDelayEfect("\n" + "     desktop commands loaded.");
            }
            else
            {
                writingDelayEfect("\n" + "     desktop commands not active");
            }
            spaceLine(2);
        }

        //read all music files from music folder and add to elysium commands
        private void readMusicReferences()
        {

            /* use this code in future versions to increase Elysium AI
             
             string[] ExtentionstemsPath = musicItemsPath.count;
             
             for (int i = 0; i < musicItemsPath.Count; i++)
             {
                ExtentionstemsPath[i] = Path.GetExtension(musicItemsPath[i])
             }
             
             for (int i = 0; i < musicItemsPath.Count; i++)
             {
                        if ((ExtentionstemsPath[i] == ".mp3") || (ExtentionstemsPath[i] == ".wav") || (ExtentionstemsPath[i] == ".wma"))
                        {
                            stringWork.Append(musicItems);
                            musicUserPaths.Add(stringWork.ToString());


                            stringWork.Remove(musicItems.Length - 4, 4);
                            string y = stringWork.ToString().Replace(" - ", " ");
                            stringWork.Clear();
                            string[] words = y.Split('\\');
                            stringWork.Append(words[words.Length - 1]);
                            line++;
                            txtNum.AppendText("\n" + line.ToString());
                            txtMain.AppendText("\n--> " + RemoveSpecialCharacters(stringWork.ToString()));

                            musicUserCommands.Add(wordToActiveMusicCommands + " " + stringWork.ToString());
                        }
                        
                    }
                    stringWork.Clear();
             }
             * */
            writingDelayEfect("\n" + "loading music commands...");
            if (System.IO.File.Exists(musicsPath)) //@"musics.dat"
            {
                string[] musicItemsPath = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), "*.*", SearchOption.AllDirectories);
                StringBuilder stringWork = new StringBuilder();

                if (musicItemsPath.Length > 0)
                {
                }
                else
                {
                    writingDelayEfect("\n" + "     you not have musics in Music Folder !");
                }

                foreach (string musicItems in musicItemsPath)
                {
                    if (musicItems.Contains("desktop.ini") != true) //hide file desktop.ini from desktop list
                    {
                        if ((musicItems.LastIndexOf(".mp3") > -1) || (musicItems.LastIndexOf(".wav") > -1) || (musicItems.LastIndexOf(".wma") > -1))
                        {
                            stringWork.Append(musicItems);
                            musicUserPaths.Add(stringWork.ToString());


                            stringWork.Remove(musicItems.Length - 4, 4);
                            string y = stringWork.ToString().Replace(" - ", " ");
                            stringWork.Clear();
                            string[] words = y.Split('\\');
                            stringWork.Append(words[words.Length - 1]);

                            musicUserCommands.Add(wordToActiveMusicCommands + " " + stringWork.ToString());
                        }

                    }
                    stringWork.Clear();
                }
                sList.Add(musicUserCommands.ToArray());
                writingDelayEfect("\n" + "     music commands loaded.");
            }else
            {
                writingDelayEfect("\n" + "     music commands not active.");
            }
            spaceLine(2);
        }

        //read all custom commands files create by user and add to elysium commands
        private void readCustomReferences()
        {
            writingDelayEfect("\n" + "loading custom commands...");

            if (Directory.Exists(customCommandsFolderPath))
            {
                string[] customCommandsItemsPath = Directory.GetFiles(customCommandsFolderPath);
                StringBuilder stringWork = new StringBuilder();

                if(customCommandsItemsPath.Length > 0)
                {
                }else
                {
                    writingDelayEfect("\n" + "     you not have custom commands created !");
                }

                foreach (string customItems in customCommandsItemsPath)
                {
                    if (customItems.Contains("desktop.ini") != true) //nao mostra o ficheciro desktop.ini
                    {
                        stringWork.Append(customItems);
                        customUserPaths.Add(stringWork.ToString());


                        stringWork.Remove(customItems.Length - 4, 4);
                        string y = stringWork.ToString().Replace(" - ", " ");
                        stringWork.Clear();
                        string[] words = y.Split('\\');
                        stringWork.Append(words[words.Length - 1]);
                        //txtMain.AppendText("\n--> " + RemoveSpecialCharacters(stringWork.ToString()));

                        customUserCommands.Add(stringWork.ToString());
                    }
                    stringWork.Clear();
                }
                sList.Add(customUserCommands.ToArray());
                writingDelayEfect("\n" + "     custom commands loaded.");
            }else
            {
                writingDelayEfect("\n " + "     primary commands not active.");
            }
            spaceLine(2);
        }

        //read default commands files and add to elysium commands
        private void loadDefaultReferences()
        {
            string str;

            // Read the file and display it line by line.
            StreamReader file = new StreamReader(defaultPath);
            while ((str = file.ReadLine()) != null)
            {
                defaultCommands.Add(str.Substring(5));
            }

            file.Close();
            sList.Add(defaultCommands.ToArray());
        }

        //load all commands references to elysium
        private void loadReferences()
        {

            readDesktopReferences();


            readMusicReferences();


            readUserReference();


            loadDefaultReferences();


            readCustomReferences();

        }

        private string getCommandCode(string cmd)
        {
            //The comment commands are useful for discovering flaws in code - NOT DELETE
            string code = null;
            string str= null;
            string ret = null;
            StringBuilder stringWork = new StringBuilder();
            // Read the file and display it line by line.
            StreamReader file = new StreamReader(defaultPath);
            while ((str = file.ReadLine()) != null)
            {
                code = str.Substring(0, 4);
                stringWork.Append(str);
                stringWork.Remove(0, 5);
               // txtMain.AppendText("\n" + "STRING WORK: "+stringWork.ToString().TrimEnd() + " CMD: "+ cmd.TrimEnd());
                if (stringWork.ToString().TrimEnd().TrimStart()==cmd.TrimEnd().TrimStart())
                {
                    ret = code;
                    //txtMain.AppendText("\n" + code);
                }
                stringWork.Clear();
                
            }

            file.Close();
            return ret;
        }

        private bool  searchCommandInFile(string cmd)
        {
            bool x=false;
            if (System.IO.File.Exists(defaultPath))
            {
                x = System.IO.File.ReadAllText(defaultPath).Contains(cmd) ? true : false;
            }
            return x;                
        }

        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == ' ') //clean special character in file names
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        
        private void spaceLine(int numberOfLines)
        {
            for (int i = 0; i < numberOfLines; i++)
            {
                writeLine("\n");
            }
        }

        private void txtCMD_KeyDown(object sender, KeyEventArgs e)
        {       
            if(e.KeyCode == Keys.Escape)
            {
                playSound(@"sounds\AAA1.wav");
                appClose = true;
                Application.Exit();
            }

            if (e.KeyCode == Keys.Enter)
            {
                if (login == false)
                {
                            login = true;
                            StringBuilder content = new StringBuilder();
                            content.Append("");

                            txtNum.AppendText("\n");
                            txtMain.AppendText("\n");
                            string txtTemp = txtCMD.Text;
                            txtCMD.Clear();
                            loadReferences();
                            
                            playSound(@"sounds\Welcome.wav");

                            welcomeToElysium();
         
                            speekEnviroment();

                }
                else
                {
                    if (txtCMD.Text != "")
                    {
                        writeLine("\n " + txtCMD.Text);
                        commandsElysium(txtCMD.Text.ToString());
                        txtCMD.Clear();
                        //updateLines();
                        SendMessage(this.txtMain.Handle, WM_VSCROLL, SB_LINEDOWN, 0);
                        SendMessage(this.txtNum.Handle, WM_VSCROLL, SB_LINEDOWN, 0);
                    }
                }
            }

            if(e.KeyCode == Keys.Up)
            {
                SendMessage(this.txtMain.Handle, WM_VSCROLL, SB_LINEUP, 0);
                SendMessage(this.txtNum.Handle, WM_VSCROLL, SB_LINEUP, 0);
            }
            if (e.KeyCode == Keys.Down)
            {
                SendMessage(this.txtMain.Handle, WM_VSCROLL, SB_LINEDOWN, 0);
                SendMessage(this.txtNum.Handle, WM_VSCROLL, SB_LINEDOWN, 0);
            }
            /*
            if (e.KeyCode == Keys.Right)
            {
                SendMessage(this.txtMain.Handle, WM_HSCROLL, SB_LINERIGHT, 0);
            }
            if (e.KeyCode == Keys.Left)
            {
                SendMessage(this.txtMain.Handle, WM_HSCROLL, SB_LINELEFT, 0);
            }*/
        }

        private void SetStartup(bool enable)
        {
            if (enable)
            {
                WshShellClass wshShell = new WshShellClass();
                IWshRuntimeLibrary.IWshShortcut shortcut;
                string startUpFolderPath =
                  Environment.GetFolderPath(Environment.SpecialFolder.Startup);

                // Create the shortcut
                shortcut =
                  (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(
                    startUpFolderPath + "\\" +
                    Application.ProductName + ".lnk");

                shortcut.TargetPath = Application.ExecutablePath;
                shortcut.WorkingDirectory = Application.StartupPath;
                shortcut.Description = "Elysium Launch";
                // shortcut.IconLocation = Application.StartupPath + @"\App.ico";
                shortcut.Save();
            }
            else
            {
                string targetExeName = Path.GetFileName(Application.ExecutablePath);
                string startUpFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

                DirectoryInfo di = new DirectoryInfo(startUpFolderPath);
                FileInfo[] files = di.GetFiles("*.lnk");

                foreach (FileInfo fi in files)
                {
                    string shortcutTargetFile = GetShortcutTargetFile(fi.FullName);

                    if (shortcutTargetFile.EndsWith(targetExeName,
                          StringComparison.InvariantCultureIgnoreCase))
                    {
                        System.IO.File.Delete(fi.FullName);
                    }
                }
            }
        }

        public string GetShortcutTargetFile(string shortcutFilename)
        {
            string pathOnly = Path.GetDirectoryName(shortcutFilename);
            string filenameOnly = Path.GetFileName(shortcutFilename);

            Shell32.Shell shell = new Shell32.ShellClass();
            Shell32.Folder folder = shell.NameSpace(pathOnly);
            Shell32.FolderItem folderItem = folder.ParseName(filenameOnly);
            if (folderItem != null)
            {
                Shell32.ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;
                return link.Path;
            }

            return String.Empty; // Not found
        }

        private void activeReference(string rpath, string soundPath)
        {
            if (System.IO.File.Exists(rpath))
            {
                string backupPath = "backup\\" + rpath;
                playSound(@"sounds\cmdAlreadyActive.wav");
                if (System.IO.File.Exists(backupPath)) // if exist copy of files in backup folder delete one of them
                {
                    System.IO.File.Delete(backupPath);
                }
            }
            else
            {
                replaceReference(rpath, soundPath); // replace from backup
                if (System.IO.File.Exists(rpath) == false)
                {
                    System.IO.File.Create(rpath);
                    playSound(soundPath);
                    writeLine("\n" + "you must restart Elysium to apply these changes !");
                }
            }
        }

        private void replaceReference(string binPath, string splay) //backup : copy backup file to root folder
        {

            string backupPath = "backup\\" + binPath;

            if ((System.IO.File.Exists(backupPath)) && (System.IO.File.Exists(binPath)==false))
            {
                System.IO.File.Move(backupPath, binPath);
                playSound(splay);
                writeLine("\n" + "you must restart Elysium to apply these changes !");
            }
        }

        private void blockReference(string targetPath, string rsound)
        {
            string destinationPath = "backup\\" + targetPath;

            if(System.IO.File.Exists(targetPath))
            {
                if(Directory.Exists("backup\\"))
                {
                    if(System.IO.File.Exists(destinationPath))
                    {
                        System.IO.File.Delete(destinationPath);
                        System.IO.File.Move(targetPath, destinationPath);
                    }
                    else
                    {
                        System.IO.File.Move(targetPath, destinationPath);
                    }
                }
                playSound(rsound);
                writeLine("\n" + "You must restart Elysium to apply these changes !");

            }else
            {
                playSound(@"sounds\cmdAlreadyBlocked.wav");  
            }
        }


        private void blockReferenceFolder(string rpath)
        {
            string targetPath = rpath;
            string destinationPath = "backup\\"+rpath;


            if ((Directory.Exists(targetPath)) && (Directory.Exists(destinationPath) == false))
            {
                Directory.Move(targetPath, destinationPath);
                playSound(@"sounds\AAC4.wav");
                writeLine("\n" + "you must restart Elysium to apply these changes !");
            }
            else
            {
                playSound(@"sounds\cmdAlreadyBlocked.wav");
            }
        }

        private void replaceReferenceFolder(string rpath) //backup
        {
            string targetPath = "backup\\" + rpath;
            string destinationPath = rpath;

            if ((Directory.Exists(targetPath)) && (Directory.Exists(destinationPath) == false))
            {
                Directory.Move(targetPath, destinationPath);
            }
            else if(Directory.Exists(rpath)==false)
            {
                Directory.CreateDirectory(customCommandsFolderPath);
            }
        }

        private void elysiumRestart(int seconds)
        {
            appClose = true;
            txtMain.Select();
            string content;
            int linesCount = txtMain.Lines.Count();
            
            line++;
            txtNum.AppendText("\n" + line.ToString());
            txtMain.AppendText("\n" + "Elysium restarting in 5 seconds.");

            updateLines();
            for (int a = seconds; a >= 0; a--)
            {
                content = txtMain.Lines[linesCount];
                txtMain.Find(content);
                txtMain.SelectedText = "Elysium restarting in " + a.ToString() + " seconds.";
                System.Threading.Thread.Sleep(1000);
            }
            Application.Restart();
        }

        private void playSound(string spath)
        {
           // if (File.Exists(spath))
            //{
                using (SoundPlayer player = new SoundPlayer(spath))
                {
                    // Use PlaySync to load and then play the sound.
                    // ... The program will pause until the sound is complete.
                    try
                    {
                        // Do not initialize this variable here.
                        player.PlaySync();
                    }
                    catch
                    {
                        writeLine("\n" + "Error in Elysium Core: Voice Response- Path Not Found ! : " + spath.ToString());
                    }
                    
                }
            //}
        }

        private bool findPrimaryCommand(string cmd)
        {
            bool founded = false;
            string[] content = System.IO.File.ReadAllLines(userPath);
            for(int i = 0 ; i<content.Length; i++)
            {
                if (i % 2 == 0)
                { 
                    if(content[i]==cmd)
                    {
                        founded = true;
                    }
                }
            }
            return founded;
        }

        private void addUserCommand()
        {
            if (txtCMD.Text != "")
            {
                string[] words = txtCMD.Text.Split('=');
                if (words.Length == 2)
                {
                    if (System.IO.File.Exists(userPath))
                    {
                        string cmd = words[0].Trim();
                        string path = words[1].Trim();

                        if ((cmd.Trim() != "") && (path.Trim() != ""))
                        {
                            if (findPrimaryCommand(cmd) == false)
                            {
                                using (StreamWriter sw = System.IO.File.AppendText(userPath))
                                {
                                    sw.WriteLine(""); //protects writing the in file last line.
                                    sw.WriteLine(cmd);
                                    sw.WriteLine(path);
                                }

                                writeLine("\n" + "command " + cmd + " updated to Elysium core.");
                                playSound(@"sounds\AAC6.wav");
                                txtCMD.Clear();

                                //clean empty lines
                                var tempVar = System.IO.File.ReadAllLines(userPath).Where(arg => !string.IsNullOrWhiteSpace(arg));
                                System.IO.File.WriteAllLines(userPath, tempVar);
                            }else
                            {
                                writeLine("\n" + "command alreeady registed in Elysium Core !");
                                playSound(@"sounds\cmdAlreadyRegisted.wav");
                            }
                        }else
                        {
                            writeLine("\n" + "command format incorrect !");
                            playSound(@"sounds\cmdWrongSyntax.wav");
                        }
                    }
                    else
                    {
                        writeLine("\n" + "Please active primary commands first.");
                        playSound(@"sounds\cmdActivePrimary.wav");
                    }
                }
                else
                {
                    writeLine("\n" + "command format incorrect !");
                    playSound(@"sounds\cmdWrongSyntax.wav");
                }
            }else
            {
                writeLine("\n" + "text box empty !");
                playSound(@"sounds\cmdEmpty.wav");
            }
        }

        private void addCustomCommand()
        {
            string cmd = txtCMD.Text;
            if (cmd != "")
            {
                if (Directory.Exists(customCommandsFolderPath))
                {
                    string cmdpath = customCommandsFolderPath + @"\" + cmd + ".txt";

                    using (FileStream fs = System.IO.File.Create(cmdpath))
                    {
                    }
                    writeLine("\n" + "command " + cmd + " updated to Elysium core.");
                    playSound(@"sounds\AAC7.wav");
                    txtCMD.Clear();
                    try
                    {
                        Process.Start(cmdpath);
                    }
                    catch
                    {
                    }
                }
                else
                {
                    writeLine("\n" + "Please active custom commands first.");
                    playSound(@"sounds\cmdActiveCustom.wav");
                }
            }
            else
            {
                writeLine("\n" + "text box empty !");
                playSound(@"sounds\cmdEmpty.wav");
            }
        }

        private void changeCommand(string filePath, string cmdType)
        {
            bool founded = false;
            if (System.IO.File.Exists(filePath))
            {
                if (txtCMD.Text != "")
                {
                    if (txtCMD.Text.Contains("=") == true)
                    {
                        string[] words = txtCMD.Text.Split('=');

                        if (words.Length == 2)
                        {

                            string oldCmd = words[0].Trim();
                            string newCmd = words[1].Trim();

                            if ((oldCmd.Trim() != "") && (newCmd.Trim() != ""))
                            {

                                string x;
                                using (StreamReader sr = new StreamReader(filePath))
                                {
                                    while ((x = sr.ReadLine()) != null)
                                    {
                                        if (x.Contains(oldCmd))
                                        {
                                            founded = true;
                                        }
                                    }
                                }

                                if (founded == false)
                                {
                                    writeLine("\n" + "command not founded !");
                                    playSound(@"sounds\cmdNotFound.wav");
                                }
                                else
                                {
                                    StringBuilder newFile = new StringBuilder();

                                    string temp = "";

                                    string[] file = System.IO.File.ReadAllLines(filePath);

                                    foreach (string l in file)
                                    {

                                        if (l.Contains(oldCmd))
                                        {

                                            temp = l.Replace(oldCmd, newCmd);

                                            newFile.Append(temp + "\r\n");

                                            continue;

                                        }
                                        newFile.Append(l + "\r\n");
                                    }

                                    System.IO.File.WriteAllText(filePath, newFile.ToString());
                                    if (cmdType == "default")
                                    {
                                        writeLine("\n" + "command " + oldCmd + " changed to " + newCmd + " in Elysium core.");
                                        playSound(@"sounds\AAC9.wav");
                                        txtCMD.Clear();
                                    }

                                    if (cmdType == "primary")
                                    {
                                        writeLine("\n" + "command " + oldCmd + " changed to " + newCmd + " in Elysium core.");
                                        playSound(@"sounds\AAC8.wav");
                                        txtCMD.Clear();
                                    }
                                }
                            }else
                            {
                                writeLine("\n" + "command format incorret !");
                                playSound(@"sounds\cmdWrongSyntax.wav");

                            }
                        }else
                        {
                            writeLine("\n" + "command format incorret !");
                            playSound(@"sounds\cmdWrongSyntax.wav");

                        }
                    }else
                    {
                        writeLine("\n" + "command format incorret !");
                        playSound(@"sounds\cmdWrongSyntax.wav");

                    }
                }else
                {
                    writeLine("\n" + "text box empty !");
                    playSound(@"sounds\cmdEmpty.wav");
                }
            }else
            {
                if (cmdType == "primary")
                {
                    writeLine("\n" + "Please active primary commands first.");
                    playSound(@"sounds\cmdActivePrimary.wav");
                    txtCMD.Clear();
                }
                if (cmdType == "default")
                {
                    writeLine("\n" + "Please active defaults commands first.");
                    playSound(@"sounds\cmdActiveDefault.wav");
                    txtCMD.Clear();
                }
            }
        }

        private void changeCustomCommand()
        {
            if(Directory.Exists(customCommandsFolderPath))
            {
                if (txtCMD.Text != "")
                {
                    if (txtCMD.Text.Contains("=") == true)
                    {
                        string[] words = txtCMD.Text.Split('=');

                        if (words.Length == 2)
                        {

                            if ((words[0].Trim() != "") && (words[1].Trim() != ""))
                            {

                            string oldCmd = customCommandsFolderPath + @"\" + words[0].Trim() + ".txt";
                            string newCmd = customCommandsFolderPath + @"\" + words[1].Trim() + ".txt"; ;


                                if (System.IO.File.Exists(oldCmd))
                                {
                                    System.IO.File.Move(oldCmd, newCmd);

                                    writeLine("\n" + "custom command " + words[1] + " updated to Elysium core.");
                                    playSound(@"sounds\AAD0.wav");
                                    txtCMD.Clear();
                                }
                                else
                                {
                                    writeLine("\n" + "custom command not founded !");
                                    playSound(@"sounds\cmdNotFound.wav");
                                }
                            }
                            else
                            {
                                writeLine("\n" + "command format incorret !");
                                playSound(@"sounds\cmdWrongSyntax.wav");
                            }
                        }
                        else
                        {
                            writeLine("\n" + "command format incorret !");
                            playSound(@"sounds\cmdWrongSyntax.wav");
                        }
                    }
                    else
                    {
                        writeLine("\n" + "command format incorret !");
                        playSound(@"sounds\cmdWrongSyntax.wav");
                    }
                }else
                {
                    writeLine("\n" + "text box empty !");
                    playSound(@"sounds\cmdEmpty.wav");
                }

            }else
            {
                writeLine("\n" + "Please active custom commands first.");
                playSound(@"sounds\cmdActiveCustom.wav");
            }
        }

        private void deleteUserCommand()
        {
            bool founded = false;
            string cmd = txtCMD.Text;
            txtCMD.Clear();
            if (System.IO.File.Exists(userPath))
            {
                if (cmd != "")
                {

                    string[] l = System.IO.File.ReadAllLines(userPath);
                        List<string> cleaned = new List<string>();

                        for (int i = 0; i < l.Length - 1; i = i + 2)
                        {

                            if (l[i] != cmd)
                            {
                                cleaned.Add(l[i]);
                                cleaned.Add(l[i + 1]);
                            }
                            else
                            {
                                founded = true;
                            }

                        }

                        if (founded == true)
                        {
                            System.IO.File.WriteAllLines(userPath, cleaned);
                            writeLine("\n" + "primary command " + cmd + " deleted of Elysium core.");
                            playSound(@"sounds\AAD1.wav");
                        }else
                        {
                            writeLine("\n" + "primary command " + cmd + " not founded !");
                            playSound(@"sounds\cmdNotFound.wav");
                        }
                }
                else
                {
                    writeLine("\n" + "text box empty !");
                    playSound(@"sounds\cmdEmpty.wav");
                }
            }
        }

        private void deleteCustomCommand()
        {
            string cmd = txtCMD.Text;
            if (Directory.Exists(customCommandsFolderPath))
            {
                if (cmd != "")
                {
                    string fileCmd = customCommandsFolderPath + @"\" + cmd + ".txt";
                    if (System.IO.File.Exists(fileCmd))
                    {
                        System.IO.File.Delete(fileCmd);
                        writeLine("\n" + "custom command " + cmd + " deleted of Elysium core.");
                        playSound(@"sounds\AAD2.wav");
                        txtCMD.Clear();
                    }
                    else
                    {
                        writeLine("\n" + "custom command " + cmd + " not founded !");
                        playSound(@"sounds\cmdNotFound.wav");
                    }
                }
                else
                {
                    writeLine("\n" + "text box empty !");
                    playSound(@"sounds\cmdEmpty.wav");
                }
            }else
            {
                writeLine("\n" + "Please active custom commands first.");
                playSound(@"sounds\cmdActiveCustom.wav");
            }
        }


        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a string.
        static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(md5Hash, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void getBatteryPercentage()
        {
            if (batterySystemYes == true)
            {
                string pathHours, pathMinutes;
                pathHours = string.Empty;
                pathMinutes = string.Empty;
                TimeSpan t = TimeSpan.FromSeconds(SystemInformation.PowerStatus.BatteryLifeRemaining);
                batteryLifePercent = Convert.ToInt32(100 * SystemInformation.PowerStatus.BatteryLifePercent);

                string pathPercentage = @"sounds\" + batteryLifePercent.ToString() + ".wav";

                if (t.Minutes > 1)
                {
                    if (t.Hours > 0)
                    {
                        pathHours = @"sounds\" + t.Hours + ".wav";
                        if (t.Minutes > 0)
                        {
                            pathMinutes = @"sounds\" + t.Minutes + ".wav";
                        }
                    }
                    else if (t.Minutes > 0)
                    {
                        pathMinutes = @"sounds\" + t.Minutes + ".wav";
                    }
                }

                if (t.Minutes > 1)
                {
                    writeLine("\n" + "     " + batteryLifePercent + " % Power Level --> " + t.Hours + ":" + t.Minutes + ":00 Remaning !");
                    if (t.Hours > 0)
                    {
                        playSound(pathPercentage);
                        playSound(@"sounds\percentPower.wav");
                        playSound(@"sounds\remains.wav");
                        playSound(pathHours);
                        playSound(@"sounds\hours.wav");
                        playSound(@"sounds\and.wav");
                        playSound(pathMinutes);
                        playSound(@"sounds\minutes.wav");
                    }
                    else
                    {
                        playSound(pathPercentage);
                        playSound(@"sounds\percentPower.wav");
                        playSound(@"sounds\remains.wav");
                        playSound(pathMinutes);
                        playSound(@"sounds\minutes.wav");
                    }
                }
                else
                {
                    writeLine("\n" + "     " + batteryLifePercent + " % Power Level --> " +"Battery Charging !");
                    playSound(pathPercentage);
                    playSound(@"sounds\percentPower.wav");
                }
            }
            else
            {
                writeLine("\n     You not have Battery Cell !");
                playSound(@"sounds\notBatterySystem.wav");
            }
        }

        private void getBatteryLevel()
        {
            if (batterySystemYes == true)
            {
                batteryLifePercent = Convert.ToInt32(100 * SystemInformation.PowerStatus.BatteryLifePercent);

                line++;
                txtNum.AppendText("\n" + line.ToString());
                txtMain.AppendText("\n" + "     " + batteryLifePercent + " % Power Level !");

                string pathPercentage = @"sounds\" + batteryLifePercent.ToString() + ".wav";
                playSound(pathPercentage);
                playSound(@"sounds\percentPower.wav");
            }
            else
            {
                playSound(@"sounds\notBatterySystem.wav");
            }
        }

        private void getbatteryStatus() //checks if the battery level is low, if so warns the user.
        {
            // Getting the current battery charge status. 
            switch (SystemInformation.PowerStatus.BatteryChargeStatus)
            {
                case BatteryChargeStatus.Charging:
                    strBatteryChargeStatus = "BatteryChargeStatus: Charging";
                    break;
                case BatteryChargeStatus.Critical:
                    strBatteryChargeStatus = "BatteryChargeStatus: Critical";
                    break;
                case BatteryChargeStatus.High:
                    strBatteryChargeStatus = "BatteryChargeStatus: High";
                    break;
                case BatteryChargeStatus.Low:
                    strBatteryChargeStatus = "BatteryChargeStatus: Low";
                    break;
                case BatteryChargeStatus.NoSystemBattery:
                    strBatteryChargeStatus = "BatteryChargeStatus: NoSystemBattery";
                    break;
                case BatteryChargeStatus.Unknown:
                    strBatteryChargeStatus = "BatteryChargeStatus: Unknown";
                    break;
            }

            // Getting the approximate percentage of full battery time remaining.
            batteryLifePercent = Convert.ToInt32(100 * SystemInformation.PowerStatus.BatteryLifePercent);

            if (strBatteryChargeStatus != "BatteryChargeStatus: Charging") //if the PC is not plugged in, give notice of low battery.
            {
                if (batteryLifePercent < 12) //if below 12%
                {
                    if ((strBatteryChargeStatus != "BatteryChargeStatus: Charging"))
                    {
                        batteryWarning = true;
                        playSound(@"sounds\pleaseCharge.wav");
                    }
                }
            }
        }

        private void checkBatterySystem() //checks the computer uses battery or not.
        {
            switch (SystemInformation.PowerStatus.BatteryChargeStatus)
            {
                case BatteryChargeStatus.Charging:
                    strBatteryChargeStatus = "BatteryChargeStatus: Charging";
                    break;
                case BatteryChargeStatus.Critical:
                    strBatteryChargeStatus = "BatteryChargeStatus: Critical";
                    break;
                case BatteryChargeStatus.High:
                    strBatteryChargeStatus = "BatteryChargeStatus: High";
                    break;
                case BatteryChargeStatus.Low:
                    strBatteryChargeStatus = "BatteryChargeStatus: Low";
                    break;
                case BatteryChargeStatus.NoSystemBattery:
                    strBatteryChargeStatus = "BatteryChargeStatus: NoSystemBattery";
                    break;
                case BatteryChargeStatus.Unknown:
                    strBatteryChargeStatus = "BatteryChargeStatus: Unknown";
                    break;
            }

            if(strBatteryChargeStatus != "BatteryChargeStatus: NoSystemBattery")
            {
                batterySystemYes = true;
            }
        }

        private void welcomeToElysium()
        {
            int hour = Convert.ToInt32(DateTime.Now.ToString("HH"));

            if((hour>=6)&&(hour<13))
            {
                playSound(@"sounds\goodMorning.wav");
            }
            else if((hour>=13)&&(hour<19))
            {
                playSound(@"sounds\goodAfternoon.wav");
            }
             else if((hour>=19)||(hour<6))
            {
                playSound(@"sounds\goodNight.wav");
            }
        }

        private void writingDelayEfect(string text)
        {
            
            line++;
            txtNum.AppendText("\n" + line.ToString());
            
            foreach (char c in text)
            {
                txtMain.AppendText(c.ToString());
                Application.DoEvents();
                Thread.Sleep(40);
            }
        }

        private void writeLine(string text)
        {
            line++;
            txtNum.AppendText("\n" + line.ToString());
            txtMain.AppendText(text);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (appClose == false)
            {
                writeLine("\n " + "CMD CLOSE NOT WORK IN ELYSIUM !");
                updateLines();
                e.Cancel = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(batterySystemYes == true)
            {
                if (batteryWarning == false)
                {
                    getbatteryStatus();
                }else
                {
                    timer1.Enabled = false;
                }
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            hideOrNot = false;
            this.Show();
            this.Activate();
            this.CenterToScreen();
            notifyIcon1.Visible = false;
        }
    }
}
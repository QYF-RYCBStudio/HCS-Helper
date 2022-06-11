using System;
using System.Net;
using System.Text;
using SharpConfig;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Drawing;

namespace WindowsFormsApp1
{
    public partial class HCSUpdater_Main : Form
    {
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            String resourceName = "Updater.source." + new AssemblyName(args.Name).Name + ".dll";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return null;
                Byte[] assemblyData = new Byte[stream.Length];
                stream.Read(assemblyData, 0, assemblyData.Length);
                return Assembly.Load(assemblyData);
            }
        }

        public Login loginWindow = new Login();
        string lang;
        bool pass;
        bool isAuto;
        FileStream file;
        bool isExists;
        //private static readonly string uSERNAME = loginWindow.USERNAME;
        //string name = uSERNAME;

        public HCSUpdater_Main()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            loginWindow.Show();
            string path = Path.GetTempPath();
            string finalPath = path + "\\HCSUpdater.tmp";
            FileInfo fInfo = new FileInfo(finalPath + "\\HCSUpdater.tmp");
            {
                if (!fInfo.Exists)
                {
                    this.isExists = true;
                }
                else
                {
                    this.isExists = false;
                }
                fInfo.Refresh();
            }
            file = new FileStream(finalPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
            if (isExists)
            {
                StreamWriter sw = new StreamWriter(finalPath, true, Encoding.UTF8);
                sw.WriteLine("true");
                sw.Close();
                file.Lock(0, fInfo.Length);
                file.Close();
            }
            else
            {
                file.Unlock(0, file.Length);
                file.Close();
                bool isCreated;
                StreamReader sr = new StreamReader(file.ToString(), Encoding.UTF8, true);
                string res = sr.ReadLine();
                sr.Close();
                if (bool.TryParse(res, out isCreated))
                {
                    MessageBox.Show("已打开此应用程序！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    string exe = Process.GetCurrentProcess().ProcessName;
                    fInfo.Encrypt();

                    ExecuteCMD("taskkill /im " + exe + "/f");
                }
                else
                {
                    if (res is null)
                    {
                        MessageBox.Show("错误：配置文件为空。请程序是否运行正常！", "致命的应用程序错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (!isCreated)
                    {
                        MessageBox.Show("错误：找不到配置文件。请查看Temp目录下是否存在此文件！", "致命的应用程序错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("错误：未知错误。请在GitHub上报告此BUG！", "致命的应用程序错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            string t = DateTime.Now.ToString();
            lang = System.Globalization.CultureInfo.CurrentCulture.Name;
            if (lang.Equals("zh-CN"))
            {
                mainTitle.Text = "HCS 升级工具";
            }
            else
            {
                mainTitle.Text = "HCS Updater";
            }
            if (lang.Equals("zh-CN"))
            {
                button1.Text = "检查更新";
            }
            else
            {
                button1.Text = "Check For Updates";
            }
        }

        private void ExecuteCMD(string command)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/S /C " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true
            };

            Process process = new Process { StartInfo = processInfo };
            process.Start();

            process.WaitForExit();
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            string t = DateTime.Now.ToString();
            toolStripStatusLabel3.Text = t;
        }

        public void getTime(object sender, EventArgs e)
        {
            string t = DateTime.Now.ToString();
            toolStripStatusLabel3.Text = t;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progressBar2.Value = 0;
            string programVersion = "";
            string downloaded = "0";
            string total = "1";
            int value = 0;
            if (lang.Equals("zh-CN"))
            {
                button1.Text = "检查更新";
            }
            else
            {
                button1.Text = "Check For Updates";
            }
            label11.Text = downloaded;
            label14.Text = "/" + total + " -- ";
            label15.Text = value.ToString();
            string downloadUrl = "https://raw.githubusercontent.com/QYF-RYCBStudio/HCS/main/Update.ucf";   //版本比较文件下载网址
            WebClient wclient = new WebClient                          //实例化 WebClient 类对象
            {
                BaseAddress = downloadUrl,                             //设置 WebClient 的URL
                Encoding = Encoding.UTF8                               //指定下载字符串的编码方式
            };
            int v = 0;
            //为 WebClient 类对象添加报头
            wclient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            //声明 path 变量来保存版本比较文件，并使用相对路径
            string path = ".\\version";
            //声明 cfgPath 变量来保存版本比较文件，并使用相对路径
            string cfgFilePath = ".\\Update.ucf";
            v = 50;
            toolStripStatusLabel4.Text = downloadUrl;
            //使用 DownloadFile() 方法下载版本比较文件，并保存到Version文件
            try
            {
                wclient.DownloadFile(downloadUrl, path);
                v = 100;
                value += 1;
            }
            catch (Exception ex)
            {
                v = 0;
                MessageBox.Show("下载失败\n详细信息：" + ex.Message, "提示", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                return;
            }
            label11.Text = value.ToString();
            label15.Text = v.ToString();
            progressBar2.Value = v;
            string text = "";
            try
            {
                text = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载文件失败\n详细信息：" + ex.Message, "提示", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Exclamation);
            }
            label8.Text = text;
            Configuration config = Configuration.LoadFromFile(cfgFilePath);
            foreach (Section item in config)
            {
                if (item.Name == "Version")
                {
                    programVersion = item["version"].StringValue;
                }
            }
            label7.Text = programVersion;
            try
            {
                for (int i = 0; i < programVersion.Length; i++)
                {
                    if (programVersion[i] == text[i])
                    {
                        pass = true;
                        continue;
                    }
                    else
                    {
                        pass = false;
                        break;
                    }
                }
                if (pass)
                {
                    label4.Text = "是";
                    MessageBox.Show("恭喜！您的程序是最新版本！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    label4.Text = "否";
                    MessageBox.Show("抱歉！您的程序不是最新版本！\n  是否下载最新版本？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                }
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("错误：\n" + "解决方案：重启程序或提交Bug", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            isAuto = true;
            progressBar1.Value = 0;
            int downloaded = 0;
            int total = 12;
            int value = 0;
            string path = "";
            if (lang.Equals("zh-CN"))
            {
                button2.Text = "安装更新";
            }
            else
            {
                button2.Text = "Install updates";
            }
            if (isAuto)
            {
                folderBrowserDialog1.Description = "请选择程序安装文件夹：";
                folderBrowserDialog1.ShowDialog();
                if (folderBrowserDialog1.SelectedPath != "")
                {
                    if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                    {
                        path = folderBrowserDialog1.SelectedPath + "\\main.py";
                    }
                    else
                    {
                        MessageBox.Show("您已取消安装更新!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                }
                else
                {
                    folderBrowserDialog1.SelectedPath = path;
                }
                WebClient webClient = new WebClient();                //实例化 WebClient 类对象
                /**
                 * 下载"Main.py"
                 */
                webClient.BaseAddress = "https://raw.githubusercontent.com/QYF-RYCBStudio/HCS/main/src/com/rycb/hcs/Main.py";
                Downloaded.Text = downloaded.ToString();
                Total.Text = "/" + total.ToString() + " -- "; ;
                Downloaded.Text = value.ToString();
                webClient.Encoding = Encoding.UTF8;                   //指定下载字符串的编码方式
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                int v = 0;
                //webClient.DownloadFile("https://raw.githubusercontent.com/QYF-RYCBStudio/HCS/main/src/com/rycb/hcs/Main.py", path);
                try
                {
                    toolStripStatusLabel4.Text = "https://raw.githubusercontent.com/QYF-RYCBStudio/HCS/main/src/com/rycb/hcs/Main.py";
                    webClient.DownloadFile("https://raw.githubusercontent.com/QYF-RYCBStudio/HCS/main/src/com/rycb/hcs/Main.py", path);
                    v = (100 / 12);
                    value += 1;
                    Downloaded.Text = value.ToString();
                }
                catch (WebException)
                {
                    toolStripStatusLabel4.Text = "https://raw.githubusercontent.com/QYF-RYCBStudio/HCS/main/src/com/rycb/hcs/Main.py";
                    v = 0;
                    MessageBox.Show("下载失败\n详细信息：对路径“" + path + "”的访问被拒绝。", "提示", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                    Downloaded.Text = value.ToString();
                }
                catch (Exception ex)
                {
                    toolStripStatusLabel4.Text = "https://raw.githubusercontent.com/QYF-RYCBStudio/HCS/main/src/com/rycb/hcs/Main.py";
                    v = 0;
                    MessageBox.Show("下载失败\n详细信息：" + ex.Message, "提示", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                    Downloaded.Text = value.ToString();
                }
                finally
                {
                    progressBar1.Value = v;
                    Downloaded.Text = value.ToString();
                }
                /**
                 * 下载“README“目录下的文件
                 */
                string pathMD = folderBrowserDialog1.SelectedPath + "\\README\\View logs (查看日志) .md";
                string pathTXT = folderBrowserDialog1.SelectedPath + "\\README\\View logs (查看日志) .txt";
                webClient.BaseAddress = "https://raw.githubusercontent.com/QYF-RYCBStudio/HCS/main/src/com/rycb/hcs/README";
                Total.Text = "/" + total.ToString() + " -- "; ;
                Downloaded.Text = value.ToString();
                webClient.Encoding = Encoding.UTF8;                   //指定下载字符串的编码方式
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                v = 0;
                //webClient.DownloadFile("https://raw.githubusercontent.com/QYF-RYCBStudio/HCS/main/src/com/rycb/hcs/Main.py", path);
                try
                {
                    toolStripStatusLabel4.Text = "https://raw.githubusercontent.com/QYF-RYCBStudio/HCS/main/src/com/rycb/hcs/README/View%20logs%20(%E6%9F%A5%E7%9C%8B%E6%97%A5%E5%BF%97)%20.md";
                    webClient.DownloadFile("https://raw.githubusercontent.com/QYF-RYCBStudio/HCS/main/src/com/rycb/hcs/README/View%20logs%20(%E6%9F%A5%E7%9C%8B%E6%97%A5%E5%BF%97)%20.md", pathMD);
                    webClient.DownloadFile("https://raw.githubusercontent.com/QYF-RYCBStudio/HCS/main/src/com/rycb/hcs/README/View%20logs%20(%E6%9F%A5%E7%9C%8B%E6%97%A5%E5%BF%97)%20.txt", pathTXT);
                    v = (100 / 12);
                    value += 1;
                    Downloaded.Text = value.ToString();
                }
                catch (WebException)
                {
                    toolStripStatusLabel4.Text = "https://raw.githubusercontent.com/QYF-RYCBStudio/HCS/main/src/com/rycb/hcs/README/View%20logs%20(%E6%9F%A5%E7%9C%8B%E6%97%A5%E5%BF%97)%20.md";
                    v = 0;
                    MessageBox.Show("下载失败\n详细信息：对路径“" + path + "”的访问被拒绝。", "提示", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                    Downloaded.Text = value.ToString();
                }
                catch (Exception ex)
                {
                    toolStripStatusLabel4.Text = "https://raw.githubusercontent.com/QYF-RYCBStudio/HCS/main/src/com/rycb/hcs/README/View%20logs%20(%E6%9F%A5%E7%9C%8B%E6%97%A5%E5%BF%97)%20.md";
                    v = 0;
                    MessageBox.Show("下载失败\n详细信息：" + ex.Message, "提示", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                    Downloaded.Text = value.ToString();
                }
                finally
                {
                    progressBar1.Value += v;
                    Downloaded.Text = value.ToString();
                }
            };
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                MessageBox.Show("您已选择自动下载模式！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                isAuto = true;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                MessageBox.Show("您已选择手动下载模式！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                isAuto = false;
            }
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void githubResDnd_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            int downloaded = 0;
            int total = 0;
            int value = 1;
            string path = "";
            if (lang.Equals("zh-CN"))
            {
                button2.Text = "安装更新";
            }
            else
            {
                button2.Text = "Install updates";
            }
            folderBrowserDialog1.Description = "请选择程序安装文件夹：";
            folderBrowserDialog1.ShowDialog();
            if (folderBrowserDialog1.SelectedPath != "")
            {
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    path = folderBrowserDialog1.SelectedPath + "\\HCS-main.zip";
                }
                else
                {
                    MessageBox.Show("您已取消安装更新!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }
            else
            {
                folderBrowserDialog1.SelectedPath = path;
            }
            WebClient webClient = new WebClient();
            webClient.BaseAddress = "https://codeload.github.com/QYF-RYCBStudio/HCS/zip/refs/heads/main";
            Downloaded.Text = downloaded.ToString();
            Total.Text = "/" + total.ToString() + " -- "; ;
            Downloaded.Text = value.ToString();
            webClient.Encoding = Encoding.UTF8;                   //指定下载字符串的编码方式
            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            int v = 0;
            try
            {
                toolStripStatusLabel4.Text = "https://codeload.github.com/QYF-RYCBStudio/HCS/zip/refs/heads/main";
                webClient.DownloadFile("https://codeload.github.com/QYF-RYCBStudio/HCS/zip/refs/heads/main", path);
                v = 100;
                value += 1;
                Downloaded.Text = value.ToString();
                progressBar1.Value = v;
            }
            catch (WebException)
            {
                toolStripStatusLabel4.Text = "https://codeload.github.com/QYF-RYCBStudio/HCS/zip/refs/heads/main";
                v = 0;
                MessageBox.Show("下载失败\n详细信息：对路径“" + path + "”的访问被拒绝。", "提示", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                Downloaded.Text = value.ToString();
                progressBar1.Value = v;
            }
            catch (Exception ex)
            {
                toolStripStatusLabel4.Text = "https://codeload.github.com/QYF-RYCBStudio/HCS/zip/refs/heads/main";
                v = 0;
                MessageBox.Show("下载失败\n详细信息：" + ex.Message, "提示", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                Downloaded.Text = value.ToString();
                progressBar1.Value = v;
            }
            finally
            {
                Downloaded.Text = value.ToString();
                progressBar1.Value = v;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            string path = Path.GetTempPath();
            FileInfo fInfo = new FileInfo(path + "\\HCSUpdater.tmp");
            if (fInfo.Exists)
            {
                fInfo.Delete();
                //file.Unlock(0, fInfo.Length);
            }
            else
            {
                Close();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            string programRunningPath = AppDomain.CurrentDomain.BaseDirectory;
            DialogResult dialogResult = MessageBox.Show("请先查看本软件的用户协议&隐私政策！", "信息", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            int Agreed = (int)dialogResult;
            bool Agree;
            if (Agreed == 1){Agree = true;}
            else { Agree = false;}
            if (Agree) { Process.Start("https://steampp.net/agreement"); Process.Start("https://steampp.net/privacy"); }
            else { MessageBox.Show("用户取消了使用VPN");  return; }
            string date = DateTime.Now.ToShortDateString();
            string agreeText = $"""
                                                           RYCB工作室免责声明
                本声明是RYCB工作室(RYCB Studio，以下简称RYCBStudio)对于用户使用Steam++ VPN(Watt Toolkit，以下简称Steam++)加速下载时的提示。
                用户于{date}委托RYCB Studio申请使用Steam++，且已阅读该软件的用户协议&隐私政策，本人现申请获得Steam++的使用权。
                为了避免纠纷，特做如下说明：用户是Steam++的真正使用者，今后由Steam++引起的纠纷和造成的一切后果，其责任概由用户承担，与RYCB Studio无关。
                特此声明！
                声明人：用户
                日期：{date}
                """;
            DialogResult dResult = MessageBoxEX.Show(agreeText, "信息", MessageBoxButtons.YesNo, new string[] {"我同意", "我不同意"});
            bool isAgreed;
            if ((int) dResult == 1) { isAgreed = true; }
            else { isAgreed = false; }
            if (isAgreed) { Process.Start(programRunningPath + "\\VPN\\Steam++\\Steam++.exe"); }
            else { return; }
        }

        public class MessageBoxEX
        {
            public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, string[] buttonTitles)
            {
                MessageForm frm = new MessageForm(buttons, buttonTitles);
                frm.Show();
                frm.WatchForActivate = true;
                DialogResult result = MessageBox.Show(frm, text, caption, buttons);
                frm.Close();

                return result;
            }

            public static DialogResult Show(string text, string caption, MessageBoxButtons buttons,
                MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, string[] buttonTitles)
            {
                MessageForm frm = new MessageForm(buttons, buttonTitles);
                frm.Show();
                frm.WatchForActivate = true;
                DialogResult result = MessageBox.Show(frm, text, caption, buttons, icon, defaultButton);
                frm.Close();

                return result;
            }

            class MessageForm : Form
            {
                IntPtr _handle;
                MessageBoxButtons _buttons;
                string[] _buttonTitles = null;

                bool _watchForActivate = false;

                public bool WatchForActivate
                {
                    get { return _watchForActivate; }
                    set { _watchForActivate = value; }
                }

                public MessageForm(MessageBoxButtons buttons, string[] buttonTitles)
                {
                    _buttons = buttons;
                    _buttonTitles = buttonTitles;

                    // Hide self form, and don't show self form in task bar.
                    this.Text = "";
                    this.StartPosition = FormStartPosition.CenterScreen;
                    this.Location = new Point(-32000, -32000);
                    this.ShowInTaskbar = false;
                }

                protected override void OnShown(EventArgs e)
                {
                    base.OnShown(e);
                    // Hide self form, don't show self form even in task list.
                    NativeWin32API.SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, 0, 0, 659);
                }

                protected override void WndProc(ref System.Windows.Forms.Message m)
                {
                    if (_watchForActivate && m.Msg == 0x0006)
                    {
                        _watchForActivate = false;
                        _handle = m.LParam;
                        CheckMsgbox();
                    }
                    base.WndProc(ref m);
                }

                private void CheckMsgbox()
                {
                    if (_buttonTitles == null || _buttonTitles.Length == 0)
                        return;

                    // Button title index
                    int buttonTitleIndex = 0;
                    // Get the handle of control in current window.
                    IntPtr h = NativeWin32API.GetWindow(_handle, GW_CHILD);

                    // Set those custom titles to the three buttons(Default title are: Yes, No and Cancle).
                    while (h != IntPtr.Zero)
                    {
                        if (NativeWin32API.GetWindowClassName(h).Equals("Button"))
                        {
                            if (_buttonTitles.Length > buttonTitleIndex)
                            {
                                // Changes the text of the specified window's title bar (if it has one). 
                                // If the specified window is a control, the text of the control is changed. 
                                // However, SetWindowText cannot change the text of a control in another application.
                                NativeWin32API.SetWindowText(h, _buttonTitles[buttonTitleIndex]);

                                buttonTitleIndex++;
                            }
                        }

                        // Get the handle of next control in current window.
                        h = NativeWin32API.GetWindow(h, GW_HWNDNEXT);
                    }
                }
            }


            public const int GW_CHILD = 5;
            public const int GW_HWNDNEXT = 2;

            public class NativeWin32API
            {
                [DllImport("user32.dll", CharSet = CharSet.Auto)]
                public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int Width, int Height, int flags);
                [DllImport("user32.dll")]
                public static extern IntPtr GetWindow(IntPtr hWnd, Int32 wCmd);
                [DllImport("user32.dll")]
                public static extern bool SetWindowText(IntPtr hWnd, string lpString);
                [DllImport("user32.dll")]
                public static extern int GetClassNameW(IntPtr hWnd, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpString, int nMaxCount);

                public static string GetWindowClassName(IntPtr handle)
                {
                    StringBuilder sb = new StringBuilder(256);

                    // Retrieves the name of the class to which the specified window belongs
                    GetClassNameW(handle, sb, sb.Capacity);
                    return sb.ToString();
                }
            }

        }
    }
}

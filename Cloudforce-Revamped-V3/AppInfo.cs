﻿using Cloudforce_Revamped_V3.Properties;
using Guna.UI2.WinForms;
using Newtonsoft.Json;
using Sentry;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Cloudforce_Revamped_V3.AppsJSON;

namespace Cloudforce_Revamped_V3
{
    public partial class AppInfo : Form
    {
        public int CurentAppJSONInt;

        public AppInfo()
        {
            InitializeComponent();
        }

        public void initalizeApp(int json) // Adds All the info's and stuffs :)
        {
            CurentAppJSONInt = json;
            AppDescriptionLBL.Clear();
            AppDescriptionLBL.AppendText(APPJson.Apps[json].AppDescription);
            AppNameLBL.Text = APPJson.Apps[json].AppName;
            AppImage.Load(APPJson.Apps[json].AppImage);
            AppImage.Refresh();
            GFNIssuesLBL.Text = "GFN Issues : " + APPJson.Apps[json].AppGFNIssues;
            if (File.Exists(mainpath + APPJson.Apps[CurentAppJSONInt].AppExe))
            {
                DownloadBTN.FillColor = Color.FromArgb(255, 65, 138, 238);
                DownloadBTN.FillColor2 = Color.FromArgb(255, 60, 131, 237);
                DownloadBTN.Image = Resources.play_96px;
                DownloadBTN.Text = "Start";
            }
            AppUpdateLBL.Text = APPJson.Apps[json].AppUpdateLog;
            switch (APPJson.Apps[json].AppGFNStatus)
            {
                case "Safe":
                    GFNStatusLBL.ForeColor = Color.Green;
                    GFNStatusLBL.Text = "GFN Status : Safe";
                    return;

                case "Unsafe":
                    GFNStatusLBL.ForeColor = Color.Orange;
                    GFNStatusLBL.Text = "GFN Status : Unsafe";
                    return;

                case "Crash":
                    GFNStatusLBL.ForeColor = Color.Red;
                    GFNStatusLBL.Text = "GFN Status : Crash";
                    return;
            }
        }

        private void AppInfo_Load(object sender, EventArgs e)
        {
        }

        #region Download Stuff

        public static string mainpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Cloudforce\\";
        public bool exractdone = false;
        private bool DownloadFinished;

        public void File_Downloader(Guna2GradientButton button)
        {
            // download file with progress bar
            DownloadFinished = false;
            WebClient client = new WebClient();
            button.Invoke(new MethodInvoker(delegate { button.Enabled = false; }));
            Progressbar.Invoke(new MethodInvoker(delegate { Progressbar.Value = 0; }));
            Progressbar.Invoke(new MethodInvoker(delegate { Progressbar.Visible = true; }));
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(FileDownloadComplete);
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadChanged);
            client.DownloadFileAsync(new Uri(APPJson.Apps[CurentAppJSONInt].AppDownloadUrl), mainpath + Path.GetExtension(APPJson.Apps[CurentAppJSONInt].AppDownloadUrl));
            while (DownloadFinished == false)
                Application.DoEvents();
        }

        private async void FileDownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                DownloadFinished = true;
                Progressbar.Invoke(new MethodInvoker(delegate { Progressbar.Visible = false; }));
                Progressbar.Invoke(new Action(() => Progressbar.Value = 0));

                ((WebClient)sender).Dispose();
                if (APPJson.Apps[CurentAppJSONInt].AppDownloadUrl.EndsWith(".zip")) // IF file is zip Extract
                {
                    await Task.Run(() =>
                    {
                        exractdone = false;
                        DownloadBTN.Text = "Extracting";
                        ZipFile.ExtractToDirectory(mainpath + Path.GetExtension(APPJson.Apps[CurentAppJSONInt].AppDownloadUrl), mainpath);
                        exractdone = true;
                    });
                    while (exractdone == false)
                    {
                        Application.DoEvents();
                    }
                    File.Delete(mainpath + Path.GetExtension(APPJson.Apps[CurentAppJSONInt].AppDownloadUrl));
                }
                else
                {
                    if (APPJson.Apps[CurentAppJSONInt].AppExe.EndsWith(".exe"))
                    {
                        if (Path.GetExtension(APPJson.Apps[CurentAppJSONInt].AppDownloadUrl) != APPJson.Apps[CurentAppJSONInt].AppExe)
                        {
                            File.Move(mainpath + Path.GetExtension(APPJson.Apps[CurentAppJSONInt].AppDownloadUrl), mainpath + APPJson.Apps[CurentAppJSONInt].AppExe);
                        }
                    }
                }
                // -------------------------------------------------------------------------------------
                //                                  Custom Patches (Apps)
                // -------------------------------------------------------------------------------------
                switch (APPJson.Apps[CurentAppJSONInt].AppName)
                {
                    case "Discord":
                        Process.Start(mainpath + Path.GetExtension(APPJson.Apps[CurentAppJSONInt].AppDownloadUrl));
                        await Task.Delay(10000);
                        Process.Start(mainpath + APPJson.Apps[CurentAppJSONInt].AppExe, APPJson.Apps[CurentAppJSONInt].AppArguments);
                        return;

                    case "Desktop":
                        Process[] pse = Process.GetProcessesByName("explorer");
                        foreach (Process p in pse)
                            p.Kill();
                        await Task.Delay(500);
                        Process.Start(mainpath + APPJson.Apps[CurentAppJSONInt].AppExe, APPJson.Apps[CurentAppJSONInt].AppArguments);
                        Process.Start("C:\\Program Files\\Classic Shell\\ClassicStartMenu.exe");
                        Process.Start("C:\\Program Files\\Classic Shell\\ClassicStartMenu.exe", " -xml " + mainpath + "ZortosShell\\bin\\shell\\Menu_Settings.xml");
                        return;

                    case "Yuzu":
                        bool waits = false;
                        await Task.Run(() =>
                        {
                            DownloadBTN.Text = "Fase 2";
                            WebClient webClient = new WebClient();
                            webClient.DownloadFile("https://files.zortos.me/Files/Emulators/yuzu_Roaming.zip", mainpath + "\\yuzu_Roaming.zip");
                            ZipFile.ExtractToDirectory(mainpath + "\\yuzu_Roaming.zip", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\");
                            waits = true;
                        });
                        while (waits == false)
                        {
                            Application.DoEvents();
                        }
                        return;
                }
                // -------------------------------------------------------------------------------------
                //                                  Custom Patches END
                // -------------------------------------------------------------------------------------
                if (APPJson.Apps[CurentAppJSONInt].AppExe.StartsWith("C:\\"))
                {
                    Process.Start(APPJson.Apps[CurentAppJSONInt].AppExe, APPJson.Apps[CurentAppJSONInt].AppArguments);
                    return;
                }
                else
                {
                    Process.Start(mainpath + APPJson.Apps[CurentAppJSONInt].AppExe, APPJson.Apps[CurentAppJSONInt].AppArguments);
                }
                if (File.Exists(mainpath + APPJson.Apps[CurentAppJSONInt].AppExe)) // Changes Icon and color to blue and changes the text to start
                {
                    DownloadBTN.Invoke(new Action(() => DownloadBTN.FillColor = Color.FromArgb(255, 65, 138, 238)));
                    DownloadBTN.Invoke(new Action(() => DownloadBTN.FillColor2 = Color.FromArgb(255, 60, 131, 237)));
                    DownloadBTN.Invoke(new Action(() => DownloadBTN.Image = Resources.play_96px));
                    DownloadBTN.Invoke(new Action(() => DownloadBTN.Text = "Start"));
                    DownloadBTN.Invoke(new Action(() => DownloadBTN.Enabled = true));
                }
            }
            else
            {
                SentrySdk.CaptureException(e.Error);
                MessageBox.Show(e.Error.Message);
                Progressbar.Invoke(new Action(() => Progressbar.Visible = false));
                DownloadBTN.Invoke(new Action(() => DownloadBTN.Enabled = true));
            }
        }

        private void DownloadChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Progressbar.Invoke(new Action(() => Progressbar.Value = e.ProgressPercentage));
            DownloadBTN.Invoke(new Action(() => DownloadBTN.Text = "Downloading " + e.ProgressPercentage + "%"));
        }

        public bool proccesrun()
        {
            if (File.Exists(mainpath + APPJson.Apps[CurentAppJSONInt].AppExe))
            {
                Process.Start(mainpath + APPJson.Apps[CurentAppJSONInt].AppExe, APPJson.Apps[CurentAppJSONInt].AppArguments);
                return true;
            }
            else return false;
        }

        #endregion Download Stuff

        private async void guna2GradientButton1_Click(object sender, EventArgs e) // Download Button
        {
            if (proccesrun() == false)
            {
                await Task.Run(() =>
                {
                    File_Downloader(DownloadBTN);
                });
            }
        }
    }
}
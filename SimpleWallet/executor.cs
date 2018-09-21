﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Security.Cryptography;

namespace SimpleWallet
{
    class Executor
    {
        Types type = new Types();
        String daemond = "anond.exe";
        String cli = "anon-cli.exe";
        const String verifyingKey = "sprout-verifying.key";
        const String provingKey = "sprout-proving.key";
        String dataToGetSync = "";
        String getResultSync = "";
        String dataToGetBalance = "";
        String getResultBalance = "";
        String dataToGetOthers = "";
        String getResultOthers = "";
        String dataToGetMasternode = "";
        String getResultMasternode = "";
        String getResultDaemond = "";
        String dataToGetTransaction = "";
        String getResultGetTransaction = "";
        String dataToGetDebug = "";
        String getResultDebug = "";

        bool error = false;
        bool isClosedDeamon = true;

        public event ReceivedDataEventHandler progressChange;
        public event ReceivedDataEventHandler progressDone;
        public event ReceivedDataEventHandler downloadProgressChange;
        public event ReceivedDataEventHandler downloadProgressDone;
        public event DeamonErrorEventHandler errorOccurs;
        public event DaemonEventHandler daemondHdl;

        WebClient wc = new WebClient();
        private static Executor instance = null;
        private static readonly object padlock = new object();

        Executor()
        {
        }

        public static Executor Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new Executor();
                        }
                    }
                }
                return instance;
            }
        }

        public bool getDeamondClosedStatus()
        {
            return isClosedDeamon;
        }

        void executeCommand(List<String> args, Types.OutputType type)
        {
            ProcessStartInfo ProcessInfo;
            String command = "";
            if (type == Types.OutputType.DAEMOND)
            {
                command = daemond;
            }
            else
            {
                command = cli;
            }
            foreach(String arg in args)
            {
                if (!String.IsNullOrEmpty(arg))
                {
                    command += " " + arg;
                }
            }
            try
            {
                Process process = new System.Diagnostics.Process();
                ProcessInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
                ProcessInfo.CreateNoWindow = true;
                ProcessInfo.UseShellExecute = false;
                ProcessInfo.RedirectStandardError = true;
                ProcessInfo.RedirectStandardOutput = true;
                process.StartInfo = ProcessInfo;
                process.EnableRaisingEvents = true;
                if (type == Types.OutputType.DAEMOND)
                {
                    process.OutputDataReceived += new DataReceivedEventHandler(this.outputHandlerDaemond);
                    process.ErrorDataReceived += new DataReceivedEventHandler(this.outputHandlerDaemond);
                }
                if (type == Types.OutputType.SYNC)
                {
                    process.OutputDataReceived += new DataReceivedEventHandler(this.outputHandlerSync);
                    process.ErrorDataReceived += new DataReceivedEventHandler(this.outputHandlerSync);
                }
                else if (type == Types.OutputType.BALANCE)
                {
                    process.OutputDataReceived += new DataReceivedEventHandler(this.outputHandlerBalance);
                    process.ErrorDataReceived += new DataReceivedEventHandler(this.outputHandlerBalance);
                }
                else if (type == Types.OutputType.MASTERNODE)
                {
                    process.OutputDataReceived += new DataReceivedEventHandler(this.outputHandlerMasternode);
                    process.ErrorDataReceived += new DataReceivedEventHandler(this.outputHandlerMasternode);
                }
                else if (type == Types.OutputType.OTHERS)
                {
                    process.OutputDataReceived += new DataReceivedEventHandler(this.outputHandlerOthers);
                    process.ErrorDataReceived += new DataReceivedEventHandler(this.outputHandlerOthers);
                }
                else if(type == Types.OutputType.GET_TRANSACTION)
                {
                    process.OutputDataReceived += new DataReceivedEventHandler(this.outputHandlerGetTransaction);
                    process.ErrorDataReceived += new DataReceivedEventHandler(this.outputHandlerGetTransaction);
                }
                else if (type == Types.OutputType.DEBUG)
                {
                    process.OutputDataReceived += new DataReceivedEventHandler(this.outputHandlerDebug);
                    process.ErrorDataReceived += new DataReceivedEventHandler(this.outputHandlerDebug);
                }
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                process.WaitForExit();
                if(type == Types.OutputType.DAEMOND)
                {
                    OnDaemonStatus(new DaemonEventArgs(true));
                }
            }
            catch (Exception ex)
            {

            }
        }

        void outputHandlerDaemond(Object sender, DataReceivedEventArgs e)
        {
            // Prepend line numbers to each line of the output.
            if (!String.IsNullOrEmpty(e.Data))
            {
                if (String.IsNullOrEmpty(getResultBalance))
                {
                    getResultDaemond += e.Data;
                }
                else
                {
                    getResultDaemond += "\n" + e.Data;
                } 
            }
        }

        void outputHandlerSync(Object sender, DataReceivedEventArgs e)
        {
            // Prepend line numbers to each line of the output.
            if (!String.IsNullOrEmpty(e.Data))
            {
                if (e.Data.Contains(dataToGetBalance))
                {
                    if (String.IsNullOrEmpty(getResultBalance))
                    {
                        getResultSync += e.Data;
                    }
                    else
                    {
                        getResultSync += "\n" + e.Data;
                    }
                }
            }
        }

        void outputHandlerBalance(Object sender, DataReceivedEventArgs e)
        {
            // Prepend line numbers to each line of the output.
            if (!String.IsNullOrEmpty(e.Data))
            {
                if (e.Data.Contains(dataToGetBalance))
                {
                    if (String.IsNullOrEmpty(getResultBalance))
                    {
                        getResultBalance += e.Data;
                    }
                    else
                    {
                        getResultBalance += "\n" + e.Data;
                    }
                }
            }
        }

        void outputHandlerMasternode(Object sender, DataReceivedEventArgs e)
        {
            // Prepend line numbers to each line of the output.
            if (!String.IsNullOrEmpty(e.Data))
            {
                if (e.Data.Contains(dataToGetMasternode))
                {
                    if (String.IsNullOrEmpty(getResultMasternode))
                    {
                        getResultMasternode += e.Data;
                    }
                    else
                    {
                        getResultMasternode += "\n" + e.Data;
                    }
                }
            }
        }

        void outputHandlerOthers(Object sender, DataReceivedEventArgs e)
        {
            // Prepend line numbers to each line of the output.
            if (!String.IsNullOrEmpty(e.Data))
            {
                if (e.Data.Contains(dataToGetOthers))
                {
                    if (String.IsNullOrEmpty(getResultOthers))
                    {
                        getResultOthers += e.Data;
                    }
                    else
                    {
                        getResultOthers += "\n" + e.Data;
                    }
                }
            }
        }

        void outputHandlerGetTransaction(Object sender, DataReceivedEventArgs e)
        {
            // Prepend line numbers to each line of the output.
            if (!String.IsNullOrEmpty(e.Data))
            {
                if (e.Data.Contains(dataToGetTransaction))
                {
                    if (String.IsNullOrEmpty(getResultGetTransaction))
                    {
                        getResultGetTransaction += e.Data;
                    }
                    else
                    {
                        getResultGetTransaction += "\n" + e.Data;
                    }
                }
            }
        }

        void outputHandlerDebug(Object sender, DataReceivedEventArgs e)
        {
            // Prepend line numbers to each line of the output.
            if (!String.IsNullOrEmpty(e.Data))
            {
                if (e.Data.Contains(dataToGetDebug))
                {
                    if (String.IsNullOrEmpty(getResultDebug))
                    {
                        getResultDebug += e.Data;
                    }
                    else
                    {
                        getResultDebug += "\n" + e.Data;
                    }
                }
            }
        }

        public String executeStart(String command)
        {
            String exportDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            List<String> args = new List<String> { command, " -exportdir=\"" + exportDir + "\"" };
            executeCommand(args, Types.OutputType.DAEMOND);
            if(!String.IsNullOrEmpty(getResultDaemond))
            {
                OnErrorOccurs(new DeamonErrorEventArgs(getResultDaemond));
            }
            return getResultDaemond;
        }

        public void executeStop()
        {
            String exportDir = Directory.GetCurrentDirectory();
            exportDir = exportDir.Split('\\')[0];
            List<String> args = new List<String> { "stop" };
            executeCommand(args, Types.OutputType.OTHERS);
        }

        public String executeSync(List<String> command, String dataToGet)
        {
            dataToGetSync = dataToGet;
            getResultSync = "";
            executeCommand(command, Types.OutputType.SYNC);
            return getResultSync;
        }

        public String executeBalance(List<String> command, String dataToGet)
        {
            dataToGetBalance = dataToGet;
            getResultBalance = "";
            executeCommand(command, Types.OutputType.BALANCE);
            return getResultBalance;
        }

        public String executeGetTransaction(List<String> command, String dataToGet)
        {
            dataToGetTransaction = dataToGet;
            getResultGetTransaction = "";
            executeCommand(command, Types.OutputType.GET_TRANSACTION);
            return getResultGetTransaction;
        }
        

        public String executeOthers(List<String> command, String dataToGet)
        {
            dataToGetOthers = dataToGet;
            getResultOthers = "";
            executeCommand(command, Types.OutputType.OTHERS);
            return getResultOthers;
        }

        public String executeMasternode(List<String> command, String dataToGet)
        {
            dataToGetMasternode = dataToGet;
            getResultMasternode = "";
            executeCommand(command, Types.OutputType.MASTERNODE);
            return getResultMasternode;
        }

        public String executeDebug(List<String> command, String dataToGet)
        {
            dataToGetDebug = dataToGet;
            getResultDebug = "";
            executeCommand(command, Types.OutputType.DEBUG);
            return getResultDebug;
        }
        static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public bool checkParamsFile(String file1, String file2)
        {
            //String webLink = "https://snowgem.org/downloads/";
            //String file1Link = webLink + file1;
            //String file2Link = webLink + file2;
            String appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            String file1Loc = appdata + "\\AnonParams\\" + file1;
            String file2Loc = appdata + "\\AnonParams\\" + file2;
            //Int64 length = 0;
            //WebClient wc = new WebClient();
            //Stream response = null;
            if (!Directory.Exists(appdata + "\\AnonParams"))
            {
                Directory.CreateDirectory(appdata + "\\AnonParams");
                return true;
            }
            if (!File.Exists(file1Loc) || !File.Exists(file2Loc))
            {
                return true;
            }

            string md5Hash = CalculateMD5(file1Loc);
            if (md5Hash != Types.verifyingKeyMD5)
            {
                return true;
            }
            md5Hash = CalculateMD5(file2Loc);
            if (md5Hash != Types.provingKeyMD5)
            {
                return true;
            }

            return false;
            //try
            //{
            //    response = wc.OpenRead(new System.Uri(file1Link));
            //    length = new System.IO.FileInfo(file1Loc).Length;
            //    response.Close();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
            //Int64 bytes_total = Convert.ToInt64(wc.ResponseHeaders["Content-Length"]);
            
            //if (bytes_total != length)
            //{
            //    ret = true;
            //}

            //try
            //{
            //    response = wc.OpenRead(new System.Uri(file2Link));
            //    length = new System.IO.FileInfo(file2Loc).Length;
            //    response.Close();
            //}
            //catch (Exception ex)
            //{
            //    //MessageBox.Show(ex.Message);
            //}
            //bytes_total = Convert.ToInt64(wc.ResponseHeaders["Content-Length"]);

            //if (bytes_total != length)
            //{
            //    ret = true;
            //}
        }

        public void stopNewVersion()
        {
            wc.CancelAsync();
        }

        public bool downloadNewVersion(String url, String name)
        {
            bool rtn = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            //String fileLoc = Path.GetTempPath() + "\\" + name;
            String fileLoc = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) + "\\" + name;

            try
            {
                wc.DownloadProgressChanged += newVersion_DownloadProgressChanged;
                wc.DownloadFileCompleted += newVersion_DownloadFileCompleted;

                wc.DownloadFileAsync(new System.Uri(url), fileLoc, true);
            }
            catch (Exception ex)
            {
                rtn = false;
            }

            return rtn;
        }

        public bool downloadParams(String file, Types.DownloadFileType downloadType)
        {
            String webLink = "https://assets.anonfork.io/";
            String fileLink = webLink + file;
            String appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            String fileLoc = appdata + "\\AnonParams\\" + file;
            Int64 length = 0;
            WebClient wc = new WebClient();
            Stream response = null;
            Int64 bytes_total = 0;


                if(downloadType == Types.DownloadFileType.PROVING)
                {
                    wc.DownloadProgressChanged += proving_DownloadProgressChanged;
                    wc.DownloadFileCompleted += proving_DownloadFileCompleted;
                }
                else
                {
                    wc.DownloadProgressChanged += verifying_DownloadProgressChanged;
                    wc.DownloadFileCompleted += verifying_DownloadFileCompleted;
                }
                wc.DownloadFileAsync(new System.Uri(fileLink), appdata + "\\AnonParams\\" + file, true);
            return true;
        }

        protected void OnDaemonStatus(DaemonEventArgs e)
        {
            if (daemondHdl != null)
                daemondHdl(this, e);
        }

        protected void OnErrorOccurs(DeamonErrorEventArgs e)
        {
            if (errorOccurs != null)
                errorOccurs(this, e);
        }  

        protected void OnProgressChange(ReceivedDataEventArgs e)
        {
            if (progressChange != null)
                progressChange(this, e);
        }

        protected void OnProgressDone(ReceivedDataEventArgs e)
        {
            if (progressDone != null)
                progressDone(this, e);
        }

        protected void OnDownloadProgressChange(ReceivedDataEventArgs e)
        {
            if (downloadProgressChange != null)
                downloadProgressChange(this, e);
        }

        protected void OnDownloadProgressDone(ReceivedDataEventArgs e)
        {
            if (downloadProgressDone != null)
                downloadProgressDone(this, e);
        }  

        void verifying_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                OnProgressDone(new ReceivedDataEventArgs(true, 100, true, verifyingKey));
            }
            else
            {
                OnProgressDone(new ReceivedDataEventArgs(false, 100, false, verifyingKey));
            }
        }

        void verifying_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            OnProgressChange(new ReceivedDataEventArgs(false, e.ProgressPercentage, false, verifyingKey));
        }

        void proving_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                OnProgressDone(new ReceivedDataEventArgs(true, 100, true, provingKey));
            }
            else
            {
                OnProgressDone(new ReceivedDataEventArgs(false, 100, false, provingKey));
            }
        }

        void proving_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            OnProgressChange(new ReceivedDataEventArgs(false, e.ProgressPercentage, false, provingKey));
        }

        void newVersion_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                OnDownloadProgressDone(new ReceivedDataEventArgs(true, 100, true, ""));
            }
            else
            {
                OnDownloadProgressDone(new ReceivedDataEventArgs(false, 100, false, ""));
            }
        }

        void newVersion_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            OnDownloadProgressChange(new ReceivedDataEventArgs(false, e.ProgressPercentage, false, ""));
        }
    }
}

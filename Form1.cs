using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Runtime.CompilerServices;
using MetroFramework;
using Microsoft.Win32;
using System.Net.Sockets;
using System.Threading;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Data.SqlClient;
using PS4_Tools.LibOrbis;
using PS4_Tools.Util;
using System.Drawing.Imaging;
using System.Net.NetworkInformation;
using Microsoft.Win32;
using IWshRuntimeLibrary;

/*Copyright Darksoftware (c) 2019-2022*/
/* LICENSED UNDER GPLv3              */

namespace Store_CDN_Server
{
    public partial class Form1 : System.Windows.Forms.Form
    {

        private TcpListener myListener;
        SQLiteConnection sql_con;
        string serverRoot = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private Ini.IniFile ini;
        string drive;

        bool isautostart = false;
        Thread th, th2, th3, th4;

        public Form1()
        {
            InitializeComponent();
            ini = new Ini.IniFile(serverRoot + @"\settings.ini");
            groupControl1.AllowDrop = true;
            metroTextBox1.Text = Properties.Settings.Default.IP;
            sql_con = new SQLiteConnection("Data Source=" + serverRoot + @"\store.db;");
            Console.WriteLine("INI: " + serverRoot + @"\settings.ini" + " ROOT: " + serverRoot);

        }

        private string apptype_info(PS4_Tools.PKG.SceneRelated.PKGType type)
        {
            switch (type)
            {
                case PS4_Tools.PKG.SceneRelated.PKGType.Game:
                    return "Game";
                case PS4_Tools.PKG.SceneRelated.PKGType.App:
                    return "App";
                case PS4_Tools.PKG.SceneRelated.PKGType.Patch:
                    return "Patch";
                case PS4_Tools.PKG.SceneRelated.PKGType.Addon_Theme:
                    return "Theme";
            }

            return "Unknown";
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (!System.IO.File.Exists(serverRoot + "/store.db"))
            {
                SQLiteConnection.CreateFile(serverRoot + "/store.db");
                sql_con.Open();
                string create_db = "CREATE TABLE homebrews ( pid int UNSIGNED NOT NULL,id varchar(255), name varchar(255), desc varchar(255), image varchar(255), package varchar(255), version varchar(255) ,picpath varchar(255) ,desc_1 varchar(255) ,desc_2 varchar(255) ,ReviewStars varchar(255) ,Size varchar(255) ,Author varchar(255) ,apptype varchar(255) ,pv varchar(255) ,main_icon_path varchar(255) ,main_menu_pic varchar(255) ,releaseddate date DEFAULT NULL,number_downloads int NOT NULL);";
                SQLiteCommand command = new SQLiteCommand(create_db, sql_con);
                command.ExecuteNonQuery();
            }
            else
                sql_con.Open();


            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    //Console.WriteLine(ni.Name);
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            //Console.WriteLine(ip.Address.ToString());
                            metroTextBox1.Items.Add(ip.Address.ToString());
                        }
                    }
                }
            }
            try
            {
                metroTextBox1.Text = ini.IniReadValue("network", "ps4ip");
                pkgText.Text = ini.IniReadValue("network", "pkgPath");
            }
            catch (Exception) { }

            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (rk != null)
            {
                try
                {
                    string value = (String)rk.GetValue("Store CDN Server");
                    if (!string.IsNullOrEmpty(value))
                        isautostart = true;
                    else
                        Console.WriteLine("Reg key not installed...");

                }
                catch
                {
                    Console.WriteLine("Reg key not installed...");
                }
            }

            button1.Text = isautostart ? "Disable" : "Enable";
            button1.ForeColor = isautostart ? Color.Red : Color.Green;
            if (isautostart)
                button3_Click(sender, e);

        }


        public static System.Drawing.Bitmap BytesToBitmap(byte[] ImgBytes)
        {
            System.Drawing.Bitmap result = null;
            if (ImgBytes != null)
            {
                MemoryStream stream = new MemoryStream(ImgBytes);
                result = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(stream);
            }
            return result;
        }

        public int count_rows()
        {
            string query = "SELECT COUNT(*) FROM homebrews";
            List<string> liteEntries = new List<string>();

            var cmd = new SQLiteCommand(query, sql_con);

            object result = cmd.ExecuteScalar();
            int nTables = Convert.ToInt32(result);
            return nTables;
        }

        private string calcsize(string file)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = new FileInfo(file).Length;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }

        public static void SavePic(Image img, string Filename)
        {
            try
            {
                new Bitmap(img, 512, 512).Save(Filename);
            }
            catch (Exception) { } //No Image

        }

        public void SendToBrowser(String sData, ref Socket mySocket)
        {
            SendToBrowser(Encoding.ASCII.GetBytes(sData), ref mySocket);
        }
        public void SendToBrowser(Byte[] bSendData, ref Socket mySocket)
        {
            int numBytes = 0;
            try
            {
                if (mySocket.Connected)
                {
                    if ((numBytes = mySocket.Send(bSendData, bSendData.Length, 0)) == -1)
                        Console.WriteLine("Socket Error cannot Send Packet");
                }
                else Console.WriteLine("Connection Dropped....");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Occurred : {0} ", e);
            }
        }


        public string md5_file(string file)
        {
            using (var md5 = MD5.Create())
            {
                FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                var hashResult = md5.ComputeHash(fs);
                fs.Close();
                return BitConverter.ToString(hashResult).Replace("-", "").ToLowerInvariant();
            }
        }
        public void SendHeader(string sHttpVersion, string sMIMEHeader, long iTotBytes, string sStatusCode, ref Socket mySocket)
        {
            String sBuffer = "";
            // if Mime type is not provided set default to text/html  
            if (sMIMEHeader.Length == 0)
                 sMIMEHeader = "text/html";// Default Mime Type is text/html         
            sBuffer = sBuffer + sHttpVersion + sStatusCode + "\r\n";
            sBuffer = sBuffer + "Server: cx1193719-b\r\n";
            sBuffer = sBuffer + "Content-Type: " + sMIMEHeader + "\r\n";
            sBuffer = sBuffer + "Accept-Ranges: bytes\r\n";
            sBuffer = sBuffer + "Content-Length: " + iTotBytes + "\r\n\r\n";
            Byte[] bSendData = Encoding.ASCII.GetBytes(sBuffer);
            SendToBrowser(bSendData, ref mySocket);
            //Console.WriteLine("Total Bytes : " + iTotBytes.ToString());
        }

        bool is_running = false;
        int x = 0;

        private void SetText(string text)
        {
            if (lblProgress.Location.X-90 > x)
                x = lblProgress.Location.X - 90;

            Invoke(new Action(() => { lblProgress.Text = text; lblProgress.Location = new Point(x, lblProgress.Location.Y); }));
        }


        public void StartListen()
        {
            int iStartPos = 0;
            String sRequest;
            String sDirName;
            String sRequestedFile;
            String sErrorMessage;
            String sPhysicalFilePath = "";
            String sMimeType = "application/octet-stream";

            Socket mySocket = null;
            while (is_running)
            {
                try
                {
                    if (!myListener.Pending())
                    {
                        Thread.Sleep(500); // choose a number (in milliseconds) that makes sense
                        continue; // skip to next iteration of loop
                    }
                }
                catch
                {
                    is_running = false;
                    return;
                }
                //Accept a new connection  
                mySocket = myListener.AcceptSocket();
                Console.WriteLine("Socket Type " + mySocket.SocketType);
                if (mySocket.Connected)
                {
                    Console.WriteLine("\nClient Connected!!\n==================\nCLient IP {0}\n", mySocket.RemoteEndPoint);
                    //make a byte array and receive data from the client   
                    Byte[] bReceive = new Byte[1024];
                    int i = mySocket.Receive(bReceive, bReceive.Length, 0);
                    //Convert Byte to String  
                    string sBuffer = Encoding.ASCII.GetString(bReceive);
                    //At present we will only deal with GET type  
                    if (sBuffer.Substring(0, 3) != "GET")
                    {
                        Console.WriteLine("Only Get Method is supported..");
                        mySocket.Close();
                        continue;
                    }
                    // Look for HTTP request  
                    iStartPos = sBuffer.IndexOf("HTTP", 1);
                    // Get the HTTP text and version e.g. it will return "HTTP/1.1"  
                    string sHttpVersion = sBuffer.Substring(iStartPos, 8);
                    // Extract the Requested Type and Requested file/directory  
                    sRequest = sBuffer.Substring(0, iStartPos - 1);
                    //Replace backslash with Forward Slash, if Any  
                    //If file name is not supplied add forward slash to indicate   
                    //that it is a directory and then we will look for the   
                    //default file name..
                    //
                    if ((sRequest.IndexOf(".") < 1) && (!sRequest.EndsWith("/")))
                    {
                        sRequest = sRequest + "/";
                    }
                    //Extract the requested file name  
                    iStartPos = sRequest.LastIndexOf("/") + 1;
                    sRequestedFile = sRequest.Substring(iStartPos);
                    //Extract The directory Name  

                    sDirName = sRequest.Substring(sRequest.IndexOf("/"), sRequest.LastIndexOf("/") - 3);

                    Console.WriteLine("Directory Requested : " + sDirName);
                    //If the physical directory does not exists then

                    // dispaly the error message  
                    Console.WriteLine("dir: " + sDirName);


                    if (sRequestedFile.Contains("api.php?page="))
                    {
                        string file_numb = sRequestedFile.Remove(0, 13);
                        Console.WriteLine("page" + file_numb + ".json");
                        if (System.IO.File.Exists("page" + file_numb + ".json"))
                        {
                            string con = System.IO.File.ReadAllText("page" + file_numb + ".json");
                            //Send to HTTP Page and set header
                            SendHeader(sHttpVersion, "application/json", con.Length, " 200 OK", ref mySocket);
                            SendToBrowser(con, ref mySocket);
                        }
                        else
                        {
                            sErrorMessage = "<H2>404 Error! File Does Not Exists...</H2>";
                            SendHeader(sHttpVersion, "text/html", sErrorMessage.Length, " 404 Not Found", ref mySocket);
                            SendToBrowser(sErrorMessage, ref mySocket);
                        }
                        mySocket.Close();
                        continue;
                    }
                    //download.php?tid

                    if (sRequestedFile.Contains("download.php?tid"))
                    {

                        string json = "{\"number_of_downloads\":\"" + 0 /* LATER MAY ACTUAL DO SMTH */ + "\"}";
                        SendHeader(sHttpVersion, "application/json", json.Length, " 200 OK", ref mySocket);
                        SendToBrowser(json, ref mySocket);

                        SetText("Last Served File: " + sRequestedFile);
                        mySocket.Close();
                        continue;
                    }

                    if (sRequestedFile.Contains("api.php?db_check_hash"))
                    {

                        string json = "{\"hash\":\"" + md5_file("./store.db") + "\"}";
                        SendHeader(sHttpVersion, "application/json", json.Length, " 200 OK", ref mySocket);
                        SendToBrowser(json, ref mySocket);

                        SetText("Last Served File: " + sRequestedFile);
                        mySocket.Close();
                        continue;
                    }

                    if (sRequestedFile.Length == 0)
                    {
                        // Get the default filename  
                        // sRequestedFile = GetTheDefaultFileName(sLocalDir);
                        if (sRequestedFile == "")
                        {
                            Console.WriteLine("file: " + sRequestedFile);
                            sErrorMessage = "<H2>Error!! No Default File Name Specified</H2>";
                            SendHeader(sHttpVersion, "", sErrorMessage.Length, " 404 Not Found", ref mySocket);
                            SendToBrowser(sErrorMessage, ref mySocket);
                            mySocket.Close();
                            continue;
                        }
                    }

                    if (sRequestedFile.Contains(".html") || sRequestedFile.Contains(".js") || sRequestedFile.Contains(".bin") || sDirName == "/update/" || sRequestedFile == "store.db" || sDirName.Contains("storedata"))
                    {
                        sPhysicalFilePath = serverRoot + sDirName + sRequestedFile;
                        //MessageBox.Show(sPhysicalFilePath);
                    }
                    else
                    {
                        if (!sDirName.Contains("/network_drive/"))
                            sPhysicalFilePath = drive + sDirName.Replace("%20", " ") + sRequestedFile.Replace("%20", " ");
                        else if (sDirName.Contains("/network_drive/"))
                        {
                            Console.WriteLine("B4 workaround PATH: " + sDirName.Replace("%20", " ") + sRequestedFile.Replace("%20", " "));
                            sPhysicalFilePath = sDirName.Replace("/network_drive/", @"\\") + sRequestedFile.Replace("/network_drive/", @"\\");
                            Console.WriteLine("after workaround: " + sPhysicalFilePath);
                        }
                    }

                    if (sRequestedFile.Contains(".html"))
                        sMimeType = "text/html";
                    else if (sRequestedFile.Contains(".js"))
                        sMimeType = "text/javascript";

                    Console.WriteLine("File Requested : " + sPhysicalFilePath);

                    if (System.IO.File.Exists(sPhysicalFilePath) == false)
                    {
                        sErrorMessage = "<H2>404 Error! File Does Not Exists...</H2>";
                        SendHeader(sHttpVersion, "", sErrorMessage.Length, " 404 Not Found", ref mySocket);
                        SendToBrowser(sErrorMessage, ref mySocket);
                        mySocket.Close();
                        continue;
                    }
                    else
                    {
                        FileInfo fi = new FileInfo(sPhysicalFilePath);
                        int bytesRead = 0;
                        FileStream inputTempFile = new FileStream(sPhysicalFilePath, FileMode.Open, FileAccess.Read);
                        byte[] Array_buffer = new byte[80 * 1024 * 1024];
                        SendHeader(sHttpVersion, sMimeType, fi.Length, " 200 OK", ref mySocket);
                        while ((bytesRead = inputTempFile.Read(Array_buffer, 0, 80 * 1024 * 1024)) > 0)
                        {
                            SendToBrowser(Array_buffer, ref mySocket);
                        }

                        inputTempFile.Close();
                        mySocket.Close();
                    }


                    SetText("Last Served File: " + sRequestedFile);
                }
            }

            if (!is_running && mySocket != null)
                mySocket.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                th.Abort();
                th2.Abort();
                th3.Abort();
                th4.Abort();
                myListener.Stop();
            }
            catch (Exception) { }

            try
            {
                ini.IniWriteValue("network", "ps4ip", metroTextBox1.Text);
                ini.IniWriteValue("network", "pkgPath", pkgText.Text);
            }
            catch (Exception) { }
        }

     
        public static IEnumerable<string> GetFiles(string root, string searchPattern) //Required to ignore folders that require admin privileges
        {
            Stack<string> pending = new Stack<string>();
            pending.Push(root);
            while (pending.Count != 0)
            {
                var path = pending.Pop();
                string[] next = null;
                try
                {
                    next = Directory.GetFiles(path, searchPattern);
                }
                catch { }
                if (next != null && next.Length != 0)
                    foreach (var file in next) yield return file;
                try
                {
                    next = Directory.GetDirectories(path);
                    foreach (var subdir in next) pending.Push(subdir);
                }
                catch { }
            }
        }
        private void clearDatabase()
        {
            try
            {
                SQLiteCommand sql_cmd = sql_con.CreateCommand();
                sql_cmd.CommandText = "DELETE FROM homebrews;";
                sql_cmd.ExecuteNonQuery();

            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.ToString());
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string hostName = Dns.GetHostName(); // Retrive the Name of HOST
            Console.WriteLine(hostName);
            // Get the IP
            string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
            metroTextBox1.Text = myIP;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                ini.IniWriteValue("network", "ps4ip", metroTextBox1.Text);
                ini.IniWriteValue("network", "pkgPath", pkgText.Text);
            }
            catch (Exception) { }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            pkgText.Text = dialog.SelectedPath;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (pkgText.Text != "" && pkgText.Text != null && pkgText.Text != string.Empty && metroTextBox1.Text != "" && metroTextBox1.Text != null && metroTextBox1.Text != string.Empty)
            {

                //Multiple Threads to Download more than one pkg file 
                th = new Thread(new ThreadStart(StartListen));
                th2 = new Thread(new ThreadStart(StartListen));
                th3 = new Thread(new ThreadStart(StartListen));
                th4 = new Thread(new ThreadStart(StartListen));

                drive = Directory.GetDirectoryRoot(pkgText.Text);
                if (!is_running)
                {
                    try
                    {
                        groupControl1.Enabled = false;
                        networkGroup.Enabled = false;
                        //IPAddress localAddr = IPAddress.Parse(metroTextBox1.Text);
                        myListener = new TcpListener(80);
                        myListener.Start();
                        Console.WriteLine("Web Server Running...");
                        //start the thread which calls the method 'StartListen'    
                        is_running = true;
                        //Task.Factory.StartNew(StartListen);

                        //Multi Threading Start
                        th.Start();
                        th2.Start();
                        th3.Start();
                        th4.Start();

                        lblProgress.Text = "Running...";
                        lblProgress.ForeColor = Color.Lime;

                    }
                    catch (Exception xx)
                    {
                        groupControl1.Enabled = true;
                        networkGroup.Enabled = true;
                        lblProgress.Location = new Point(x + 90, lblProgress.Location.Y);
                        Console.WriteLine("An Exception Occurred while Listening :" + xx.ToString());
                        lblProgress.Text = "Listening Error";
                        MessageBox.Show("Listening Error: " + xx.ToString());
                        lblProgress.ForeColor = Color.Red;
                        is_running = false;
                    }
                }
                else
                {
                    // set everything back to normal
                    lblProgress.Location = new Point(x+90, lblProgress.Location.Y);
                    groupControl1.Enabled = true;
                    networkGroup.Enabled = true;
                    lblProgress.ForeColor = Color.Red;
                    lblProgress.Text = "Stopping ...";
                    myListener.Stop();
                    th.Abort();
                    th2.Abort();
                    th3.Abort();
                    th4.Abort();
                    lblProgress.Text = "Stopped";
                    is_running = false;
                }
            }
            else
            {
                MessageBox.Show("IP or Pkg Path is empty or invalid", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void groupControl1_DragDrop_1(object sender, DragEventArgs e)
        {
            string[] pkgFolder = (string[])e.Data.GetData(DataFormats.FileDrop);
            pkgText.Text = pkgFolder[0];
        }
        private void groupControl1_DragEnter_1(object sender, DragEventArgs e)
        {
            e.Effect = System.Windows.Forms.DragDropEffects.All;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            clearDatabase();
            if (pkgText.Text != "" && pkgText.Text != null && pkgText.Text != string.Empty && metroTextBox1.Text != "" && metroTextBox1.Text != null && metroTextBox1.Text != string.Empty)
            {
                string tmpPath = pkgText.Text;
                IEnumerable<string> PkgFiles = GetFiles(tmpPath, "*.pkg");
                int counter = 0;
                progressBar1.Value = 0;
                progressBar1.Maximum = PkgFiles.Count();
                //SQLiteConnection sql_con = new SQLiteConnection("Data Source=store.db;");
                int i = 0;
                foreach (string pkgTitle in PkgFiles)
                {
                    try
                    {
                        var pkgfile = PS4_Tools.PKG.SceneRelated.Read_PKG(pkgTitle);
                        pictureBox1.Image = BytesToBitmap(pkgfile.Image);

                        // safety check
                        string tid = pkgfile.Param.TitleID;
                        if (string.IsNullOrEmpty(tid))
                            tid = "UTID000" + i++;
                        string title = pkgfile.PS4_Title;
                        if (string.IsNullOrEmpty(title))
                            title = "Unknown title: 000" + i;

                        label9.Text = title;
                        label10.Text = tid;
                        label11.Text = calcsize(pkgTitle);
                        label12.Text = "storedata/ " + tid + ".png";
                        label13.Text = pkgfile.Param.APP_VER;
                        label14.Text = pkgfile.Param.PlaystationVersion.ToString();


                        if (!Directory.Exists(serverRoot + "/storedata"))
                            Directory.CreateDirectory(serverRoot + "/storedata");

                        SavePic(BytesToBitmap(pkgfile.Icon), serverRoot + "/storedata/" + tid + ".png");

                        //start listing on the given port  
                        int rc = count_rows() + 1;
                        string pkg = "http://" + metroTextBox1.Text + "/" + pkgTitle.Substring(3).Replace(@"\", "/");
                        if (pkgText.Text.Contains(@"\\"))
                        {
                            pkg = "http://" + metroTextBox1.Text + "/network_drive/" + pkgTitle.Replace(@"\", "/");
                        }

                        //sql_con.Open();
                        SQLiteCommand sql_cmd = sql_con.CreateCommand();
                        sql_cmd.CommandText = "INSERT INTO homebrews (pid, id, name, desc, image, package, version, picpath, desc_1, desc_2, ReviewStars, Size, Author, apptype, pv, main_icon_path, main_menu_pic, releaseddate,number_downloads) VALUES(@pid, @id, @name, @desc, @image, @package, @version, @picpath, @desc_1, @desc_2, @ReviewStars, @Size, @Author, @apptype, @pv, @main_icon_path, @main_menu_pic, @releaseddate, @number_downloads);";
                        sql_cmd.Parameters.AddWithValue("@pid", rc.ToString());
                        sql_cmd.Parameters.AddWithValue("@id", tid);
                        sql_cmd.Parameters.AddWithValue("@name", title);
                        sql_cmd.Parameters.AddWithValue("@desc", "PKG Added via Store CDN tool");
                        sql_cmd.Parameters.AddWithValue("@image", "http://" + metroTextBox1.Text + "/storedata/" + tid + ".png");
                        sql_cmd.Parameters.AddWithValue("@package", pkg);
                        sql_cmd.Parameters.AddWithValue("@version", pkgfile.Param.APP_VER);
                        sql_cmd.Parameters.AddWithValue("@picpath", "/user/app/NPXS39041/storedata/" + tid + ".png");
                        sql_cmd.Parameters.AddWithValue("@desc_1", "");
                        sql_cmd.Parameters.AddWithValue("@desc_2", "");
                        sql_cmd.Parameters.AddWithValue("@ReviewStars", "0/5");
                        sql_cmd.Parameters.AddWithValue("@Size", calcsize(pkgTitle));
                        sql_cmd.Parameters.AddWithValue("@Author", "Store Tool");
                        sql_cmd.Parameters.AddWithValue("@apptype", apptype_info(pkgfile.PKG_Type));
                        sql_cmd.Parameters.AddWithValue("@pv", pkgfile.Param.PlaystationVersion);
                        sql_cmd.Parameters.AddWithValue("@main_icon_path", "http://" + metroTextBox1.Text + "/storedata/" + tid + ".png");
                        sql_cmd.Parameters.AddWithValue("@main_menu_pic", "/user/app/NPXS39041/storedata/" + tid + ".png");
                        sql_cmd.Parameters.AddWithValue("@releaseddate", DateTime.Now.ToString());
                        sql_cmd.Parameters.AddWithValue("@number_downloads", "0");
                        sql_cmd.ExecuteNonQuery();
                        //main_icon_path
                        //sql_con.Dispose();
                        counter++;
                        //MessageBox.Show("App: " + pkgfile.Param.Title + ", has been successfully Added to the Database\n\nPlease copy the PKG to the pkgs folder in this tools root folder");
                    }
                    catch (Exception ex) { }
                    progressBar1.Value = (int)progressBar1.Value + 1;
                    progressBar1.Update();
                    progressBar1.Refresh();

                }

                PkgCount.Text = "Found " + counter.ToString() + " Valid Pkg(s) of " + PkgFiles.Count().ToString();
                counter = 0;
            }
            else
            {
                MessageBox.Show("IP or Pkg Path is empty or invalid", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool add_to_start(bool add = true)
        {
            try
            {
                WshShell wshShell = new WshShell();
                IWshRuntimeLibrary.IWshShortcut shortcut;
                string startUpFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                if (add)
                {
                    // Create the shortcut
                    shortcut = (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(startUpFolderPath + "\\" + Application.ProductName + ".lnk");

                    shortcut.TargetPath = Application.ExecutablePath;
                    shortcut.WorkingDirectory = Application.StartupPath;
                    shortcut.Description = "Store CDN Server";
                    // shortcut.IconLocation = Application.StartupPath + @"\App.ico";
                    shortcut.Save();
                }
                else
                {
                    System.IO.File.Delete(startUpFolderPath + "\\" + Application.ProductName + ".lnk");
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        private void button1_Click_3(object sender, EventArgs e)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (!isautostart)
            {
                try
                {
                    if (rk == null)
                        throw new ArgumentException("rip reg key is null");

                    rk.SetValue("Store CDN Server", Application.ExecutablePath);

                    MessageBox.Show("CDN Server will now start on Windows Startup");
                    isautostart = true;
                }
                catch
                {
                    try
                    {
                        add_to_start(true);
                    }
                    catch
                    {
                        MessageBox.Show("Failed to make shortcut");

                    }
                    MessageBox.Show("An error has occured when trying to set the reg key");
                }
            }
            else
            {
                try
                {
                    if (rk == null)
                        throw new ArgumentException("rip reg key is null");

                    rk.DeleteValue("Store CDN Server", false);
                    MessageBox.Show("Booting on statup is now disabled");
                    isautostart = false;
                }
                catch
                {
                    try
                    {
                        add_to_start(false);
                    }
                    catch { };

                    MessageBox.Show("An error has occured when trying to delete the reg key");
                }
            }
            button1.Text = isautostart ? "Disable" : "Enable";
            button1.ForeColor = isautostart ? Color.Red : Color.Green;
        }
    }
}

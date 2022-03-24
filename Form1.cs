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

/*Copyright Darksoftware (c) 2019*/

namespace DesktopApp1
{
    public partial class Form1 : System.Windows.Forms.Form
    {

        private TcpListener myListener;
        SQLiteConnection sql_con = new SQLiteConnection("Data Source=store.db;");
        private Ini.IniFile ini = new Ini.IniFile(Application.StartupPath + @"\settings.ini");

        public Form1()
        {
            InitializeComponent();
            groupControl1.AllowDrop = true;
            metroTextBox1.Text = Properties.Settings.Default.IP;

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {


        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void helloWorldLabel_Click(object sender, EventArgs e)
        {

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!File.Exists("store.db"))
            {
                SQLiteConnection.CreateFile("store.db");
                sql_con.Open();
                string create_db = "CREATE TABLE homebrews ( pid int UNSIGNED NOT NULL,id varchar(255) ,name varchar(255) ,desc varchar(255) ,image varchar(255) ,package varchar(255) ,version varchar(255) ,picpath varchar(255) ,desc_1 varchar(255) ,desc_2 varchar(255) ,ReviewStars varchar(255) ,Size varchar(255) ,Author varchar(255) ,apptype varchar(255) ,pv varchar(255) ,main_icon_path varchar(255) ,main_menu_pic varchar(255) ,releaseddate date DEFAULT NULL,number_downloads int NOT NULL);";
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
            //SQLiteConnection liteCon = new SQLiteConnection("Data Source=store.db;");
            //liteCon.Open();

            string query = "SELECT COUNT(*) FROM homebrews";
            int sizeOfDR = 0;
            List<string> liteEntries = new List<string>();

            var cmd = new SQLiteCommand(query, sql_con);

            object result = cmd.ExecuteScalar();
            int nTables = Convert.ToInt32(result);
            //sql_con.Close();
            //SQLiteConnection.ClearPool(sql_con);

            //sql_con.ClearCachedSettings();
            //sql_con.ReleaseMemory();
            //sql_con.Dispose();

            //SQLiteConnection.ClearAllPools();
            //
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
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
        private void metroButton2_Click(object sender, EventArgs e)
        {

            OpenFileDialog opendialog = new OpenFileDialog();
            opendialog.CheckFileExists = true;
            opendialog.Multiselect = true;
            //opendialog.AddExtension 
            opendialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            opendialog.Filter = "PS4 Package File (*.pkg) | *.pkg";
            if (opendialog.ShowDialog() == DialogResult.OK)
            {


                //Adding PS4 Tools so we can get an image pkg information ext
                //xDPx
                try
                {
                    foreach (string pkgName in opendialog.FileNames)
                    {
                        var pkgfile = PS4_Tools.PKG.SceneRelated.Read_PKG(pkgName);
                        pictureBox1.Image = BytesToBitmap(pkgfile.Image);

                        label9.Text = pkgfile.PS4_Title;
                        label10.Text = pkgfile.Param.TitleID;
                        label11.Text = calcsize(pkgName);
                        label12.Text = "storedata/ " + pkgfile.Param.TitleID + ".png";
                        label13.Text = pkgfile.Param.APP_VER;
                        label14.Text = pkgfile.Param.PlaystationVersion.ToString();


                        if (!Directory.Exists("storedata"))
                            Directory.CreateDirectory("storedata");


                        if (!Directory.Exists("pkgs"))
                            Directory.CreateDirectory("pkgs");

                        SavePic(BytesToBitmap(pkgfile.Image), "storedata/" + pkgfile.Param.TitleID + ".png");

                        //start listing on the given port  
                        int rc = count_rows() + 1;

                        SQLiteConnection sql_con = new SQLiteConnection("Data Source=store.db;");

                        sql_con.Open();
                        SQLiteCommand sql_cmd = sql_con.CreateCommand();
                        sql_cmd.CommandText = "INSERT INTO homebrews (pid, id, name, desc, image, package, version, picpath, desc_1, desc_2, ReviewStars, Size, Author, apptype, pv, main_icon_path, main_menu_pic, releaseddate,number_downloads) VALUES(@pid, @id, @name, @desc, @image, @package, @version, @picpath, @desc_1, @desc_2, @ReviewStars, @Size, @Author, @apptype, @pv, @main_icon_path, @main_menu_pic, @releaseddate, @number_downloads);";
                        sql_cmd.Parameters.AddWithValue("@pid", rc.ToString());
                        sql_cmd.Parameters.AddWithValue("@id", pkgfile.Param.TitleID);
                        sql_cmd.Parameters.AddWithValue("@name", pkgfile.PS4_Title);
                        sql_cmd.Parameters.AddWithValue("@desc", "PKG Added via Store CDN tool");
                        sql_cmd.Parameters.AddWithValue("@image", "http://" + metroTextBox1.Text + "/storedata/" + pkgfile.Param.TitleID + ".png");
                        sql_cmd.Parameters.AddWithValue("@package", "http://" + metroTextBox1.Text + "/pkgs/" + Path.GetFileName(pkgName));
                        sql_cmd.Parameters.AddWithValue("@version", pkgfile.Param.APP_VER);
                        sql_cmd.Parameters.AddWithValue("@picpath", "/user/app/NPXS39041/storedata/" + pkgfile.Param.TitleID + ".png");
                        sql_cmd.Parameters.AddWithValue("@desc_1", "");
                        sql_cmd.Parameters.AddWithValue("@desc_2", "");
                        sql_cmd.Parameters.AddWithValue("@ReviewStars", "0/5");
                        sql_cmd.Parameters.AddWithValue("@Size", calcsize(pkgName));
                        sql_cmd.Parameters.AddWithValue("@Author", "Store Tool");
                        sql_cmd.Parameters.AddWithValue("@apptype", "HB Game");
                        sql_cmd.Parameters.AddWithValue("@pv", pkgfile.Param.PlaystationVersion);
                        sql_cmd.Parameters.AddWithValue("@main_icon_path", "http://" + metroTextBox1.Text + "/storedata/" + pkgfile.Param.TitleID + ".png");
                        sql_cmd.Parameters.AddWithValue("@main_menu_pic", "/user/app/NPXS39041/storedata/" + pkgfile.Param.TitleID + ".png");
                        sql_cmd.Parameters.AddWithValue("@releaseddate", DateTime.Now.ToString());
                        sql_cmd.Parameters.AddWithValue("@number_downloads", "0");
                        sql_cmd.ExecuteNonQuery();
                        //main_icon_path
                        sql_con.Close();
                        MessageBox.Show("App: " + pkgfile.Param.Title + ", has been successfully Added to the Database\n\nPlease copy the PKG to the pkgs folder in this tools root folder");
                    }


                }
                catch (Exception ee)
                {
                    MessageBox.Show("Invaild Package!");
                }


            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }

        public string GetTheDefaultFileName(string sLocalDirectory)
        {
            StreamReader sr;
            String sLine = "";
            try
            {
                //Open the default.dat to find out the list  
                // of default file  
                sr = new StreamReader("data/Default.Dat");
                while ((sLine = sr.ReadLine()) != null)
                {
                    //Look for the default file in the web server root folder  
                    if (File.Exists(sLocalDirectory + sLine) == true)
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An Exception Occurred : " + e.ToString());
            }
            if (File.Exists(sLocalDirectory + sLine) == true)
                return sLine;
            else
                return "";
        }

        public string GetLocalPath(string sMyWebServerRoot, string sDirName)
        {
            StreamReader sr;
            String sLine = "";
            String sVirtualDir = "";
            String sRealDir = "";
            int iStartPos = 0;
            //Remove extra spaces  
            sDirName.Trim();
            // Convert to lowercase  
            sMyWebServerRoot = sMyWebServerRoot.ToLower();
            // Convert to lowercase  
            sDirName = sDirName.ToLower();
            try
            {
                //Open the Vdirs.dat to find out the list virtual directories  
                sr = new StreamReader("data/VDirs.Dat");
                while ((sLine = sr.ReadLine()) != null)
                {
                    //Remove extra Spaces  
                    sLine.Trim();
                    if (sLine.Length > 0)
                    {
                        //find the separator  
                        iStartPos = sLine.IndexOf(";");
                        // Convert to lowercase  
                        sLine = sLine.ToLower();
                        sVirtualDir = sLine.Substring(0, iStartPos);
                        sRealDir = sLine.Substring(iStartPos + 1);
                        if (sVirtualDir == sDirName)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An Exception Occurred : " + e.ToString());
            }
            if (sVirtualDir == sDirName)
                return sRealDir;
            else
                return "";
        }

        public string GetMimeType(string sRequestedFile)
        {
            StreamReader sr;
            String sLine = "";
            String sMimeType = "";
            String sFileExt = "";
            String sMimeExt = "";
            // Convert to lowercase  
            sRequestedFile = sRequestedFile.ToLower();
            int iStartPos = sRequestedFile.IndexOf(".");
            sFileExt = sRequestedFile.Substring(iStartPos);
            try
            {
                //Open the Vdirs.dat to find out the list virtual directories  
                sr = new StreamReader("data/Mime.Dat");
                while ((sLine = sr.ReadLine()) != null)
                {
                    sLine.Trim();
                    if (sLine.Length > 0)
                    {
                        //find the separator  
                        iStartPos = sLine.IndexOf(";");
                        // Convert to lower case  
                        sLine = sLine.ToLower();
                        sMimeExt = sLine.Substring(0, iStartPos);
                        sMimeType = sLine.Substring(iStartPos + 1);
                        if (sMimeExt == sFileExt)
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An Exception Occurred : " + e.ToString());
            }
            if (sMimeExt == sFileExt)
                return sMimeType;
            else
                return "";
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


                //using (FileStream stream = File.OpenRead(file))
                //{
                //    var hashResult = md5.ComputeHash(stream);
                //    stream.Close();
                //    return BitConverter.ToString(hashResult).Replace("-", "").ToLowerInvariant(); md5.ComputeHash(stream);
                //}
            }
        }
        public void SendHeader(string sHttpVersion, string sMIMEHeader, long iTotBytes, string sStatusCode, ref Socket mySocket)
        {
            String sBuffer = "";
            // if Mime type is not provided set default to text/html  
            if (sMIMEHeader.Length == 0)
            {
                sMIMEHeader = "text/html";// Default Mime Type is text/html  
            }
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

        private void SetText(string text)
        {
            Invoke(new Action(() => { lblProgress.Text = text; }));
        }


        public void StartListen()
        {
            int iStartPos = 0;
            String sRequest;
            String sDirName;
            String sRequestedFile;
            String sErrorMessage;
            String sLocalDir;
            String sMyWebServerRoot = "./";
            String sPhysicalFilePath = "";
            String sFormattedMessage = "";
            String sResponse = "";
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
                        if (File.Exists("page" + file_numb + ".json"))
                        {
                            string con = File.ReadAllText("page" + file_numb + ".json");
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

                    String sMimeType = "text/html";//GetMimeType(sRequestedFile);
                    //Build the physical path

                    if (sDirName == "/update/" || sRequestedFile.Contains("store.db") || sDirName == "/storedata/")
                        sPhysicalFilePath = "./" + sDirName + sRequestedFile;
                    else
                        sPhysicalFilePath = serverRoot + sDirName.Replace("%20", " ") + sRequestedFile.Replace("%20", " ");
                    Console.WriteLine("File Requested : " + sPhysicalFilePath);


                    if (File.Exists(sPhysicalFilePath) == false)
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
                        FileStream inputTempFile = new FileStream(sPhysicalFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        byte[] Array_buffer = new byte[80 * 1024 * 1024];
                        SendHeader(sHttpVersion, "application/octet-stream", fi.Length, " 200 OK", ref mySocket);
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
        //Thread th = null;



        private void metroButton1_Click(object sender, EventArgs e)
        {


        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void lblProgress_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void metroTextBox1_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
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

        string serverRoot = "";

        private void metroButton3_Click_1(object sender, EventArgs e)
        {



        }

        private void button2_Click_1(object sender, EventArgs e)
        {

        }

        private void button1_Click_2(object sender, EventArgs e)
        {

        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {

        }

        private void groupControl1_DragDrop(object sender, DragEventArgs e)
        {

        }

        private void groupControl1_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void addPkgs_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void clearDB_CheckedChanged(object sender, EventArgs e)
        {

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

        private void addPkgs_Click(object sender, EventArgs e)
        {

        }

        private void clearDatabase()
        {
            try
            {
                SQLiteCommand sql_cmd = sql_con.CreateCommand();
                sql_cmd.CommandText = "DELETE FROM homebrews;";
                sql_cmd.ExecuteNonQuery();
                //sql_con.Close();
                //SQLiteConnection.ClearPool(sql_con);

                //sql_con.ClearCachedSettings();
                //sql_con.ReleaseMemory();
                //sql_con.Dispose();

                //SQLiteConnection.ClearAllPools();

                //GC.Collect();
                //GC.WaitForPendingFinalizers();

            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.ToString());
            }
        }

        private void metroButton1_CheckedChanged(object sender, EventArgs e)
        {


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
                Thread th = new Thread(new ThreadStart(StartListen));

                Thread th2 = new Thread(new ThreadStart(StartListen));

                Thread th3 = new Thread(new ThreadStart(StartListen));

                Thread th4 = new Thread(new ThreadStart(StartListen));

                serverRoot = Directory.GetDirectoryRoot(pkgText.Text);
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

                        lblProgress.Text = "                           Running...";
                        lblProgress.ForeColor = Color.Lime;
                    }
                    catch (Exception xx)
                    {
                        groupControl1.Enabled = true;
                        networkGroup.Enabled = true;
                        Console.WriteLine("An Exception Occurred while Listening :" + xx.ToString());
                        lblProgress.Text = "Listening Error: " + xx.ToString();
                        lblProgress.ForeColor = Color.Red;
                        is_running = false;
                    }
                }
                else
                {
                    groupControl1.Enabled = true;
                    networkGroup.Enabled = true;
                    lblProgress.ForeColor = Color.Red;
                    lblProgress.Text = "                           Stopping ...";
                    myListener.Stop();
                    th.Abort();
                    lblProgress.Text = "                           Stopped";
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
                serverRoot = Directory.GetDirectoryRoot(tmpPath);
                IEnumerable<string> PkgFiles = GetFiles(tmpPath, "*.pkg");
                int counter = 0;
                progressBar1.Value = 0;
                progressBar1.Maximum = PkgFiles.Count();
                //SQLiteConnection sql_con = new SQLiteConnection("Data Source=store.db;");
                foreach (string pkgTitle in PkgFiles)
                {
                    try
                    {
                        var pkgfile = PS4_Tools.PKG.SceneRelated.Read_PKG(pkgTitle);
                        pictureBox1.Image = BytesToBitmap(pkgfile.Image);

                        label9.Text = pkgfile.PS4_Title;
                        label10.Text = pkgfile.Param.TitleID;
                        label11.Text = calcsize(pkgTitle);
                        label12.Text = "storedata/ " + pkgfile.Param.TitleID + ".png";
                        label13.Text = pkgfile.Param.APP_VER;
                        label14.Text = pkgfile.Param.PlaystationVersion.ToString();


                        if (!Directory.Exists("storedata"))
                            Directory.CreateDirectory("storedata");

                        SavePic(BytesToBitmap(pkgfile.Icon), "storedata/" + pkgfile.Param.TitleID + ".png");

                        //start listing on the given port  
                        int rc = count_rows() + 1;

                        //sql_con.Open();
                        SQLiteCommand sql_cmd = sql_con.CreateCommand();
                        sql_cmd.CommandText = "INSERT INTO homebrews (pid, id, name, desc, image, package, version, picpath, desc_1, desc_2, ReviewStars, Size, Author, apptype, pv, main_icon_path, main_menu_pic, releaseddate,number_downloads) VALUES(@pid, @id, @name, @desc, @image, @package, @version, @picpath, @desc_1, @desc_2, @ReviewStars, @Size, @Author, @apptype, @pv, @main_icon_path, @main_menu_pic, @releaseddate, @number_downloads);";
                        sql_cmd.Parameters.AddWithValue("@pid", rc.ToString());
                        sql_cmd.Parameters.AddWithValue("@id", pkgfile.Param.TitleID);
                        sql_cmd.Parameters.AddWithValue("@name", pkgfile.PS4_Title);
                        sql_cmd.Parameters.AddWithValue("@desc", "PKG Added via Store CDN tool");
                        sql_cmd.Parameters.AddWithValue("@image", "http://" + metroTextBox1.Text + "/storedata/" + pkgfile.Param.TitleID + ".png");
                        sql_cmd.Parameters.AddWithValue("@package", "http://" + metroTextBox1.Text + "/" + pkgTitle.Substring(3).Replace(@"\", "/"));
                        sql_cmd.Parameters.AddWithValue("@version", pkgfile.Param.APP_VER);
                        sql_cmd.Parameters.AddWithValue("@picpath", "/user/app/NPXS39041/storedata/" + pkgfile.Param.TitleID + ".png");
                        sql_cmd.Parameters.AddWithValue("@desc_1", "");
                        sql_cmd.Parameters.AddWithValue("@desc_2", "");
                        sql_cmd.Parameters.AddWithValue("@ReviewStars", "0/5");
                        sql_cmd.Parameters.AddWithValue("@Size", calcsize(pkgTitle));
                        sql_cmd.Parameters.AddWithValue("@Author", "Store Tool");
                        sql_cmd.Parameters.AddWithValue("@apptype", "HB Game");
                        sql_cmd.Parameters.AddWithValue("@pv", pkgfile.Param.PlaystationVersion);
                        sql_cmd.Parameters.AddWithValue("@main_icon_path", "http://" + metroTextBox1.Text + "/storedata/" + pkgfile.Param.TitleID + ".png");
                        sql_cmd.Parameters.AddWithValue("@main_menu_pic", "/user/app/NPXS39041/storedata/" + pkgfile.Param.TitleID + ".png");
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

                //sql_con.Close();
                //SQLiteConnection.ClearPool(sql_con);

                //sql_con.ClearCachedSettings();
                //sql_con.Dispose();

                //SQLiteConnection.ClearAllPools();

                //GC.Collect();
                //GC.WaitForPendingFinalizers();

                PkgCount.Text = "Found " + counter.ToString() + " Valid Pkg(s) of " + PkgFiles.Count().ToString();
                counter = 0;
            }
            else
            {
                MessageBox.Show("IP or Pkg Path is empty or invalid", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using HtmlAgilityPack;
using Lumex.Tech;

using System.Net;
using System.Timers;
using Microsoft.Win32;
using System.Security;


namespace LEDApp
{

    public partial class HomePage : Form
    {

        // this timer calls bgWorker again and again after regular intervals
        //System.Windows.Forms.Timer tmrCallBgWorker;

        // this is our worker
        BackgroundWorker bgWorker;

        // this is the timer to make sure that worker gets called
        //System.Threading.Timer tmrEnsureWorkerGetsCalled;

        // object used for safe access
        object lockObject = new object();
        RegistryKey add = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        private bool CheckClick = false;
        private bool IsRunAsAdministrator()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);

            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }


        string connectionString = ConfigurationManager.ConnectionStrings["LumexDBConString"].ConnectionString;
        SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["AppKey"].ConnectionString);
        string path =  @"C:\temp\ConnectionString.txt";
        public HomePage()
        {
            InitializeComponent();

            //WriteConnectionStringToLogFile(connectionString, logFile);
            using (var fs = new FileStream(path, FileMode.Truncate))
            {
            }

            StreamWriter objWriter = new StreamWriter(path, true);
            objWriter.WriteLine(connectionString);
            objWriter.Close();


            string AppKeyStatic = "KxYU-LUm3-Ts34-XSOP";
            string AppKeyFromDB = "";
            string status = "";
            string Mac = "";


            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            String sMacAddress = string.Empty;
            foreach (NetworkInterface adapter in nics)
            {
                if (sMacAddress == String.Empty)// only return MAC Address from first card  
                {
                    //IPInterfaceProperties properties = adapter.GetIPProperties(); Line is not required
                    sMacAddress = adapter.GetPhysicalAddress().ToString();
                }
            }


            conn.Open();

            string query = "SELECT * FROM [dbo].[AppsKey]";
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            conn.Close();
            foreach (DataRow row in dt.Rows)
            {
                AppKeyFromDB = row["AppKey"].ToString();
                status = row["Status"].ToString();
                Mac = row["Mac"].ToString();
            }

            if (Mac == "" && AppKeyStatic == AppKeyFromDB && status == "Yes")
            {
                conn.Open();

                query = "Update [dbo].[AppsKey] Set Mac='" + sMacAddress + "'";
                cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                add.SetValue("LEDApp", "\"" + Application.ExecutablePath.ToString() + "\"");


                // this is our worker
                bgWorker = new BackgroundWorker();
                table.Columns.Add(new DataColumn("Id"));
                table.Columns.Add(new DataColumn("Url"));
                table.Columns.Add(new DataColumn("ScheduleTime"));
                table.Columns.Add(new DataColumn("UserName"));
                table.Columns.Add(new DataColumn("Password"));
                // work happens in this method
                bgWorker.DoWork += new DoWorkEventHandler(bg_DoWork);

                bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);
                bgWorker.RunWorkerAsync();

                btnClearData.Enabled = true;
                btnGetImageFromWeb.Enabled = true;
                btnSendDuelLine.Enabled = true;
                // Application.Exit();
                //tmrCallBgWorker.Start();
            }
            else
            {
                if (sMacAddress == Mac && AppKeyStatic == AppKeyFromDB && status == "Yes")
                {
                    add.SetValue("LEDApp", "\"" + Application.ExecutablePath.ToString() + "\"");


                    // this is our worker
                    bgWorker = new BackgroundWorker();
                    table.Columns.Add(new DataColumn("Id"));
                    table.Columns.Add(new DataColumn("Url"));
                    table.Columns.Add(new DataColumn("ScheduleTime"));
                    table.Columns.Add(new DataColumn("UserName"));
                    table.Columns.Add(new DataColumn("Password"));
                    // work happens in this method
                    bgWorker.DoWork += new DoWorkEventHandler(bg_DoWork);

                    bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_RunWorkerCompleted);
                    bgWorker.RunWorkerAsync();

                    // Application.Exit();
                    //tmrCallBgWorker.Start();

                    btnClearData.Enabled = true;
                    btnGetImageFromWeb.Enabled = true;
                    btnSendDuelLine.Enabled = true;
                }
                else
                {
                    //Process.GetCurrentProcess().Kill();
                    btnClearData.Enabled = false;
                    btnGetImageFromWeb.Enabled = false;
                    btnSendDuelLine.Enabled = false;
                    Application.Exit();
                }
            }


        }
        void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                //Sleeps the Worker for Some times which we will get from database

                bgWorker.RunWorkerAsync();

            }
            catch (Exception)
            {

                throw;
            }
        }



        private string CheckStatus = "";
        private string DeviceId = "";
        private string Url = "";
        private string ScheduleTime = "";
        private string userName = "";
        private string password = "";
        private bool isUpdateMode = false;
        DataTable table = new DataTable();
        DataRow dr = null;

        void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            // does a job like writing to serial communication, webservices etc

            table.Clear();
            //Process.GetCurrentProcess().Kill();


            try
            {
                DataTable dt = GetAllDeviceInfo();
                int SleepTime = 0;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    WebClient x = new WebClient();
                    //DataTable dt = GetImageUrl(logInDeviceComBx.SelectedValue);
                    //string url = System.Configuration.ConfigurationManager.AppSettings["Host"];
                    Url = dt.Rows[i]["AreaId"].ToString();//AreaId is Web Url for downloading Image
                    DeviceId = dt.Rows[i]["DeviceId"].ToString();
                    ScheduleTime = dt.Rows[i]["ScheduleTime"].ToString();
                    userName = dt.Rows[i]["UserName"].ToString();
                    password = dt.Rows[i]["PostCode"].ToString(); // Post Code is the PassWord
                    string ImageUrl = Url + @"/status";
                    // string source = x.DownloadString(@"http://180.211.159.172/damweb/PublicPortal/commodity_name/");
                    string source = x.DownloadString(ImageUrl);

                    HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                    document.LoadHtml(source);

                    CheckStatus = document.DocumentNode.InnerHtml;

                    dr = table.NewRow();
                    if (CheckStatus == "Yes")
                    {
                        dr["Id"] = DeviceId;
                        dr["Url"] = Url;
                        dr["ScheduleTime"] = ScheduleTime;
                        dr["UserName"] = userName;
                        dr["Password"] = password;

                        table.Rows.Add(dr);
                    }

                }
                SleepTime = Convert.ToInt32(ScheduleTime) * 60 * 1000;
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    CheckClick = true;
                    DeviceId = table.Rows[i]["Id"].ToString();
                    Url = table.Rows[i]["Url"].ToString();
                    ScheduleTime = table.Rows[i]["ScheduleTime"].ToString();

                    userName = table.Rows[i]["UserName"].ToString();
                    password = table.Rows[i]["Password"].ToString();

                    this.btnLogin.Invoke(new MethodInvoker(() => InvokeOnClick(btnLogin, EventArgs.Empty)));

                    this.btnGetImageFromWeb.Invoke(new MethodInvoker(() => InvokeOnClick(btnGetImageFromWeb, EventArgs.Empty)));

                    this.btnClearData.Invoke(new MethodInvoker(() => InvokeOnClick(btnClearData, EventArgs.Empty)));

                    this.btnSendDuelLine.Invoke(new MethodInvoker(() => InvokeOnClick(btnSendDuelLine, EventArgs.Empty)));

                }
                CheckClick = false;
                Thread.Sleep(SleepTime);


            }
            catch (Exception ex)
            {
                WriteToLogFile("Exception Messsage from Bg_DoWork" + ex.ToString(), logFile);
                //this.btnLogin.Invoke(new MethodInvoker(() => InvokeOnClick(btnLogin, EventArgs.Empty)));
            }
            //System.Threading.Thread.Sleep(100);
        }

        private DataTable GetAllDeviceInfo()
        {
            LumexDBPlayer db = LumexDBPlayer.Start(true);
            DataTable dt = db.ExecuteDataTable("SELECT Device.Id , Device.DeviceId, Device.AreaId,Device.PostCode,Device.UserName, DisplaySetting.ScheduleTime FROM Device INNER JOIN DisplaySetting ON Device.DeviceId=DisplaySetting.DeviceId");
            //SqlCommand command = new SqlCommand(query, con);
            db.Stop();

            return dt;
        }




        ledcontrol.ILEDDevice PLed;
        ledcontrol.ICOMExtend PCom;//= new ledcontrol.COMControl();
        ledcontrol.IGPRSExtend PGprs;// = new ledcontrol.GPRSControl();
        ledcontrol.GPRSDevice pgsDevice;// = new ledcontrol.GPRSDevice();

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (isUpdateMode)
                {

                    LumexDBPlayer db = LumexDBPlayer.Start(true);
                    db.ExecuteNonQuery("Update Device set DeviceId='" + txtbxDeviceId.Text + "',PostCode='" +
                            txtbxPostCode.Text + "',AreaId='" + txtbxAreaId.Text + "',UserName='" + txtbxUserName.Text + "',DeviceName='" + txtbxDeviceName.Text + "' where Id='" + hiddenField.Text + "'");

                    MessageBox.Show("Device Information Updated Successfully!!!");
                    db.Stop();
                    txtbxDeviceId.Text = "";
                    txtbxPostCode.Text = "";
                    txtbxDeviceName.Text = "";
                    txtbxUserName.Text = "";
                    txtbxAreaId.Text = "";
                    btnSave.Text = "Save";
                    isUpdateMode = false;
                    txtbxDeviceId.Enabled = true;
                    ShowAllDevice();
                }
                else
                {
                    LumexDBPlayer db = LumexDBPlayer.Start(true);
                    db.ExecuteNonQuery(
                            "Insert into Device (DeviceId,PostCode,AreaId,DeviceName,UserName) values('" + txtbxDeviceId.Text + "','" +
                            txtbxPostCode.Text + "','" + txtbxAreaId.Text + "','" + txtbxDeviceName.Text + "','" + txtbxUserName.Text + "')");

                    MessageBox.Show("Device Successfully Inserted!!!");
                    db.Stop();
                    txtbxDeviceId.Text = "";
                    txtbxPostCode.Text = "";
                    txtbxDeviceName.Text = "";
                    txtbxUserName.Text = "";
                    txtbxAreaId.Text = "";
                    ShowAllDisplaySetting();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAddDevice_Click(object sender, EventArgs e)
        {
            tabControl3.SelectTab(1);
        }

        private void btnDisplaySettings_Click(object sender, EventArgs e)
        {
            tabControl3.SelectTab(2);
        }

        private void btnDeviceSetting_Click(object sender, EventArgs e)
        {
            tabControl3.SelectTab(3);
        }





        private void HomePage_Load(object sender, EventArgs e)
        {
            LoadDevice();
            ShowAllDevice();
            ShowAllDisplaySetting();

        }


        private void LoadDevice()
        {
            try
            {
                LumexDBPlayer db = LumexDBPlayer.Start(true);
                DataTable dt = db.ExecuteDataTable("SELECT Id , DeviceId ,DeviceName FROM Device");
                //SqlCommand command = new SqlCommand(query, con);
                db.Stop();

                combxDevice.DataSource = dt;
                combxDevice.DisplayMember = "DeviceName";
                combxDevice.ValueMember = "DeviceId";
                combxDevice.SelectedIndex = 0;

                logInDeviceComBx.DataSource = dt;
                logInDeviceComBx.DisplayMember = "DeviceName";
                logInDeviceComBx.ValueMember = "DeviceId";
                logInDeviceComBx.SelectedIndex = 0;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void btnSaveDisplaySetting_Click(object sender, EventArgs e)
        {

            try
            {
                if (isUpdateMode)
                {
                    LumexDBPlayer db = LumexDBPlayer.Start(true);
                    db.ExecuteNonQuery(
                            "Update DisplaySetting SET DeviceId='" + combxDevice.SelectedValue + "',Height='" + txtbxHeight.Text + "',Width='" + txtbxWidth.Text + "',Style='" +
                            combxStyle.SelectedValue + "',Animation='" + combxAnimation.SelectedValue + "',RegionTop='" + txtbxRegion1Top.Text + "',RegionLeft='" + txtbxRegion1Left.Text + "',Region2Top='" + txtbxRegion2Top.Text + "',Region2Left='" + txtbxRegion2Left.Text + "' where Id='" + hiddenField.Text + "'");
                    // cmd.ExecuteNonQuery();
                    MessageBox.Show("Display Settings Updated Successfully!!!");
                    db.Stop();
                    combxDevice.SelectedValue = 0;
                    combxStyle.SelectedValue = 0;
                    combxAnimation.SelectedValue = 0;
                    txtbxHeight.Text = "";
                    txtbxWidth.Text = "";
                    txtbxRegion1Top.Text = "";
                    txtbxRegion1Left.Text = "";
                    txtbxRegion2Top.Text = "";
                    txtbxRegion2Left.Text = "";
                    combxDevice.Enabled = true;
                    btnSaveDisplaySetting.Text = "Save";
                    ShowAllDisplaySetting();
                }
                else
                {
                    LumexDBPlayer db = LumexDBPlayer.Start(true);
                    db.ExecuteNonQuery(
                            "Insert into DisplaySetting (DeviceId,Height,Width,Style,Animation,RegionTop,RegionLeft) values('" +
                            combxDevice.SelectedValue + "','" + txtbxHeight.Text + "','" + txtbxWidth.Text + "','" +
                            combxStyle.SelectedValue + "','" + combxAnimation.SelectedValue + "','" + txtbxRegion1Top.Text + "','" + txtbxRegion1Left.Text + "','" + txtbxRegion2Top.Text + "','" + txtbxRegion2Left.Text + "')");
                    // cmd.ExecuteNonQuery();
                    MessageBox.Show("Display Settings Successfully Inserted!!!");
                    db.Stop();
                    combxDevice.SelectedValue = 0;
                    combxStyle.SelectedValue = 0;
                    combxAnimation.SelectedValue = 0;
                    txtbxHeight.Text = "";
                    txtbxWidth.Text = "";
                    txtbxRegion1Left.Text = "";
                    txtbxRegion1Top.Text = "";
                    txtbxRegion2Left.Text = "";
                    txtbxRegion2Top.Text = "";
                    ShowAllDisplaySetting();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void ShowAllDevice()
        {
            listViewDevice.Items.Clear();


            LumexDBPlayer db = LumexDBPlayer.Start(true);
            DataTable dt = db.ExecuteDataTable("SELECT Id , DeviceId , UserName , PostCode , DeviceName , AreaId FROM Device");
            db.Stop();


            //while (dt.Rows.Count > 0)
            //{
            //    device aDevice = new device();
            //    aDevice.id = (int)dt.Rows[0]["Id"];
            //    aDevice.DeviceId = (int)dt.Rows[0]["DeviceId"];
            //    aDevice.UserName = dt.Rows[0]["UserName"].ToString();
            //    aDevice.PostCode = dt.Rows[0]["PostCode"].ToString(); //Password
            //    aDevice.DeviceName = dt.Rows[0]["DeviceName"].ToString();
            //    aDevice.AreaId = dt.Rows[0]["AreaId"].ToString();
            //    deviceList.Add(aDevice);
            //}
            foreach (DataRow row in dt.Rows)
            {

                ListViewItem item = new ListViewItem(row[0].ToString());
                for (int i = 1; i < dt.Columns.Count; i++)
                {
                    item.SubItems.Add(row[i].ToString());
                }
                listViewDevice.Items.Add(item);
            }

        }


        private void listViewDevice_DoubleClick(object sender, EventArgs e)
        {
            // 1. Select selected Student

            ListViewItem item = listViewDevice.SelectedItems[0];
            int id = Convert.ToInt32(item.Text);

            //Selecting the Item of the list
            LumexDBPlayer db = LumexDBPlayer.Start(true);
            DataTable dt = db.ExecuteDataTable("SELECT Id , DeviceId , UserName , PostCode , DeviceName , AreaId FROM Device  where Id='" + id + "'");
            //SqlCommand command = new SqlCommand(query, con);
            db.Stop();
            //device aDevice = (device)item.Tag;

            //if (aDevice != null)
            //{
            //    //2. Enable update mode -- save button = update button, grab id

            isUpdateMode = true;

            //    deviceId = aDevice.id;


            //    //3. Fill Text with student data 
            btnSave.Text = "Update";
            hiddenField.Text = dt.Rows[0]["Id"].ToString();

            txtbxDeviceId.Text = dt.Rows[0]["DeviceId"].ToString();
            txtbxDeviceName.Text = dt.Rows[0]["DeviceName"].ToString();
            txtbxUserName.Text = dt.Rows[0]["UserName"].ToString();
            txtbxPostCode.Text = dt.Rows[0]["PostCode"].ToString();
            txtbxAreaId.Text = dt.Rows[0]["AreaId"].ToString();


        }

        public void ShowAllDisplaySetting()
        {
            listViewDisplay.Items.Clear();

            LumexDBPlayer db = LumexDBPlayer.Start(true);
            DataTable dt = db.ExecuteDataTable("SELECT Id , DeviceId , Height , Width , Style , Animation,RegionTop,RegionLeft,Region2Top,Region2Left FROM DisplaySetting");

            db.Stop();

            foreach (DataRow row in dt.Rows)
            {
                ListViewItem item = new ListViewItem(row[0].ToString());
                for (int i = 1; i < dt.Columns.Count; i++)
                {
                    item.SubItems.Add(row[i].ToString());
                }
                listViewDisplay.Items.Add(item);
            }
            //SqlDataReader reader = command.ExecuteReader();


            //while (dt.Rows.Count > 0)
            //{
            //    DisplaySetting displaySetting = new DisplaySetting();
            //    displaySetting.id = (int)dt.Rows[0]["Id"];
            //    displaySetting.DeviceId = (int)dt.Rows[0]["DeviceId"];
            //    displaySetting.Height = dt.Rows[0]["Height"].ToString();
            //    displaySetting.Width = dt.Rows[0]["Width"].ToString();
            //    displaySetting.style = dt.Rows[0]["Style"].ToString();
            //    displaySetting.Animation = dt.Rows[0]["Animation"].ToString();



            //    displayList.Add(displaySetting);


            //}


        }

        private void listViewDisplay_DoubleClick(object sender, EventArgs e)
        {


            ListViewItem item = listViewDisplay.SelectedItems[0];
            int id = Convert.ToInt32(item.Text);


            LumexDBPlayer db = LumexDBPlayer.Start(true);
            DataTable dt = db.ExecuteDataTable("SELECT Id , DeviceId , Height , Width , Style , Animation,RegionTop, RegionLeft ,Region2Top, Region2Left,ScheduleTime FROM DisplaySetting where id='" + id + "'");

            db.Stop();

            isUpdateMode = true;
            btnSaveDisplaySetting.Text = "Update";

            hiddenField.Text = dt.Rows[0]["Id"].ToString();
            txtbxHeight.Text = dt.Rows[0]["Height"].ToString();
            txtbxWidth.Text = dt.Rows[0]["Width"].ToString();
            txtbxRegion1Top.Text = dt.Rows[0]["RegionTop"].ToString();
            txtbxRegion1Left.Text = dt.Rows[0]["RegionLeft"].ToString();
            txtbxRegion2Top.Text = dt.Rows[0]["Region2Top"].ToString();
            txtbxRegion2Left.Text = dt.Rows[0]["Region2Left"].ToString();
            combxStyle.SelectedValue = dt.Rows[0]["Style"].ToString();
            combxDevice.SelectedValue = dt.Rows[0]["DeviceId"].ToString();
            combxAnimation.SelectedValue = dt.Rows[0]["Animation"].ToString();
            txtbxScheduleTime.Text = dt.Rows[0]["ScheduleTime"].ToString();


        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                if (CheckClick) //To Check If its Click or Automated
                {
                    PGprs = new ledcontrol.GPRSControl();

                    //DataTable dt = GetUserPassWordByDeviceId(DeviceId);//previous loginDeviceId.selectedValue
                    // DataTable dtTime = GetHeightWidth(logInDeviceComBx.SelectedValue);
                    // ScheduleTime = Convert.ToInt32(dtTime.Rows[0]["ScheduleTime"].ToString());

                    PGprs.Host = System.Configuration.ConfigurationManager.AppSettings["Host"];//"42.121.6.228";
                    PGprs.Port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["Port"]);//int.Parse("9099");


                    //Select Device ID User and Pass and Device serial


                    PGprs.UserName = userName;//dt.Rows[0]["UserName"].ToString();//"szlccl";
                    PGprs.PassWord = password;//dt.Rows[0]["PostCode"].ToString();//"123456";
                    //logInDeviceComBx.SelectedValue = DeviceId;

                    PGprs.DeviceMgr.refresh();
                    string dn = (DeviceId).ToString();//PGprs.DeviceMgr.Items[8]; // Put Device Serial Here //previous loginDeviceId.selectedValue
                    Boolean IsOnLine3 = PGprs.DeviceMgr.OnLineByIndex[8];
                    Boolean IsOnLine = PGprs.DeviceMgr.OnLineByID[dn];

                    if (!IsOnLine)
                    {
                        lblVerify.Text = "✓";
                        lblVerify.ForeColor = Color.Green;
                    }
                    else
                    {
                        lblVerify.Text = "X";
                        lblVerify.ForeColor = Color.Red;
                    }
                    //Boolean IsOnLine1 = PGprs.DeviceMgr.get_OnLineByID(dn);
                    //Boolean IsOnLine2 = PGprs.DeviceMgr.get_OnLineByIndex(8);
                    //if (IsOnLine)
                    //    MessageBox.Show("equipment:" + dn + " Online");
                    //else
                    // Connection device object, a plurality of equipment ',' Split

                    PLed = PGprs.Device;
                    PGprs.TargetDeviceID = dn;// The Selected One

                }
                else
                {
                    PGprs = new ledcontrol.GPRSControl();

                    //DataTable dt = GetUserPassWordByDeviceId(DeviceId);//previous loginDeviceId.selectedValue
                    // DataTable dtTime = GetHeightWidth(logInDeviceComBx.SelectedValue);
                    // ScheduleTime = Convert.ToInt32(dtTime.Rows[0]["ScheduleTime"].ToString());

                    PGprs.Host = System.Configuration.ConfigurationManager.AppSettings["Host"];//"42.121.6.228";
                    PGprs.Port = int.Parse(System.Configuration.ConfigurationManager.AppSettings["Port"]);//int.Parse("9099");


                    //Select Device ID User and Pass and Device serial

                    DataTable dt = GetUserPassWordByDeviceId(logInDeviceComBx.SelectedValue);
                    PGprs.UserName = dt.Rows[0]["UserName"].ToString();//"szlccl";
                    PGprs.PassWord = dt.Rows[0]["PostCode"].ToString();//"123456";
                    //logInDeviceComBx.SelectedValue = DeviceId;

                    PGprs.DeviceMgr.refresh();
                    string dn = (logInDeviceComBx.SelectedValue).ToString();//PGprs.DeviceMgr.Items[8]; // Put Device Serial Here //previous loginDeviceId.selectedValue
                    Boolean IsOnLine3 = PGprs.DeviceMgr.OnLineByIndex[8];
                    Boolean IsOnLine = PGprs.DeviceMgr.OnLineByID[dn];

                    if (!IsOnLine)
                    {
                        lblVerify.Text = "✓";
                        lblVerify.ForeColor = Color.Green;
                    }
                    else
                    {
                        lblVerify.Text = "X";
                        lblVerify.ForeColor = Color.Red;
                    }
                    //Boolean IsOnLine1 = PGprs.DeviceMgr.get_OnLineByID(dn);
                    //Boolean IsOnLine2 = PGprs.DeviceMgr.get_OnLineByIndex(8);
                    //if (IsOnLine)
                    //    MessageBox.Show("equipment:" + dn + " Online");
                    //else
                    // Connection device object, a plurality of equipment ',' Split

                    PLed = PGprs.Device;
                    PGprs.TargetDeviceID = dn;// The Selected One
                }



            }
            catch (Exception ex)
            {
                WriteToLogFile("Error from Clear Data" + ex.ToString(), logFile);
                // throw;
            }



        }

        private DataTable GetUserPassWordByDeviceId(object p)
        {
            LumexDBPlayer db = LumexDBPlayer.Start(true);
            DataTable dt = db.ExecuteDataTable("SELECT Id , DeviceId , UserName , PostCode ,AreaId, DeviceName FROM Device Where DeviceId='" + p + "'");
            //SqlCommand command = new SqlCommand(query, con);
            db.Stop();

            return dt;
        }

        private void btnClearData_Click(object sender, EventArgs e)
        {
            try
            {
                PLed.ClearScreen();
            }
            catch (Exception ex)
            {
                WriteToLogFile("Error from Clear Data" + ex.ToString(), logFile);
                // throw;
            }
        }
        public List<string> ImageList = new List<string>();
        StringBuilder sb;
        string textSource = AppDomain.CurrentDomain.BaseDirectory.ToString();
        private string logFile = "C:\\temp\\log.txt";

        public void WriteToLogFile(string strMessage, string outputFile)
        {

            string line = DateTime.Now.ToString() + " | ";

            line += strMessage;

            FileStream fs = new FileStream(outputFile, FileMode.Append, FileAccess.Write, FileShare.None);

            StreamWriter swFromFileStream = new StreamWriter(fs);

            swFromFileStream.WriteLine(line);

            swFromFileStream.Flush();

            swFromFileStream.Close();

        }
        public void WriteConnectionStringToLogFile(string strMessage, string outputFile)
        {

            FileStream fs = new FileStream(outputFile, FileMode.Append, FileAccess.Write, FileShare.None);

            StreamWriter swFromFileStream = new StreamWriter(fs);

            swFromFileStream.Write(strMessage);

            swFromFileStream.Flush();

            swFromFileStream.Close();

        }
        private void btnGetImageFromWeb_Click(object sender, EventArgs e)
        {

            // string filepath = @"C:";
            //string path = System.Reflection.Assembly.GetExecutingAssembly().Location;

            //To get the location the assembly normally resides on disk or the install directory
            //string path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;

            //once you have the path you get the directory with:
            // var directory = System.IO.Path.GetDirectoryName(path + "TEST.jpg");


            try
            {
                WebClient x = new WebClient();
                // DataTable dt = GetImageUrl(logInDeviceComBx.SelectedValue);
                //string url = System.Configuration.ConfigurationManager.AppSettings["Host"];
                // string ImageUrl = dt.Rows[0]["AreaId"].ToString();//AreaId is Web Url for downloading Image
                // string source = x.DownloadString(@"http://180.211.159.172/damweb/PublicPortal/commodity_name/");
                //string source = x.DownloadString(ImageUrl);

                if (CheckClick) //To Check If its Click or Automated
                {
                    //writer.WriteLine("Into the getImageFromWeb Method");
                    WriteToLogFile("Into the getImageFromWeb Method", logFile);



                    string source = x.DownloadString(Url);
                    //writer.WriteLine("Download Source using URL");
                    WriteToLogFile("Download Source using URL", logFile);

                    Url = source;
                    HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                    document.LoadHtml(source);
                    source = x.DownloadString(document.DocumentNode.InnerHtml);
                    document.LoadHtml(source);
                    ImageList.Clear();

                    foreach (HtmlNode link in document.DocumentNode.SelectNodes("//a"))
                    {

                        if (link.InnerHtml.Contains(".jpeg"))
                        {
                            ImageList.Add(link.InnerHtml);
                        }

                    }
                    //writer.WriteLine("Image added to the ImageList");
                    WriteToLogFile("Image added to the ImageList", logFile);

                    string currentPath = Directory.GetCurrentDirectory();
                    DateTime.Now.ToShortDateString();

                    string newPath = DateTime.Now.ToString("dd-MM-yyyy");
                    WriteToLogFile("New Path Created" + newPath, logFile);

                    if (!Directory.Exists(Path.Combine(currentPath, newPath, DeviceId)))
                    {
                        Directory.CreateDirectory(Path.Combine(currentPath, newPath, DeviceId));

                        // writer.WriteLine("Image Folder Created");
                        WriteToLogFile("Image Folder Created at" + currentPath + newPath + DeviceId, logFile);


                        foreach (string name in ImageList)
                        {
                            WebClient webClient = new WebClient();
                            //string url = "http://180.211.159.172/damweb/PublicPortal/commodity_name/" + name;
                            //string url = ImageUrl + name;
                            string url = Url + @"/" + name;
                            string path = currentPath + @"\" + newPath + @"\" + DeviceId + @"\";
                            webClient.DownloadFile(url, path + name);

                        }
                        WriteToLogFile("Image download Completed at" + currentPath + newPath + DeviceId, logFile);
                        //WriteToLogFile("Image download Completed", logFile);
                        // writer.WriteLine("Image download Completed");

                    }
                    else
                    {
                        //writer.WriteLine("Image Folder Created");

                        foreach (string name in ImageList)
                        {
                            WebClient webClient = new WebClient();
                            //string url = "http://180.211.159.172/damweb/PublicPortal/commodity_name/" + name;
                            //string url = ImageUrl + name;
                            string url = Url + @"/" + name;
                            string path = currentPath + @"\" + newPath + @"\" + DeviceId + @"\";
                            webClient.DownloadFile(url, path + name);

                        }
                        WriteToLogFile("Image download Completed", logFile);
                        // writer.WriteLine("Image download Completed");

                    }

                    MessageBox.Show("Image Downloaded Successfully For Device : " + DeviceId);

                }

                else
                {
                    //writer.WriteLine("Into the getImageFromWeb Method");
                    WriteToLogFile("Into the getImageFromWeb Method", logFile);


                    DataTable dt = GetUserPassWordByDeviceId(logInDeviceComBx.SelectedValue);
                    string source = x.DownloadString(dt.Rows[0]["AreaId"].ToString());
                    //writer.WriteLine("Download Source using URL");
                    WriteToLogFile("Download Source using URL", logFile);

                    Url = source;
                    HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                    document.LoadHtml(source);
                    source = x.DownloadString(document.DocumentNode.InnerHtml);
                    document.LoadHtml(source);
                    ImageList.Clear();

                    foreach (HtmlNode link in document.DocumentNode.SelectNodes("//a"))
                    {

                        if (link.InnerHtml.Contains(".jpeg"))
                        {
                            ImageList.Add(link.InnerHtml);
                        }

                    }
                    //writer.WriteLine("Image added to the ImageList");
                    WriteToLogFile("Image added to the ImageList", logFile);

                    string currentPath = Directory.GetCurrentDirectory();
                    DateTime.Now.ToShortDateString();

                    string newPath = DateTime.Now.ToString("dd-MM-yyyy");
                    WriteToLogFile("New Path Created" + newPath, logFile);

                    if (!Directory.Exists(Path.Combine(currentPath, newPath, logInDeviceComBx.SelectedValue.ToString())))
                    {
                        Directory.CreateDirectory(Path.Combine(currentPath, newPath, logInDeviceComBx.SelectedValue.ToString()));

                        // writer.WriteLine("Image Folder Created");
                        WriteToLogFile("Image Folder Created", logFile);


                        foreach (string name in ImageList)
                        {
                            WebClient webClient = new WebClient();
                            //string url = "http://180.211.159.172/damweb/PublicPortal/commodity_name/" + name;
                            //string url = ImageUrl + name;
                            string url = Url + @"/" + name;
                            string path = currentPath + @"\" + newPath + @"\" + DeviceId + @"\";
                            webClient.DownloadFile(url, path + name);

                        }
                        WriteToLogFile("Image download Completed", logFile);
                        // writer.WriteLine("Image download Completed");

                    }
                    else
                    {
                        //writer.WriteLine("Image Folder Created");

                        foreach (string name in ImageList)
                        {
                            WebClient webClient = new WebClient();
                            //string url = "http://180.211.159.172/damweb/PublicPortal/commodity_name/" + name;
                            //string url = ImageUrl + name;
                            string url = Url + @"/" + name;
                            string path = currentPath + @"\" + newPath + @"\" + DeviceId + @"\";
                            webClient.DownloadFile(url, path + name);

                        }
                        WriteToLogFile("Image download Completed", logFile);
                        // writer.WriteLine("Image download Completed");

                    }

                    MessageBox.Show("Image Downloaded Successfully For Device : " + DeviceId);

                }


            }
            catch (Exception ex)
            {

                //MessageBox.Show(ex.ToString());
                // writer.WriteLine("Exception Messege" + ex.ToString());
                WriteToLogFile("Exception Messege" + ex.ToString(), logFile);

            }

        }

        private void tabControl3_Click(object sender, System.EventArgs e)
        {
            LoadDevice();
        }

        private void btnSendDataImg_Click(object sender, EventArgs e)
        {
            try
            {

                int k = 0;

                //DataTable dt = GetHeightWidth(logInDeviceComBx.SelectedValue);
                DataTable dt = GetHeightWidth(logInDeviceComBx.SelectedValue);
                string currentPath = Directory.GetCurrentDirectory();
                DateTime.Now.ToShortDateString();
                string newPath = DateTime.Now.ToString("dd-MM-yyyy");
                newPath = currentPath + @"\" + newPath;
                string[] filePaths = Directory.GetFiles(newPath, "*.jpeg");
                Array.Sort(filePaths, StringComparer.InvariantCulture);
                decimal totalPrograme = ((filePaths.Length) / 8);
                totalPrograme = Math.Ceiling(totalPrograme);



                for (int i = 0; i < totalPrograme; i++)
                {
                    ledcontrol.LEDProgram Program1 = new ledcontrol.LEDProgram();
                    ledcontrol.LEDRegion Region1Top = new ledcontrol.LEDRegion();

                    Program1.LEDKind = ledcontrol.LEDKind.lkSingle;
                    // Screen monochrome color Type Single = 0, Double = 1 color, lkMultiple = 2 tricolor,
                    Program1.ProgramID = 4 + i;
                    // Program number> = 5 corresponds to the client program number id + 4
                    Program1.PlayMode = 0; //Broadcast mode
                    Program1.PlayModeValue = 1; //Show player
                    Program1.FromDate = System.Convert.ToDateTime("2000-01-01"); //"2000-01-01";          //Validity start
                    Program1.ToDate = System.Convert.ToDateTime("2199-01-01");
                    //"2000-01-01";          //Validity start"2199-01-01";            //有效期结束
                    Program1.Weeks = "1,2,3,4,5,6,7"; //Play Days Monday to weeks5 :'1,2,3,4,5'

                    Program1.Width = 512; //width
                    Program1.Height = 48; //height            
                    Program1.ProgramName = "SImage" + i; //Program Title
                    Program1.ClearHours(); //Cloudy periods
                    Program1.AddHours(System.Convert.ToDateTime("00:00"), System.Convert.ToDateTime("23:59")); //Add hours
                    Program1.IsInsert = false; //Whether spots are generally useless
                    Program1.IsPlayingTime = false;
                    Program1.ClearRegions();


                    Region1Top.RegionId = 1; //Partition No.   1~8 

                    Region1Top.Mode = ledcontrol.LEDRegionMode.rmStatic; //default
                    Region1Top.Left = 0;
                    //Convert.ToInt32(dt.Rows[0]["RegionLeft"].ToString());                           // Left Margin   x
                    Region1Top.Top = 0;
                    //Convert.ToInt32(dt.Rows[0]["RegionTop"].ToString());                           //Top margin    y
                    Region1Top.Width = Convert.ToInt32(dt.Rows[0]["Width"].ToString());//496;  //width      w
                    Region1Top.Height = Convert.ToInt32(dt.Rows[0]["Height"].ToString());//48; 


                    for (int j = 0; j < 8; j++, k++)
                    {
                        if (k < filePaths.Length)
                        {
                            ledcontrol.IPictureElement PicEle = new ledcontrol.PictureElement();

                            PicEle.Height = 48;
                            PicEle.Width = 496;
                            //PicEle.Mode = 1;// Image Type
                            PicEle.BoderStyle = 1; // Type of border
                            PicEle.AnimateStyle = 0; // Animated
                            PicEle.AnimateSpeed = 8; // Movement speed
                            PicEle.AnimateDelay = 0; // Residence time

                            //PicEle.LoadFromFile(@filePaths[j]); // Load picture
                            //PicEle.LoadFromFile(@"E:\Atib\Projects\LEDApp\LEDApp\bin\Debug\02-04-2016\crop13.png");
                            PicEle.LoadFromFile(filePaths[k]);
                            Region1Top.AddElment(PicEle);
                        }
                        else
                        {
                            break;
                        }

                    }
                    Program1.AddRegions(Region1Top);

                    PLed.SendProgram(Program1);
                    // PLed.SendRamProgram(Program1);
                }

                MessageBox.Show("Data Send Successfully");
            }

            catch (Exception)
            {

                MessageBox.Show("Data Send Failed");
            }

        }

        private DataTable GetHeightWidth(object DeviceId)
        {
            LumexDBPlayer db = LumexDBPlayer.Start(true);
            DataTable dt = db.ExecuteDataTable("SELECT Id , DeviceId , Height , Width , Style , Animation,RegionTop,RegionLeft ,Region2Top,Region2Left,ScheduleTime FROM DisplaySetting where Deviceid='" + DeviceId + "'");
            //SqlCommand command = new SqlCommand(query, con);
            db.Stop();

            return dt;
        }


        private void btnSendDuelLine_Click(object sender, EventArgs e)
        {
            try
            {
                if (CheckClick) //To Check If its Click or Automated
                {
                    int k = 0;
                    int w = 0;
                    int r = 0;

                    string[] imageName = new string[50];
                    // string[] WholeSaleImage = new string[50];
                    //string[] RetailImage = new string[50];

                    List<string> WholeSaleImage = new List<string>();
                    List<string> RetailImage = new List<string>();


                    DataTable dt = GetHeightWidth(DeviceId);
                    string currentPath = Directory.GetCurrentDirectory();
                    DateTime.Now.ToShortDateString();
                    string newPath = DateTime.Now.ToString("dd-MM-yyyy");
                    newPath = currentPath + @"\" + newPath + @"\" + DeviceId;
                    string[] filePaths = Directory.GetFiles(newPath, "*.jpeg");
                    Array.Sort(filePaths, StringComparer.InvariantCulture);
                    for (int i = 0; i < filePaths.Length; i++)
                    {
                        imageName[i] = filePaths[i].Replace(newPath, "").ToString();
                        //string test = imageName[i].Substring(0, 2);
                        if (imageName[i].Substring(0, 2) == "\\w")
                        {
                            // WholeSaleImage[w++] = newPath + imageName[i];
                            WholeSaleImage.Insert(w++, newPath + imageName[i]);

                        }
                        else
                        {
                            RetailImage.Insert(r++, newPath + imageName[i]);
                            // RetailImage[r++] = newPath + imageName[i];
                        }
                    }


                    w = 0;
                    r = 0;
                    //double totalImageCountForRW = RetailImage.Count;
                    //
                    double totalPrograme = ((filePaths.Length) / 16.00);
                    //double totalPrograme = ((filePaths.Length) / totalImageCountForRW);
                    totalPrograme = Math.Ceiling(totalPrograme);



                    //for (int i = 0; i < totalPrograme; i++)
                    //{

                    for (int i = 0; i < totalPrograme; i++)
                    {

                        ledcontrol.LEDProgram Program1 = new ledcontrol.LEDProgram();


                        Program1.LEDKind = ledcontrol.LEDKind.lkSingle;
                        // Screen monochrome color Type Single = 0, Double = 1 color, lkMultiple = 2 tricolor,
                        Program1.ProgramID = 4 + i;
                        // Program number> = 5 corresponds to the client program number id + 4
                        Program1.PlayMode = 0; //Broadcast mode
                        Program1.PlayModeValue = 2; //Show player prev 1
                        Program1.FromDate = System.Convert.ToDateTime("2000-01-01"); //"2000-01-01";          //Validity start
                        Program1.ToDate = System.Convert.ToDateTime("2199-01-01");
                        //"2000-01-01";          //Validity start"2199-01-01";            //有效期结束
                        Program1.Weeks = "1,2,3,4,5,6,7"; //Play Days Monday to weeks5 :'1,2,3,4,5'

                        Program1.Width = 512; //width
                        Program1.Height = 96; //height            
                        Program1.ProgramName = "DAM" + DateTime.Now.ToString("dd") + i; ; //Program Title
                        Program1.ClearHours(); //Cloudy periods
                        Program1.AddHours(System.Convert.ToDateTime("00:00"), System.Convert.ToDateTime("23:59")); //Add hours
                        Program1.IsInsert = false; //Whether spots are generally useless
                        Program1.IsPlayingTime = false;
                        Program1.ClearRegions();





                        int rgn = 1;

                        for (int m = 0; m < 2; m++)
                        {
                            ledcontrol.LEDRegion Region1Top = new ledcontrol.LEDRegion();
                            Region1Top.RegionId = m + 1; //Partition No.   1~8 

                            Region1Top.Mode = ledcontrol.LEDRegionMode.rmStatic; //default

                            if (rgn == 1)
                            {
                                Region1Top.Left = 0;
                                //Convert.ToInt32(dt.Rows[0]["RegionLeft"].ToString());  
                                // Left Margin   x
                                Region1Top.Top = 0;//Convert.ToInt32(dt.Rows[0]["RegionTop"].ToString());  
                                //rgn = 0;

                            }
                            else
                            {
                                Region1Top.Left = 0;//Convert.ToInt32(dt.Rows[0]["Region1Left"].ToString());  
                                Region1Top.Top = 48;//Convert.ToInt32(dt.Rows[0]["Region1Top"].ToString());  

                            }
                            //Convert.ToInt32(dt.Rows[0]["RegionTop"].ToString());                           //Top margin    y
                            Region1Top.Width = 496; //Convert.ToInt32(dt.Rows[0]["Width"].ToString()); //width      w
                            Region1Top.Height = 48; //Convert.ToInt32(dt.Rows[0]["Height"].ToString());


                            if (rgn == 1)
                            {
                                rgn = 0;
                                for (int j = 0; j < 8; j++, w++)
                                {
                                    if (w < WholeSaleImage.Count)
                                    {
                                        ledcontrol.IPictureElement PicEle = new ledcontrol.PictureElement();
                                        PicEle.Width = 496;
                                        PicEle.Height = 48;

                                        //PicEle.Mode = 1;// Image Type
                                        PicEle.BoderStyle = 0; // Type of border
                                        PicEle.AnimateStyle = 0; // Animated
                                        PicEle.AnimateSpeed = 15; // Movement speed
                                        PicEle.AnimateDelay = 0; // Residence time

                                        //PicEle.LoadFromFile(@filePaths[j]); // Load picture
                                        //PicEle.LoadFromFile(@"E:\Atib\Projects\LEDApp\LEDApp\bin\Debug\02-04-2016\crop13.png");
                                        PicEle.LoadFromFile(WholeSaleImage[w]);
                                        Region1Top.AddElment(PicEle);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                Program1.AddRegions(Region1Top);


                            }
                            else
                            {
                                for (int j = 0; j < 8; j++, r++)
                                {
                                    if (r < RetailImage.Count)
                                    {
                                        ledcontrol.IPictureElement PicEle = new ledcontrol.PictureElement();
                                        PicEle.Width = 496;
                                        PicEle.Height = 48;

                                        //PicEle.Mode = 1;// Image Type
                                        PicEle.BoderStyle = 0; // Type of border
                                        PicEle.AnimateStyle = 0; // Animated
                                        PicEle.AnimateSpeed = 15; // Movement speed
                                        PicEle.AnimateDelay = 0; // Residence time

                                        //PicEle.LoadFromFile(@filePaths[j]); // Load picture
                                        //PicEle.LoadFromFile(@"E:\Atib\Projects\LEDApp\LEDApp\bin\Debug\02-04-2016\crop13.png");
                                        PicEle.LoadFromFile(RetailImage[r]);
                                        Region1Top.AddElment(PicEle);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                Program1.AddRegions(Region1Top);
                            }

                        }


                        // PLed.SendRamProgram(Program1);

                        PLed.SendProgram(Program1);
                    }


                    //}

                    MessageBox.Show("Data Send Successfully for Device : " + DeviceId);
                }
                else
                {
                    int k = 0;
                    int w = 0;
                    int r = 0;
                    //DataTable dt = GetUserPassWordByDeviceId(logInDeviceComBx.SelectedValue);
                    string[] imageName = new string[50];
                    // string[] WholeSaleImage = new string[50];
                    //string[] RetailImage = new string[50];

                    List<string> WholeSaleImage = new List<string>();
                    List<string> RetailImage = new List<string>();


                    DataTable dt = GetUserPassWordByDeviceId(logInDeviceComBx.SelectedValue);
                    DeviceId = dt.Rows[0]["DeviceId"].ToString();


                    string currentPath = Directory.GetCurrentDirectory();
                    DateTime.Now.ToShortDateString();
                    string newPath = DateTime.Now.ToString("dd-MM-yyyy");
                    newPath = currentPath + @"\" + newPath + @"\" + DeviceId;
                    string[] filePaths = Directory.GetFiles(newPath, "*.jpeg");
                    Array.Sort(filePaths, StringComparer.InvariantCulture);
                    for (int i = 0; i < filePaths.Length; i++)
                    {
                        imageName[i] = filePaths[i].Replace(newPath, "").ToString();
                        //string test = imageName[i].Substring(0, 2);
                        if (imageName[i].Substring(0, 2) == "\\w")
                        {
                            // WholeSaleImage[w++] = newPath + imageName[i];
                            WholeSaleImage.Insert(w++, newPath + imageName[i]);

                        }
                        else
                        {
                            RetailImage.Insert(r++, newPath + imageName[i]);
                            // RetailImage[r++] = newPath + imageName[i];
                        }
                    }


                    w = 0;
                    r = 0;
                    //double totalImageCountForRW = RetailImage.Count;
                    //
                    double totalPrograme = ((filePaths.Length) / 16.00);
                    //double totalPrograme = ((filePaths.Length) / totalImageCountForRW);
                    totalPrograme = Math.Ceiling(totalPrograme);



                    //for (int i = 0; i < totalPrograme; i++)
                    //{

                    for (int i = 0; i < totalPrograme; i++)
                    {

                        ledcontrol.LEDProgram Program1 = new ledcontrol.LEDProgram();


                        Program1.LEDKind = ledcontrol.LEDKind.lkSingle;
                        // Screen monochrome color Type Single = 0, Double = 1 color, lkMultiple = 2 tricolor,
                        Program1.ProgramID = 4 + i;
                        // Program number> = 5 corresponds to the client program number id + 4
                        Program1.PlayMode = 0; //Broadcast mode
                        Program1.PlayModeValue = 2; //Show player prev 1
                        Program1.FromDate = System.Convert.ToDateTime("2000-01-01"); //"2000-01-01";          //Validity start
                        Program1.ToDate = System.Convert.ToDateTime("2199-01-01");
                        //"2000-01-01";          //Validity start"2199-01-01";            //有效期结束
                        Program1.Weeks = "1,2,3,4,5,6,7"; //Play Days Monday to weeks5 :'1,2,3,4,5'

                        Program1.Width = 512; //width
                        Program1.Height = 96; //height            
                        Program1.ProgramName = "DAM" + DateTime.Now.ToString("dd") + i; ; //Program Title
                        Program1.ClearHours(); //Cloudy periods
                        Program1.AddHours(System.Convert.ToDateTime("00:00"), System.Convert.ToDateTime("23:59")); //Add hours
                        Program1.IsInsert = false; //Whether spots are generally useless
                        Program1.IsPlayingTime = false;
                        Program1.ClearRegions();





                        int rgn = 1;

                        for (int m = 0; m < 2; m++)
                        {
                            ledcontrol.LEDRegion Region1Top = new ledcontrol.LEDRegion();
                            Region1Top.RegionId = m + 1; //Partition No.   1~8 

                            Region1Top.Mode = ledcontrol.LEDRegionMode.rmStatic; //default

                            if (rgn == 1)
                            {
                                Region1Top.Left = 0;
                                //Convert.ToInt32(dt.Rows[0]["RegionLeft"].ToString());  
                                // Left Margin   x
                                Region1Top.Top = 0;//Convert.ToInt32(dt.Rows[0]["RegionTop"].ToString());  
                                //rgn = 0;

                            }
                            else
                            {
                                Region1Top.Left = 0;//Convert.ToInt32(dt.Rows[0]["Region1Left"].ToString());  
                                Region1Top.Top = 48;//Convert.ToInt32(dt.Rows[0]["Region1Top"].ToString());  

                            }
                            //Convert.ToInt32(dt.Rows[0]["RegionTop"].ToString());                           //Top margin    y
                            Region1Top.Width = 496; //Convert.ToInt32(dt.Rows[0]["Width"].ToString()); //width      w
                            Region1Top.Height = 48; //Convert.ToInt32(dt.Rows[0]["Height"].ToString());


                            if (rgn == 1)
                            {
                                rgn = 0;
                                for (int j = 0; j < 8; j++, w++)
                                {
                                    if (w < WholeSaleImage.Count)
                                    {
                                        ledcontrol.IPictureElement PicEle = new ledcontrol.PictureElement();
                                        PicEle.Width = 496;
                                        PicEle.Height = 48;

                                        //PicEle.Mode = 1;// Image Type
                                        PicEle.BoderStyle = 0; // Type of border
                                        PicEle.AnimateStyle = 0; // Animated
                                        PicEle.AnimateSpeed = 15; // Movement speed
                                        PicEle.AnimateDelay = 0; // Residence time

                                        //PicEle.LoadFromFile(@filePaths[j]); // Load picture
                                        //PicEle.LoadFromFile(@"E:\Atib\Projects\LEDApp\LEDApp\bin\Debug\02-04-2016\crop13.png");
                                        PicEle.LoadFromFile(WholeSaleImage[w]);
                                        Region1Top.AddElment(PicEle);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                Program1.AddRegions(Region1Top);


                            }
                            else
                            {
                                for (int j = 0; j < 8; j++, r++)
                                {
                                    if (r < RetailImage.Count)
                                    {
                                        ledcontrol.IPictureElement PicEle = new ledcontrol.PictureElement();
                                        PicEle.Width = 496;
                                        PicEle.Height = 48;

                                        //PicEle.Mode = 1;// Image Type
                                        PicEle.BoderStyle = 0; // Type of border
                                        PicEle.AnimateStyle = 0; // Animated
                                        PicEle.AnimateSpeed = 15; // Movement speed
                                        PicEle.AnimateDelay = 0; // Residence time

                                        //PicEle.LoadFromFile(@filePaths[j]); // Load picture
                                        //PicEle.LoadFromFile(@"E:\Atib\Projects\LEDApp\LEDApp\bin\Debug\02-04-2016\crop13.png");
                                        PicEle.LoadFromFile(RetailImage[r]);
                                        Region1Top.AddElment(PicEle);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                Program1.AddRegions(Region1Top);
                            }

                        }


                        // PLed.SendRamProgram(Program1);

                        PLed.SendProgram(Program1);
                    }


                    //}

                    MessageBox.Show("Data Send Successfully for Device : " + DeviceId);
                }

            }

            catch (Exception ex)
            {
                WriteToLogFile("Error from dual data send" + ex.ToString(), logFile);
                //MessageBox.Show("Data Send Failed");
            }
        }





    }
}

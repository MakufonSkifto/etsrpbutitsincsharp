using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DiscordRPC;
using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;


namespace etsrp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        
        private void button1_Click(object sender, EventArgs e)
        {
            int RoundNum(int num)
            {
                int rem = num % 10;
                return rem >= 5 ? (num - rem + 10) : (num - rem);
            }
            
            var client = new DiscordRpcClient("793495110440583178");
            
            if (button1.Text == "Start RP")
            {
                string url = @"http://localhost:25555/api/ets2/telemetry";

                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url); 
                request.AutomaticDecompression = DecompressionMethods.GZip;
                
                HttpWebResponse response = (HttpWebResponse) request.GetResponse(); 
                Stream stream = response.GetResponseStream(); 
                StreamReader reader = new StreamReader(stream);
                
                var details = JObject.Parse(reader.ReadLine()); 
                label2.Text = "ETS Telemetry Server Connection Status: Connected ✅"; 
                label2.ForeColor = Color.ForestGreen;
                
                client.Initialize(); 
                List<string> pl = new List<string>(); 
                Process[] processlist = Process.GetProcesses();
                
                foreach (Process theprocess in processlist) 
                { 
                    pl.Add(theprocess.ProcessName);
                }

                
                if (RoundNum(Int32.Parse(details["truck"]["speed"].ToString())) != 0 && 
                    details["truck"]["cruiseControlOn"].ToString() is "true")
                { 
                    string speed = 
                        $"Truck Speed: {RoundNum(Int32.Parse(details["truck"]["speed"].ToString()))} (Cruise Control)";
                }
                else if (RoundNum(Int32.Parse(details["truck"]["speed"].ToString())) == 0) 
                { 
                    string speed = "Truck Speed: Stopped";
                }
                else 
                { 
                    string speed = $"Truck Speed: {RoundNum(Int32.Parse(details["truck"]["speed"].ToString()))}";
                }

                
                if (details["game"]["paused"].ToString() is "true") 
                { 
                    string text = "Paused / Idle"; 
                    string speed = null;
                }
                else if (details["truck"]["make"].ToString() != "" && 
                         details["job"]["destinationCity"].ToString() != "")
                { 
                    string text = 
                        $"Driving with {details["truck"]["make"]} {details["truck"]["model"]} to {details["job"]["destinationCity"]}";
                }
                else if (details["game"]["paused"].ToString() is "false") 
                { 
                    string text = $"Driving with {details["truck"]["make"]} {details["truck"]["model"]}";
                }
                else 
                { 
                    Random rand = new Random(); 
                    string[] list = {"in Europe", "with some truck"}; 
                    int index = rand.Next(0, list.Length); 
                    string text = $"Driving {list[index]}";
                }
                
                if (details["game"]["paused"].ToString() is "true") 
                { 
                    string dist = null;
                }
                else if (Int32.Parse(details["navigation"]["estimatedDistance"].ToString()) != 0) 
                { 
                    string dist = 
                        $"Estimated Distance: {RoundNum(Int32.Parse(details["navigation"]["estimatedDistance"].ToString()) / 1000):n}km";
                }
                else 
                { 
                    string dist = null;
                }
                Console.WriteLine();
                
                bool alreadyExist = pl.Contains("eurotrucks2"); 
                if (alreadyExist) 
                { 
                    label1.Text = "ETSRP Status: Loading..."; 
                    label1.ForeColor = Color.Orange;
                    
                    client.SetPresence(new RichPresence() 
                    {
                        Details = text, 
                        Timestamps = Timestamps.Now, 
                        State = dist, 
                        Assets = new Assets() 
                        {
                            LargeImageKey = "ets", 
                            LargeImageText = speed, 
                            SmallImageText = "RP Mod by MakufonSkifto", 
                            SmallImageKey = "eu"
                        }
                    }); 
                    button1.Enabled = false; 
                    client.OnReady += (sendero, e_o) => 
                    { 
                        label1.Text = ($"ETSRP Status: Showing for {e_o.User.Username} ✅"); 
                        label1.ForeColor = Color.ForestGreen; 
                        button1.Text = "Stop RP"; 
                        button1.Enabled = true;
                    };
                }
                else 
                { 
                    MessageBox.Show("No running ETS2 found!", "Error", 
                        MessageBoxButtons.OK ,MessageBoxIcon.Hand);
                        
                }
            }
            else if (button1.Text == "Stop RP")
            {
                try
                {
                    button1.Enabled = false;
                    client.ClearPresence();
                    client.OnClose += (sendero, e_o) =>
                    {
                        label1.Text = "ETSRP Status: Not Showing ❌";
                        label1.ForeColor = Color.Red;
                        button1.Text = "Start RP";
                        button1.Enabled = true;
                    };
                    label1.Text = "ETSRP Status: Not Showing ❌";
                    label2.Text = "ETS Telemetry Server Connection Status: Connected ✅";
                
                    label1.ForeColor = Color.Red;
                    label2.ForeColor = Color.ForestGreen;
                }
                catch (DiscordRPC.Exceptions.UninitializedException exception)
                {
                    MessageBox.Show($"That didn't work! Please close the program to stop RP\n \n" +
                                    $"Exception: {exception}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    button1.Enabled = true;
                }
            }
        }
            

        private void Form1_Load(object sender, EventArgs e)
        {
            label1.Text = "ETSRP Status: Not Showing ❌";
            label1.ForeColor = Color.Red;
            try
            {
                string url = @"http://localhost:25555/api/ets2/telemetry";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;
                HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                
                label2.Text = "ETS Telemetry Server Connection Status: Connected ✅";
                label2.ForeColor = Color.ForestGreen;
            }
            catch (WebException)
            {
                SystemSounds.Beep.Play();
                MessageBox.Show("Please open the ETS Telemetry server in case to use this program!", "Error",
                    MessageBoxButtons.OK ,MessageBoxIcon.Hand);
                this.Close();
            }
        }
    }
}
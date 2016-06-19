using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScpDriverInterface;
using HidLibrary;
using System.Windows.Forms;
using System.Diagnostics;

namespace mi
{
    public partial class Form1 : Form
    {
        public static ScpBus global_scpBus;
        public static HidDevice global_device;

        delegate void SetTextCallback(string text);

        public void SetText(string text)
        {
            if (true)
            {
                if (this.textBox.InvokeRequired)
                {
                    SetTextCallback d = new SetTextCallback(SetText);
                    this.Invoke(d, new object[] { text });
                }
                else
                {
                    this.textBox.AppendText(text + Environment.NewLine);
                }
            }
        }

        private void Connect()
        {
            ScpBus scpBus = new ScpBus();
            scpBus.UnplugAll();
            global_scpBus = scpBus;

            Xiaomi_gamepad[] gamepads = new Xiaomi_gamepad[4];
            int index = 1;
            var compatibleDevices = HidDevices.Enumerate(0x2717, 0x3144).ToList();
            foreach (var deviceInstance in compatibleDevices)
            {
                //Console.WriteLine(deviceInstance);
                textBox.AppendText(deviceInstance + Environment.NewLine);
                HidDevice Device = deviceInstance;
                global_device = deviceInstance;
                try
                {
                    Device.OpenDevice(DeviceMode.Overlapped, DeviceMode.Overlapped, ShareMode.Exclusive);
                }
                catch (Exception exception)
                {
                    //Console.WriteLine("Could not open gamepad in exclusive mode. Please close anything that could be using the controller\nAttempting to open it in shared mode.");
                    textBox.AppendText("Could not open gamepad in exclusive mode.Please close anything that could be using the controller" + Environment.NewLine + "Attempting to open it in shared mode." + Environment.NewLine);
                    Device.OpenDevice(DeviceMode.Overlapped, DeviceMode.Overlapped, ShareMode.ShareRead | ShareMode.ShareWrite);
                }

                byte[] Vibration = { 0x20, 0x00, 0x00 };
                if (Device.WriteFeatureData(Vibration) == false)
                {
                    //Console.WriteLine("Could not write to gamepad (is it closed?), skipping");
                    textBox.AppendText("Could not write to gamepad (is it closed?), skipping" + Environment.NewLine);
                    Device.CloseDevice();
                    continue;
                }

                byte[] serialNumber;
                byte[] product;
                Device.ReadSerialNumber(out serialNumber);
                Device.ReadProduct(out product);


                gamepads[index - 1] = new Xiaomi_gamepad(Device, scpBus, index);
                ++index;

                if (index >= 5)
                {
                    break;
                }
            }

            //Console.WriteLine("{0} controllers connected", index - 1);
            textBox.AppendText(index - 1 + " controllers connected" + Environment.NewLine);
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Connect();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            global_scpBus.UnplugAll();
            notifyIcon.Dispose();
            Environment.Exit(0);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(3000);
                this.ShowInTaskbar = false;
            }
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            notifyIcon.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //byte[] test = { 0x20, 0x00, 0x00 };
            //global_device.WriteFeatureData(test);
            global_scpBus.UnplugAll();
            Connect();
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            if(textBox.Lines.Length > 100)
            {
                string[] test = textBox.Lines;
                var tesete = test.ToList();
                tesete.RemoveRange(0, 1);
                test = tesete.ToArray();
                textBox.Lines = test;
                textBox.SelectionStart = textBox.Text.Length;
                textBox.ScrollToCaret();
            }
        }
    }
}

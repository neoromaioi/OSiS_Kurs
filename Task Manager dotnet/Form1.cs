using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Management;
using Microsoft.VisualBasic;
using System.Diagnostics.Tracing;
using System.Threading;
using Microsoft.VisualBasic.Devices;

namespace Task_Manager_dotnet
{
    public partial class Form1 : Form
    {
        private List<Process> processes = null;
        private ListViewItemComparer comparer = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void GetProcesses()
        {
            processes.Clear();
            processes = Process.GetProcesses().ToList<Process>();
        }

        private void RefreshProcessesList()
        {
            listView1.Items.Clear();
            double memSize = 0;
            double cpuUsage = 0;
            
            foreach(Process p in processes)
            {
                memSize = 0;
                cpuUsage = 0;
                PerformanceCounter pc = new PerformanceCounter();
                pc.CategoryName = "Process";
                pc.CounterName = "Working Set - Private";
                pc.InstanceName = p.ProcessName;
                memSize = (double)pc.NextValue() / 1024 / 1024 /*(1000 * 1000)*/;
                //PerformanceCounter cpu = new PerformanceCounter("Process", "% Processor Time", p.ProcessName, true);
                /*cpu.CategoryName = "Process";
                cpu.CounterName = "% Processor Time";
                cpu.InstanceName = p.ProcessName;
                cpuUsage = (double)(cpu.NextValue() / Environment.ProcessorCount);
                Thread.Sleep(50);*/
                //cpuUsage = (cpu.NextValue() / ((Environment.ProcessorCount) * cpu.NextValue()))*100;

                string network = "0";
                string[] row = new string[] { p.ProcessName.ToString(), Math.Round(memSize, 2).ToString(),
                    cpuUsage.ToString() + "%", network + " bytes" };
                listView1.Items.Add(new ListViewItem(row));
                pc.Close();
                pc.Dispose();
            }
            Text = "Number of running processes: " + processes.Count.ToString();
        }

        private void RefreshProcessesList(List<Process> processes, string keyword)
        {
            try
            {
                listView1.Items.Clear();
                double memSize = 0;
                double cpuUsage = 0;
                foreach (Process p in processes)
                {
                    if (p != null)
                    {
                        memSize = 0;
                        cpuUsage = 0;
                        string network = "0";
                        PerformanceCounter pc = new PerformanceCounter();
                        pc.CategoryName = "Process";
                        pc.CounterName = "Working Set - Private";
                        pc.InstanceName = p.ProcessName;
                        memSize = (double)pc.NextValue() / 1024 / 1024 /*(1000 * 1000)*/;
                        string[] row = new string[] { p.ProcessName.ToString(), Math.Round(memSize, 1).ToString(),
                            cpuUsage.ToString(), network + " bytes" };
                        listView1.Items.Add(new ListViewItem(row));
                        pc.Close();
                        pc.Dispose();
                    }
                }
                Text = $"Number of running processes: {keyword}" + processes.Count.ToString();
            }
            catch (Exception) { }
        }

        private void KillProcess(Process process)
        {
            process.Kill();
            process.WaitForExit();
        }

        private void KillProcessAndChildren(int pid)
        {
            if (pid == 0) { return; }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(
                "Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection objectCollection = searcher.Get();
            foreach(ManagementObject obj in objectCollection)
            {
                KillProcessAndChildren(Convert.ToInt32(obj["ProcessID"]));
            }
            try
            {
                Process p = Process.GetProcessById(pid);
                p.Kill();
                p.WaitForExit();
            }
            catch (ArgumentException) { }
        }

        private int GetParentProcessId(Process p)
        {
            int parentID = 0;
            try
            {
                ManagementObject managementObject = new ManagementObject("win32_process.handle='" + p.Id + "'");
                managementObject.Get();
                parentID = Convert.ToInt32(managementObject["ParentProcessID"]);
            }
            catch (Exception) { }
            return parentID;
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            processes = new List<Process>();
            GetProcesses();
            RefreshProcessesList();
            comparer = new ListViewItemComparer();
            comparer.ColumnIndex = 0;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            GetProcesses();
            RefreshProcessesList();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName ==
                     listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];
                    KillProcess(processToKill);
                    GetProcesses();
                    RefreshProcessesList();
                }
            }
            catch (Exception) { }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName ==
                     listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];
                    KillProcessAndChildren(GetParentProcessId(processToKill));
                    GetProcesses();
                    RefreshProcessesList();
                }
            }
            catch (Exception) { }
        }

        private void endTaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName ==
                     listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];
                    KillProcess(processToKill);
                    GetProcesses();
                    RefreshProcessesList();
                }
            }
            catch (Exception) { }
        }
        private void endTaskParentTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName ==
                     listView1.SelectedItems[0].SubItems[0].Text).ToList()[0];
                    KillProcessAndChildren(GetParentProcessId(processToKill));
                    GetProcesses();
                    RefreshProcessesList();
                }
            }
            catch (Exception) { }
        }

        private void startTaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Interaction.InputBox("Enter program name", "Start a new task");
            try
            {
                Process.Start(path);
            }
            catch (Exception) { }
        }

        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            GetProcesses();
            List<Process> filteredprocesses = processes.Where((x) =>
             x.ProcessName.ToLower().Contains(toolStripTextBox1.Text.ToLower())).ToList<Process>();
            RefreshProcessesList(filteredprocesses, toolStripTextBox1.Text);
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            comparer.ColumnIndex = e.Column;
            comparer.SortDirection = comparer.SortDirection == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            listView1.ListViewItemSorter = comparer;
            listView1.Sort();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void refreshCPUUsageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                listView1.Items.Clear();
                double memSize = 0;
                float cpuUsage = 0;
                foreach (Process p in processes)
                {
                    memSize = 0;
                    cpuUsage = 0;
                    string network = "0";
                    PerformanceCounter pc = new PerformanceCounter();
                    pc.CategoryName = "Process";
                    pc.CounterName = "Working Set - Private";
                    pc.InstanceName = p.ProcessName;
                    memSize = (double)pc.NextValue() / 1024 / 1024 /*(1000 * 1000)*/;
                    PerformanceCounter cpu = new PerformanceCounter("Process", "% Processor Time", p.ProcessName, true);
                    /*cpu.CategoryName = "Process";
                    cpu.CounterName = "% Processor Time";
                    cpu.InstanceName = p.ProcessName;*/
                    cpuUsage = cpu.NextValue();
                    Thread.Sleep(100);
                    cpuUsage = cpu.NextValue();
                    //cpuUsage = (cpu.NextValue() / ((Environment.ProcessorCount) * cpu.NextValue()))*100;
                    string[] row = new string[] { p.ProcessName.ToString(), Math.Round(memSize, 2).ToString(),
                    cpuUsage.ToString() + "%", network + " bytes" };
                    listView1.Items.Add(new ListViewItem(row));
                    cpu.Close();
                    cpu.Dispose();
                    pc.Close();
                    pc.Dispose();
                }
                Text = $"Number of running processes: " + processes.Count.ToString();
            }
            catch (Exception) { }

        }

        private void refreshNetworkUsageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            double memSize = 0;
            double cpuUsage = 0;

            foreach (Process p in processes)
            {
                memSize = 0;
                cpuUsage = 0;
                PerformanceCounter pc = new PerformanceCounter();
                pc.CategoryName = "Process";
                pc.CounterName = "Working Set - Private";
                pc.InstanceName = p.ProcessName;
                memSize = (double)pc.NextValue() / 1024 / 1024 /*(1000 * 1000)*/;
                //PerformanceCounter cpu = new PerformanceCounter("Process", "% Processor Time", p.ProcessName, true);
                /*cpu.CategoryName = "Process";
                cpu.CounterName = "% Processor Time";
                cpu.InstanceName = p.ProcessName;
                cpuUsage = (double)(cpu.NextValue() / Environment.ProcessorCount);
                Thread.Sleep(50);*/
                //cpuUsage = (cpu.NextValue() / ((Environment.ProcessorCount) * cpu.NextValue()))*100;

                string network = "";
                try
                {
                    string pn = p.ProcessName;//"MyProcessName.exe";
                    /*var readOpSec = new PerformanceCounter("Process", "IO Read Operations/sec", pn);
                    var writeOpSec = new PerformanceCounter("Process", "IO Write Operations/sec", pn);
                    var dataOpSec = new PerformanceCounter("Process", "IO Data Operations/sec", pn);*/
                    var readBytesSec = new PerformanceCounter("Process", "IO Read Bytes/sec", pn);
                    var writeByteSec = new PerformanceCounter("Process", "IO Write Bytes/sec", pn);
                    //var dataBytesSec = new PerformanceCounter("Process", "IO Data Bytes/sec", pn);

                    var counters = new List<PerformanceCounter>
                    {
                    /*readOpSec,
                    writeOpSec,
                    dataOpSec,*/
                    readBytesSec,
                    writeByteSec,
                    //dataBytesSec
                    };
                    network = "";
                    // get current value
                    foreach (PerformanceCounter counter in counters)
                    {
                        float rawValue = counter.NextValue();
                        Thread.Sleep(50);
                        rawValue = counter.NextValue();
                        network = network + "; " + rawValue.ToString();
                        // display the value
                    }
                    /*readOpSec.Close();
                    readOpSec.Dispose();
                    writeOpSec.Close();
                    writeOpSec.Dispose();
                    dataOpSec.Close();
                    dataOpSec.Dispose();*/
                    readBytesSec.Close();
                    readBytesSec.Dispose();
                    writeByteSec.Close();
                    writeByteSec.Dispose();
                    /*dataBytesSec.Close();
                    dataBytesSec.Dispose();*/
                    counters.Clear();
                }
                catch (Exception) { }
                string[] row = new string[] { p.ProcessName.ToString(), Math.Round(memSize, 2).ToString(),
                   cpuUsage.ToString() + "%", network + " bytes"};
                //cpuUsage.ToString() + "%" };
                listView1.Items.Add(new ListViewItem(row));
                pc.Close();
                pc.Dispose();
            }
            Text = "Number of running processes: " + processes.Count.ToString();
        }
    }
}

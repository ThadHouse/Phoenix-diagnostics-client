﻿using CTRE.Phoenix.Diagnostics.BackEnd;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CTRE_Phoenix_GUI_Dashboard
{
    public class ExportPackager
    {
        DataGridView gridDiagnosticLog;
        DeviceListContainer _deviceListContainer;
        ListView lstDevices;
        ComboBox cboHostSelectorAddr;
        ComboBox cboHostSelectorPrt;

        public ExportPackager(DataGridView gridDiagnosticLog,
                                DeviceListContainer _deviceListContainer,
                                ListView lstDevices,
                                ComboBox cboHostSelectorAddr,
                                ComboBox cboHostSelectorPrt)
        {
            this.gridDiagnosticLog = gridDiagnosticLog;
            this._deviceListContainer = _deviceListContainer;
            this.lstDevices = lstDevices;
            this.cboHostSelectorAddr = cboHostSelectorAddr;
            this.cboHostSelectorPrt = cboHostSelectorPrt;
        }
        public async Task Export()
        {
            /* If directory doesn't exist, create it */
            if (!Directory.Exists(".\\Exports"))
            {
                Directory.CreateDirectory(".\\Exports");
            }

            /* Create directory for this specific Export */
            string newExportPath = $".\\Exports\\Export {DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss")}";
            Directory.CreateDirectory(newExportPath);

            using (FileStream diagLog = new FileStream(newExportPath + "\\Diagnostic Log.txt", FileMode.Create))
            {
                /* Export data to a file */
                await ExportDiagnosticLog(diagLog);

            }
            using (FileStream devLog = new FileStream(newExportPath + "\\Devices Log.txt", FileMode.Create))
            {
                /* Export data to a file */
                await ExportDevices(devLog);
            }
            using (FileStream info = new FileStream(newExportPath + "\\Client Info.txt", FileMode.Create))
            {
                /* Export data to a file */
                await ExportClientInfo(info);
            }
            /* Zip up all files */
            await ZipEverythingUp(newExportPath, newExportPath + ".zip");

            /* Delete original directory to save space */
            new DirectoryInfo(newExportPath).Delete(true);

            System.Diagnostics.Process.Start(".\\Exports");
        }

        private async Task ExportDiagnosticLog(FileStream diagLog)
        {
            string content = "";
            foreach (DataGridViewRow row in gridDiagnosticLog.Rows)
            {
                /* Format data in tab-delimited txt file */
                content += row.Cells[0].Value + "\t" +
                    row.Cells[1].Value + "\t" +
                    row.Cells[2].Value + "\r\n";
            }
            byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
            await diagLog.WriteAsync(contentBytes, 0, contentBytes.Length);
        }

        private async Task ExportDevices(FileStream devLog)
        {
            string content = "";
            foreach (ListViewItem item in lstDevices.Items)
            {
                CTRE.Phoenix.Diagnostics.DeviceDescrip device;
                _deviceListContainer.GetDescriptor(item, out device);
                content += device.jsonStrings.Name + "\t" +
                    device.jsonStrings.SoftStatus + "\t" +
                    device.jsonStrings.Model + "\t" +
                    device.jsonStrings.ID + "\t" +
                    device.jsonStrings.CurrentVers + "\t" +
                    device.jsonStrings.ManDate + "\t" +
                    device.jsonStrings.BootloaderRev + "\t" +
                    device.jsonStrings.HardwareRev + "\r\n";
            }
            byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
            await devLog.WriteAsync(contentBytes, 0, contentBytes.Length);
        }

        private async Task ExportClientInfo(FileStream connectionInfo)
        {
            string a, b, c;
            BackEnd.Instance.GetStatus(out a, out b, out c);

            string content = "";
            content += "IP Address is: " + cboHostSelectorAddr.Text + "\r\n";
            content += "Port is: " + cboHostSelectorPrt.Text + "\r\n";
            content += "Backend Status is: " + a + "\r\n";
            content += "Server Version is: " + BackEnd.Instance.GetVersionNumbers() + "\r\n";
            content += "Client Version is: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\r\n";

            byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
            await connectionInfo.WriteAsync(contentBytes, 0, contentBytes.Length);
        }

        private Task ZipEverythingUp(string sourceDestination, string zipDestination)
        {
            return Task.Run(() =>
            {
                ZipFile.CreateFromDirectory(sourceDestination, zipDestination);
            });
        }
    }
}

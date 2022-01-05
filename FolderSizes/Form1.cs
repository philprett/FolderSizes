using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
// ReSharper disable LocalizableElement

namespace FolderSizes
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private bool CancelScan;
        private string lastScanFolder;
        private void ScanButton_Click(object sender, EventArgs e)
        {
            if (ScanButton.Text == "Scan")
            {
                ScanButton.Text = "Cancel";
                CancelScan = false;
                Application.DoEvents();

                lastScanFolder = FolderToScan.Text;
                StatusLabel.Text = $"Scanning {lastScanFolder}...";
                Application.DoEvents();
                ScanFolder(lastScanFolder);

                StatusLabel.Text = CancelScan ? $"Scan canceled" : $"Scan complete";

                ScanButton.Text = "Scan";
                CancelScan = false;
            }
            else
            {
                CancelScan = true;
            }
        }

        private void ScanFolder(string path)
        {
            StatusLabel.Text = $"Scanning {path}...";
            Application.DoEvents();

            var dir = new DirectoryInfo(path);
            if (dir.Exists == false)
            {
                return;
            }

            var folderFiles = new List<FolderFile>();
            foreach (var sub in dir.GetDirectories())
            {
                if (CancelScan)
                {
                    break;
                }

                StatusLabel.Text = $"Scanning {sub.FullName}...";
                Application.DoEvents();

                folderFiles.Add(new FolderFile(sub));
            }
            foreach (var file in dir.GetFiles())
            {
                if (CancelScan)
                {
                    break;
                }
                folderFiles.Add(new FolderFile(file));
            }
            UpdateGrid(folderFiles);
        }

        private void UpdateGrid(List<FolderFile> folderFiles)
        {
            if (CancelScan)
            {
                return;
            }
            Grid.Rows.Clear();
            foreach (var folderFile in folderFiles.OrderByDescending(f => f.Size))
            {
                var cellIcon = new DataGridViewImageCell();
                if (string.IsNullOrWhiteSpace(folderFile.ErrorMessage) == false)
                {
                    cellIcon.Value = (System.Drawing.Image)Properties.Resources.Error;
                }
                else if (folderFile.IsFolder)
                {
                    cellIcon.Value = (System.Drawing.Image)Properties.Resources.FolderIcon;
                }
                else
                {
                    cellIcon.Value = (System.Drawing.Image)Properties.Resources.FileIcon;
                }

                var cellName = new DataGridViewTextBoxCell();
                cellName.Value = folderFile.Name;

                var cellSize = new DataGridViewTextBoxCell();
                cellSize.Value = folderFile.Size;

                var row = new DataGridViewRow();
                row.Cells.Add(cellIcon);
                row.Cells.Add(cellName);
                row.Cells.Add(cellSize);
                row.Tag = folderFile;

                Grid.Rows.Add(row);

                Application.DoEvents();
            }
        }

        private void Grid_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (ScanButton.Text != "Scan")
            {
                return;
            }

            if (e.RowIndex < 0)
            {
                return;
            }

            FolderToScan.Text = ((FolderFile)Grid.Rows[e.RowIndex].Tag).FullName;
            ScanButton_Click(sender, null);
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            var export = new StringBuilder();

            int longestName = "Name".Length;
            int longestSize = "Size".Length;

            foreach (var folderFile in Grid.Rows.Cast<DataGridViewRow>().Select(o => (FolderFile)o.Tag))
            {
                if (folderFile.Name.Length > longestName)
                {
                    longestName = folderFile.Name.Length;
                }
                if (folderFile.SizeFormatted.Length > longestSize)
                {
                    longestSize = folderFile.SizeFormatted.Length;
                }
            }

            longestName++;

            export.AppendLine($"Contents of {lastScanFolder}");

            export.Append(".--------");
            export.Append(".-");
            export.Append(new string('-', longestName));
            export.Append(".-");
            export.Append(new string('-', longestSize));
            export.Append("-.");
            export.AppendLine();

            export.Append("| Type   ");
            export.Append("| Name ");
            export.Append(new string(' ', longestName - 5));
            export.Append("| Size ");
            export.Append(new string(' ', longestSize - 5));
            export.Append(" |");
            export.AppendLine();

            export.Append("|--------");
            export.Append("|-");
            export.Append(new string('-', longestName));
            export.Append("|-");
            export.Append(new string('-', longestSize));
            export.Append("-|");
            export.AppendLine();

            foreach (var folderFile in Grid.Rows.Cast<DataGridViewRow>().Select(o => (FolderFile)o.Tag))
            {
                export.Append("| ");
                if (string.IsNullOrWhiteSpace(folderFile.ErrorMessage) == false)
                {
                    export.Append("Error  ");
                }
                else if (folderFile.IsFolder)
                {
                    export.Append("Folder ");
                }
                else
                {
                    export.Append("File   ");
                }

                export.Append("| ");
                export.Append(folderFile.Name);
                export.Append(new string(' ', longestName - folderFile.Name.Length));

                export.Append("| ");
                export.Append(new string(' ', longestSize - folderFile.SizeFormatted.Length));
                export.Append(folderFile.SizeFormatted);

                export.Append(" |");
                export.AppendLine();
            }

            export.Append("'--------");
            export.Append("'-");
            export.Append(new string('-', longestName));
            export.Append("'-");
            export.Append(new string('-', longestSize));
            export.Append("-'");
            export.AppendLine();

            var tempFilename = Path.GetTempFileName();
            File.WriteAllText(tempFilename, export.ToString());

            Process.Start("notepad.exe", tempFilename);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FolderToScan.Text = Environment.CurrentDirectory;

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                FolderToScan.Text = args[1];
            }
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            var f = new FolderBrowserDialog();
            f.SelectedPath = FolderToScan.Text;
            if (f.ShowDialog() == DialogResult.OK)
            {
                FolderToScan.Text = f.SelectedPath;
            }
        }

        private void UpButton_Click(object sender, EventArgs e)
        {
            var path = FolderToScan.Text;
            if (path.Split('\\').Count() < 2)
            {
                return;
            }

            path = path.Substring(0, path.LastIndexOf("\\"));
            if (path.EndsWith(":"))
            {
                path += "\\";
            }

            FolderToScan.Text = path;
        }
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace DiscFit
{
    struct MediaType
    {
        private string _Caption;
        private ulong _Size;

        public MediaType(string Caption, ulong Size)
        {
            this._Caption = Caption;
            this._Size = Size;
        }

        public string Caption
        {
            get { return this._Caption; }
        }

        public ulong Size
        {
            get { return this._Size; }
        }

    }

    public partial class Form1 : Form
    {
        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern void DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int pvAttribute, uint cbAttribute);

        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

        [DllImport("uxtheme.dll", EntryPoint = "#133")]
        public static extern int AllowDarkModeForWindow(IntPtr hWnd, int allow);

        [DllImport("uxtheme.dll", EntryPoint = "#135")]
        public static extern int SetPreferredAppMode(int preferredAppMode);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            public uint wFunc;
            public IntPtr pFrom;
            public IntPtr pTo;
            public ushort fFlags;
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string lpszProgressTitle;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHFileOperation([In] ref SHFILEOPSTRUCT lpFileOp);

        public Form1()
        {
            InitializeComponent();
            
            this.Resize += (s, e) => UpdateTitle();
            UpdateTitle();
            
            
            this.comboMediaType.DrawMode = DrawMode.OwnerDrawFixed;
            this.comboMediaType.DrawItem += ComboMediaType_DrawItem;
            
            ApplyModernStyle();

            List<MediaType> MediaTypes = new List<MediaType>();
            MediaTypes.Add(new MediaType("CD-R 700 MB", 737280000));
            MediaTypes.Add(new MediaType("CD-R 700 MB (overburn)", 749731840));
            MediaTypes.Add(new MediaType("DVD+R", 4700372992));
            MediaTypes.Add(new MediaType("DVD-R", 4707319808));
            MediaTypes.Add(new MediaType("DVD-R DL", 8543666176));
            MediaTypes.Add(new MediaType("DVD+R DL", 8547991552));
            MediaTypes.Add(new MediaType("BD-R", 25025314816));
            MediaTypes.Add(new MediaType("BD-R DL", 50050629632));

            this.comboMediaType.DataSource = MediaTypes;
            this.comboMediaType.DisplayMember = "Caption";
            this.comboMediaType.ValueMember = "Size";
            this.comboMediaType.SelectedIndex = 2;
        }

        private void buttonPack_Click(object sender, EventArgs e)
        {
            newResults(); // Clears UI Results

            // Bin Pack
            BinPacker bp = new BinPacker(Convert.ToUInt64(this.maskedTextBox_MediaSize.Text));
            int nIndex = 0; // ImageList index

            foreach (ListViewItem item in listView1.Items)
            {
                string path = item.SubItems[2].Text;
                ulong size = Convert.ToUInt64(item.SubItems[1].Text);
                BinItem binItem = new BinItem(item.Text, path,size);
                bp.listAdd(binItem);
            }

            statusStrip1.Items["toolStripStatusLabel1"].Text = "Best Fit Descendant";
            bp.BestFitDesc();

            for (int b = 0; b < bp.bins.Count; b++)
            {
                SHFILEINFO shinfo = new SHFILEINFO();
                IntPtr hImgLarge;    //the handle to the system image list

                string counter = (b + 1).ToString();

                Bin bin = bp.bins[b];

                TabPage newPage = new TabPage();
                newPage.Name = "tabPage" + counter;
                newPage.Text = "Set " + counter;
                newPage.ToolTipText = string.Format("{0:N0} bytes", bin.size);

                tabControl1.TabPages.Add(newPage);
                newPage.Update();

                ListView newListView = new ListView();
                newListView.View = View.Tile;
                newListView.TileSize = new System.Drawing.Size(280, 66);
                newListView.Name = "listView" + counter;
                newListView.Columns.Add("Name", "Name", 180);
                newListView.Columns.Add("Size", "Size", 100);
                newListView.Columns.Add("Path", "Path", 270);

                newListView.LargeImageList = imageListResults;
                newListView.ItemDrag += new ItemDragEventHandler(this.newListView_ItemDrag);
                newListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
                newListView.BackColor = System.Drawing.Color.White;
                newListView.GridLines = false;
                newListView.FullRowSelect = true;

                newListView.Dock = DockStyle.Fill;
                newPage.Padding = new Padding(0);
                newListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
                newListView.BackColor = System.Drawing.Color.White;
                newListView.GridLines = false;
                newListView.FullRowSelect = true;

                ContextMenuStrip setMenu = new ContextMenuStrip();
                ToolStripMenuItem exportItem = new ToolStripMenuItem("Export to CDBurnerXP (.dxp)");
                exportItem.Tag = newListView;
                exportItem.Click += new EventHandler(this.ExportToDXP_Click);
                setMenu.Items.Add(exportItem);

                ToolStripMenuItem copyFolderItem = new ToolStripMenuItem("Copy Set to Folder");
                copyFolderItem.Tag = newListView;
                copyFolderItem.Click += new EventHandler(this.CopyToFolder_Click);
                setMenu.Items.Add(copyFolderItem);

                ToolStripMenuItem exportTxtItem = new ToolStripMenuItem("Export to Text (.txt)");
                exportTxtItem.Tag = newListView;
                exportTxtItem.Click += new EventHandler(this.ExportToTxt_Click);
                setMenu.Items.Add(exportTxtItem);

                newListView.ContextMenuStrip = setMenu;

                newPage.Controls.Add(newListView);

                foreach (BinItem item in bin.items)
                {
                    //Use this to get the small Icon
                    //hImgSmall = Win32.SHGetFileInfo(item.itemPath,
                    //    0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                    //    Win32.SHGFI_ICON | Win32.SHGFI_SMALLICON);

                    //Use this to get the large Icon
                    hImgLarge = Win32.SHGetFileInfo(item.itemPath,
                        0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                        Win32.SHGFI_ICON | Win32.SHGFI_LARGEICON);

                    //The icon is returned in the hIcon member of the shinfo struct
                    System.Drawing.Icon myIcon;
                    if (shinfo.hIcon != IntPtr.Zero)
                        myIcon = System.Drawing.Icon.FromHandle(shinfo.hIcon);
                    else
                        myIcon = System.Drawing.SystemIcons.Application;
                    imageListResults.Images.Add(myIcon);

                    ListViewItem lvItem = new ListViewItem(item.displayName, nIndex++);
                    lvItem.SubItems.Add(item.itemSize.ToString());
                    lvItem.SubItems.Add(item.itemPath);
                    lvItem.ToolTipText = item.itemPath;

                    ListViewItem newItem = newListView.Items.Add(lvItem);
                    // newItem.Selected = true; // Removed so it doesn't default to a sea of light blue.
                }

                newPage = null;
                newListView = null;
            }

            if (bp.oversized.Count > 0)
            {
                ulong oversizedSize = 0;
                for (int o = 0; o < bp.oversized.Count; o++)
                {
                    oversizedSize += bp.oversized[o].itemSize;
                }

                TabPage newPage = new TabPage();
                newPage.Name = "tabPageOversized";
                newPage.Text = "Oversized";
                newPage.ToolTipText = string.Format("{0:N0} bytes", oversizedSize);

                tabControl1.TabPages.Add(newPage);

                ListView newListView = new ListView();
                newListView.View = View.Details;
                newListView.Name = "listViewOversized";
                newListView.Columns.Add("Name", "Name", 180);
                newListView.Columns.Add("Size", "Size", 100);
                newListView.Columns.Add("Path", "Path", 270);

                newListView.Dock = DockStyle.Fill;
                newPage.Padding = new Padding(0);
                newListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
                newListView.BackColor = System.Drawing.Color.White;
                newListView.GridLines = false;
                newListView.FullRowSelect = true;

                ContextMenuStrip setMenu = new ContextMenuStrip();
                ToolStripMenuItem exportItem = new ToolStripMenuItem("Export to CDBurnerXP (.dxp)");
                exportItem.Tag = newListView;
                exportItem.Click += new EventHandler(this.ExportToDXP_Click);
                setMenu.Items.Add(exportItem);

                ToolStripMenuItem copyFolderItem = new ToolStripMenuItem("Copy Set to Folder");
                copyFolderItem.Tag = newListView;
                copyFolderItem.Click += new EventHandler(this.CopyToFolder_Click);
                setMenu.Items.Add(copyFolderItem);

                ToolStripMenuItem exportTxtItem = new ToolStripMenuItem("Export to Text (.txt)");
                exportTxtItem.Tag = newListView;
                exportTxtItem.Click += new EventHandler(this.ExportToTxt_Click);
                setMenu.Items.Add(exportTxtItem);

                newListView.ContextMenuStrip = setMenu;

                newPage.Controls.Add(newListView);

                for (int o = 0; o < bp.oversized.Count; o++)
                {
                    ListViewItem lvItem = new ListViewItem(bp.oversized[o].displayName);
                    lvItem.SubItems.Add(bp.oversized[o].itemSize.ToString());
                    lvItem.SubItems.Add(bp.oversized[o].itemPath);
                    lvItem.ToolTipText = bp.oversized[o].itemPath;
                    newListView.Items.Add(lvItem);
                }

                newPage = null;
                newListView = null;

            }

            //formResult = null;
            bp = null;
            tabControl1_SelectedIndexChanged(null, null);
            ApplyModernStyle();
            GC.Collect();
            
        }

        class DirNode
        {
            public Dictionary<string, DirNode> SubDirs = new Dictionary<string, DirNode>(StringComparer.OrdinalIgnoreCase);
            public List<string[]> Files = new List<string[]>();
        }

        private void WriteDirNode(StreamWriter sw, string name, DirNode node, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 2);
            sw.WriteLine(indent + "<dir name=\"" + System.Security.SecurityElement.Escape(name) + "\">");
            foreach (var kvp in node.SubDirs)
            {
                WriteDirNode(sw, kvp.Key, kvp.Value, indentLevel + 1);
            }
            foreach (var file in node.Files)
            {
                sw.WriteLine(indent + "  <file name=\"" + System.Security.SecurityElement.Escape(file[0]) + "\" path=\"" + System.Security.SecurityElement.Escape(file[1]) + "\" />");
            }
            sw.WriteLine(indent + "</dir>");
        }

        private void ExportToDXP_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null) return;
            ListView targetLv = item.Tag as ListView;
            if (targetLv == null) return;

            string title = "Set";
            if (targetLv.Parent is TabPage)
            {
                title = ((TabPage)targetLv.Parent).Text.Replace(" ", "");
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CDBurnerXP Compilation (*.dxp)|*.dxp";
            sfd.FileName = title + ".dxp";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    DirNode root = new DirNode();
                    foreach (ListViewItem lvItem in targetLv.Items)
                    {
                        string absPath = lvItem.SubItems[2].Text;
                        string displayPath = lvItem.Text;
                        string name = Path.GetFileName(displayPath);
                        string dirPath = Path.GetDirectoryName(displayPath);
                        
                        string relPath = "";
                        if (dirPath != null && Path.IsPathRooted(dirPath))
                        {
                            string rootPath = Path.GetPathRoot(dirPath);
                            if (dirPath.Length > rootPath.Length)
                                relPath = dirPath.Substring(rootPath.Length);
                        }
                        else if (dirPath != null)
                        {
                            relPath = dirPath;
                        }

                        string[] parts = string.IsNullOrEmpty(relPath) ? new string[0] : relPath.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                        
                        DirNode current = root;
                        foreach (string part in parts)
                        {
                            if (!current.SubDirs.ContainsKey(part))
                                current.SubDirs[part] = new DirNode();
                            current = current.SubDirs[part];
                        }
                        current.Files.Add(new string[] { name, absPath });
                    }

                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
                        sw.WriteLine("<compilation>");
                        sw.WriteLine("  <layout>");
                        WriteDirNode(sw, "\\", root, 2);
                        sw.WriteLine("  </layout>");
                        sw.WriteLine("</compilation>");
                    }
                    MessageBox.Show("Successfully exported to " + sfd.FileName, "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CopyToFolder_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null) return;
            ListView targetLv = item.Tag as ListView;
            if (targetLv == null) return;

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select a destination folder to copy the files into:";
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string destPath = fbd.SelectedPath;
                try
                {
                    List<string> sourceList = new List<string>();
                    List<string> destList = new List<string>();

                    foreach (ListViewItem lvItem in targetLv.Items)
                    {
                        string sourceFilePath = lvItem.SubItems[2].Text;
                        if (!File.Exists(sourceFilePath)) continue;

                        string displayPath = lvItem.Text;
                        string dirPath = Path.GetDirectoryName(displayPath);
                        
                        string relPath = "";
                        if (dirPath != null && Path.IsPathRooted(dirPath))
                        {
                            string rootPath = Path.GetPathRoot(dirPath);
                            if (dirPath.Length > rootPath.Length)
                                relPath = dirPath.Substring(rootPath.Length);
                        }
                        else if (dirPath != null)
                        {
                            relPath = dirPath;
                        }

                        string targetDir = Path.Combine(destPath, relPath.TrimStart('\\'));
                        Directory.CreateDirectory(targetDir);

                        string targetFile = Path.Combine(targetDir, Path.GetFileName(sourceFilePath));
                        
                        sourceList.Add(sourceFilePath);
                        destList.Add(targetFile);
                    }

                    if (sourceList.Count > 0)
                    {
                        string pFrom = string.Join("\0", sourceList) + "\0\0";
                        string pTo = string.Join("\0", destList) + "\0\0";

                        IntPtr pFromPtr = Marshal.StringToHGlobalUni(pFrom);
                        IntPtr pToPtr = Marshal.StringToHGlobalUni(pTo);

                        try
                        {
                            SHFILEOPSTRUCT fileOp = new SHFILEOPSTRUCT
                            {
                                hwnd = this.Handle,
                                wFunc = 0x0002, // FO_COPY
                                pFrom = pFromPtr,
                                pTo = pToPtr,
                                fFlags = 0x0001 | 0x0200 // FOF_MULTIDESTFILES | FOF_NOCONFIRMMKDIR
                            };

                            int res = SHFileOperation(ref fileOp);
                            if (res == 0 && !fileOp.fAnyOperationsAborted)
                            {
                                MessageBox.Show("Successfully copied files to " + destPath, "Copy Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(pFromPtr);
                            Marshal.FreeHGlobal(pToPtr);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error copying files: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportToTxt_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (item == null) return;
            ListView targetLv = item.Tag as ListView;
            if (targetLv == null) return;

            string title = "Set";
            if (targetLv.Parent is TabPage)
            {
                title = ((TabPage)targetLv.Parent).Text.Replace(" ", "");
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Text File (*.txt)|*.txt";
            sfd.FileName = title + ".txt";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        foreach (ListViewItem lvItem in targetLv.Items)
                        {
                            sw.WriteLine(lvItem.Text);
                        }
                    }
                    MessageBox.Show("Successfully exported to " + sfd.FileName, "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("DiscFit is a lightweight Bin Packing tool.\nAuthor: David Strencsev\n\nWould you like to visit the author's GitHub page at https://github.com/PhiSYS?", "About DiscFit", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://github.com/PhiSYS") { UseShellExecute = true });
            }
        }

        private void addFolderContentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult folder_result = this.addFolderDialog.ShowDialog();
            if (folder_result == DialogResult.OK)
            {
                FileFuncs ffunc = new FileFuncs();
                string path = this.addFolderDialog.SelectedPath;
                string basePath = path;
                if (basePath != null && !basePath.EndsWith("\\")) basePath += "\\";
                
                string[] contents = ffunc.GetFolderContents(path);

                foreach (string item in contents)
                {
                    ulong size = ffunc.GetItemSize(item);
                    
                    string displayPath = (basePath != null && item.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                                         ? item.Substring(basePath.Length)
                                         : item;

                    ListViewItem folder = new ListViewItem(displayPath);
                    folder.SubItems.Add(size.ToString());
                    folder.SubItems.Add(item);
                    this.listView1.Items.Add(folder);
                }

                this.displaySystemIcons(listView1, imageList1);
            }
        }

        private void addFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult folder_result = this.addFolderDialog.ShowDialog();
            if (folder_result == DialogResult.OK)
            {
                FileFuncs ffunc = new FileFuncs();
                string path = this.addFolderDialog.SelectedPath;

                string basePath = Path.GetDirectoryName(path);
                if (basePath != null && !basePath.EndsWith("\\")) basePath += "\\";

                string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    ulong size = ffunc.GetItemSize(file);
                    
                    string displayPath = (basePath != null && file.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                                         ? file.Substring(basePath.Length)
                                         : file;

                    ListViewItem item = new ListViewItem(displayPath);
                    item.SubItems.Add(size.ToString());
                    item.SubItems.Add(file);
                    this.listView1.Items.Add(item);
                }

                this.displaySystemIcons(listView1, imageList1);
            }

        }

        private void addFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult files_result = this.addFilesDialog.ShowDialog();
            if (files_result == DialogResult.OK)
            {
                FileFuncs ffunc = new FileFuncs();
                string[] files = this.addFilesDialog.FileNames;

                foreach (string file in files)
                {
                    ulong size = ffunc.GetItemSize(file);

                    ListViewItem folder = new ListViewItem(file);
                    folder.SubItems.Add(size.ToString());
                    folder.SubItems.Add(file);
                    folder.ToolTipText = file;
                    this.listView1.Items.Add(folder);
                }

                this.displaySystemIcons(listView1, imageList1);
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.listView1.Clear();
            this.imageList1.Images.Clear();
            newResults();
            GC.Collect();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection lvSelected = listView1.SelectedItems;

            foreach (ListViewItem lvi in lvSelected)
            {
                listView1.Items.Remove(lvi);
            }
        }

        private void newListView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            //Begins a drag-and-drop operation in the ListView control.
            ListView DynamicControl = sender as ListView;
            string[] items = new string[DynamicControl.SelectedItems.Count];
            int c = 0;

            foreach (ListViewItem item in DynamicControl.SelectedItems)
            {
                items[c] = item.SubItems[2].Text; // Path
                c++;
            }

            DynamicControl.DoDragDrop(new DataObject(DataFormats.FileDrop, items), DragDropEffects.Copy);
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            FileFuncs ffunc = new FileFuncs();
            string[] handles = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach (string s in handles)
            {
                if (Directory.Exists(s))
                {
                    string basePath = Path.GetDirectoryName(s);
                    if (basePath != null && !basePath.EndsWith("\\")) basePath += "\\";

                    string[] files = Directory.GetFiles(s, "*.*", SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        ulong size = ffunc.GetItemSize(file);
                        string displayPath = (basePath != null && file.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                                             ? file.Substring(basePath.Length)
                                             : file;

                        ListViewItem item = new ListViewItem(displayPath);
                        item.SubItems.Add(size.ToString());
                        item.SubItems.Add(file);
                        item.ToolTipText = file;
                        this.listView1.Items.Add(item);
                    }
                }
                else if (File.Exists(s))
                {
                    ulong size = ffunc.GetItemSize(s);

                    ListViewItem item = new ListViewItem(s);
                    item.SubItems.Add(size.ToString());
                    item.SubItems.Add(s);
                    item.ToolTipText = s;
                    this.listView1.Items.Add(item);
                }
            }

            this.displaySystemIcons(listView1, imageList1);
        }

        private void displaySystemIcons(ListView lv, ImageList il)
        {
            int nIndex = 0;
            il.Images.Clear();

            SHFILEINFO shinfo = new SHFILEINFO();
            //IntPtr hImgSmall;    //the handle to the system image list
            IntPtr hImgLarge;    //the handle to the system image list
            //listView1.SmallImageList = imageList1;
            lv.LargeImageList = il;

            foreach (ListViewItem item in lv.Items)
            {
                //Use this to get the small Icon
                //hImgSmall = Win32.SHGetFileInfo(item,
                //    0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                //    Win32.SHGFI_ICON | Win32.SHGFI_SMALLICON);

                //Use this to get the large Icon
                hImgLarge = Win32.SHGetFileInfo(item.SubItems[2].Text,
                    0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                    Win32.SHGFI_ICON | Win32.SHGFI_LARGEICON);

                //The icon is returned in the hIcon member of the shinfo struct
                System.Drawing.Icon myIcon;
                if (shinfo.hIcon != IntPtr.Zero)
                    myIcon = System.Drawing.Icon.FromHandle(shinfo.hIcon);
                else
                    myIcon = System.Drawing.SystemIcons.Application;

                il.Images.Add(myIcon);
                item.ImageIndex = nIndex++;
            }
        }

        private void ListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            ListView lv = (ListView)sender;
            bool isDark = checkBoxTheme != null && checkBoxTheme.Checked;

            Brush bgBrush;
            Color fgColor = isDark ? Color.White : Color.Black;
            Color subFgColor = isDark ? Color.LightGray : Color.Gray;

            if (e.Item.Selected) {
                if (isDark) {
                    bgBrush = new SolidBrush(Color.FromArgb(60, 60, 60)); // Highlight matching dark screenshot
                } else {
                    bgBrush = new SolidBrush(Color.FromArgb(204, 232, 255)); // Typical Windows light selection color
                }
            } else {
                bgBrush = new SolidBrush(lv.BackColor);
            }

            Rectangle fullRect = new Rectangle(0, e.Bounds.Top, lv.ClientSize.Width, e.Bounds.Height);
            e.Graphics.FillRectangle(bgBrush, fullRect);

            // Draw Icon
            Rectangle iconRect = new Rectangle(e.Bounds.Left + 4, e.Bounds.Top + ((e.Bounds.Height - 32) / 2), 32, 32);
            if (lv.LargeImageList != null && e.Item.ImageIndex >= 0 && e.Item.ImageIndex < lv.LargeImageList.Images.Count)
            {
                e.Graphics.DrawImage(lv.LargeImageList.Images[e.Item.ImageIndex], iconRect);
            }

            // Separate and measure elements
            string fullItemText = e.Item.Text;
            string fileName = fullItemText;
            string folderName = "";
            int lastSlash = fullItemText.LastIndexOf('\\');
            if (lastSlash < 0) lastSlash = fullItemText.LastIndexOf('/');
            if (lastSlash >= 0)
            {
                fileName = fullItemText.Substring(lastSlash + 1);
                folderName = fullItemText.Substring(0, lastSlash + 1);
            }

            int safeWidth = Math.Max(1, lv.ClientSize.Width - 50);

            // Measure FileName
            Size fnMax = new Size(safeWidth, e.Bounds.Height - 24);
            Size fnSz = TextRenderer.MeasureText(e.Graphics, fileName, lv.Font, fnMax, TextFormatFlags.Left | TextFormatFlags.WordBreak);
            int fnHeight = Math.Min(fnSz.Height, e.Bounds.Height - 24);
            Rectangle nameRect = new Rectangle(e.Bounds.Left + 42, e.Bounds.Top + 6, safeWidth, fnHeight);
            TextRenderer.DrawText(e.Graphics, fileName, lv.Font, nameRect, fgColor, TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);

            // Measure Folder Path
            int folderY = nameRect.Bottom + 2;
            int maxSpace = Math.Max(lv.Font.Height, e.Bounds.Bottom - folderY - 20);
            int maxLines = maxSpace / lv.Font.Height;
            int maxFHeight = maxLines * lv.Font.Height;
            
            int folderHeight = 0;
            if (!string.IsNullOrEmpty(folderName))
            {
                Size fSz = TextRenderer.MeasureText(e.Graphics, folderName, lv.Font, new Size(safeWidth, maxFHeight), TextFormatFlags.Left | TextFormatFlags.WordBreak);
                folderHeight = Math.Min(fSz.Height, maxFHeight);
                Rectangle folderRect = new Rectangle(e.Bounds.Left + 42, folderY, safeWidth, folderHeight);
                TextRenderer.DrawText(e.Graphics, folderName, lv.Font, folderRect, subFgColor, TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.TextBoxControl);
            }

            // Draw Size
            if (e.Item.SubItems.Count > 1)
            {
                string sizeText = e.Item.SubItems[1].Text;
                Rectangle sizeRect = new Rectangle(e.Bounds.Left + 42, folderY + folderHeight + 2, safeWidth, 20);
                TextRenderer.DrawText(e.Graphics, sizeText, lv.Font, sizeRect, subFgColor, TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
            }
            bgBrush.Dispose();
        }

        private void ListView_Resize(object sender, EventArgs e)
        {
            if (sender is ListView lv && lv.View == View.Tile)
            {
                int newWidth = Math.Max(lv.ClientSize.Width - 20, 10);
                if (lv.TileSize.Width != newWidth)
                {
                    lv.Resize -= ListView_Resize;
                    lv.BeginUpdate();
                    lv.TileSize = new Size(newWidth, 66);
                    // Force complete bounds invalidation by destroying the view cache
                    lv.View = View.Details;
                    lv.View = View.Tile;
                    lv.EndUpdate();
                    lv.Resize += ListView_Resize;
                }
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null)
            {
                ListView targetLv = tabControl1.SelectedTab.Controls.OfType<ListView>().FirstOrDefault();
                if (targetLv != null)
                {
                    ulong totalSize = 0;
                    foreach (ListViewItem item in targetLv.Items)
                    {
                        totalSize += Convert.ToUInt64(item.SubItems[1].Text);
                    }
                    toolStripStatusLabel2.Text = string.Format("{0:N0} bytes", totalSize);
                }
                else
                {
                    toolStripStatusLabel2.Text = "";
                }
            }
            else
            {
                toolStripStatusLabel2.Text = "";
            }
        }

        private void comboSizes_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void newResults()
        {
            imageListResults.Images.Clear();
            tabControl1.TabPages.Clear();

            // Show Panel of results
            splitContainer1.Panel2Collapsed = false;
            splitContainer1.Refresh();
        }

        private void comboMediaType_SelectedIndexChanged(object sender, EventArgs e)
        {
            maskedTextBox_MediaSize.Text = comboMediaType.SelectedValue.ToString();
        }

        private void checkBoxTheme_CheckedChanged(object sender, EventArgs e)
        {
            ApplyModernStyle();
        }

        private void ApplyModernStyle()
        {
            bool isDark = checkBoxTheme != null && checkBoxTheme.Checked;

            try { SetPreferredAppMode(isDark ? 2 : 0); } catch { }

            int useImmersiveDarkMode = isDark ? 1 : 0;
            try
            {
                // DWMWA_USE_IMMERSIVE_DARK_MODE = 20
                DwmSetWindowAttribute(this.Handle, 20, ref useImmersiveDarkMode, sizeof(int));
                // DWMWA_USE_IMMERSIVE_DARK_MODE_V2 = 19 (for older Windows 10)
                DwmSetWindowAttribute(this.Handle, 19, ref useImmersiveDarkMode, sizeof(int));
                
                // DWMWA_CAPTION_COLOR = 35 
                // Color format is BGR (0x00bbggrr)
                int captionColor = isDark ? 0x00141414 : 0x00FFFFFF; 
                DwmSetWindowAttribute(this.Handle, 35, ref captionColor, sizeof(int));
            }
            catch { }

            Color bgColor = isDark ? Color.FromArgb(45, 45, 48) : Color.FromArgb(243, 243, 243);
            Color fgColor = isDark ? Color.White : Color.Black;
            Color panelColor = isDark ? Color.FromArgb(30, 30, 30) : Color.FromArgb(243, 243, 243);
            Color listBgColor = isDark ? Color.FromArgb(30, 30, 30) : Color.White;
            Color listFgColor = isDark ? Color.White : Color.Black;
            Color btnBgColor = isDark ? Color.FromArgb(0, 122, 204) : Color.FromArgb(0, 120, 212);
            Color btnFgColor = Color.White;
            Color splitterColor = isDark ? Color.FromArgb(60, 60, 60) : Color.FromArgb(220, 220, 220);
            
            this.BackColor = bgColor;
            this.ForeColor = fgColor;

            // Manage TabControl empty state
            this.tabControl1.IsDarkMode = isDark;
            this.tabControl1.Appearance = TabAppearance.Normal;
            this.tabControl1.Visible = this.tabControl1.TabPages.Count > 0;
            this.tabControl1.Invalidate();

            // Style Main ListView
            this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView1.ShowItemToolTips = true;
            if (this.listView1.View != View.Details)
            {
                this.listView1.OwnerDraw = true;
                this.listView1.DrawItem -= ListView_DrawItem;
                this.listView1.DrawItem += ListView_DrawItem;
            }
            this.listView1.Resize -= ListView_Resize;
            this.listView1.Resize += ListView_Resize;
            // Native Theme disabled to allow OwnerDraw to work properly.
            string themeName = isDark ? "DarkMode_Explorer" : "Explorer";
            SetWindowTheme(this.listView1.Handle, themeName, null);
            try { AllowDarkModeForWindow(this.listView1.Handle, isDark ? 1 : 0); } catch { }
            if (this.listView1.View == View.Tile)
            {
                int tw = Math.Max(this.listView1.ClientSize.Width - 20, 10);
                this.listView1.TileSize = new Size(tw, 66); 
            }
            this.listView1.BackColor = listBgColor;
            this.listView1.ForeColor = listFgColor;

            // Update created tabs
            if (this.groupMedia != null && this.groupMedia is DarkGroupBox dgb)
            {
                dgb.IsDarkMode = isDark;
                dgb.SetUserPaint(isDark);
                dgb.Invalidate();
            }
            if (this.tabControl1 != null && this.tabControl1 is DarkTabControl dtc)
            {
                dtc.IsDarkMode = isDark;
                dtc.SetUserPaint(isDark);
                dtc.Invalidate();
            }
            foreach (TabPage tab in tabControl1.TabPages)
            {
                tab.BackColor = listBgColor;
                tab.ForeColor = listFgColor;
                foreach (Control c in tab.Controls)
                {
                    if (c is ListView lv)
                    {
                        lv.BackColor = listBgColor;
                        lv.ForeColor = listFgColor;
                        lv.ShowItemToolTips = true;
                        if (lv.View != View.Details)
                        {
                            lv.OwnerDraw = true;
                            lv.DrawItem -= ListView_DrawItem;
                            lv.DrawItem += ListView_DrawItem;
                        }
                        lv.Resize -= ListView_Resize;
                        lv.Resize += ListView_Resize;
                        SetWindowTheme(lv.Handle, themeName, null);
                        try { AllowDarkModeForWindow(lv.Handle, isDark ? 1 : 0); } catch { }
                        if (lv.View == View.Tile)
                        {
                            int tw = Math.Max(lv.ClientSize.Width - 20, 10);
                            lv.TileSize = new Size(tw, 66);
                        }
                    }
                }
            }

            // Style the Pack Button
            this.buttonPack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPack.FlatAppearance.BorderSize = 0;
            this.buttonPack.BackColor = btnBgColor;
            this.buttonPack.ForeColor = btnFgColor;
            this.buttonPack.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonPack.Cursor = System.Windows.Forms.Cursors.Hand;

            // Style SplitContainer
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.splitContainer1.Panel1.BackColor = panelColor;
            this.splitContainer1.Panel2.BackColor = panelColor;
            this.splitContainer1.BackColor = splitterColor; // Border/Splitter color
            this.splitContainer1.SplitterWidth = 2;

            // Style Menus
            this.menuStrip1.BackColor = panelColor;
            this.menuStrip1.ForeColor = fgColor;
            this.statusStrip1.BackColor = panelColor;
            this.statusStrip1.ForeColor = fgColor;
            if (this.contextMenuStrip1 != null)
            {
                this.contextMenuStrip1.BackColor = panelColor;
                this.contextMenuStrip1.ForeColor = fgColor;
            }
            
            if (isDark) {
                this.menuStrip1.Renderer = new DarkToolStripRenderer();
                this.statusStrip1.Renderer = new DarkToolStripRenderer();
                if (this.contextMenuStrip1 != null) this.contextMenuStrip1.Renderer = new DarkToolStripRenderer();
                
                foreach (TabPage tab in tabControl1.TabPages)
                {
                    foreach (Control c in tab.Controls)
                    {
                        if (c is ListView lv && lv.ContextMenuStrip != null)
                        {
                            lv.ContextMenuStrip.Renderer = new DarkToolStripRenderer();
                            lv.ContextMenuStrip.ForeColor = fgColor;
                        }
                    }
                }
            } else {
                this.menuStrip1.RenderMode = ToolStripRenderMode.ManagerRenderMode;
                this.statusStrip1.RenderMode = ToolStripRenderMode.ManagerRenderMode;
                if (this.contextMenuStrip1 != null) this.contextMenuStrip1.RenderMode = ToolStripRenderMode.ManagerRenderMode;
                foreach (TabPage tab in tabControl1.TabPages)
                {
                    foreach (Control c in tab.Controls)
                    {
                        if (c is ListView lv && lv.ContextMenuStrip != null)
                        {
                            lv.ContextMenuStrip.RenderMode = ToolStripRenderMode.ManagerRenderMode;
                            lv.ContextMenuStrip.ForeColor = fgColor;
                        }
                    }
                }
            }

            // Style Media Type Combo & Text Box
            this.comboMediaType.IsDarkMode = isDark;
            this.comboMediaType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboMediaType.BackColor = isDark ? Color.FromArgb(45, 45, 48) : SystemColors.Window;
            this.comboMediaType.ForeColor = fgColor;
            this.comboMediaType.Invalidate();

            this.maskedTextBox_MediaSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.maskedTextBox_MediaSize.BackColor = isDark ? Color.FromArgb(45, 45, 48) : SystemColors.Window;
            this.maskedTextBox_MediaSize.ForeColor = fgColor;

            // GroupBox
            this.groupMedia.IsDarkMode = isDark;
            this.groupMedia.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.groupMedia.ForeColor = fgColor;
            this.groupMedia.Invalidate();
            
            // checkBoxTheme
            if (this.checkBoxTheme != null)
            {
                this.checkBoxTheme.Text = "Dark Mode";
            }
        }
        
        private void ComboMediaType_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            bool isDark = checkBoxTheme != null && checkBoxTheme.Checked;
            Color bg = e.State.HasFlag(DrawItemState.Selected) ? 
                (isDark ? Color.FromArgb(0, 120, 212) : SystemColors.Highlight) : 
                (isDark ? Color.FromArgb(45, 45, 48) : SystemColors.Window);
            Color fg = e.State.HasFlag(DrawItemState.Selected) ? 
                (isDark ? Color.White : SystemColors.HighlightText) : 
                (isDark ? Color.White : SystemColors.WindowText);
            
            using (SolidBrush b = new SolidBrush(bg))
                e.Graphics.FillRectangle(b, e.Bounds);
            
            string text = comboMediaType.GetItemText(comboMediaType.Items[e.Index]);
            using (SolidBrush b = new SolidBrush(fg))
                e.Graphics.DrawString(text, e.Font, b, e.Bounds.X + 2, e.Bounds.Y + ((e.Bounds.Height - e.Font.Height) / 2));
        }

        private string baseTitle = "DiscFit - Version 1.7 - David Strencsev";
        
        private void UpdateTitle()
        {
            using (Graphics g = this.CreateGraphics())
            {
                this.Text = baseTitle;
            }
        }
    }

    public class DarkTabControl : TabControl
    {
        public bool IsDarkMode { get; set; } = false;

        public DarkTabControl()
        {
            this.DrawMode = TabDrawMode.Normal;
        }

        public void SetUserPaint(bool enabled)
        {
            this.SetStyle(ControlStyles.UserPaint, enabled);
            this.DrawMode = enabled ? TabDrawMode.OwnerDrawFixed : TabDrawMode.Normal;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Color bgColor = IsDarkMode ? Color.FromArgb(30, 30, 30) : SystemColors.Control;
            Color tabBg = IsDarkMode ? Color.FromArgb(45, 45, 48) : SystemColors.ControlLight;
            Color selectedTabBg = IsDarkMode ? Color.FromArgb(60, 60, 60) : SystemColors.Window;
            Color fgColor = IsDarkMode ? Color.White : SystemColors.ControlText;
            Color borderColor = IsDarkMode ? Color.FromArgb(60, 60, 60) : SystemColors.ControlDark;

            // Theme the automatically spawned UpDown arrow buttons sub-component
            IntPtr upDown = Form1.FindWindowEx(this.Handle, IntPtr.Zero, "msctls_updown32", null);
            if (upDown != IntPtr.Zero)
            {
                Form1.SetWindowTheme(upDown, IsDarkMode ? "DarkMode_Explorer" : "Explorer", null);
                try { Form1.AllowDarkModeForWindow(upDown, IsDarkMode ? 1 : 0); } catch { }
            }

            // Draw Background
            e.Graphics.Clear(bgColor);

            // Draw Tab Buttons
            for (int i = 0; i < this.TabCount; i++)
            {
                Rectangle tabRect = this.GetTabRect(i);
                bool isSelected = (this.SelectedIndex == i);

                using (SolidBrush b = new SolidBrush(isSelected ? selectedTabBg : tabBg))
                {
                    e.Graphics.FillRectangle(b, tabRect);
                }

                // Draw solid block line separating tabs or connecting background
                if (!isSelected) {
                    using (Pen p = new Pen(Color.FromArgb(50, 50, 50)))
                        e.Graphics.DrawLine(p, tabRect.Right - 1, tabRect.Top + 2, tabRect.Right - 1, tabRect.Bottom - 2);
                }

                // Draw Text
                TextRenderer.DrawText(e.Graphics, this.TabPages[i].Text, this.Font, tabRect, fgColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            // Draw Top Border across the panel so it connects selected
            if (this.TabCount > 0)
            {
                int yTop = this.GetTabRect(0).Bottom;
                Rectangle selectedTabRect = this.SelectedIndex >= 0 ? this.GetTabRect(this.SelectedIndex) : Rectangle.Empty;

                using (Pen p = new Pen(selectedTabBg))
                {
                    // Draw continuous line under unselected tabs to separate tabs from content panel
                    e.Graphics.DrawLine(p, 0, yTop, this.Width, yTop);
                    
                    // Overwrite the line under the selected tab with its own background color to "connect" it to the panel
                    if (this.SelectedIndex >= 0)
                    {
                        using (Pen mask = new Pen(selectedTabBg, 2))
                            e.Graphics.DrawLine(mask, selectedTabRect.Left + 1, yTop, selectedTabRect.Right - 1, yTop);
                    }
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x14 && IsDarkMode) // WM_ERASEBKGND
            {
                using (Graphics g = Graphics.FromHdc(m.WParam))
                {
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(30, 30, 30)))
                        g.FillRectangle(b, this.ClientRectangle);
                }
                m.Result = (IntPtr)1;
                return;
            }
            base.WndProc(ref m);
        }
    }

    public class DarkComboBox : ComboBox
    {
        public bool IsDarkMode { get; set; } = false;
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0xF && IsDarkMode && this.DropDownStyle != ComboBoxStyle.Simple) // WM_PAINT
            {
                using (Graphics g = Graphics.FromHwnd(this.Handle))
                {
                    Rectangle rect = new Rectangle(this.Width - 17, 0, 17, this.Height);
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(45, 45, 48)))
                        g.FillRectangle(b, rect);
                    Point[] arrow = new Point[] { 
                        new Point(rect.X + 4, rect.Y + (rect.Height / 2) - 1), 
                        new Point(rect.X + 10, rect.Y + (rect.Height / 2) - 1), 
                        new Point(rect.X + 7, rect.Y + (rect.Height / 2) + 2) 
                    };
                    using (SolidBrush b = new SolidBrush(Color.White))
                        g.FillPolygon(b, arrow);
                }
            }
        }
    }
    public class DarkGroupBox : GroupBox
    {
        public bool IsDarkMode { get; set; } = false;

        public DarkGroupBox()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        public void SetUserPaint(bool enabled)
        {
            // Lock out native painting transitions. Manual painting guarantees no UXTheme caching glitches
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Color bgColor = IsDarkMode ? Color.FromArgb(30, 30, 30) : (this.Parent != null ? this.Parent.BackColor : SystemColors.Control);
            Color fgColor = IsDarkMode ? Color.White : SystemColors.ControlText;
            Color borderColor = IsDarkMode ? Color.FromArgb(80, 80, 80) : SystemColors.ControlDark;

            e.Graphics.Clear(bgColor);

            Size tSize = TextRenderer.MeasureText(this.Text, this.Font);
            Rectangle borderRect = new Rectangle(0, tSize.Height / 2, this.Width - 1, this.Height - tSize.Height / 2 - 1);
            using (Pen borderPen = new Pen(borderColor))
            {
                e.Graphics.DrawRectangle(borderPen, borderRect);
            }

            Rectangle textRect = new Rectangle(6, 0, tSize.Width, tSize.Height);
            e.Graphics.FillRectangle(new SolidBrush(bgColor), textRect);
            TextRenderer.DrawText(e.Graphics, this.Text, this.Font, textRect, fgColor, TextFormatFlags.Left);
        }
    }

    public class DarkToolStripRenderer : ToolStripProfessionalRenderer
    {
        public DarkToolStripRenderer() : base(new DarkColorTable()) { }
        
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = Color.White;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            // Do not paint a border
        }
    }

    public class DarkColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground { get { return Color.FromArgb(43, 43, 43); } }
        public override Color ToolStripBorder { get { return Color.FromArgb(43, 43, 43); } }
        public override Color MenuItemSelected { get { return Color.FromArgb(60, 60, 60); } }
        public override Color MenuItemSelectedGradientBegin { get { return Color.FromArgb(60, 60, 60); } }
        public override Color MenuItemSelectedGradientEnd { get { return Color.FromArgb(60, 60, 60); } }
        public override Color MenuBorder { get { return Color.FromArgb(60, 60, 60); } }
        public override Color MenuItemPressedGradientBegin { get { return Color.FromArgb(60, 60, 60); } }
        public override Color MenuItemPressedGradientEnd { get { return Color.FromArgb(60, 60, 60); } }
        public override Color MenuItemPressedGradientMiddle { get { return Color.FromArgb(60, 60, 60); } }
        public override Color ImageMarginGradientBegin { get { return Color.FromArgb(43, 43, 43); } }
        public override Color ImageMarginGradientEnd { get { return Color.FromArgb(43, 43, 43); } }
        public override Color ImageMarginGradientMiddle { get { return Color.FromArgb(43, 43, 43); } }
        public override Color ToolStripGradientBegin { get { return Color.FromArgb(30, 30, 30); } }
        public override Color ToolStripGradientEnd { get { return Color.FromArgb(30, 30, 30); } }
        public override Color ToolStripGradientMiddle { get { return Color.FromArgb(30, 30, 30); } }
        public override Color StatusStripGradientBegin { get { return Color.FromArgb(30, 30, 30); } }
        public override Color StatusStripGradientEnd { get { return Color.FromArgb(30, 30, 30); } }
    }
}

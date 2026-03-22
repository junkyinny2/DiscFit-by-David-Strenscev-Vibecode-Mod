using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DiscFit
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public IntPtr iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };

    class Win32
    {
        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0;    // 'Large icon
        public const uint SHGFI_SMALLICON = 0x1;    // 'Small icon

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath,
            uint dwFileAttributes,
            ref SHFILEINFO psfi,
            uint cbSizeFileInfo,
            uint uFlags);
    }

    class FileFuncs
    {
        public ulong GetItemSize(string path)
        {
            ulong size = 0;

            if (Directory.Exists(path))
            {
                try
                {
                    string[] subdirs = Directory.GetDirectories(path);
                    string[] subfiles = Directory.GetFiles(path);

                    foreach (string dir in subdirs)
                    {
                        size += this.GetItemSize(dir);
                    }

                    foreach (string file in subfiles)
                    {
                        FileInfo fileinfo = new FileInfo(file);
                        size += (ulong)fileinfo.Length;
                    }
                }
                catch (UnauthorizedAccessException uae)
                {
                    Console.WriteLine("ERROR: {0}", uae.Message);
                }
                catch (IOException ioe)
                {
                    Console.WriteLine("ERROR: {0}", ioe.Message);
                }
                finally {}

            }
            else
            {
                FileInfo fileinfo = new FileInfo(path);
                size += (ulong)fileinfo.Length;
            }

            return size;
        }

        public string[] GetFolderContents(string folder)
        {
            string[] myfolders;
            string[] myfiles;
            string[] contents;

            try
            {
                myfolders = Directory.GetDirectories(folder);
                myfiles = Directory.GetFiles(folder);

                contents = new string[myfolders.Length + myfiles.Length];
                myfolders.CopyTo(contents, 0);
                myfiles.CopyTo(contents, myfolders.Length);
                myfolders = null;
                myfiles = null;

                return contents;
            }
            catch (UnauthorizedAccessException uae)
            {
                Console.WriteLine("ERROR: {0}", uae.Message);
                return new string[0];
            }
            catch (IOException ioe)
            {
                Console.WriteLine("ERROR: {0}", ioe.Message);
                return new string[0];
            }
            finally {}
        }
    }
}

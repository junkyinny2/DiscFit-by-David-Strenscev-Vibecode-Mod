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
        public const uint SHGFI_ICON               = 0x000000100;
        public const uint SHGFI_LARGEICON          = 0x000000000; // Large icon
        public const uint SHGFI_SMALLICON          = 0x000000001; // Small icon
        public const uint SHGFI_USEFILEATTRIBUTES  = 0x000000010; // Use dwFileAttributes, don't hit disk

        public const uint FILE_ATTRIBUTE_NORMAL    = 0x00000080;
        public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath,
            uint dwFileAttributes,
            ref SHFILEINFO psfi,
            uint cbSizeFileInfo,
            uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyIcon(IntPtr hIcon);
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

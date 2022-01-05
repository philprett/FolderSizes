using System;
using System.IO;

namespace FolderSizes
{
	internal class FolderFile
	{
		public FolderFile(FileInfo file)
		{
			try
			{
				IsFolder = false;
				FullName = file.FullName;
				Name = file.Name;
				Size = file.Length;
				ErrorMessage = string.Empty;
			}
			catch (Exception ex)
			{
				ErrorMessage = ex.Message;
			}
		}

		public FolderFile(DirectoryInfo dir)
		{
			try
			{
				IsFolder = true;
				FullName = dir.FullName;
				Name = dir.Name;
				Size = GetFolderSize(dir.FullName);
				ErrorMessage = string.Empty;
			}
			catch (Exception ex)
			{
				ErrorMessage = ex.Message;
			}
		}

		public bool IsFolder { get; set; }
		public string FullName { get; set; }
		public string Name { get; set; }
		public long Size { get; set; }
		public string ErrorMessage { get; set; }
		public string SizeFormatted
		{
			get { return Size.ToString("###,###,###,###,##0"); }
		}

		public static long GetFolderSize(string path)
		{
			var dir = new DirectoryInfo(path);
			if (dir.Exists == false) return 0;

			long size = 0;
			foreach (var file in dir.GetFiles()) size += file.Length;
			foreach (var sub in dir.GetDirectories()) size += GetFolderSize(sub.FullName);

			return size;
		}
	}
}
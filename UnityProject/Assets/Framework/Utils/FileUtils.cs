using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;

public static class FileTools
{
    public static bool TryReadFileToBytes (string fileFullName, out byte[] bytes)
    {
        bytes = null;
        FileInfo fi = new FileInfo (fileFullName);
        if (!fi.Exists) {
            return false;
        }
        using (FileStream fs = new FileStream (fileFullName, FileMode.Open, FileAccess.Read)) {
            bytes = new byte[fs.Length];
            fs.Read (bytes, 0, (int)fs.Length);
            fs.Close ();
            return true;
        }
    }
	public static byte[] ReadFileToBytes (string fileFullName)
	{
		FileInfo fi = new FileInfo(fileFullName);
		if (!fi.Exists)
		{
			Debug.LogError($"File {fileFullName} doesn't exist.");
			return null;
		}
        using (FileStream fs = new FileStream (fileFullName, FileMode.Open, FileAccess.Read)) {
            byte[] buffer = new byte[fs.Length];
            fs.Read (buffer, 0, (int)fs.Length);
            fs.Close ();
            return buffer;
        }
	}

    public static bool TryReadFileToString (string fileFullName, out string content)
    {
        content = string.Empty;
        byte[] bytes;
        if (TryReadFileToBytes (fileFullName, out bytes))
        {
            if (null == bytes || bytes.Length == 0)
            {
                return false;
            }
            content = Encoding.UTF8.GetString(bytes);
            return true;
        } 
        else
        {
            return false;
        }
    }

	public static string ReadFileToString (string fileFullName)
	{
		byte[] bytes = ReadFileToBytes(fileFullName);
		if (null == bytes || bytes.Length == 0)
		{
			return null;
		}
		return Encoding.UTF8.GetString(bytes);
	}

	public static bool WriteBytesToFile (byte[] bytes, string fileFullName, bool overwrite = true)
	{
		FileInfo fi = new FileInfo(fileFullName);
		if (fi.Exists && !overwrite)
		{
			Debug.LogError($"File {fileFullName} already exist, write file failed.");
			return false;
		}
        using (FileStream fs = new FileStream (fileFullName, FileMode.Create, FileAccess.Write))
        {
            fs.Write (bytes, 0, (int)bytes.Length);
            fs.Flush ();
            fs.Close ();
            return true;
        }
	}

	public static bool WriteStringToFile(string content, string fileFullName, bool overwrite = true)
	{
		if (string.IsNullOrEmpty(content))
		{
			Debug.LogError($"WriteStringToFile failed content IsNullOrEmpty! FileFullName: {fileFullName}.");
			return false;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(content);
		if (bytes.Length == 0)
		{
			Debug.LogError($"WriteStringToFile failed array size is 0! FileFullName: {fileFullName}");
			return false;
		}

		return WriteBytesToFile(bytes, fileFullName, overwrite);
	}

	public static void RenameFile (string fileFullName, string newFileFullName, bool overwrite = true)
	{
//			LogTool.Info("RenameFile, {0} to {1}", fileFullName, newFileFullName);
		if (!File.Exists(fileFullName))
		{
			Debug.LogError($"Source file {fileFullName} does not exist, rename file failed.");
			return;
		}
		if (File.Exists(newFileFullName))
		{
			if (overwrite)
			{
//					LogTool.Info("{0} exist, delete");
				File.Delete(newFileFullName);
//					LogTool.Info("After exist, File.Exists(newFileFullName): {0}", File.Exists(newFileFullName));
			}
			else
			{
				Debug.LogError($"File {newFileFullName} already exist, rename file failed.");
				return;
			}
		}

		File.Move(fileFullName, newFileFullName);
	}

	public static void CopyFileSync(string sourceFileFullName, string destFileFullName, bool overwrite = true)
	{
		if (!File.Exists(sourceFileFullName))
		{
			Debug.LogError($"Source file {sourceFileFullName} does not exist, copy file failed.");
			return;
		}
		if (File.Exists(destFileFullName))
		{
			if (overwrite)
			{
				File.Delete(destFileFullName);
			}
			else
			{
				Debug.LogError($"File {destFileFullName} already exist, copy file failed.");
				return;
			}
		}
		File.Copy(sourceFileFullName, destFileFullName);
	}
	
	public static void CopyDirectory(string srcDir, string tgtDir)
	{
		var source = new DirectoryInfo(srcDir);
		var target = new DirectoryInfo(tgtDir);

		if (target.FullName.StartsWith(source.FullName, StringComparison.Ordinal))
		{
			throw new Exception("父目录不能拷贝到子目录！");
		}

		if (!source.Exists)
		{
			return;
		}

		if (!target.Exists)
		{
			target.Create();
		}

		FileInfo[] files = source.GetFiles();

		for (int i = 0; i < files.Length; i++)
		{
			File.Copy(files[i].FullName, Path.Combine(target.FullName, files[i].Name), true);
		}

		DirectoryInfo[] dirs = source.GetDirectories();

		for (int j = 0; j < dirs.Length; j++)
		{
			CopyDirectory(dirs[j].FullName, Path.Combine(target.FullName, dirs[j].Name));
		}
	}

	public static long GetFileMd5 (string fileFullName, out string md5)
	{
		using (FileStream fs = new FileStream(fileFullName, FileMode.Open))
		{
			MD5 md5Provider = new MD5CryptoServiceProvider();
			byte[] retVal = md5Provider.ComputeHash(fs);
			long size = fs.Length;
			fs.Close();
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < retVal.Length; i++)
			{
				sb.Append(retVal[i].ToString("x2"));
			}
			md5 = sb.ToString();
			return size;
		}
	}

    public static string GetBytesMd5 (byte[] bytes)
    {
	    using MemoryStream ms = new MemoryStream(bytes);
	    MD5 md5Provider = new MD5CryptoServiceProvider();
	    byte[] retVal = md5Provider.ComputeHash(ms);
	    long size = ms.Length;
	    StringBuilder sb = new StringBuilder();
	    for (int i = 0; i < retVal.Length; i++)
	    {
		    sb.Append(retVal[i].ToString("x2"));
	    }
	    return sb.ToString();
    }

	public static bool CheckAndCreateFolder (string path)
	{
		var di = new DirectoryInfo(path);
		if (!di.Exists)
		{
			di.Create();
			return false;
		}
		return true;
	}

    public static bool DeleteFolder (string path, bool recursive = true)
    {
        var di = new DirectoryInfo (path);
        if (di.Exists)
        {
            if ((di.GetFiles ().Length != 0 || di.GetDirectories ().Length != 0) && !recursive)
                return false;
            di.Delete (true);
            return true;
        }
        return false;
    }

    public static bool DeleteFile (string fullFileName)
    {
        FileInfo fi = new FileInfo (fullFileName);
        {
            if (fi.Exists)
            {
                fi.Delete ();
                return true;
            }
        }
        return false;
    }
}
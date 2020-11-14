﻿using System;
using System.IO;

namespace buildSATools
{
	class Program
	{
		static void Main(string[] args)
		{
			string[] script = File.ReadAllLines("BuildScript.ini");
			Directory.CreateDirectory("build");
			for (int i = 0; i < script.Length; i++)
			{
				string[] srcdest = script[i].Split('=');
				Console.WriteLine("Source: {1}, Destination: {0}", srcdest[0], srcdest[1]);
				if (File.Exists(srcdest[1]))
				{
					File.Copy(srcdest[1], "build\\" + srcdest[0], true);
				}
				else if (Directory.Exists(srcdest[1]))
				{
					DirectoryCopy(srcdest[1], "build\\" + srcdest[0], true);
				}
				else
				{
					Console.WriteLine("{0} does not exist", srcdest[1]);
				}
			}
		}

		private static void DirectoryCopy(
	   string sourceDirName, string destDirName, bool copySubDirs)
		{
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			DirectoryInfo[] dirs = dir.GetDirectories();

			// If the source directory does not exist, throw an exception.
			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException(
					"Source directory does not exist or could not be found: "
					+ sourceDirName);
			}

			// If the destination directory does not exist, create it.
			if (!Directory.Exists(destDirName))
			{
				Directory.CreateDirectory(destDirName);
			}


			// Get the file contents of the directory to copy.
			System.IO.FileInfo[] files = dir.GetFiles();

			foreach (System.IO.FileInfo file in files)
			{
				// Create the path to the new copy of the file.
				string temppath = Path.Combine(destDirName, file.Name);

				// Copy the file.
				file.CopyTo(temppath, true);
			}

			// If copySubDirs is true, copy the subdirectories.
			if (copySubDirs)
			{

				foreach (DirectoryInfo subdir in dirs)
				{
					// Create the subdirectory.
					string temppath = Path.Combine(destDirName, subdir.Name);

					// Copy the subdirectories.
					DirectoryCopy(subdir.FullName, temppath, copySubDirs);
				}
			}
		}
	}
}
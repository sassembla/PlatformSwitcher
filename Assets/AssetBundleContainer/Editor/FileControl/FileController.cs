using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

class FileController {

	public static void CopyOutAllFiles (List<string> souceFilePaths, string destinationFolderPath) {
		foreach (var sourceFilePath in souceFilePaths) {
			Copy(sourceFilePath, destinationFolderPath);
		}
	}

	public static void CopyOutAllFolders (string sourceFolderPath, string destinationFolderPath) {
		var sourceFolders = Directory.GetDirectories(sourceFolderPath);

		foreach (var sourceFolder in sourceFolders) {
			var directoryName = new DirectoryInfo(sourceFolder).Name;
			Renew(Path.Combine(destinationFolderPath, directoryName));

			CopyAllInDirectoryFromTo(sourceFolder, Path.Combine(destinationFolderPath, directoryName));
		}
	}

	


	public static void CopyAllInDirectoryFromTo (string sourceFolderPath, string destinationFolderPath) {
		var filePaths = Directory.GetFiles(sourceFolderPath);
		
		foreach (var sourceFilePath in filePaths) {
			var fileName = Path.GetFileName(sourceFilePath);
			var destinationFilePath = Path.Combine(destinationFolderPath, fileName);
			
			Copy(sourceFilePath, destinationFilePath);
		}
	}

	public static void Copy (string sourceFilePath, string destinationFilePath) {
		if ((File.GetAttributes(sourceFilePath) & FileAttributes.Directory) == FileAttributes.Directory) {
			Debug.Log("Copy: should copy folderCopy for directory copy. sourceFilePath" + sourceFilePath);
			return;
		}
		
		// create directory if not exist.
		var destinationFileParentPath = Directory.GetParent(destinationFilePath).ToString();
		if (!Directory.Exists(destinationFileParentPath)) Directory.CreateDirectory(destinationFileParentPath);
		
		try {
			File.Copy(sourceFilePath, destinationFilePath, true);
		} catch (Exception e) {
			Debug.Log("Copy error:" + e);

		}
	}

	public static void Renew (string path) {
		Debug.Log("Renew!:" + path);
		if (Directory.Exists(path)) {
			Directory.Delete(path, true);
		}

		Directory.CreateDirectory(path);
	}

	public static void RenameWithNewExtension (string path, string newExtension) {
		if (path.EndsWith(newExtension)) return;
		if (path.EndsWith(newExtension + ".meta")) return;

		var newPath = path + newExtension;
		// avoid overwrite
		if (File.Exists(newPath)) return;
		File.Move(path, newPath);
	}

	public static bool RemoveExtension (string path, string targetExtension) {
		// Debug.Log("RemoveExtension path:" + path);
		var targetPath = path.Replace(targetExtension, string.Empty);
		if (path.EndsWith(targetExtension)) {
			if (File.Exists(path)) {
				if (File.Exists(targetPath)) {
					Debug.Log("overwrite:" + targetPath);
					File.Delete(targetPath);
				}
				File.Move(path, targetPath);
				return true;
			}
		}
		
		return false;
	}
}
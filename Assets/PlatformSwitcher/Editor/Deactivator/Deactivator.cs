using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;


class Deactivator {
	public static void DeactivateFilesUnderPath (string path, string platformKeyPhrase) {
		DeactivateFolderItemsRecursive(path, platformKeyPhrase);
	}

	/*
		collect deactivate targets from path.
	*/
	private static void DeactivateFolderItemsRecursive (string path, string platformKeyPhrase) {
		var innerDirectories = Directory.GetDirectories(path);

		foreach (var folderPath in innerDirectories) {
			DeactivateFolderItemsRecursive(folderPath, platformKeyPhrase);
		}
		// run for all items in the path recursively.
		DeactivateItemAndMetasInFolder(path, platformKeyPhrase);
	}

	/**
		ignore "." start items.
		+ ignore folder.meta items.
		+ ignore already deactivated == has "[.]deactivate.*" extension.
		deactivate "item" and "item.meta" files.
	*/
	private static List<string> DeactivateItemAndMetasInFolder (string path, string platformKeyPhrase) {
		var items = Directory.GetFiles(path)
			.Where(t => !Path.GetFileName(t).StartsWith("."))
			.Where(t => Path.GetFileName(t).EndsWith(".meta"))
			.ToList();

		foreach (var item in items) {
			Deactivator.Deactivate(item, platformKeyPhrase);
		}
		return items;
	}

	/**
		deactivates target file which is NOT
			・already deactivated. 			something.jpg.deactivatedPLATFORM
			・deactivated file's meta file.	something.jpg.deactivatedPLATFORM.meta
	*/
	private static void Deactivate (string path, string platformKeyPhrase) {
		if (Regex.Match(path, @"[.]deactivate.*[.]meta").Success) return;
		if (Regex.Match(path, @"[.]deactivate.*").Success) return;

		if (path.EndsWith(".meta")) {// metaファイルは改名する => 元の奴を消す
			FileController.RenameAsNewExtension(path, ".deactivate" + platformKeyPhrase);
		} else {// それ以外のファイルはコピーする

		}
		Debug.Log("Deactivated:" + path);
	}




	public static void ActivateFilesUnderPath (string path, string platformKeyPhrase) {
		var innerDirectories = Directory.GetDirectories(path);

		foreach (var folderPath in innerDirectories) {
			ActivateFolderItemsRecursive(folderPath, platformKeyPhrase);
		}

		// run for all items in the path.
		ActivateItemsInFolder(path, platformKeyPhrase);
	}

	private static void ActivateFolderItemsRecursive (string path, string platformKeyPhrase) {
		var innerDirectories = Directory.GetDirectories(path);

		foreach (var folderPath in innerDirectories) {
			ActivateFolderItemsRecursive(folderPath, platformKeyPhrase);
		}

		// run for all items in the path.
		ActivateItemsInFolder(path, platformKeyPhrase);
	}
	
	private static void ActivateItemsInFolder (string path, string platformKeyPhrase) {
		var items = Directory.GetFiles(path);
		foreach (var item in items) {
			Activate(item, platformKeyPhrase);
		}
	}

	public static bool Activate (string itemPath, string platformKeyPhrase) {
		return FileController.CloneWithRemoveExtension(itemPath, ".deactivate" + platformKeyPhrase);
	}


}
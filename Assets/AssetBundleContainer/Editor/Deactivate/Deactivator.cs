using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;


class Deactivator {
	private static Dictionary<string, Deactivateds> activateTransactionDict = new Dictionary<string, Deactivateds>();


	public static string DeactivateFilesUnderPath (string path) {
		var transactionId = Guid.NewGuid().ToString();

		activateTransactionDict[transactionId] = new Deactivateds(path);
		return transactionId;
	}

	
	/**
		force deactivate in transaction.
	*/
	public static void ForceDeactivate (string transactionId) {
		if (activateTransactionDict.ContainsKey(transactionId)) {
			var activatablePaths = activateTransactionDict[transactionId].deactivatedPaths
				.Where(path => File.Exists(path))
				.ToList();

			foreach (var path in activatablePaths) {
				Deactivate(path);
			}
		}
	}


	public static void Deactivate (string path) {
		FileController.RenameWithNewExtension(path, ".deactivate");
	}






	/*
		activate method series
	*/

	public static void RollbackTransaction (string transactionId) {
		if (activateTransactionDict.ContainsKey(transactionId)) {
			var entryPath = activateTransactionDict[transactionId].entryPointPath;
			var targetFolders = Directory.GetDirectories(entryPath);
			foreach (var path in targetFolders) {
				ActivateFolderItemsRecursive(path);
			}
		}
	}

	public static void ActivateFolderItemsRecursive (string path) {
		var innerDirectories = Directory.GetDirectories(path);

		foreach (var folderPath in innerDirectories) {
			ActivateFolderItemsRecursive(folderPath);
		}

		// run for all items in the path.
		ActivateItemsInFolder(path);
	}
	
	public static void ActivateItemsInFolder (string path) {
		var items = Directory.GetFiles(path);
		foreach (var item in items) {
			Activate(item);
		}
	}

	public static bool Activate (string itemPath) {
		return FileController.RemoveExtension(itemPath, ".deactivate");
	}



	/**
		collect paths under the basePath. exclude basePath itself. and only 1 depth.
		
		/a/basePath/c/d.something
		/a/basePath/c/e/f.something

			->
		
		ok: 
			return /a/basePath/c only.
		
		ng:
			return /a/basePath/c and /a/basePath/c/e

	*/
	public static List<string> DeactivatedDirectoriesUnderPath (string transactionId, string basePath) {
		var list = new List<string>();
		var separator = '/';

		// valid depth = basePath's length + 1 only.
		var baseDepth = basePath.Split(separator).Length;
		var targetDepth = baseDepth + 2; // .../basePath/a/something.some

		if (activateTransactionDict.ContainsKey(transactionId)) {
			var candidates = activateTransactionDict[transactionId].deactivatedPaths
				.Where(path => path.StartsWith(basePath))
				.Where(path => path.Split(separator).Length == targetDepth)// 深度を限る
				.ToList();

			foreach (var dir in candidates) {
				var targetPath = Path.GetDirectoryName(dir);
				
				// ignore basepath itself.
				if (targetPath == basePath) continue;

				if (!list.Contains(targetPath)) {
					list.Add(targetPath);
				} 
			}
		}
		return list;
	}

	
	/**
		activate files if exist.
		then return activated & not ".meta" file paths.
	*/
	public static List<string> ActivateFilesUnderPath (string transactionId, string folderPath) {
		if (!activateTransactionDict.ContainsKey(transactionId)) return new List<string>();
		var deactivatedsObj = activateTransactionDict[transactionId];
		return deactivatedsObj.Reactivate(folderPath);
	}
}


class Deactivateds {
	public readonly string entryPointPath;
	public readonly List<string> deactivatedPaths;

	public Deactivateds (string entryPointPath) {
		this.entryPointPath = entryPointPath;
		this.deactivatedPaths = new List<string>();
		

		var targetFolderPaths = Directory.GetDirectories(entryPointPath);
		foreach (var targetFolderPath in targetFolderPaths) {
			DeactivateFolderItemsRecursive(targetFolderPath, deactivatedPaths);
		}
	}

	/*
		collect deactivate targets from path.
	*/
	private void DeactivateFolderItemsRecursive (string path, List<string> list) {
		var innerDirectories = Directory.GetDirectories(path);

		foreach (var folderPath in innerDirectories) {
			DeactivateFolderItemsRecursive(folderPath, list);
		}
		// run for all items in the path recursively.
		var deactivatedFilePaths = DeactivateItemAndMetasInFolder(path);
		list.AddRange(deactivatedFilePaths);
	}

	/**
		ignore "." start items.
		also ignore folder.meta items.

		deactivate "item" and "item.meta" files.
	*/
	private List<string> DeactivateItemAndMetasInFolder (string path) {
		var items = Directory.GetFiles(path)
			.Where(t => !Path.GetFileName(t).StartsWith("."))
			.ToList();

		foreach (var item in items) {
			Deactivator.Deactivate(item);
		}
		return items;
	}


	public List<string> Reactivate (string folderPath) {
		var activatableCandidates = deactivatedPaths
			.Where(path => path.StartsWith(folderPath))
			.ToList();

		var activatedPaths = new List<string>();

		// activate candidates if exist.
		foreach (var candidate in activatableCandidates) {
			var deactivatedCandidatePath = candidate + ".deactivate";
			if (File.Exists(deactivatedCandidatePath)) {
				var result = Deactivator.Activate(deactivatedCandidatePath);
				if (result) {
					activatedPaths.Add(candidate);
				}
			} else {
				if (File.Exists(candidate)) {
					Debug.Log("no-changed candidate is exist:" + candidate);
					activatedPaths.Add(candidate);
				}
			}
		}

		return activatedPaths;
	}

}
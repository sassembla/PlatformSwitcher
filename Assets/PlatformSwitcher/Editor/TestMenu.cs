using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TestInterface {


	[MenuItem ("Window/changePlatform/OSXPlatform", false, 1)]
	public static void OSXPlatform () {
		var newBuildTarget = BuildTarget.StandaloneOSXIntel;
		ChangeOrNot(newBuildTarget);
	}

	[MenuItem ("Window/changePlatform/iOS", false, 1)]
	public static void iOS () {
		var newBuildTarget = BuildTarget.iOS;
		ChangeOrNot(newBuildTarget);
	}

	[MenuItem ("Window/changePlatform/Android", false, 1)]
	public static void Android () {
		var newBuildTarget = BuildTarget.Android;
		ChangeOrNot(newBuildTarget);
	}



	private static void ChangeOrNot (BuildTarget newBuildTarget) {
		var currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;

		if (newBuildTarget != currentBuildTarget) {
			var targetResourcePath = Path.Combine(Application.dataPath, "Resources");
			
			// 既存のplatform用の物を、最新で上書きする
			Deactivator.DeactivateFilesUnderPath(targetResourcePath, currentBuildTarget.ToString());

			var alreadyDeactivatedPlatforms = DetectAlreadyDeactivatedPlatforms(targetResourcePath);

			// すでにキャッシュされているものについては、ここで新Platformの物として展開する
			if (alreadyDeactivatedPlatforms.Contains(newBuildTarget.ToString())) {
				Deactivator.ActivateFilesUnderPath(targetResourcePath, newBuildTarget.ToString());
			}

			// change platform then deactivate.
			uint crc;
			var mainAssetObject = Resources.Load("dummyTextResource");
			if (mainAssetObject != null) {

				BuildPipeline.BuildAssetBundle(
					mainAssetObject,
					null,
					"Assets/PlatformSwitcher/Resources/dummyTextResource_output.dummy",
					out crc,
					BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets,
					newBuildTarget
				);

				Deactivator.DeactivateFilesUnderPath(targetResourcePath, newBuildTarget.ToString());
				Debug.Log("new platform:" + newBuildTarget);
			}
			return;
		}

		Debug.Log("no changed.");
	}

	private static List<string> DetectAlreadyDeactivatedPlatforms (string detectBasePath) {
		var deactivatedPlatformStrs = new List<string>();
		var existFiles = Directory.GetFiles(detectBasePath);
		foreach (var filePath in existFiles) {
			if (Regex.Match(filePath, @".*[.]deactivate.*[.]meta").Success) continue;// skip .meta ends file.
			
			var match = Regex.Match(filePath, @".*[.]deactivate(.*)");// .* = platform specific string.
			if (match.Success) {
				var platformStr = match.Groups[1].ToString();
				if (!deactivatedPlatformStrs.Contains(platformStr)) deactivatedPlatformStrs.Add(platformStr);
			}
		}
		return deactivatedPlatformStrs;
	}
}

[InitializeOnLoad]
public class PlatformChangedOrNotChanged {
	static PlatformChangedOrNotChanged () {
		Debug.Log("変わったはず、で、変わったあとの奴が含まれてなければ、その時点でcacheすべき、って感じかな。");

		// var currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;
		// var targetResourcePath = Path.Combine(Application.dataPath, "Resources");
		
		// // 既存のplatform用の物を、最新で上書きする(新規ファイルが含まれるはず)
		// Deactivator.DeactivateFilesUnderPath(targetResourcePath, currentBuildTarget.ToString());	
	}
}

public class A : UnityEditor.AssetPostprocessor {
	public void OnPreprocessTexture () {
		Debug.Log("OnPreprocessTexture:" + assetPath);
	}
}
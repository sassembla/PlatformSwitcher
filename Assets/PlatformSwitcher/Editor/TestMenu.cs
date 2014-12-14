using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TestInterface {

	#if UNITY_IPHONE
	[MenuItem ("Window/changePlatform/Now: iOS Platform", false, 1)]
	#elif UNITY_ANDROID
	[MenuItem ("Window/changePlatform/Now: Android Platform", false, 1)]
	#elif UNITY_STANDALONE_OSX
	[MenuItem ("Window/changePlatform/Now: OSX Platform", false, 1)]
	#endif
	public static void CurrentPlatform () {}


	[MenuItem ("Window/changePlatform/OSXPlatform", false, 1)]
	public static void OSXPlatform () {
		var newBuildTarget = BuildTarget.StandaloneOSXIntel;
		ChangeOrNot(newBuildTarget);
	}


	[MenuItem ("Window/changePlatform/iOS", false, 1)]
	public static void iOS () {
		var newBuildTarget = BuildTarget.iPhone;
		ChangeOrNot(newBuildTarget);
	}

	[MenuItem ("Window/changePlatform/Android", false, 1)]
	public static void Android () {
		var newBuildTarget = BuildTarget.Android;
		ChangeOrNot(newBuildTarget);
	}



	private static void ChangeOrNot (BuildTarget newBuildTarget) {
		var currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;
		Debug.Log("curr:" + currentBuildTarget);
		Debug.Log("next:" + newBuildTarget);

		if (newBuildTarget != currentBuildTarget) {
			var targetResourcePath = Path.Combine(Application.dataPath, "Resources");
			
			// 既存のplatform用の物を改名して退避する→これだと、Unityに記録してある感じで、ファイルに変化が無いので駄目。なくなったことにしても駄目、、
			// Deactivator.DeactivateFilesUnderPath(targetResourcePath, currentBuildTarget.ToString());

			// var current = Directory.GetFiles(targetResourcePath);
			// foreach (var a in current) {
			// 	Debug.Log("a:" + a);// 画像ファイルと改名されたファイル？
			// }

			// var alreadyDeactivatedPlatforms = DetectAlreadyDeactivatedPlatforms(targetResourcePath);

			// // 次のplatformがすでにキャッシュされているかどうかチェック、含まれていれば解凍する
			// // if (alreadyDeactivatedPlatforms.Contains(newBuildTarget.ToString())) {
			// // 	Deactivator.ActivateFilesUnderPath(targetResourcePath, newBuildTarget.ToString());
			// // }

			// // change platform then deactivate.
			// uint crc;
			// var mainAssetObject = Resources.Load("dummyTextResource");
			// if (mainAssetObject != null) {

			// 	BuildPipeline.BuildAssetBundle(
			// 		mainAssetObject,
			// 		null,
			// 		"Assets/PlatformSwitcher/Resources/dummyTextResource_output.dummy",
			// 		out crc,
			// 		BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets,
			// 		newBuildTarget
			// 	);

			// 	// Deactivator.DeactivateFilesUnderPath(targetResourcePath, newBuildTarget.ToString());
			// 	Debug.Log("new platform:" + newBuildTarget);
			// }


			// // なんでもいいからコンパイルが発生するようにする
			// // rewrite as file
			// var triggetPath = Path.Combine(Application.dataPath, "trigger.cs");
   //          var commentedDateDescription = "//"+DateTime.Now.ToString();
   //          using (StreamWriter sw = new StreamWriter(triggetPath)) {
   //              sw.WriteLine(commentedDateDescription);
   //          }


            // 持ってるカードは、
            /*
				・ビルドプラットフォームをコードから変更できる
				・ファイルを消す→戻す、ではidは変わらない？
				・リネームではidは変わった(ただし再度読み込みになる)
				・すでに持っているファイルをmetaごとコピーした場合、GUIDの書き換えだけですむっぽい？->読み直し
				・別GUIDを勝手に作るとどうなる？->読み直し
				・移動するとどうなる？


				・importを発生させない方法
			

	
            */
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
		Debug.LogError("OnPreprocessTexture:" + assetPath);
	}
}
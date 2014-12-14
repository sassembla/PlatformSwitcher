using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;


class AssetBundleContainer {
	[MenuItem ("Window/Bundlize", false, 1)]
	static void AssetBundlize () {
		
		// Bundle化したあとのモノの置き場を適当に作る
		var destinationPath = "Bundlized";
		FileController.Renew(destinationPath);
		

		/*
			deactivate
			対象が含まれる、すべての「importされると面倒なファイル」を、importされなそうな拡張子にリネームする。
		*/
		var deactivateTargetPath = Path.Combine(Application.dataPath, "Resources");
		var transactionId = Deactivator.DeactivateFilesUnderPath(deactivateTargetPath);
		
		var targetFolderName = "BundlizeTarget";
		var targetFolderPath = Path.Combine(deactivateTargetPath, targetFolderName);

		
		// 現在のプラットフォームを記録
		var beforePlatform = EditorUserBuildSettings.activeBuildTarget;

			
		// AssetBundleにする対象のファイルの拡張子だけを戻す(この場合フォルダ単位で戻している)
		Deactivator.ActivateFilesUnderPath(transactionId, targetFolderPath);

		
		// iOS用にAssetBundleを作成
		if (true) {
			var res = Resources.Load(Path.Combine(targetFolderName, "Yeaaahhhhh"));
			var targetPlatform = BuildTarget.iPhone;// Unity5だとiOSになったな〜
			uint crc;
			if (res != null) {
				BuildPipeline.BuildAssetBundle(
					res,
					null,
					Path.Combine(destinationPath, "Yeaaahhhhh_bundlized_ios"),
					out crc,
					BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets,
					targetPlatform
				);
			}
		}

		var afterPlatform1 = EditorUserBuildSettings.activeBuildTarget;
		Debug.Log("after1:" + afterPlatform1);


		// Android用にAssetBundleを作成
		if (true) {
			var res = Resources.Load(Path.Combine(targetFolderName, "Yeaaahhhhh"));
			var targetPlatform = BuildTarget.Android;// Unity5だとiOSになったな〜
			uint crc;
			if (res != null) {
				BuildPipeline.BuildAssetBundle(
					res,
					null,
					Path.Combine(destinationPath, "Yeaaahhhhh_bundlized_android"),
					out crc,
					BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets,
					targetPlatform
				);
			}
		}

		var afterPlatform2 = EditorUserBuildSettings.activeBuildTarget;
		Debug.Log("after2:" + afterPlatform2);

		// リセット
		if (true) {
			uint crc;
			var resetterRes = Resources.Load("dummyText");
			if (resetterRes != null) {

				BuildPipeline.BuildAssetBundle(
					resetterRes,
					null,
					Path.Combine(destinationPath, "resetter"),
					out crc,
					BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets,
					beforePlatform
				);
			}
		}

		var afterPlatform3 = EditorUserBuildSettings.activeBuildTarget;
		Debug.Log("after3:" + afterPlatform3);

		// 無効化していたのを元に戻す
		Deactivator.RollbackTransaction(transactionId);
	}
}

class Importer : UnityEditor.AssetPostprocessor {
	public void OnPreprocessTexture () {
		Debug.LogError("assetPath:" + assetPath);
	}
}

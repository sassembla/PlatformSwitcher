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

class Container {
	public void Something () {
		var desitnationPath = "";
		FileController.Renew(desitnationPath);
		
		/*
			deactivate
			rename everything under the folder for avoiding platform switch based convert.
		*/
		var deactivateTargetPath = Path.Combine(Application.dataPath, "Resources");
		var transactionId = Deactivator.DeactivateFilesUnderPath(deactivateTargetPath);
		

		var bundlizableFolderPaths = Deactivator.DeactivatedDirectoriesUnderPath(transactionId, sourcePath);

		// will be resetted after generate assetBundle in bundlizer runnrers.
		var beforePlatform = EditorUserBuildSettings.activeBuildTarget;

		var destinationPathBySetting = bundlizeOpts.projectBasePath;

		// ここでのフォルダ単位がassetbundleのもとになるすべて、という感じ。
		// 例えばここで、外部からvirtualな代物を読み込めれば変わる。
		// ただし、Resourcesフォルダの内部でないと駄目。
		foreach (var bundlizablePath in bundlizableFolderPaths) {
			
			// re-activate this folder's item.
			var activatedItemPaths = Deactivator.ActivateFilesUnderPath(transactionId, bundlizablePath);
			var bundlizeTargetPaths = LimitWithSourceFolderLimit(activatedItemPaths, sourcePath);

				
			// /SOMEWHERE/PROJECT/Assets/AssetRails/temp/Resources/prefabricate/sound/titanic in 5 seconds.mp3
			AssetBundlePathComponent assetPathComp = new AssetBundlePathComponent(bundlizeTargetPaths, activatedItemPaths);

			// generate recommended destination path for output prefab or output other resources.
			// replace somewhere/PROJECT/Assets -> Assets for Unity's AssetBundleBuild function's default target setting. it aims "Assets/...".
			var destinationBasePath = desitnationPath.Replace(destinationPathBySetting, "Assets");

			var currentDestinationBundledPath = destinationBasePath;
			
			if (!Directory.Exists(currentDestinationBundledPath)) {
				Directory.CreateDirectory(currentDestinationBundledPath);
			}

			// run all : ASSETRAILS_PROJECT_RESOURCE_BASEPATH classes as runner.
			foreach (Type currentType in bundlizerTypeRunners) {
				var runner = (AssetRails.BundlizerBase)Activator.CreateInstance(currentType);

				runner.Bundlize(assetPathComp.bundleName, assetPathComp.allResourcePaths, assetPathComp.allResourceFullPaths, currentDestinationBundledPath);
			}


			// delete output target folder if empty.
			if (Directory.GetFiles(currentDestinationBundledPath).Length == 0) {
				Directory.Delete(currentDestinationBundledPath);
			}

			// var afterPlatform1 = EditorUserBuildSettings.activeBuildTarget;

			// export assetbundle for reset platform.
			uint crc;
			var mainAssetObject = Resources.Load(AssetRailsSettings.PLATFORMUPDATER_DUMMY_RESOURCE_NAME);
			if (mainAssetObject != null) {

				BuildPipeline.BuildAssetBundle(
					mainAssetObject,
					null,
					AssetRailsSettings.PLATFORMUPDATER_DUMMY_RESOURCE_OUTPUT_PATH,
					out crc,
					BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets,
					beforePlatform
				);
			}
			// var afterPlatform2 = EditorUserBuildSettings.activeBuildTarget;

			// deactivate all active items which was activated before.
			Deactivator.ForceDeactivate(transactionId);
		}

		Deactivator.RollbackTransaction(transactionId);
	}

		/**
		path component of assetBundlize target.
	*/
	private class AssetBundlePathComponent {
		public readonly string bundleName;
		public readonly List<string> allResourcePaths;
		public readonly List<string> allResourceFullPaths;

		public AssetBundlePathComponent (List<string> allItemPaths, List<string> allItemFullPaths) {
			Debug.Log("allItemPaths:" + allItemPaths.Count);
			foreach (var a in allItemPaths) {
				Debug.Log("a:" + a);
			}

			// sound
			bundleName = Directory.GetParent(allItemPaths[0]).Name;
			// Debug.Log("bundleName:" + bundleName);

			allResourcePaths = new List<string>();

			foreach (var itemPath in allItemPaths) {
				// prefabricate/sound/titanic in 5 seconds.mp3
				var assetInResourcePathSource = Regex.Split(itemPath, AssetRailsSettings.ASSETRAILS_PROJECT_RESOURCE_BASEPATH)[1];
				
				/*
				 generate resource path for Resources.Load enable.
				 */

				// prefabricate/sound/titanic in 5 seconds
				var resourcePath = ResourceLoadablePath(itemPath);
				// Debug.Log("resourcePath:" + resourcePath);

				allResourcePaths.Add(resourcePath);
			}

			allResourceFullPaths = allItemFullPaths;
		}

		private string ResourceLoadablePath (string itemPath) {
			var assetInResourcePathSource = Regex.Split(itemPath, AssetRailsSettings.ASSETRAILS_PROJECT_RESOURCE_BASEPATH)[1];
			
			// titanic in 5 seconds
			var withoutExtFileName = Path.GetFileNameWithoutExtension(assetInResourcePathSource);
			return Path.Combine(Path.GetDirectoryName(assetInResourcePathSource), withoutExtFileName);
		}

		private bool NotMetaAndNotFolder (string path) {
			if (path.EndsWith(".meta")) return false;
			if (Directory.Exists(path)) return false;
			return true;
		}

	}

}


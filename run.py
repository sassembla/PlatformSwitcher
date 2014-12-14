# -*- coding: utf-8 -*-
import sys
import subprocess

if __name__=='__main__':
	projectPath = sys.argv[1]
	print("projectPath", projectPath)

	command = "/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -quit -projectPath " + projectPath + " -executeMethod AssetBundleContainer.AssetBundlize"
	p = subprocess.check_call(command, shell=True)

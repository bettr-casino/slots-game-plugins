MODULE_SUBDIRECTORY ?= 

PACKAGE_PATH := casino/bettr/core

UNITY_PROJECT_PATH:=Unity
UNITY_TEST_RESULTS_PATH:=TestResults
UNITY_VERSION:= "2022.3.16f1"
UNITY_APP:= "${UNITY_HOME}/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity"

RUNTIME_VERSION:="v0_1_0"

ASSET_BUNDLES_LOG_FILE_PATH:="${BETTR_CASINO_ASSET_BUNDLES_LOGS_HOME}/logfile.log"
UNIT_TESTS_LOG_FILE_PATH:="${BETTR_CASINO_UNIT_TESTS_LOGS_HOME}/logfile.log"
UNITY_PACKAGES_LOG_FILE_PATH:="${BETTR_CASINO_LOGS_HOME}/UnityPackages/logfile.log"

MODULE_SUBDIRECTORIES := MainLobby LobbyCard
MODULE_OUTPUT_DIRECTORY := ${BETTR_CASINO_BUILDS_HOME}/UnityPackages

CRAYONSCRIPT_DLL_HOME := ${CRAYONSCRIPT_DLL_HOME}

AWS_DEFAULT_PROFILE := ${AWS_DEFAULT_PROFILE}

S3_BUCKET := bettr-casino-tasks
S3_OBJECT_KEY := module-packages

.PHONY: all

all: package

build_assets_all:
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrCustomMenu.CleanupTestScenes
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrCustomMenu.BuildAssets -buildTarget Android
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrCustomMenu.BuildAssets -buildTarget WebGL 
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrCustomMenu.BuildAssets -buildTarget iOS

build_assets_ios:
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrCustomMenu.CleanupTestScenes
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrCustomMenu.BuildAssets -buildTarget iOS

build_assets_android:
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrCustomMenu.CleanupTestScenes
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrCustomMenu.BuildAssets -buildTarget Android

build_assets_webgl:
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrCustomMenu.CleanupTestScenes
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrCustomMenu.BuildAssets -buildTarget WebGL 

testcore:
	$(UNITY_APP) -batchmode -logFile $(UNIT_TESTS_LOG_FILE_PATH) -runTests -projectPath $(UNITY_PROJECT_PATH) -testResults $(UNITY_TEST_RESULTS_PATH)/testResultsCore.xml -testPlatform playmode -testFilter "casino.bettr.plugin.Core.Tests"

testmain:
	$(UNITY_APP) -batchmode -logFile $(UNIT_TESTS_LOG_FILE_PATH) -runTests -projectPath $(UNITY_PROJECT_PATH) -testResults $(UNITY_TEST_RESULTS_PATH)/testResultsMain.xml -testPlatform playmode -testFilter "casino.bettr.plugin.Main.Tests" -timeScale=10

test: testcore testmain printtestresults

printtestresults:
	perl -ne 'if (/<test-run .*?result="([^"]+)" .*?total="([^"]+)" .*?passed="([^"]+)" .*?failed="([^"]+)" .*?inconclusive="([^"]+)" .*?skipped="([^"]+)" .*?end-time="([^"]+)"/) { print "result=$$1 total=$$2 passed=$$3 failed=$$4 inconclusive=$$5 skipped=$$6 end-time=$$7\n" }' $(UNITY_PROJECT_PATH)/$(UNITY_TEST_RESULTS_PATH)/testResultsCore.xml
	perl -ne 'if (/<test-run .*?result="([^"]+)" .*?total="([^"]+)" .*?passed="([^"]+)" .*?failed="([^"]+)" .*?inconclusive="([^"]+)" .*?skipped="([^"]+)" .*?end-time="([^"]+)"/) { print "result=$$1 total=$$2 passed=$$3 failed=$$4 inconclusive=$$5 skipped=$$6 end-time=$$7\n" }' $(UNITY_PROJECT_PATH)/$(UNITY_TEST_RESULTS_PATH)/testResultsMain.xml

preparedll:
	@echo "Preparing DLL..."
	@mkdir -p Unity/Assets/Bettr/Plugins
	@cp $(CRAYONSCRIPT_DLL_HOME)/Debug/CrayonScript.dll Unity/Assets/Bettr/Plugins/


# packagemodule_all: $(MODULE_SUBDIRECTORIES)	

# packagemodule:
# 	$(UNITY_APP) -batchmode -logFile $(UNITY_PACKAGES_LOG_FILE_PATH) -nographics -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrCustomMenu.ExportPackage -outputDirectory ${MODULE_OUTPUT_DIRECTORY} -moduleName $(MODULE_SUBDIRECTORY)
# 	aws --profile $(AWS_DEFAULT_PROFILE) s3 cp $(MODULE_OUTPUT_DIRECTORY)/$(MODULE_SUBDIRECTORY)/ s3://$(S3_BUCKET)/$(S3_OBJECT_KEY)/$(MODULE_SUBDIRECTORY)/ --recursive --exclude "*.DS_Store"

# $(MODULE_SUBDIRECTORIES):
# 	$(UNITY_APP) -batchmode -logFile $(UNITY_PACKAGES_LOG_FILE_PATH) -nographics -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrCustomMenu.ExportPackage -outputDirectory ${MODULE_OUTPUT_DIRECTORY} -moduleName $@
# 	aws --profile $(AWS_DEFAULT_PROFILE) s3 cp $(MODULE_OUTPUT_DIRECTORY)/$@/ s3://$(S3_BUCKET)/$(S3_OBJECT_KEY)/$@/ --recursive --exclude "*.DS_Store"

package: preparedll build_assets_all test


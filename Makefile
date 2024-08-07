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
ASSET_DATA_LOG_FILE_PATH:="${BETTR_CASINO_LOGS_HOME}/UnityAssetData/logfile.log"

MODULE_SUBDIRECTORIES := MainLobby LobbyCard
MODULE_OUTPUT_DIRECTORY := ${BETTR_CASINO_BUILDS_HOME}/UnityPackages

CRAYONSCRIPT_DLL_HOME := ${CRAYONSCRIPT_DLL_HOME}

AWS_DEFAULT_PROFILE := ${AWS_DEFAULT_PROFILE}

MODELS_DIR = $(PWD)/../../bettr-infrastructure/bettr-infrastructure/tools/publish-data/published_models

S3_BUCKET := bettr-casino-assets
S3_OBJECT_KEY := tasks

.PHONY: all

all: package

build_assets_all:
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.CleanupTestScenes
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssets -buildTarget Android
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssets -buildTarget WebGL 
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssets -buildTarget iOS

build_assets_ios:
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.CleanupTestScenes
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssets -buildTarget iOS

build_assets_android:
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.CleanupTestScenes
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssets -buildTarget Android

build_assets_webgl:
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.CleanupTestScenes
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssets -buildTarget WebGL 

testmain:
	$(UNITY_APP) -batchmode -logFile $(UNIT_TESTS_LOG_FILE_PATH) -runTests -projectPath $(UNITY_PROJECT_PATH) -testResults $(UNITY_TEST_RESULTS_PATH)/testResultsMain.xml -testPlatform playmode -testFilter "casino.bettr.plugin.Main.Tests" -timeScale=10

test: testmain

printtestresults:
	perl -ne 'if (/<test-run .*?result="([^"]+)" .*?total="([^"]+)" .*?passed="([^"]+)" .*?failed="([^"]+)" .*?inconclusive="([^"]+)" .*?skipped="([^"]+)" .*?end-time="([^"]+)"/) { print "result=$$1 total=$$2 passed=$$3 failed=$$4 inconclusive=$$5 skipped=$$6 end-time=$$7\n" }' $(UNITY_PROJECT_PATH)/$(UNITY_TEST_RESULTS_PATH)/testResultsCore.xml
	perl -ne 'if (/<test-run .*?result="([^"]+)" .*?total="([^"]+)" .*?passed="([^"]+)" .*?failed="([^"]+)" .*?inconclusive="([^"]+)" .*?skipped="([^"]+)" .*?end-time="([^"]+)"/) { print "result=$$1 total=$$2 passed=$$3 failed=$$4 inconclusive=$$5 skipped=$$6 end-time=$$7\n" }' $(UNITY_PROJECT_PATH)/$(UNITY_TEST_RESULTS_PATH)/testResultsMain.xml

preparedll:
	@echo "Preparing DLL..."
	@mkdir -p Unity/Assets/Bettr/Plugins
	@cp $(CRAYONSCRIPT_DLL_HOME)/Debug/CrayonScript.dll Unity/Assets/Bettr/Plugins/


packagemodule_all: $(MODULE_SUBDIRECTORIES)	

packagemodule:
	$(UNITY_APP) -batchmode -logFile $(UNITY_PACKAGES_LOG_FILE_PATH) -nographics -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.ExportPackage -outputDirectory ${MODULE_OUTPUT_DIRECTORY} -moduleName $(MODULE_SUBDIRECTORY)
	aws --profile $(AWS_DEFAULT_PROFILE) s3 cp $(MODULE_OUTPUT_DIRECTORY)/$(MODULE_SUBDIRECTORY)/ s3://$(S3_BUCKET)/$(S3_OBJECT_KEY)/$(MODULE_SUBDIRECTORY)/ --recursive --exclude "*.DS_Store"

$(MODULE_SUBDIRECTORIES):
	$(UNITY_APP) -batchmode -logFile $(UNITY_PACKAGES_LOG_FILE_PATH) -nographics -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.ExportPackage -outputDirectory ${MODULE_OUTPUT_DIRECTORY} -moduleName $@
	aws --profile $(AWS_DEFAULT_PROFILE) s3 cp $(MODULE_OUTPUT_DIRECTORY)/$@/ s3://$(S3_BUCKET)/$(S3_OBJECT_KEY)/$@/ --recursive --exclude "*.DS_Store"

prepare: preparedll

package: preparedll build_assets_all test packagemodule_all printtestresults

sync-machines:
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game001 -machineVariant AncientAdventures -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game001 -machineVariant AtlantisTreasures -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game001 -machineVariant ClockworkChronicles -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game001 -machineVariant CosmicVoyage -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game001 -machineVariant DragonsHoard -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game001 -machineVariant EnchantedForest -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game001 -machineVariant GalacticQuest -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game001 -machineVariant GuardiansOfOlympus -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game001 -machineVariant LostCityOfGold -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game001 -machineVariant MysticalLegends -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game001 -machineVariant PharosFortune -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game001 -machineVariant PiratesPlunder -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game001 -machineVariant SamuraisFortune -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game002 -machineVariant BuffaloAdventureQuest -machineModel $(MODELS_DIR)/Game002/Game002Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game002 -machineVariant BuffaloFrontierFortune -machineModel $(MODELS_DIR)/Game002/Game002Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game002 -machineVariant BuffaloJackpotMadness -machineModel $(MODELS_DIR)/Game002/Game002Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game002 -machineVariant BuffaloMagicSpins -machineModel $(MODELS_DIR)/Game002/Game002Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game002 -machineVariant BuffaloSafariExpedition -machineModel $(MODELS_DIR)/Game002/Game002Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game002 -machineVariant BuffaloTreasureHunter -machineModel $(MODELS_DIR)/Game002/Game002Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game002 -machineVariant BuffaloWheelOfRiches -machineModel $(MODELS_DIR)/Game002/Game002Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game002 -machineVariant BuffaloWildPicks -machineModel $(MODELS_DIR)/Game002/Game002Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game002 -machineVariant BuffaloWildSpins -machineModel $(MODELS_DIR)/Game002/Game002Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game003 -machineVariant HighStakesAlpineAdventure -machineModel $(MODELS_DIR)/Game003/Game003Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game003 -machineVariant HighStakesCascadingCash -machineModel $(MODELS_DIR)/Game003/Game003Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game003 -machineVariant HighStakesHotLinks -machineModel $(MODELS_DIR)/Game003/Game003Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game003 -machineVariant HighStakesMegaMultipliers -machineModel $(MODELS_DIR)/Game003/Game003Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game003 -machineVariant HighStakesMonacoThrills -machineModel $(MODELS_DIR)/Game003/Game003Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game003 -machineVariant HighStakesStackedSpins -machineModel $(MODELS_DIR)/Game003/Game003Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game003 -machineVariant HighStakesWonderWays -machineModel $(MODELS_DIR)/Game003/Game003Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game004 -machineVariant RichesBeverlyHillsMansions -machineModel $(MODELS_DIR)/Game004/Game004Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game004 -machineVariant RichesBillionaireBets -machineModel $(MODELS_DIR)/Game004/Game004Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game004 -machineVariant RichesDiamondDash -machineModel $(MODELS_DIR)/Game004/Game004Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game004 -machineVariant RichesJetsetJackpot -machineModel $(MODELS_DIR)/Game004/Game004Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game004 -machineVariant RichesPharaohsRiches -machineModel $(MODELS_DIR)/Game004/Game004Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game004 -machineVariant RichesRaceToRiches -machineModel $(MODELS_DIR)/Game004/Game004Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game004 -machineVariant RichesRubyRush -machineModel $(MODELS_DIR)/Game004/Game004Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game004 -machineVariant RichesSapphireSprint -machineModel $(MODELS_DIR)/Game004/Game004Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game005 -machineVariant FortunesFourLeafClover -machineModel $(MODELS_DIR)/Game005/Game005Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game005 -machineVariant FortunesJadeOfFortune -machineModel $(MODELS_DIR)/Game005/Game005Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game005 -machineVariant FortunesKeysOfFortune -machineModel $(MODELS_DIR)/Game005/Game005Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game005 -machineVariant FortunesLuckyBamboo -machineModel $(MODELS_DIR)/Game005/Game005Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game005 -machineVariant FortunesLuckyCharms -machineModel $(MODELS_DIR)/Game005/Game005Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game005 -machineVariant FortunesManekiNeko -machineModel $(MODELS_DIR)/Game005/Game005Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game005 -machineVariant FortunesNorseAcorns -machineModel $(MODELS_DIR)/Game005/Game005Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game005 -machineVariant FortunesShootingStars -machineModel $(MODELS_DIR)/Game005/Game005Models.lua
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game006 -machineVariant WheelsCapitalCityTycoon -machineModel $(MODELS_DIR)/Game006/Game006Models.lua
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game006 -machineVariant WheelsEmpireBuilder -machineModel $(MODELS_DIR)/Game006/Game006Models.lua
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game006 -machineVariant WheelsFantasyKingdom -machineModel $(MODELS_DIR)/Game006/Game006Models.lua
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game006 -machineVariant WheelsGlobalInvestor -machineModel $(MODELS_DIR)/Game006/Game006Models.lua
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game006 -machineVariant WheelsIndustrialRevolution -machineModel $(MODELS_DIR)/Game006/Game006Models.lua
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game006 -machineVariant WheelsRealEstateMogul -machineModel $(MODELS_DIR)/Game006/Game006Models.lua
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game006 -machineVariant WheelsSpaceColonization -machineModel $(MODELS_DIR)/Game006/Game006Models.lua
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game006 -machineVariant WheelsTreasureIslandTycoon -machineModel $(MODELS_DIR)/Game006/Game006Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game007 -machineVariant TrueVegasInfiniteSpins -machineModel $(MODELS_DIR)/Game007/Game007Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game007 -machineVariant TrueVegasLucky7s -machineModel $(MODELS_DIR)/Game007/Game007Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game007 -machineVariant TrueVegasMegaJackpot -machineModel $(MODELS_DIR)/Game007/Game007Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game007 -machineVariant TrueVegasMegaWheels -machineModel $(MODELS_DIR)/Game007/Game007Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game007 -machineVariant TrueVegasSuper7s -machineModel $(MODELS_DIR)/Game007/Game007Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game007 -machineVariant TrueVegasTripleSpins -machineModel $(MODELS_DIR)/Game007/Game007Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game007 -machineVariant TrueVegasWheelBonanza -machineModel $(MODELS_DIR)/Game007/Game007Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game007 -machineVariant TrueVegasWildCherries -machineModel $(MODELS_DIR)/Game007/Game007Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game008 -machineVariant GodsCelestialGuardians -machineModel $(MODELS_DIR)/Game008/Game008Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game008 -machineVariant GodsDivineRiches -machineModel $(MODELS_DIR)/Game008/Game008Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game008 -machineVariant GodsEternalDivinity -machineModel $(MODELS_DIR)/Game008/Game008Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game008 -machineVariant GodsHeavenlyMonarchs -machineModel $(MODELS_DIR)/Game008/Game008Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game008 -machineVariant GodsMysticPantheon -machineModel $(MODELS_DIR)/Game008/Game008Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game008 -machineVariant GodsMythicDeities -machineModel $(MODELS_DIR)/Game008/Game008Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game008 -machineVariant GodsSacredLegends -machineModel $(MODELS_DIR)/Game008/Game008Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game008 -machineVariant GodsTitansOfWealth -machineModel $(MODELS_DIR)/Game008/Game008Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game009 -machineVariant SpaceInvadersApolloAdventurers -machineModel $(MODELS_DIR)/Game009/Game009Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game009 -machineVariant SpaceInvadersCosmicRaiders -machineModel $(MODELS_DIR)/Game009/Game009Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game009 -machineVariant SpaceInvadersGalacticPioneers -machineModel $(MODELS_DIR)/Game009/Game009Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game009 -machineVariant SpaceInvadersInterstellarTreasureHunters -machineModel $(MODELS_DIR)/Game009/Game009Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game009 -machineVariant SpaceInvadersNebulaNavigators -machineModel $(MODELS_DIR)/Game009/Game009Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game009 -machineVariant SpaceInvadersRaidersOfPlanetMooney -machineModel $(MODELS_DIR)/Game009/Game009Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game009 -machineVariant SpaceInvadersStellarExpedition -machineModel $(MODELS_DIR)/Game009/Game009Models.lua
	# ${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine -machineName Game009 -machineVariant SpaceInvadersVoyagersOfTheCosmos -machineModel $(MODELS_DIR)/Game009/Game009Models.lua



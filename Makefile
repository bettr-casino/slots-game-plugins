# =============================================================================
#
# Makefile for Unity Project Build and Deployment
#
# =============================================================================



# =============================================================================
#
# VARIABLES
#
# =============================================================================

MODULE_SUBDIRECTORY ?= 

PACKAGE_PATH := casino/bettr/core

UNITY_PROJECT_PATH := Unity
UNITY_TEST_RESULTS_PATH := TestResults
UNITY_VERSION := "2022.3.16f1"
UNITY_APP := "${UNITY_HOME}/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity"
UNITY_PROJECT_SETTINGS := $(UNITY_PROJECT_PATH)/ProjectSettings/ProjectSettings.asset

RUNTIME_VERSION := "v0_1_0"

ASSET_BUNDLES_LOG_FILE_PATH := "${BETTR_CASINO_ASSET_BUNDLES_LOGS_HOME}/logfile.log"
UNIT_TESTS_LOG_FILE_PATH := "${BETTR_CASINO_UNIT_TESTS_LOGS_HOME}/logfile.log"
UNITY_PACKAGES_LOG_FILE_PATH := "${BETTR_CASINO_LOGS_HOME}/UnityPackages/logfile.log"
ASSET_DATA_LOG_FILE_PATH := "${BETTR_CASINO_LOGS_HOME}/UnityAssetData/logfile.log"

MODULE_SUBDIRECTORIES := MainLobby LobbyCard
MODULE_OUTPUT_DIRECTORY := ${BETTR_CASINO_BUILDS_HOME}/UnityPackages

CRAYONSCRIPT_DLL_HOME := ${CRAYONSCRIPT_DLL_HOME}

AWS_DEFAULT_PROFILE := ${AWS_DEFAULT_PROFILE}

MODELS_DIR := $(PWD)/../../bettr-infrastructure/bettr-infrastructure/tools/publish-data/published_models

S3_BUCKET := bettr-casino-assets
S3_OBJECT_KEY := tasks

BETTR_CASINO_HOME := ${BETTR_CASINO_HOME}

BUILDS_HOME := ${BETTR_CASINO_BUILDS_HOME}/UnityBuilds
LOGS_HOME := ${BETTR_CASINO_LOGS_HOME}/UnityBuilds

SYNC_MACHINE_METHOD := "Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine"

BUILD_IOS := ${BUILDS_HOME}/iOS
BUILD_IOS_ARCHIVE := ${BETTR_CASINO_BUILDS_HOME}/iOSArchives
LOGS_IOS := ${LOGS_HOME}/iOS
BUILD_METHOD_IOS := "Bettr.Editor.CommandLine.BuildIOS"

BUILD_WEBGL := ${BUILDS_HOME}/WebGL
LOGS_WEBGL := ${LOGS_HOME}/WebGL
BUILD_METHOD_WEBGL := "Bettr.Editor.CommandLine.BuildWebGL"

S3_WEBGL_BUCKET := "bettr.casino"
S3_WEBGL_OBJECT_KEY := ""

APPLE_ID := ${APPLE_ID}
APPLE_PASSWORD := ${APPLE_PASSWORD}
APPLE_API_KEY_ID := ${APPLE_API_KEY_ID}
APPLE_API_ISSUER_ID := ${APPLE_API_ISSUER_ID}
APPLE_BUILD_SCHEME := Unity-iPhone
APPLE_BUILD_CONFIGURATION := Release
APPLE_BUILD_DESTINATION := 'generic/platform=iOS'
APPLE_BUILD_INFO_PLIST := $(BUILD_IOS)/BettrSlots/Info.plist
APPLE_ARCHIVE_PATH := $(BUILD_IOS_ARCHIVE)
APPLE_EXPORT_OPTIONS_PLIST := $(APPLE_ARCHIVE_PATH)/TestFlightExportOptions.plist

SLEEP_DURATION := 10



# =============================================================================
#
# TARGETS
#
# =============================================================================

.PHONY: all pull bump unity_patch_ios_build_version prepare preparedll clean_ios build_ios archive_ios build_webgl publish_webgl sync-machines


# Default target
all: prepare-project



# =============================================================================
#
# PREPARE
#
# =============================================================================

preparedll:
	@echo "Preparing DLL..."
	@mkdir -p Unity/Assets/Bettr/Plugins
	@cp $(CRAYONSCRIPT_DLL_HOME)/Debug/CrayonScript.dll Unity/Assets/Bettr/Plugins/

prepare-project: preparedll



# =============================================================================
#
# BUILD ASSETS
#
# =============================================================================

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

build-assets: prepare-project build_assets_all


# =============================================================================
#
# BUILD MACHINES
#
# =============================================================================

sync-machines:
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game001 -machineVariant AncientAdventures -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game001 -machineVariant AtlantisTreasures -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game001 -machineVariant ClockworkChronicles -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game001 -machineVariant CosmicVoyage -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game001 -machineVariant DragonsHoard -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game001 -machineVariant EnchantedForest -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game001 -machineVariant GalacticQuest -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game001 -machineVariant GuardiansOfOlympus -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game001 -machineVariant LostCityOfGold -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game001 -machineVariant MysticalLegends -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game001 -machineVariant PharosFortune -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game001 -machineVariant PiratesPlunder -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game001 -machineVariant SamuraisFortune -machineModel $(MODELS_DIR)/Game001/Game001Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game002 -machineVariant BuffaloAdventureQuest -machineModel $(MODELS_DIR)/Game002/Game002Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game002 -machineVariant BuffaloFrontierFortune -machineModel $(MODELS_DIR)/Game002/Game002Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game002 -machineVariant BuffaloJackpotMadness -machineModel $(MODELS_DIR)/Game002/Game002Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game002 -machineVariant BuffaloMagicSpins -machineModel $(MODELS_DIR)/Game002/Game002Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game002 -machineVariant BuffaloSafariExpedition -machineModel $(MODELS_DIR)/Game002/Game002Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game002 -machineVariant BuffaloTreasureHunter -machineModel $(MODELS_DIR)/Game002/Game002Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game002 -machineVariant BuffaloWheelOfRiches -machineModel $(MODELS_DIR)/Game002/Game002Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game002 -machineVariant BuffaloWildPicks -machineModel $(MODELS_DIR)/Game002/Game002Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game002 -machineVariant BuffaloWildSpins -machineModel $(MODELS_DIR)/Game002/Game002Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game003 -machineVariant HighStakesAlpineAdventure -machineModel $(MODELS_DIR)/Game003/Game003Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game003 -machineVariant HighStakesCascadingCash -machineModel $(MODELS_DIR)/Game003/Game003Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game003 -machineVariant HighStakesHotLinks -machineModel $(MODELS_DIR)/Game003/Game003Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game003 -machineVariant HighStakesMegaMultipliers -machineModel $(MODELS_DIR)/Game003/Game003Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game003 -machineVariant HighStakesMonacoThrills -machineModel $(MODELS_DIR)/Game003/Game003Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game003 -machineVariant HighStakesStackedSpins -machineModel $(MODELS_DIR)/Game003/Game003Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game003 -machineVariant HighStakesWonderWays -machineModel $(MODELS_DIR)/Game003/Game003Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game004 -machineVariant RichesBeverlyHillsMansions -machineModel $(MODELS_DIR)/Game004/Game004Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game004 -machineVariant RichesBillionaireBets -machineModel $(MODELS_DIR)/Game004/Game004Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game004 -machineVariant RichesDiamondDash -machineModel $(MODELS_DIR)/Game004/Game004Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game004 -machineVariant RichesJetsetJackpot -machineModel $(MODELS_DIR)/Game004/Game004Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game004 -machineVariant RichesPharaohsRiches -machineModel $(MODELS_DIR)/Game004/Game004Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game004 -machineVariant RichesRaceToRiches -machineModel $(MODELS_DIR)/Game004/Game004Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game004 -machineVariant RichesRubyRush -machineModel $(MODELS_DIR)/Game004/Game004Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game004 -machineVariant RichesSapphireSprint -machineModel $(MODELS_DIR)/Game004/Game004Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game005 -machineVariant FortunesFourLeafClover -machineModel $(MODELS_DIR)/Game005/Game005Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game005 -machineVariant FortunesJadeOfFortune -machineModel $(MODELS_DIR)/Game005/Game005Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game005 -machineVariant FortunesKeysOfFortune -machineModel $(MODELS_DIR)/Game005/Game005Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game005 -machineVariant FortunesLuckyBamboo -machineModel $(MODELS_DIR)/Game005/Game005Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game005 -machineVariant FortunesLuckyCharms -machineModel $(MODELS_DIR)/Game005/Game005Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game005 -machineVariant FortunesManekiNeko -machineModel $(MODELS_DIR)/Game005/Game005Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game005 -machineVariant FortunesNorseAcorns -machineModel $(MODELS_DIR)/Game005/Game005Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game005 -machineVariant FortunesShootingStars -machineModel $(MODELS_DIR)/Game005/Game005Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game006 -machineVariant WheelsCapitalCityTycoon -machineModel $(MODELS_DIR)/Game006/Game006Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game006 -machineVariant WheelsEmpireBuilder -machineModel $(MODELS_DIR)/Game006/Game006Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game006 -machineVariant WheelsFantasyKingdom -machineModel $(MODELS_DIR)/Game006/Game006Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game006 -machineVariant WheelsGlobalInvestor -machineModel $(MODELS_DIR)/Game006/Game006Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game006 -machineVariant WheelsIndustrialRevolution -machineModel $(MODELS_DIR)/Game006/Game006Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game006 -machineVariant WheelsRealEstateMogul -machineModel $(MODELS_DIR)/Game006/Game006Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game006 -machineVariant WheelsSpaceColonization -machineModel $(MODELS_DIR)/Game006/Game006Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game006 -machineVariant WheelsTreasureIslandTycoon -machineModel $(MODELS_DIR)/Game006/Game006Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game007 -machineVariant TrueVegasInfiniteSpins -machineModel $(MODELS_DIR)/Game007/Game007Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game007 -machineVariant TrueVegasLucky7s -machineModel $(MODELS_DIR)/Game007/Game007Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game007 -machineVariant TrueVegasMegaJackpot -machineModel $(MODELS_DIR)/Game007/Game007Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game007 -machineVariant TrueVegasMegaWheels -machineModel $(MODELS_DIR)/Game007/Game007Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game007 -machineVariant TrueVegasSuper7s -machineModel $(MODELS_DIR)/Game007/Game007Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game007 -machineVariant TrueVegasTripleSpins -machineModel $(MODELS_DIR)/Game007/Game007Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game007 -machineVariant TrueVegasWheelBonanza -machineModel $(MODELS_DIR)/Game007/Game007Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game007 -machineVariant TrueVegasWildCherries -machineModel $(MODELS_DIR)/Game007/Game007Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game008 -machineVariant GodsCelestialGuardians -machineModel $(MODELS_DIR)/Game008/Game008Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game008 -machineVariant GodsDivineRiches -machineModel $(MODELS_DIR)/Game008/Game008Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game008 -machineVariant GodsEternalDivinity -machineModel $(MODELS_DIR)/Game008/Game008Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game008 -machineVariant GodsHeavenlyMonarchs -machineModel $(MODELS_DIR)/Game008/Game008Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game008 -machineVariant GodsMysticPantheon -machineModel $(MODELS_DIR)/Game008/Game008Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game008 -machineVariant GodsMythicDeities -machineModel $(MODELS_DIR)/Game008/Game008Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game008 -machineVariant GodsSacredLegends -machineModel $(MODELS_DIR)/Game008/Game008Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game008 -machineVariant GodsTitansOfWealth -machineModel $(MODELS_DIR)/Game008/Game008Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game009 -machineVariant SpaceInvadersApolloAdventurers -machineModel $(MODELS_DIR)/Game009/Game009Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game009 -machineVariant SpaceInvadersCosmicRaiders -machineModel $(MODELS_DIR)/Game009/Game009Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game009 -machineVariant SpaceInvadersGalacticPioneers -machineModel $(MODELS_DIR)/Game009/Game009Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game009 -machineVariant SpaceInvadersInterstellarTreasureHunters -machineModel $(MODELS_DIR)/Game009/Game009Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game009 -machineVariant SpaceInvadersNebulaNavigators -machineModel $(MODELS_DIR)/Game009/Game009Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game009 -machineVariant SpaceInvadersRaidersOfPlanetMooney -machineModel $(MODELS_DIR)/Game009/Game009Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game009 -machineVariant SpaceInvadersStellarExpedition -machineModel $(MODELS_DIR)/Game009/Game009Models.lua
	sleep $(SLEEP_DURATION)
	${UNITY_APP} -batchmode -logFile $(ASSET_DATA_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(SYNC_MACHINE_METHOD) -machineName Game009 -machineVariant SpaceInvadersVoyagersOfTheCosmos -machineModel $(MODELS_DIR)/Game009/Game009Models.lua


# =============================================================================
#
# BUILD & DEPLOY UNITY IOS
#
# =============================================================================

build_ios:
	@echo "Building iOS project..."
	@$(UNITY_APP) -quit -batchmode -logFile $(LOGS_IOS)/logfile.log -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(BUILD_METHOD_IOS) -buildOutput $(BUILD_IOS) -buildTarget iOS -development -scriptDebugging
	@echo "Build completed."

deploy_ios:
	@echo "Creating IPA file and deploying to TestFlight..."
	echo "(API_PRIVATE_KEYS_DIR is set in the environment to the path to the auth key file)"
	CFBundleShortVersionString=$$(/usr/libexec/PlistBuddy -c "Print :CFBundleShortVersionString" $(APPLE_BUILD_INFO_PLIST)); \
	echo "CFBundleShortVersionString: $$CFBundleShortVersionString"; \
	xcodebuild -exportArchive -archivePath $(APPLE_ARCHIVE_PATH)/BettrSlots-$$CFBundleShortVersionString.xcarchive -exportOptionsPlist $(APPLE_EXPORT_OPTIONS_PLIST) -allowProvisioningUpdates

clean_ios:
	@echo "Cleaning previous build artifacts..."
	@rm -rf $(BUILD_IOS)/BettrSlots

archive_ios:
	@echo "Archiving Xcode project..."
	CFBundleShortVersionString=$$(/usr/libexec/PlistBuddy -c "Print :CFBundleShortVersionString" $(APPLE_BUILD_INFO_PLIST)); \
	echo "CFBundleShortVersionString: $$CFBundleShortVersionString"; \
	xcodebuild -project $(BUILD_IOS)/BettrSlots/Unity-iPhone.xcodeproj -scheme $(APPLE_BUILD_SCHEME) -configuration $(APPLE_BUILD_CONFIGURATION) -destination $(APPLE_BUILD_DESTINATION) -archivePath $(APPLE_ARCHIVE_PATH)/BettrSlots-$$CFBundleShortVersionString.xcarchive archive	

bump: unity_patch_ios_build_version

unity_patch_bundle_version:
	@echo "Incrementing Unity bundleVersion..."
	$(eval CURRENT_BUNDLE_VERSION := $(shell grep 'bundleVersion' $(UNITY_PROJECT_SETTINGS) | sed -E 's/.*bundleVersion: (.*)/\1/'))
	$(eval NEW_BUNDLE_VERSION := $(shell echo $(CURRENT_BUNDLE_VERSION) | awk -F'.' '{$$NF+=1;OFS="."}1' OFS='.'))
	@echo "Current bundleVersion: $(CURRENT_BUNDLE_VERSION)"
	@echo "New bundleVersion: $(NEW_BUNDLE_VERSION)"
	@sed -i '' 's/bundleVersion: $(CURRENT_BUNDLE_VERSION)/bundleVersion: $(NEW_BUNDLE_VERSION)/' $(UNITY_PROJECT_SETTINGS)

unity_patch_ios_build_version:
	@echo "Incrementing Unity iPhone buildNumber..."
	# Extract the current build number for iPhone using grep with context lines
	$(eval CURRENT_BUILD_NUMBER := $(shell grep -A 5 'buildNumber:' $(UNITY_PROJECT_SETTINGS) | grep 'iPhone:' | awk '{print $$2}'))
	# Increment the build number
	$(eval NEW_BUILD_NUMBER := $(shell echo $$(( $(CURRENT_BUILD_NUMBER) + 1 ))))
	@echo "Current iPhone buildNumber: $(CURRENT_BUILD_NUMBER)"
	@echo "New iPhone buildNumber: $(NEW_BUILD_NUMBER)"
	# Update the build number in the ProjectSettings.asset file
	@sed -i '' '/iPhone:/s/$(CURRENT_BUILD_NUMBER)/$(NEW_BUILD_NUMBER)/' $(UNITY_PROJECT_SETTINGS)


# =============================================================================
#
# BUILD & DEPLOY UNITY WEBGL
#
# =============================================================================

build_webgl:
	@echo "Building WebGL project..."
	@$(UNITY_APP) -quit -batchmode -logFile $(LOGS_WEBGL)/logfile.log -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(BUILD_METHOD_WEBGL) -buildOutput $(BUILD_WEBGL) -buildTarget WebGL -development -scriptDebugging
	@echo "Build completed."

publish_webgl:
	@echo "Publishing WebGL project to S3..."
	# Sync all files except .gz files first
	@aws s3 sync $(BUILD_WEBGL)/BettrSlots s3://$(S3_WEBGL_BUCKET)/$(S3_WEBGL_OBJECT_KEY) --profile $(AWS_DEFAULT_PROFILE) --exclude "*.gz"
	# Now sync only .gz files with the Content-Encoding header set to gzip
	@aws s3 sync $(BUILD_WEBGL)/BettrSlots s3://$(S3_WEBGL_BUCKET)/$(S3_WEBGL_OBJECT_KEY) --profile $(AWS_DEFAULT_PROFILE) --include "*.gz" --content-encoding "gzip"
	@echo "Publish completed."


# =============================================================================
#
# HELPERS
#
# =============================================================================

pull:
	git pull origin main




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

GENERATED_LOBBY_IMAGES_DIR := $(PWD)/../../bettr-infrastructure/bettr-infrastructure/tools/pipelines/game-gpt/pipelines/gpt-generated-files/lobby-images
# the subpath is control/Runtime/Asset/Game001/LobbyCard
LOBBY_CARD_IMAGES_DIR := $(PWD)/Unity/Assets/Bettr/Runtime/Plugin/LobbyCard/variants/v0_1_0/

# using splash images for both the background, the lobby card image is the resized splash image 
GENERATED_BACKGROUND_IMAGES_DIR := $(PWD)/../../bettr-infrastructure/bettr-infrastructure/tools/pipelines/game-gpt/pipelines/gpt-generated-files/splash-images

S3_BUCKET := bettr-casino-assets
S3_ASSETS_LATEST_OBJECT_KEY := "assets/latest"

S3_OBJECT_KEY := tasks

ASSET_BUNDLES_BASE_DIRECTORY="$(PWD)/Unity/Assets/Bettr/LocalStore/AssetBundles"

BETTR_CASINO_HOME := ${BETTR_CASINO_HOME}

BUILDS_HOME := ${BETTR_CASINO_BUILDS_HOME}/UnityBuilds
LOGS_HOME := ${BETTR_CASINO_LOGS_HOME}/UnityBuilds

SYNC_MACHINE_METHOD := "Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine"
SYNC_BACKGROUND_TEXTURES_METHOD := "Bettr.Editor.BettrMenu.SyncBackgroundTexturesFromCommandLine"

BUILD_IOS := ${BUILDS_HOME}/iOS
BUILD_IOS_ARCHIVE := ${BETTR_CASINO_BUILDS_HOME}/iOSArchives
LOGS_IOS := ${LOGS_HOME}/iOS
BUILD_METHOD_IOS := "Bettr.Editor.CommandLine.BuildIOS"

BUILD_WEBGL := ${BUILDS_HOME}/WebGL
LOGS_WEBGL := ${LOGS_HOME}/WebGL
BUILD_METHOD_WEBGL := "Bettr.Editor.CommandLine.BuildWebGL"

S3_WEBGL_BUCKET := "bettr.casino"
S3_WEBGL_OBJECT_KEY := ""
S3_WEBGL_CLOUDFRONT_DISTRIBUTION_ID := "E1SZKTT445YO6X"

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
# DEPLOY ASSETS
#
# =============================================================================

clean-assets-all: clean-assets-ios clean-assets-android clean-assets-webgl

clean-assets-ios:
	@echo "Cleaning iOS asset bundles..."
	@rm -rf $(ASSET_BUNDLES_BASE_DIRECTORY)/iOS

clean-assets-android:
	@echo "Cleaning Android asset bundles..."
	@rm -rf $(ASSET_BUNDLES_BASE_DIRECTORY)/Android

clean-assets-webgl:
	@echo "Cleaning WebGL asset bundles..."
	@rm -rf $(ASSET_BUNDLES_BASE_DIRECTORY)/WebGL

build-assets-all: prepare-project clean-assets-all build-assets-ios build-assets-android build-assets-webgl

build-assets-ios: prepare-project clean-assets-ios
	@echo "Building iOS asset bundles..."
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.CleanupTestScenes
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssets -buildTarget iOS

build-assets-android: prepare-project clean-assets-android
	@echo "Building Android asset bundles..."
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.CleanupTestScenes
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssets -buildTarget Android

build-assets-webgl: prepare-project clean-assets-webgl
	@echo "Building WebGL asset bundles..."
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.CleanupTestScenes
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssets -buildTarget WebGL 

publish-assets-all: publish-assets-ios publish-assets-android publish-assets-webgl

publish-assets-ios:
	@echo "Publishing iOS asset bundles..."
	aws s3 sync $(ASSET_BUNDLES_BASE_DIRECTORY)/iOS s3://$(S3_BUCKET)/$(S3_ASSETS_LATEST_OBJECT_KEY)/iOS --delete --profile $(AWS_DEFAULT_PROFILE)

publish-assets-android:
	@echo "Publishing Android asset bundles..."
	aws s3 sync $(ASSET_BUNDLES_BASE_DIRECTORY)/Android s3://$(S3_BUCKET)/$(S3_ASSETS_LATEST_OBJECT_KEY)/Android --delete --profile $(AWS_DEFAULT_PROFILE)

publish-assets-webgl:
	@echo "Publishing WebGL asset bundles..."
	aws s3 sync $(ASSET_BUNDLES_BASE_DIRECTORY)/WebGL s3://$(S3_BUCKET)/$(S3_ASSETS_LATEST_OBJECT_KEY)/WebGL --delete --profile $(AWS_DEFAULT_PROFILE)

deploy-assets-all: publish-assets-all
	@echo "Deploying asset bundles..."

deploy-assets-ios: publish-assets-ios
	@echo "Deploying iOS asset bundles..."

deploy-assets-android: publish-assets-android
	@echo "Deploying Android asset bundles..."

deploy-assets-webgl: publish-assets-webgl
	@echo "Deploying WebGL asset bundles..."


#
# Build specific asset bundles
#

build-lobby-assets-webgl: prepare-project
	@echo "Building WebGL lobby asset bundles..."
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssetLabels -assetLabel "lobbycardv0_1_0" -assetSubLabel "control" -buildTarget WebGL; \
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssetLabels -assetLabel "mainlobbyv0_1_0" -assetSubLabel "control" -buildTarget WebGL; \
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssetLabels -assetLabel "mainlobbyv0_1_0_scenes" -assetSubLabel "control" -buildTarget WebGL

build-main-assets-webgl: prepare-project
	@echo "Building WebGL main asset bundles..."
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssetLabels -assetLabel "mainv0_1_0" -assetSubLabel "control" -buildTarget WebGL; \
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssetLabels -assetLabel "mainv0_1_0_scenes" -assetSubLabel "control" -buildTarget WebGL

build-game001-assets-webgl: prepare-project
	@echo "Building WebGL game001 variants asset bundles..."
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssetLabels -assetLabel "game001epicancientadventures" -assetSubLabel "control" -buildTarget WebGL; \
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssetLabels -assetLabel "game001epicancientadventures_scenes" -assetSubLabel "control" -buildTarget WebGL; \
	# ${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssetLabels -assetLabel "game001epicancientadventures" -assetSubLabel "variant1" -buildTarget WebGL; \
	# ${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssetLabels -assetLabel "game001epicancientadventures_scenes" -assetSubLabel "variant1" -buildTarget WebGL;


build-game006-assets-webgl: prepare-project
	@echo "Building WebGL game001 variants asset bundles..."
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssetLabels -assetLabel "game006wheelsempirebuilder" -assetSubLabel "control" -buildTarget WebGL; \
	${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssetLabels -assetLabel "game006wheelsempirebuilder_scenes" -assetSubLabel "control" -buildTarget WebGL; \
	# ${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssetLabels -assetLabel "game006wheelsempirebuilder" -assetSubLabel "variant1" -buildTarget WebGL; \
	# ${UNITY_APP} -batchmode -logFile $(ASSET_BUNDLES_LOG_FILE_PATH) -quit -projectPath $(UNITY_PROJECT_PATH) -executeMethod Bettr.Editor.BettrMenu.BuildAssetLabels -assetLabel "game006wheelsempirebuilder_scenes" -assetSubLabel "variant1" -buildTarget WebGL;


# =============================================================================
#
# BUILD IMAGES
#
# =============================================================================

sync-lobby-images: prepare-project
	@echo "Running sync-lobby-images..."
	GENERATED_LOBBY_IMAGES_DIR="${GENERATED_LOBBY_IMAGES_DIR}"; \
	LOBBY_CARD_IMAGES_DIR="${LOBBY_CARD_IMAGES_DIR}"; \
	echo "Copying files from GENERATED_LOBBY_IMAGES_DIR=$${GENERATED_LOBBY_IMAGES_DIR} to LOBBY_CARD_IMAGES_DIR=$${LOBBY_CARD_IMAGES_DIR}"; \
	for VARIANT_DIR in "$${GENERATED_LOBBY_IMAGES_DIR}/"*/; do \
		VARIANT=$$(basename "$${VARIANT_DIR}"); \
		LOBBY_CARD_IMAGES_VARIANT_DIR="$${LOBBY_CARD_IMAGES_DIR}/$${VARIANT}/Runtime/Asset/"; \
		echo "Processing VARIANT: $${VARIANT} VARIANT_DIR: $${VARIANT_DIR} LOBBY_CARD_IMAGES_DIR: $${LOBBY_CARD_IMAGES_DIR}"; \
		for VARIANT_IMAGE in "$${VARIANT_DIR}"*; do \
			GENERATED_LOBBY_IMAGE=$$(basename "$${VARIANT_IMAGE}"); \
			if [[ "$${GENERATED_LOBBY_IMAGE}" =~ ^Game[0-9]{3}.*\.png$$ ]]; then \
				echo "Processing GENERATED_LOBBY_IMAGE: $${GENERATED_LOBBY_IMAGE} VARIANT_IMAGE: $${VARIANT_IMAGE}"; \
				LOBBY_CARD_IMAGE=$$(echo "$${GENERATED_LOBBY_IMAGE}" | sed -E 's/(Game[0-9]{3})([A-Za-z]+)\.png/\1__\2__LobbyCard.png/'); \
				MACHINE_NAME=$$(echo "$${LOBBY_CARD_IMAGE}" | sed -E 's/(Game[0-9]{3}).*/\1/'); \
				cp "$${VARIANT_IMAGE}" "$${LOBBY_CARD_IMAGES_VARIANT_DIR}/$${MACHINE_NAME}/LobbyCard/Materials/$${LOBBY_CARD_IMAGE}"; \
			else \
				echo "Skipping directory: $${GENERATED_LOBBY_IMAGE} (does not match Game<NNN> pattern)"; \
			fi; \
		done; \
	done;


sync-background-images: prepare-project
	@echo "Running sync-background-images..."
	GENERATED_BACKGROUND_IMAGES_DIR="${GENERATED_BACKGROUND_IMAGES_DIR}"; \
	for VARIANT_DIR in "$${GENERATED_BACKGROUND_IMAGES_DIR}/"*/; do \
		VARIANT=$$(basename "$${VARIANT_DIR}"); \
		for GAME_NAME_DIR in "$${VARIANT_DIR}/"*/; do \
			GAME_NAME=$$(basename "$${GAME_NAME_DIR}"); \
			echo "Processing GAME_NAME: $${GAME_NAME}"; \
			MACHINE_NAME=$$(echo "$${GAME_NAME}" | sed -E 's/(Game[0-9]{3}).*/\1/'); \
			MACHINE_VARIANT=$$(echo "$${GAME_NAME}" | sed -E 's/Game[0-9]{3}(.*)/\1/'); \
			echo "Processing MACHINE_NAME: $${MACHINE_NAME} MACHINE_VARIANT: $${MACHINE_VARIANT}"; \
			BACKGROUND_IMAGES_DIR="${UNITY_PROJECT_PATH}/Assets/Bettr/Runtime/Plugin/$${MACHINE_NAME}/variants/$${MACHINE_VARIANT}/$${VARIANT}/Runtime/Asset/Textures"; \
			cp "$${GAME_NAME_DIR}/splash.png" "$${BACKGROUND_IMAGES_DIR}/Background.png"; \
		done; \
	done;
	

# =============================================================================
#
# BUILD MACHINES
#
# =============================================================================

sync-machines: prepare-project
	@echo "Running sync-machines..."
	@echo "Running caffeinate to keep the drives awake..."
	# keep the drives awake
	caffeinate -i -m -u &
	@MODELS_DIR="${MODELS_DIR}"; \
	for MACHINE_NAME_DIR in "$${MODELS_DIR}/"*/; do \
		MACHINE_NAME=$$(basename "$${MACHINE_NAME_DIR}"); \
		if [[ "$${MACHINE_NAME}" =~ ^Game[0-9]{3}$$ ]]; then \
			echo "Processing MACHINE_NAME: $${MACHINE_NAME}"; \
			for MACHINE_VARIANT_DIR in "$${MACHINE_NAME_DIR}/"*/; do \
				MACHINE_VARIANT=$$(basename "$${MACHINE_VARIANT_DIR}"); \
				echo "Processing MACHINE_VARIANT: $${MACHINE_VARIANT}"; \
				MACHINE_MODEL="$${MODELS_DIR}/$${MACHINE_NAME}/$${MACHINE_VARIANT}/$${MACHINE_NAME}Models.lua"; \
				UNITY_OUTPUT=$$(${UNITY_APP} -batchmode -logFile "${ASSET_DATA_LOG_FILE_PATH}" -quit -projectPath "${UNITY_PROJECT_PATH}" -executeMethod "${SYNC_MACHINE_METHOD}" -machineName "$${MACHINE_NAME}" -machineVariant "$${MACHINE_VARIANT}" -machineModel "$${MACHINE_MODEL}" 2>&1); \
				UNITY_EXIT_STATUS=$$?; \
				if [ "$$UNITY_EXIT_STATUS" -ne 0 ]; then \
					echo "Error executing Unity for MACHINE_NAME=$${MACHINE_NAME}, MACHINE_VARIANT=$${MACHINE_VARIANT} ASSET_DATA_LOG_FILE_PATH=${ASSET_DATA_LOG_FILE_PATH}"; \
					echo "Unity Output: $$UNITY_OUTPUT"; \
					exit $$UNITY_EXIT_STATUS; \
				fi; \
				echo "Executed for MACHINE_NAME=$${MACHINE_NAME}, MACHINE_VARIANT=$${MACHINE_VARIANT}, MACHINE_MODEL=$${MACHINE_MODEL} ASSET_DATA_LOG_FILE_PATH=${ASSET_DATA_LOG_FILE_PATH}"; \
				sleep ${SLEEP_DURATION}; \
			done; \
		else \
			echo "Skipping directory: $${MACHINE_NAME} (does not match Game<NNN> pattern)"; \
		fi; \
	done
	# Kill all caffeinate processes
	@echo "Killing caffeinate..."
	killall caffeinate


# Define the arrays for MACHINE_NAME and MACHINE_VARIANT
MACHINE_NAME_ARRAY := Game001
MACHINE_VARIANT_ARRAY := EpicAncientAdventures
sync-machines-specific: prepare-project
	@echo "Running sync-machines-specific..."
	@echo "Running caffeinate to keep the drives awake..."
	# Keep the drives awake
	caffeinate -i -m -u &
	@MODELS_DIR="${MODELS_DIR}"; \
	for MACHINE_NAME in $(MACHINE_NAME_ARRAY); do \
		echo "Processing MACHINE_NAME: $${MACHINE_NAME}"; \
		for MACHINE_VARIANT in $(MACHINE_VARIANT_ARRAY); do \
			echo "Processing MACHINE_VARIANT: $${MACHINE_VARIANT}"; \
			MACHINE_MODEL="$${MODELS_DIR}/$${MACHINE_NAME}/$${MACHINE_VARIANT}/$${MACHINE_NAME}Models.lua"; \
			UNITY_OUTPUT=$$(${UNITY_APP} -batchmode -logFile "${ASSET_DATA_LOG_FILE_PATH}" -quit -projectPath "${UNITY_PROJECT_PATH}" -executeMethod "${SYNC_MACHINE_METHOD}" -machineName "$${MACHINE_NAME}" -machineVariant "$${MACHINE_VARIANT}" -machineModel "$${MACHINE_MODEL}" 2>&1); \
			UNITY_EXIT_STATUS=$$?; \
			if [ "$$UNITY_EXIT_STATUS" -ne 0 ]; then \
				echo "Error executing Unity for MACHINE_NAME=$${MACHINE_NAME}, MACHINE_VARIANT=$${MACHINE_VARIANT} ASSET_DATA_LOG_FILE_PATH=${ASSET_DATA_LOG_FILE_PATH}"; \
				echo "Unity Output: $$UNITY_OUTPUT"; \
				exit $$UNITY_EXIT_STATUS; \
			fi; \
			echo "Executed for MACHINE_NAME=$${MACHINE_NAME}, MACHINE_VARIANT=$${MACHINE_VARIANT}, MACHINE_MODEL=$${MACHINE_MODEL} ASSET_DATA_LOG_FILE_PATH=${ASSET_DATA_LOG_FILE_PATH}"; \
			sleep ${SLEEP_DURATION}; \
		done; \
	done
	# Kill all caffeinate processes
	@echo "Killing caffeinate..."
	killall caffeinate


sync-background-textures:
	@echo "Running sync-background-textures..."
	@echo "Running caffeinate to keep the drives awake..."
	# keep the drives awake
	caffeinate -i -m -u &
	@MODELS_DIR="${MODELS_DIR}"; \
	for MACHINE_NAME_DIR in "$${MODELS_DIR}/"*/; do \
		MACHINE_NAME=$$(basename "$${MACHINE_NAME_DIR}"); \
		if [[ "$${MACHINE_NAME}" =~ ^Game[0-9]{3}$$ ]]; then \
			for MACHINE_VARIANT_DIR in "$${MACHINE_NAME_DIR}/"*/; do \
				MACHINE_VARIANT=$$(basename "$${MACHINE_VARIANT_DIR}"); \
				EXPERIMENT_VARIANT="control"; \
				echo "Processing MACHINE_NAME: $${MACHINE_NAME} MACHINE_VARIANT: $${MACHINE_VARIANT} EXPERIMENT_VARIANT: $${EXPERIMENT_VARIANT}"; \
				UNITY_OUTPUT=$$(${UNITY_APP} -batchmode -logFile "${ASSET_DATA_LOG_FILE_PATH}" -quit -projectPath "${UNITY_PROJECT_PATH}" -executeMethod "${SYNC_BACKGROUND_TEXTURES_METHOD}" -machineName "$${MACHINE_NAME}" -machineVariant "$${MACHINE_VARIANT}" -experimentVariant "$${EXPERIMENT_VARIANT}" 2>&1); \
				UNITY_EXIT_STATUS=$$?; \
				if [ "$$UNITY_EXIT_STATUS" -ne 0 ]; then \
					echo "Error executing Unity for MACHINE_NAME=$${MACHINE_NAME}, MACHINE_VARIANT=$${MACHINE_VARIANT} EXPERIMENT_VARIANT=$${EXPERIMENT_VARIANT}, ASSET_DATA_LOG_FILE_PATH=${ASSET_DATA_LOG_FILE_PATH}"; \
					echo "Unity Output: $$UNITY_OUTPUT"; \
					exit $$UNITY_EXIT_STATUS; \
				fi; \
				echo "Executed for MACHINE_NAME=$${MACHINE_NAME}, MACHINE_VARIANT=$${MACHINE_VARIANT}, EXPERIMENT_VARIANT=$${EXPERIMENT_VARIANT}, ASSET_DATA_LOG_FILE_PATH=${ASSET_DATA_LOG_FILE_PATH}"; \
				EXPERIMENT_VARIANT="variant1"; \
				echo "Processing MACHINE_NAME: $${MACHINE_NAME} MACHINE_VARIANT: $${MACHINE_VARIANT} EXPERIMENT_VARIANT: $${EXPERIMENT_VARIANT}"; \
				UNITY_OUTPUT=$$(${UNITY_APP} -batchmode -logFile "${ASSET_DATA_LOG_FILE_PATH}" -quit -projectPath "${UNITY_PROJECT_PATH}" -executeMethod "${SYNC_BACKGROUND_TEXTURES_METHOD}" -machineName "$${MACHINE_NAME}" -machineVariant "$${MACHINE_VARIANT}" -experimentVariant "$${EXPERIMENT_VARIANT}" 2>&1); \
				UNITY_EXIT_STATUS=$$?; \
				if [ "$$UNITY_EXIT_STATUS" -ne 0 ]; then \
					echo "Error executing Unity for MACHINE_NAME=$${MACHINE_NAME}, MACHINE_VARIANT=$${MACHINE_VARIANT} EXPERIMENT_VARIANT=$${EXPERIMENT_VARIANT}, ASSET_DATA_LOG_FILE_PATH=${ASSET_DATA_LOG_FILE_PATH}"; \
					echo "Unity Output: $$UNITY_OUTPUT"; \
					exit $$UNITY_EXIT_STATUS; \
				fi; \
				echo "Executed for MACHINE_NAME=$${MACHINE_NAME}, MACHINE_VARIANT=$${MACHINE_VARIANT}, EXPERIMENT_VARIANT=$${EXPERIMENT_VARIANT}, ASSET_DATA_LOG_FILE_PATH=${ASSET_DATA_LOG_FILE_PATH}"; \
			done; \
		else \
			echo "Skipping directory: $${MACHINE_NAME} (does not match Game<NNN> pattern)"; \
		fi; \
	done
	# Kill all caffeinate processes
	@echo "Killing caffeinate..."
	killall caffeinate



# =============================================================================
#
# DEPLOY UNITY IOS
#
# =============================================================================

build-target-ios: prepare-project
	@echo "Building iOS project..."
	@$(UNITY_APP) -quit -batchmode -logFile $(LOGS_IOS)/logfile.log -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(BUILD_METHOD_IOS) -buildOutput $(BUILD_IOS) -buildTarget iOS -development -scriptDebugging
	@echo "Build completed."

deploy-target-ios: build-target-ios archive-target-ios
	@echo "Creating IPA file and deploying to TestFlight..."
	echo "(API_PRIVATE_KEYS_DIR is set in the environment to the path to the auth key file)"
	CFBundleShortVersionString=$$(/usr/libexec/PlistBuddy -c "Print :CFBundleShortVersionString" $(APPLE_BUILD_INFO_PLIST)); \
	echo "CFBundleShortVersionString: $$CFBundleShortVersionString"; \
	xcodebuild -exportArchive -archivePath $(APPLE_ARCHIVE_PATH)/BettrSlots-$$CFBundleShortVersionString.xcarchive -exportOptionsPlist $(APPLE_EXPORT_OPTIONS_PLIST) -allowProvisioningUpdates

clean-target-ios: 
	@echo "Cleaning previous build artifacts..."
	@rm -rf $(BUILD_IOS)/BettrSlots

archive-target-ios: clean-target-ios build-target-ios
	@echo "Archiving Xcode project..."
	CFBundleShortVersionString=$$(/usr/libexec/PlistBuddy -c "Print :CFBundleShortVersionString" $(APPLE_BUILD_INFO_PLIST)); \
	echo "CFBundleShortVersionString: $$CFBundleShortVersionString"; \
	xcodebuild -project $(BUILD_IOS)/BettrSlots/Unity-iPhone.xcodeproj -scheme $(APPLE_BUILD_SCHEME) -configuration $(APPLE_BUILD_CONFIGURATION) -destination $(APPLE_BUILD_DESTINATION) -archivePath $(APPLE_ARCHIVE_PATH)/BettrSlots-$$CFBundleShortVersionString.xcarchive archive	

bump: unity-patch-version-target-ios

unity-patch-bundle-version:
	@echo "Incrementing Unity bundleVersion..."
	$(eval CURRENT_BUNDLE_VERSION := $(shell grep 'bundleVersion' $(UNITY_PROJECT_SETTINGS) | sed -E 's/.*bundleVersion: (.*)/\1/'))
	$(eval NEW_BUNDLE_VERSION := $(shell echo $(CURRENT_BUNDLE_VERSION) | awk -F'.' '{$$NF+=1;OFS="."}1' OFS='.'))
	@echo "Current bundleVersion: $(CURRENT_BUNDLE_VERSION)"
	@echo "New bundleVersion: $(NEW_BUNDLE_VERSION)"
	@sed -i '' 's/bundleVersion: $(CURRENT_BUNDLE_VERSION)/bundleVersion: $(NEW_BUNDLE_VERSION)/' $(UNITY_PROJECT_SETTINGS)

unity-patch-version-target-ios:
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
# DEPLOY UNITY WEBGL
#
# =============================================================================

build-target-webgl: prepare-project
	@echo "Building WebGL project..."
	@$(UNITY_APP) -quit -batchmode -logFile $(LOGS_WEBGL)/logfile.log -projectPath $(UNITY_PROJECT_PATH) -executeMethod $(BUILD_METHOD_WEBGL) -buildOutput $(BUILD_WEBGL) -buildTarget WebGL -development -scriptDebugging
	@echo "Build completed."

publish-target-webgl:
	@echo "Publishing WebGL project to S3..."
	# Sync all files except .gz files and disable caching
	@aws s3 sync $(BUILD_WEBGL)/BettrSlots s3://$(S3_WEBGL_BUCKET)/$(S3_WEBGL_OBJECT_KEY) --profile $(AWS_DEFAULT_PROFILE) --exclude "*.gz" --cache-control "no-cache, no-store, must-revalidate"
	# Sync only .gz files with the Content-Encoding header set to gzip and disable caching
	@aws s3 sync $(BUILD_WEBGL)/BettrSlots s3://$(S3_WEBGL_BUCKET)/$(S3_WEBGL_OBJECT_KEY) --profile $(AWS_DEFAULT_PROFILE) --include "*.gz" --content-encoding "gzip" --cache-control "no-cache, no-store, must-revalidate"
	@echo "Publish completed."

invalidate-target_webgl: publish-target-webgl
	@echo "Invalidating CloudFront cache..."
	aws cloudfront create-invalidation --distribution-id $(S3_WEBGL_CLOUDFRONT_DISTRIBUTION_ID) --paths "/*" --profile $(AWS_DEFAULT_PROFILE)
	@echo "CloudFront invalidation completed."

deploy-target-webgl: publish-target-webgl invalidate-target_webgl
	@echo "Deploying WebGL project..."
	@echo "Deployment completed."

build-webgl:  build-assets-webgl build-target-webgl

deploy-webgl:  deploy-assets-webgl deploy-target-webgl

deploy-webgl-all: build-webgl deploy-webgl build-assets-webgl deploy-assets-webgl
	@echo "Deploying WebGL project..."
	@echo "Deployment completed."

# =============================================================================
#
# HELPERS
#
# =============================================================================

pull:
	git pull origin main




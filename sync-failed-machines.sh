#!/bin/bash

# Exit on any error
set -ex

UNITY_PROJECT_PATH="Unity"
ASSET_DATA_LOG_FILE_PATH="${BETTR_CASINO_LOGS_HOME}/UnityAssetData/logfile.log"
UNITY_APP="${UNITY_HOME}/2022.3.16f1/Unity.app/Contents/MacOS/Unity"
SYNC_MACHINE_METHOD="Bettr.Editor.BettrMenu.BuildMachinesFromCommandLine"

# Run Unity commands
# Specific Machines

# MACHINE_NAME="Game004"
# MACHINE_VARIANT="RichesBeverlyHillMansions"
# MACHINE_MODEL="/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/../../bettr-infrastructure/bettr-infrastructure/tools/publish-data/published_models/${MACHINE_NAME}/${MACHINE_VARIANT}/${MACHINE_NAME}Models.lua"
# "${UNITY_APP}" -batchmode -logFile "${ASSET_DATA_LOG_FILE_PATH}" -quit -projectPath "${UNITY_PROJECT_PATH}" -executeMethod "${SYNC_MACHINE_METHOD}" -machineName "${MACHINE_NAME}" -machineVariant "${MACHINE_VARIANT}" -machineModel "${MACHINE_MODEL}"

# MACHINE_NAME="Game004"
# MACHINE_VARIANT="RichesPharosRiches"
# MACHINE_MODEL="/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/../../bettr-infrastructure/bettr-infrastructure/tools/publish-data/published_models/${MACHINE_NAME}/${MACHINE_VARIANT}/${MACHINE_NAME}Models.lua"
# "${UNITY_APP}" -batchmode -logFile "${ASSET_DATA_LOG_FILE_PATH}" -quit -projectPath "${UNITY_PROJECT_PATH}" -executeMethod "${SYNC_MACHINE_METHOD}" -machineName "${MACHINE_NAME}" -machineVariant "${MACHINE_VARIANT}" -machineModel "${MACHINE_MODEL}"

# MACHINE_NAME="Game005"
# MACHINE_VARIANT="FortunesPharosRiches"
# MACHINE_MODEL="/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/../../bettr-infrastructure/bettr-infrastructure/tools/publish-data/published_models/${MACHINE_NAME}/${MACHINE_VARIANT}/${MACHINE_NAME}Models.lua"
# "${UNITY_APP}" -batchmode -logFile "${ASSET_DATA_LOG_FILE_PATH}" -quit -projectPath "${UNITY_PROJECT_PATH}" -executeMethod "${SYNC_MACHINE_METHOD}" -machineName "${MACHINE_NAME}" -machineVariant "${MACHINE_VARIANT}" -machineModel "${MACHINE_MODEL}"

# MACHINE_NAME="Game006"
# MACHINE_VARIANT="WheelsAncientKingdom"
# MACHINE_MODEL="/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/../../bettr-infrastructure/bettr-infrastructure/tools/publish-data/published_models/${MACHINE_NAME}/${MACHINE_VARIANT}/${MACHINE_NAME}Models.lua"
# "${UNITY_APP}" -batchmode -logFile "${ASSET_DATA_LOG_FILE_PATH}" -quit -projectPath "${UNITY_PROJECT_PATH}" -executeMethod "${SYNC_MACHINE_METHOD}" -machineName "${MACHINE_NAME}" -machineVariant "${MACHINE_VARIANT}" -machineModel "${MACHINE_MODEL}"


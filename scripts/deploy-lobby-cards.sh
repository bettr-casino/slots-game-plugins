#!/bin/bash

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"

source $SCRIPT_DIR/venv/bin/activate

pip install -r $SCRIPT_DIR/requirements.txt

asset_directory="/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/LocalStore/AssetBundles/WebGL"
lobbycard_pattern="lobbycardgame*.control"
lobbycard_manifest_pattern="lobbycardgame*.control.manifest"
lobbycard_merged_file="lobbycardv0_1_0.merged.control.bin"
manifest_file="lobbycardv0_1_0.merged.control.manifest"
s3_bucket_name="bettr-casino-assets"
s3_object_prefix="assets/latest/WebGL"

python deploy-lobby-cards.py --directory $asset_directory \
    --patterns $lobbycard_pattern $lobbycard_manifest_pattern \
    --output_file $lobbycard_merged_file \
    --manifest_file $manifest_file \
    --s3_bucket_name $s3_bucket_name \
    --s3_object_prefix $s3_object_prefix

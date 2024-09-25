source_base_dir="/Users/rvergis/Documents/External/Bettr/GitHub/bettr-infrastructure/bettr-infrastructure/tools/pipelines/game-assets/pipelines/game-assets-generated-files"
target_base_dir="/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Runtime/Plugin"

function delete_files() {
    game_id=$1
    machine_id=$2
    experiment="control"
    # remove all files witht the prefix BackgroundAnnotation of type .fbx and .json and .yaml
    rm -f $target_base_dir/$game_id/variants/$machine_id/$experiment/Runtime/Asset/Textures/BackgroundAnnotation*.fbx
    rm -f $target_base_dir/$game_id/variants/$machine_id/$experiment/Runtime/Asset/Textures/BackgroundAnnotation*.fbx.meta
    rm -f $target_base_dir/$game_id/variants/$machine_id/$experiment/Runtime/Asset/Textures/BackgroundAnnotation*.json
    rm -f $target_base_dir/$game_id/variants/$machine_id/$experiment/Runtime/Asset/Textures/BackgroundAnnotation*.json.meta
    rm -f $target_base_dir/$game_id/variants/$machine_id/$experiment/Runtime/Asset/Textures/BackgroundAnnotation*.yaml
    rm -f $target_base_dir/$game_id/variants/$machine_id/$experiment/Runtime/Asset/Textures/BackgroundAnnotation*.yaml.meta
}

# Copy the background annotation files to the plugin directory
function copy_files() {
    game_id=$1
    machine_id=$2
    experiment="control"
    cp $source_base_dir/BackgroundAnnotations/$game_id/$machine_id/$experiment/BackgroundAnnotation*.fbx $target_base_dir/$game_id/variants/$machine_id/$experiment/Runtime/Asset/Textures
    cp $source_base_dir/BackgroundAnnotations/$game_id/$machine_id/$experiment/BackgroundAnnotation*.json $target_base_dir/$game_id/variants/$machine_id/$experiment/Runtime/Asset/Textures
    cp $source_base_dir/BackgroundAnnotations/$game_id/$machine_id/$experiment/BackgroundAnnotation*.yaml $target_base_dir/$game_id/variants/$machine_id/$experiment/Runtime/Asset/Textures
}

delete_files "Game001" "EpicAncientAdventures"
delete_files "Game001" "EpicAtlantisTreasures"
delete_files "Game001" "EpicClockworkChronicles"

copy_files "Game001" "EpicAncientAdventures"
copy_files "Game001" "EpicAtlantisTreasures"
copy_files "Game001" "EpicClockworkChronicles"
#!/bin/bash

# Define paths
templates_path="/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/templates"
script_dir=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
mechanics_dir="$script_dir/../Mechanics"

function process_mechanics_dir() {
  dir=$1
  game=$2
  machine_variant=$3
  experiment_variant=$4
  # Find all .cscript.txt files recursively in the script directory
  find "$dir" -type f -name "*.cscript.txt" | while read -r file; do
      # Extract the filename from the path
      filename=$(basename "$file")
  
      # Check for the first pattern: GameNNNBaseGameMachine{mechanic}Mechanic.cscript.txt
      if [[ "$filename" =~ Game[0-9]+BaseGameMachine([a-zA-Z]+)Mechanic.cscript.txt ]]; then
          mechanic="${BASH_REMATCH[1]}"
          
          # Convert mechanic to lowercase for the directory path
          mechanic_lower=$(echo "$mechanic" | tr '[:upper:]' '[:lower:]')
  
          # Define the target path for the file
          target_path="$templates_path/mechanics/$mechanic_lower/scripts/GameBaseGameMachine${mechanic}Mechanic.cscript.txt.template"
  
          # Ensure the target directory exists
          mkdir -p "$(dirname "$target_path")"
  
          # Copy the file with multiple replacements
          sed -e "s/${game}/{{machineName}}/g" \
              -e "s/${machine_variant}/{{machineVariant}}/g" \
              -e "s/${experiment_variant}/{{experimentVariant}}/g" \
              -e "s/${mechanic}/{{mechanicName}}/g" \
              "$file" > "$target_path"
  
          echo "Processed $filename for Game[0-9]+BaseGameMachine([a-zA-Z]+)Mechanic.cscript.txt pattern."
  
      # Check for the pattern: Game<NNN>BaseGameReel{mechanic}Mechanic.cscript.txt
      elif [[ "$filename" =~ Game([0-9]+)BaseGameReel([a-zA-Z]+)Mechanic.cscript.txt ]]; then
          game_number="${BASH_REMATCH[1]}"
          mechanic="${BASH_REMATCH[2]}"
          
          # Convert mechanic to lowercase for the directory path
          mechanic_lower=$(echo "$mechanic" | tr '[:upper:]' '[:lower:]')
  
          # Define the target path for the file
          target_path="$templates_path/mechanics/$mechanic_lower/scripts/GameBaseGameReel${mechanic}Mechanic.cscript.txt.template"
  
          # Ensure the target directory exists
          mkdir -p "$(dirname "$target_path")"
  
          # Copy the file with replacement
          # Copy the file with multiple replacements
          sed -e "s/${game}/{{machineName}}/g" \
              -e "s/${machine_variant}/{{machineVariant}}/g" \
              -e "s/${experiment_variant}/{{experimentVariant}}/g" \
              -e "s/${mechanic}/{{mechanicName}}/g" \
              "$file" > "$target_path"
  
          echo "Processed $filename for Game([0-9]+)BaseGameReel([a-zA-Z]+)Mechanic.cscript.txt pattern."
          
      # Check for the pattern: Game<NNN>BaseGameMachine{mechanic}.cscript.txt
      elif [[ "$filename" =~ Game([0-9]+)BaseGameMachine([a-zA-Z]+).cscript.txt ]]; then
            game_number="${BASH_REMATCH[1]}"
            mechanic="${BASH_REMATCH[2]}"
            
            # Convert mechanic to lowercase for the directory path
            mechanic_lower=$(echo "$mechanic" | tr '[:upper:]' '[:lower:]')
    
            # Define the target path for the file
            target_path="$templates_path/mechanics/$mechanic_lower/scripts/BaseGameMachine${mechanic}.cscript.txt.template"
    
            # Ensure the target directory exists
            mkdir -p "$(dirname "$target_path")"
    
            # Copy the file with replacement
            # Copy the file with multiple replacements
            sed -e "s/${game}/{{machineName}}/g" \
                -e "s/${machine_variant}/{{machineVariant}}/g" \
                -e "s/${experiment_variant}/{{experimentVariant}}/g" \
                -e "s/${mechanic}/{{mechanicName}}/g" \
                "$file" > "$target_path"
    
            echo "Processed $filename for Game<NNN>BaseGameMachine{mechanic}.cscript.txt pattern."
            
      # Check for the pattern: Game<NNN>BaseGameBackground{mechanic}Mechanic.cscript.txt
      elif [[ "$filename" =~ Game([0-9]+)BaseGameBackground([a-zA-Z]+)Mechanic.cscript.txt ]]; then
            game_number="${BASH_REMATCH[1]}"
            mechanic="${BASH_REMATCH[2]}"
            
            # Convert mechanic to lowercase for the directory path
            mechanic_lower=$(echo "$mechanic" | tr '[:upper:]' '[:lower:]')
    
            # Define the target path for the file
            target_path="$templates_path/mechanics/$mechanic_lower/scripts/BaseGameBackground${mechanic}Mechanic.cscript.txt.template"
    
            # Ensure the target directory exists
            mkdir -p "$(dirname "$target_path")"
    
            # Copy the file with replacement
            # Copy the file with multiple replacements
            sed -e "s/${game}/{{machineName}}/g" \
                -e "s/${machine_variant}/{{machineVariant}}/g" \
                -e "s/${experiment_variant}/{{experimentVariant}}/g" \
                -e "s/${mechanic}/{{mechanicName}}/g" \
                "$file" > "$target_path"
    
            echo "Processed $filename for Game<NNN>BaseGameBackground{mechanic}Mechanic.cscript.txt pattern."
      fi
  done    
}

function process_scripts_dir() {
  dir=$1
  # Find all .cscript.txt files recursively in the script directory
  find "$dir" -type f -name "*.cscript.txt" | while read -r file; do
      # Extract the filename from the path
      filename=$(basename "$file")
  
      # Check for the first pattern: Game<NNN>BaseGameMachine{mechanic}.cscript.txt
      if [[ "$filename" =~ Game([0-9]+)BaseGameMachine([a-zA-Z]*).cscript.txt ]]; then
          game_number="${BASH_REMATCH[1]}"
          ext="${BASH_REMATCH[2]}"
            
          # Define the target path for the file
          target_path="$templates_path/scripts/GameBaseGameMachine${ext}.cscript.txt.template"
  
          # Ensure the target directory exists
          mkdir -p "$(dirname "$target_path")"
  
          # Copy the file with replacement
          sed "s/Game${game_number}/{{machineName}}/g" "$file" > "$target_path"
  
          echo "Processed $filename for GameBaseGameMachine pattern."
  
      # Check for the second pattern: Game<NNN>BaseGameReel{mechanic}.cscript.txt
      elif [[ "$filename" =~ Game([0-9]+)BaseGameReel([a-zA-Z]*).cscript.txt ]]; then
          game_number="${BASH_REMATCH[1]}"
          ext="${BASH_REMATCH[2]}"
          
          # Define the target path for the file
          target_path="$templates_path/scripts/GameBaseGameReel${ext}.cscript.txt.template"
  
          # Ensure the target directory exists
          mkdir -p "$(dirname "$target_path")"
  
          # Copy the file with replacement
          sed "s/Game${game_number}/{{machineName}}/g" "$file" > "$target_path"
  
          echo "Processed $filename for GameBaseGameReel pattern."
      fi
  done    
}


mechanics_directories=(
    "${mechanics_dir}"
)

for dir in "${mechanics_directories[@]}"; do
    echo "Processing directory $dir"
    # get the processing dir
    #  Assets/Bettr/Runtime/Plugin/Game001/variants/EpicAtlantisTreasures/control/Runtime/Asset/Scripts/../Mechanics
    # .../<GameNNN>/variants/<MachineVariant>/<ExperimentVariant>/Runtime/Assets/Scripts/../Mechanics
    # remove the "/Runtime/Assets/Scripts/../Mechanics" suffix from the $dir
    suffix="/Runtime/Asset/Scripts/../Mechanics"
    modified_dir=${dir%$suffix}
    # experiment variant is the last part of the modified_dir path
    experiment_variant=$(basename $modified_dir)
    # now remove the experiment variant from the modified_dir
    modified_dir=${modified_dir%/$experiment_variant}
    # machine variant is the last part of the modified_dir path
    machine_variant=$(basename $modified_dir)
    # now remove the machine variant from the modified_dir
    modified_dir=${modified_dir%/$machine_variant}
    # now remove the "variants" from the modified_dir
    modified_dir=${modified_dir%variants}
    # now the game is the last part of the modified_dir path
    game=$(basename $modified_dir)
    echo "Game: $game, Machine Variant: $machine_variant, Experiment Variant: $experiment_variant"
     process_mechanics_dir "$dir" "$game" "$machine_variant" "$experiment_variant" 
done

#
# TODO: FIXME TURN OFF SCRIPTS DIRECTORY PROCESSING 
#
#script_directories=(
#    "${script_dir}"
#)
#
#for dir in "${script_directories[@]}"; do
#    echo "Processing directory $dir"
#    process_scripts_dir "$dir"
#done
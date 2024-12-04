#!/bin/bash

# Define paths
templates_path="/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/templates"
script_dir=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
mechanics_dir="$script_dir/../Mechanics"

function process_mechanics_dir() {
  dir=$1
  # Find all .cscript.txt files recursively in the script directory
  find "$dir" -type f -name "*.cscript.txt" | while read -r file; do
      # Extract the filename from the path
      filename=$(basename "$file")
  
      # Check for the first pattern: Game<NNN>BaseGameMachine{mechanic}Mechanic.cscript.txt
      if [[ "$filename" =~ Game([0-9]+)BaseGameMachine([a-zA-Z]+)Mechanic.cscript.txt ]]; then
          game_number="${BASH_REMATCH[1]}"
          mechanic="${BASH_REMATCH[2]}"
          
          # Convert mechanic to lowercase for the directory path
          mechanic_lower=$(echo "$mechanic" | tr '[:upper:]' '[:lower:]')
  
          # Define the target path for the file
          target_path="$templates_path/mechanics/$mechanic_lower/scripts/GameBaseGameMachine${mechanic}Mechanic.cscript.txt.template"
  
          # Ensure the target directory exists
          mkdir -p "$(dirname "$target_path")"
  
          # Copy the file with replacement
          sed "s/Game${game_number}/{{machineName}}/g" "$file" > "$target_path"
  
          echo "Processed $filename for GameBaseGameMachine pattern."
  
      # Check for the second pattern: Game<NNN>BaseGameReel{mechanic}Mechanic.cscript.txt
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
          sed "s/Game${game_number}/{{machineName}}/g" "$file" > "$target_path"
  
          echo "Processed $filename for GameBaseGameReel pattern."
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
    process_mechanics_dir "$dir"
done

script_directories=(
    "${script_dir}"
)

for dir in "${script_directories[@]}"; do
    echo "Processing directory $dir"
    process_scripts_dir "$dir"
done
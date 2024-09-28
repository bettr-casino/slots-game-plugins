#!/bin/bash

webp_dir="/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/textures"

# List of symbols to be converted
symbols_list=("M1" "M2" "M3" "M4" "F5" "F6" "F7" "F8" "F9" "F10" "WD" "SC")

# Convert WebP to JPG for each symbol
for symbol in "${symbols_list[@]}"
do
    # Input WebP file path
    input_webp="$webp_dir/$symbol.webp"
    # Output JPG file path
    output_jpg="$webp_dir/$symbol.jpg"
    
    # Check if input WebP file exists before converting
    if [ -f "$input_webp" ]; then
        # Convert WebP to JPG using ImageMagick
        magick "$input_webp" "$output_jpg"
        echo "Converted $input_webp to $output_jpg"
    else
        echo "File $input_webp does not exist"
    fi
done

# Games parent directory
games_parent_dir="/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/textures"

# Loop through game directories
for game_dir in "$games_parent_dir"/*; do
  if [ -d "$game_dir" ]; then
    # Loop through machine directories within each game directory
    for machine_dir in "$game_dir"/*; do
      if [ -d "$machine_dir" ]; then
        # Copy JPG files from the symbols directory to each machine directory
        # cp the symbols in the symbols directory to the machine directory
        for symbol in "${symbols_list[@]}"
        do
          cp "$webp_dir/$symbol.jpg" "$machine_dir"
        done
        echo "Copied JPG files to $machine_dir"
      fi
    done
  fi
done

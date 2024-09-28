#!/bin/bash

# Input and output directories
input_directory="/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/audio"
output_directory="/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/audio"

# Loop through all the OGG files in the input directory
for input_file in "$input_directory"/*.ogg; do
    # Get the base name of the file without the extension
    base_name=$(basename "$input_file" .ogg)
    
    # Create the output file path
    output_file="$output_directory/$base_name-reencoded.ogg"
    
    # Convert the OGG file to OGG Vorbis (re-encode)
    echo "Re-encoding $input_file to $output_file"
    ffmpeg -i "$input_file" -acodec libvorbis -q:a 5 "$output_file"
    
    # Check if conversion was successful
    if [[ $? -eq 0 ]]; then
        echo "Successfully re-encoded $input_file to $output_file"
    else
        echo "Failed to re-encode $input_file"
    fi
done

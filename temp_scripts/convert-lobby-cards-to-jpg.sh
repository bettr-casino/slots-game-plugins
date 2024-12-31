#!/bin/bash

# convert all .png files recursively in the target_dir directory to .jpg
# if they start with "Game"

target_dir="/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Runtime/Plugin/LobbyCard"

for file in $(find $target_dir -name "Game*.png"); do
    echo "Converting $file to ${file%.*}.jpg"
    # find the parent directory of the file
    parent_dir=$(dirname $file)
    # its parent is Textures
    textures_dir=$(dirname $parent_dir)
    # mkdir the jpg directory under Textures
    mkdir -p "$textures_dir/jpg"
    # use magick to convert and save the file to the jpg directory    
    target_file="$textures_dir/jpg/$(basename ${file%.*}.jpg)"
    magick "$file" -quality 85 "$target_file"
done
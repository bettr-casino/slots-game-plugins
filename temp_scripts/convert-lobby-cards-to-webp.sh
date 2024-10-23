#!/bin/bash

# convert all .png files recursively in the target_dir directory to .webp
# if they start with "Game"

target_dir="/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Runtime/Plugin/LobbyCard"

for file in $(find $target_dir -name "Game*.png"); do
    echo "Converting $file to ${file%.*}.webp"
    # use magick to convert and save the file at the same relative path
    magick "$file" "${file%.*}.webp"
done
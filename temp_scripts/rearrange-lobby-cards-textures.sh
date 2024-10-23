#!/bin/bash

# convert all .png files recursively in the target_dir directory to .jpg
# if they start with "Game"

target_dir="/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Runtime/Plugin/LobbyCard"

for file in $(find $target_dir -name "Game*.png.meta"); do
    # find the parent directory of the file
    parent_dir=$(dirname $file)
    # make a new directories  Textures/png at the same level as the parent directory
    mkdir -p "$parent_dir/../Textures/png"
    # move the file to the new directory
    mv "$file" "$parent_dir/../Textures/png"
done

for file in $(find $target_dir -name "Game*.webp.meta"); do
    # find the parent directory of the file
    parent_dir=$(dirname $file)
    # make a new directories  Textures/webp at the same level as the parent directory
    mkdir -p "$parent_dir/../Textures/webp"
    # move the file to the new directory
    mv "$file" "$parent_dir/../Textures/webp"
done

for file in $(find $target_dir -name "Game*.jpg.meta"); do
    # find the parent directory of the file
    parent_dir=$(dirname $file)
    # make a new directories  Textures/webp at the same level as the parent directory
    mkdir -p "$parent_dir/../Textures/jpg"
    # move the file to the new directory
    mv "$file" "$parent_dir/../Textures/jpg"
done
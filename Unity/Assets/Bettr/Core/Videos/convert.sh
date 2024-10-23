#!/bin/bash

mp4_file="BettrVideo.mp4"
webm_file="BettrVideo.webm"

echo "Converting $mp4_file to $webm_file"
ffmpeg -y -i "$mp4_file" -c:v libvpx -b:v 1M -pix_fmt yuva420p -auto-alt-ref 0 -an "$webm_file"
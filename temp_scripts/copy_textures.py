import os
import shutil

# Source directory
source_dir = "/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/textures/Game001/ClockworkChronicles"

# Target directories
target_dirs = [
    "/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/textures/Game002",
    "/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/textures/Game003",
    "/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/textures/Game004",
    "/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/textures/Game005",
    "/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/textures/Game006",
    "/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/textures/Game007",
    "/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/textures/Game008",
    "/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/textures/Game009"
]

# Function to copy jpg files from source to every existing subdirectory in the target directories, excluding Background.jpg
def copy_jpg_files(src, target_dirs):
    for root, _, files in os.walk(src):
        for file in files:
            if file.endswith('.jpg') and file != 'Background.jpg':
                for target_dir in target_dirs:
                    for sub_dir in os.listdir(target_dir):
                        sub_dir_path = os.path.join(target_dir, sub_dir)
                        if os.path.isdir(sub_dir_path):
                            dest_file = os.path.join(sub_dir_path, file)
                            shutil.copy2(os.path.join(root, file), dest_file)
                            print(f"Copied {file} to {dest_file}")

# Copy the jpg files from ClockworkChronicles to all target directories
copy_jpg_files(source_dir, target_dirs)

print("JPG file copying process completed.")


import os
from pydub import AudioSegment

def convert_wav_to_ogg(input_file, output_file):
    """
    Converts a WAV file to OGG Vorbis format.
    
    :param input_file: Path to the input WAV file.
    :param output_file: Path to the output OGG file.
    """
    try:
        # Load the WAV file
        audio = AudioSegment.from_wav(input_file)

        # Export as OGG
        audio.export(output_file, format="ogg")
        print(f"Successfully converted {input_file} to {output_file}")
    except Exception as e:
        print(f"Error converting {input_file} to OGG: {e}")

def batch_convert_wav_to_ogg(input_directory, output_directory):
    """
    Batch converts all WAV files in a directory to OGG Vorbis format.
    
    :param input_directory: Path to the directory containing WAV files.
    :param output_directory: Path to the directory where OGG files will be saved.
    """
    if not os.path.exists(output_directory):
        os.makedirs(output_directory)

    for filename in os.listdir(input_directory):
        if filename.endswith(".wav"):
            input_file = os.path.join(input_directory, filename)
            output_file = os.path.join(output_directory, f"{os.path.splitext(filename)[0]}.ogg")
            convert_wav_to_ogg(input_file, output_file)

if __name__ == "__main__":
    # Example usage: Convert a single file
    # convert_wav_to_ogg("path_to_input_file.wav", "path_to_output_file.ogg")
    
    # Example usage: Batch convert all WAV files in a directory
    input_directory = "/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/audio"
    output_directory = "/Users/rvergis/Documents/External/Bettr/GitHub/bettr-casino/slots-game-plugins/Unity/Assets/Bettr/Editor/audio"
    batch_convert_wav_to_ogg(input_directory, output_directory)

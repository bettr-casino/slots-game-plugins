import os
import sys
from PIL import Image, ImageDraw, ImageFont
from collections import Counter
from write_text_to_image_input import inputs

def get_font_path(font_name):
    """
    Get the path to a specified system font.
    """
    if sys.platform == 'win32':
        font_path = f'C:\\Windows\\Fonts\\{font_name}.ttf'
    elif sys.platform == 'darwin':
        font_path = f'/System/Library/Fonts/Supplemental/{font_name}.ttf'
    else:
        font_path = f'/usr/share/fonts/truetype/{font_name}.ttf'

    if not os.path.exists(font_path):
        raise FileNotFoundError(f"Font '{font_name}' not found at path: {font_path}")

    return font_path

def hex_to_rgb(hex_color):
    """
    Convert a hex color string to an RGB tuple.
    """
    hex_color = hex_color.lstrip('#')
    return tuple(int(hex_color[i:i+2], 16) for i in (0, 2, 4))

def replace_background_color(image, replacement_color):
    """
    Replace the target background color with the specified replacement color.
    """
    image = image.convert("RGBA")
    data = image.load()

    width, height = image.size
    for y in range(height):
        for x in range(width):
            r, g, b, a = data[x, y]
            data[x, y] = replacement_color + (a,)

    return image

def draw_text(draw, text, position, font, text_color, max_width, h_align):
    """
    Draw text with word wrapping and alignment on the image.
    """
    # Initialize variables
    lines = []
    words = text.split()
    line = ""

    # Create lines with word wrapping
    for word in words:
        test_line = f"{line} {word}".strip()
        width, _ = draw.textsize(test_line, font=font)
        if width <= max_width:
            line = test_line
        else:
            lines.append(line)
            line = word
    lines.append(line)

    # Draw lines of text with specified alignment
    x, y = position
    for line in lines:
        width, height = draw.textsize(line, font=font)
        if h_align == 'Center':
            draw.text((x - width // 2, y), line, font=font, fill=text_color)
        elif h_align == 'Right':
            draw.text((x - width, y), line, font=font, fill=text_color)
        else:  # Left alignment
            draw.text((x, y), line, font=font, fill=text_color)
        y += height + 5

def write_text_to_image(input_filename, lines, output_filename, font_name='Arial', font_size=20, text_start_pos=(10, 10), text_color=(0, 0, 0), background_color_hex=None, max_width=None):
    """
    Write an array of lines of text to an existing image file (PNG or JPG).

    :param input_filename: Path to the input image file starting from 'input-files'.
    :param lines: List of tuples, each representing a line of text, horizontal alignment, vertical alignment, horizontal offset, and vertical offset.
    :param output_filename: Path to the output image file starting from 'output-files' with the same file name as input_filename.
    :param font_name: Name of the font to use.
    :param font_size: Size of the font.
    :param text_start_pos: Tuple (x, y) indicating the start position for the text.
    :param text_color: Text color as an RGB tuple.
    :param background_color_hex: Hex code of the background color.
    :param max_width: Maximum width for text wrapping.
    """
    # Directories
    input_dir = 'input-files'
    output_dir = 'output-files'
    
    # Construct the full input path
    input_image_path = os.path.join(input_dir, input_filename)
    
    # Ensure the output directory exists
    os.makedirs(output_dir, exist_ok=True)
    
    # Construct the full output path, maintaining the same file name
    output_image_path = os.path.join(output_dir, output_filename)
    
    # Open the existing image
    image = Image.open(input_image_path).convert("RGBA")
    
    # Replace background color if specified
    if background_color_hex:
        background_color = hex_to_rgb(background_color_hex)
        image = replace_background_color(image, background_color)
    
    # Create a drawing context
    draw = ImageDraw.Draw(image)
    
    # Load font
    font_path = get_font_path(font_name)
    font = ImageFont.truetype(font_path, font_size)
    
    # Define maximum width for text wrapping
    if max_width is None:
        max_width = image.width - 20  # Default max width with some padding
    
    # Draw each line of text
    for line, h_align, v_align, h_offset, v_offset in lines:
        x, y = text_start_pos
        x += h_offset
        y += v_offset
        
        # Adjust starting y position for vertical alignment
        if v_align == 'Center':
            y = (image.height // 2 + v_offset) - (font_size // 2)
        elif v_align == 'Bottom':
            y = image.height - v_offset - font_size
        else:  # Top alignment
            y = text_start_pos[1] + v_offset
            
        # Adjust starting x position for horizontal alignment
        if h_align == 'Center':
            x = (image.width // 2 + h_offset)
        elif h_align == 'Right':
            x = image.width - h_offset
        
        draw_text(draw, line, (x, y), font, text_color, max_width, h_align)
    
    # Convert image to RGB if saving as JPEG
    if output_filename.lower().endswith('.jpg') or output_filename.lower().endswith('.jpeg'):
        image = image.convert("RGB")
    
    # Save the image with the text added
    image.save(output_image_path)

for input_set in inputs:
    write_text_to_image(
        input_set['input_filename'],
        input_set['lines'],
        input_set['output_filename'],
        font_name=input_set['font_name'],
        font_size=input_set['font_size'],
        text_color=input_set['text_color'],
        background_color_hex=input_set.get('background_color')  # Optional background color
    )
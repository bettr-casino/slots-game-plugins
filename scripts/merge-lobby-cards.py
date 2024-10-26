import os
import json
import re
import gzip
import boto3
import argparse
from botocore.exceptions import NoCredentialsError

def find_files(directory, patterns):
    """Find all files in the directory that match any of the given patterns."""
    matched_files = []
    for root, _, files in os.walk(directory):
        for file in files:
            if file.endswith('.meta'):
                continue  # Skip .meta files
            if any(re.match(pattern, file) for pattern in patterns):  # Check each pattern
                matched_files.append(os.path.join(root, file))
    return matched_files

def extract_game_info(file_name):
    """Extract game name, variant, and experiment from the file name."""
    game_match = re.search(r'game(\d{3})', file_name)
    variant_match = re.search(r'game\d{3}([a-zA-Z]+)', file_name)
    experiment_match = re.search(r'game\d{3}[a-zA-Z]+\.(\w+)', file_name)

    # get the file extension
    file_type = file_name.split('.')[-1]
    is_manifest = file_type == 'manifest'

    game_name = f"game{game_match.group(1)}" if game_match else "unknown"
    variant = variant_match.group(1) if variant_match else "unknown"
    experiment = experiment_match.group(1) if experiment_match else "unknown"

    return game_name, variant, experiment, is_manifest

def merge_files(files, output_file):
    """Merge all files into one large file and return manifest info."""
    manifest = {}
    current_position = 0

    with open(output_file, 'wb') as merged:
        for file in files:
            file_name = os.path.basename(file)
            game_name, variant, experiment, is_manifest = extract_game_info(file_name)

            file_type = 'manifest' if is_manifest else 'bundle'

            file_size = os.path.getsize(file)
            # Write file to merged file
            with open(file, 'rb') as f:
                merged.write(f.read())

            # Build nested structure
            if game_name not in manifest:
                manifest[game_name] = {}
            if variant not in manifest[game_name]:
                manifest[game_name][variant] = {}
            if experiment not in manifest[game_name][variant]:
                manifest[game_name][variant][experiment] = {}

            # Add the byte start and length info to the structure under the correct type
            manifest[game_name][variant][experiment][file_type] = {
                'byte_start': current_position,
                'byte_length': file_size,
                'file_name': file_name
            }

            current_position += file_size

    return manifest

def write_manifest(manifest, manifest_file):
    """Write the manifest information to a JSON file."""
    with open(manifest_file, 'w') as mf:
        json.dump({"manifests": manifest}, mf, indent=4)

def compress_manifest(manifest_file):
    """Compress the manifest file using gzip."""
    gz_manifest_file = f"{manifest_file}.gz"
    with open(manifest_file, 'rb') as mf:
        with gzip.open(gz_manifest_file, 'wb') as gz_mf:
            gz_mf.writelines(mf)
    return gz_manifest_file

def upload_to_s3(file_path, bucket, prefix, content_type=None, content_encoding=None):
    """Upload a file to an S3 bucket with specified metadata."""
    s3_client = boto3.client('s3')
    try:
        file_name = os.path.basename(file_path)
        object_path = f"{prefix}/{file_name}"
        extra_args = {}

        if content_type:
            extra_args['ContentType'] = content_type
        if content_encoding:
            extra_args['ContentEncoding'] = content_encoding

        s3_client.upload_file(file_path, bucket, object_path, ExtraArgs=extra_args)
        print(f"Uploaded {file_path} to s3://{bucket}/{object_path}")
    except NoCredentialsError:
        print("Credentials not available")

def main():
    parser = argparse.ArgumentParser(description="Merge files and upload to S3 with manifest creation")
    parser.add_argument("--directory", help="Directory to search for files")
    parser.add_argument("--patterns", nargs='+', help="File patterns to match, e.g., '*.txt' '*.log'")
    parser.add_argument("--output_file", help="Output merged file path")
    parser.add_argument("--manifest_file", help="Output manifest file path")
    parser.add_argument("--s3_bucket_name", help="S3 bucket name for upload")
    parser.add_argument("--s3_object_prefix", help="S3 object prefix for upload")

    args = parser.parse_args()

    # Convert shell-style patterns to regex
    regex_patterns = [re.sub(r'\*', '.*', pattern) for pattern in args.patterns]

    files = find_files(args.directory, regex_patterns)

    output_file_path = f"{args.directory}/{args.output_file}"
    manifest_file_path = f"{args.directory}/{args.manifest_file}"

    if files:
        manifest = merge_files(files, output_file_path)
        write_manifest(manifest, manifest_file_path)
        gz_manifest_file_path = compress_manifest(manifest_file_path)

        # Upload gzipped manifest file to S3
        upload_to_s3(gz_manifest_file_path, args.s3_bucket_name, args.s3_object_prefix, content_type="application/json", content_encoding="gzip")

        # Delete the gzipped manifest file locally
        os.remove(gz_manifest_file_path)

        # Delete the binary merged file locally
        os.remove(output_file_path)

        print(f'Merging completed. {len(files)} files merged into {args.output_file}.')
        print(f'Manifest file created: {args.manifest_file}')
    else:
        print(f'No files matching patterns {args.patterns} found in {args.directory}.')

if __name__ == '__main__':
    main()

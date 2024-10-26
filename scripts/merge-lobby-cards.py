import os
import json
import re
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

def merge_files(files, output_file):
    """Merge all files into one large file and return manifest info."""
    manifest = []
    current_position = 0

    with open(output_file, 'wb') as merged:
        for file in files:
            file_size = os.path.getsize(file)
            # Write file to merged file
            with open(file, 'rb') as f:
                merged.write(f.read())
            
            # Add manifest entry
            manifest.append({
                'file_name': os.path.basename(file),
                'byte_start': current_position,
                'byte_length': file_size
            })
            
            current_position += file_size
    
    return manifest

def write_manifest(manifest, manifest_file):
    """Write the manifest information to a JSON file."""
    with open(manifest_file, 'w') as mf:
        json.dump(manifest, mf, indent=4)

def upload_to_s3(file_path, bucket, prefix):
    """Upload a file to an S3 bucket."""
    s3_client = boto3.client('s3')
    try:
        file_name = os.path.basename(file_path)
        object_path = f"{prefix}/{file_name}"
        s3_client.upload_file(file_path, bucket, object_path)
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
        upload_to_s3(output_file_path, args.s3_bucket_name, args.s3_object_prefix)
        upload_to_s3(manifest_file_path, args.s3_bucket_name, args.s3_object_prefix)
        print(f'Merging completed. {len(files)} files merged into {args.output_file}.')
        print(f'Manifest file created: {args.manifest_file}')
    else:
        print(f'No files matching patterns {args.patterns} found in {args.directory}.')

if __name__ == '__main__':
    main()

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
    manifests = []
    current_position = 0

    manifest_bundles = {}

    with open(output_file, 'wb') as merged:
        for file in files:
            file_name = os.path.basename(file)
            game_name, variant, experiment, is_manifest = extract_game_info(file_name)

            file_type = 'manifest' if is_manifest else 'bundle'

            file_size = os.path.getsize(file)
            # Write file to merged file
            with open(file, 'rb') as f:
                merged.write(f.read()) 

            bundle_name = f"{game_name}{variant}"
            bundle_version = f"{experiment}"

            # search bundles for the current bundle
            if bundle_name in manifest_bundles:
                manifest_bundle = manifest_bundles[bundle_name]
            else:
                manifest_bundle = {}
                manifest_bundles[bundle_name] = manifest_bundle
                manifests.append(manifest_bundle)

            manifest_payload = {
                'byte_start': current_position,
                'byte_length': file_size,
                'file_name': file_name,
                'file_type': file_type,
                'bundle_name': f"{game_name}{variant}",
                'bundle_version': f"{experiment}"
            }

            if is_manifest:
                manifest_bundle['manifest'] = manifest_payload
            else:
                manifest_bundle['bundle'] = manifest_payload

            current_position += file_size

    return manifests

def write_manifest(manifest, manifest_file):
    """Write the manifest information to a JSON file."""
    with open(manifest_file, 'w') as mf:
        json.dump({"manifests": manifest}, mf, indent=4, sort_keys=True)

def compress_file(uncompressed_file):
    """Compress the file using gzip."""
    gz_file = f"{uncompressed_file}.gz"
    with open(uncompressed_file, 'rb') as mf:
        with gzip.open(gz_file, 'wb') as gz_mf:
            gz_mf.writelines(mf)
    return gz_file

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
    except Exception as e:
        print(f"An error occurred: {e}")

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
        # Merge files into a single output file
        manifest = merge_files(files, output_file_path)
        print(f'Merging completed. {len(files)} files merged into {args.output_file}.')

        # sort the manifest by the bundle name
        manifest = sorted(manifest, key=lambda x: x['manifest']['bundle_name'])

        # Write manifest to file
        write_manifest(manifest, manifest_file_path)
        print(f'Manifest file created: {args.manifest_file}')

        # Compress manifest file and upload
        gz_manifest_file_path = compress_file(manifest_file_path)
        upload_to_s3(gz_manifest_file_path, args.s3_bucket_name, args.s3_object_prefix, content_type="application/json", content_encoding="gzip")
        
        # Upload the uncompressed .bin file
        upload_to_s3(output_file_path, args.s3_bucket_name, args.s3_object_prefix, content_type="application/octet-stream")
        
        # Delete local files to clean up
        os.remove(gz_manifest_file_path)
        os.remove(output_file_path)

    else:
        print(f'No files matching patterns {args.patterns} found in {args.directory}.')

if __name__ == '__main__':
    main()

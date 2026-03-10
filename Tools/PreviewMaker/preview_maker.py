#!/usr/bin/env python3


import sys
import os
import cv2
import imageio
import argparse


def process_video(video_path, parts, frames_per_part, fps, width):
    if not os.path.isfile(video_path):
        print(f"  [!] File '{video_path}' not found.")
        return

    cap = cv2.VideoCapture(video_path)
    total_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
    source_fps = cap.get(cv2.CAP_PROP_FPS)

    if total_frames == 0 or source_fps == 0:
        print(f"  [!] Could not read frames or FPS from '{video_path}'.")
        return

    frame_stride = max(1, round(source_fps / fps))

    chunk_size = total_frames // parts
    extracted_frames = []

    for i in range(parts):
        chunk_start = i * chunk_size
        chunk_center = chunk_start + (chunk_size // 2)

        span_in_source = frames_per_part * frame_stride
        
        start_frame = max(chunk_start, int(chunk_center - (span_in_source / 2)))

        for j in range(frames_per_part):
            frame_idx = start_frame + (j * frame_stride)
            
            if frame_idx >= total_frames:
                break

            cap.set(cv2.CAP_PROP_POS_FRAMES, frame_idx)
            ret, frame = cap.read()
            if not ret:
                break
            
            h, w, _ = frame.shape
            new_h = int(h * (width / w))
            frame_resized = cv2.resize(frame, (width, new_h))

            frame_rgb = cv2.cvtColor(frame_resized, cv2.COLOR_BGR2RGB)
            extracted_frames.append(frame_rgb)

    cap.release()

    base_name = os.path.splitext(video_path)[0]
    out_name = f"{base_name}.gif"

    imageio.mimsave(out_name, extracted_frames, fps=fps, loop=0)
    print(f"  --> Successfully saved to {out_name}")


def main():
    parser = argparse.ArgumentParser(description="Convert MP4 to a preview GIF by sampling different parts.")
    parser.add_argument('-p', '--parts', type=int, default=4, help="Number of parts to split the video into (default: 4).")
    parser.add_argument('-f', '--frames', type=int, default=5, help="Number of frames to extract per part (default: 5).")
    parser.add_argument('-F', '--fps', type=int, default=10, help="Frames per second for the output GIF (default: 10).")
    parser.add_argument('-w', '--width', type=int, default=480, help="Width of the output GIF to control file size (default: 480).")
    parser.add_argument('files', nargs='*', help="Video files to process")

    args = parser.parse_args()
    video_files = args.files

    if not sys.stdin.isatty():
        piped_input = sys.stdin.read().splitlines()
        video_files.extend([f.strip() for f in piped_input if f.strip()])

    if not video_files:
        print(f"{parser.format_usage()}Try '{parser.prog} --help' for more information.")
        sys.exit(1)

    for vf in video_files:
        print(f"Processing {vf}")
        process_video(vf, args.parts, args.frames, args.fps, args.width)


if __name__ == "__main__":
    main()
import os
import subprocess

import cv2
import imageio
from database import SessionLocal
from models import CapstoneModel
from PIL import Image, ImageOps


def process_image_to_jpeg(file_path: str, target_width: int = 720) -> str:
    if not file_path or not os.path.exists(file_path):
        return file_path

    try:
        with Image.open(file_path) as img:
            img = ImageOps.exif_transpose(img)

            w, h = img.size
            new_h = int(h * (target_width / w))

            img = img.resize((target_width, new_h), Image.Resampling.LANCZOS)

            if img.mode in ("RGBA", "LA") or (
                img.mode == "P" and "transparency" in img.info
            ):
                background = Image.new("RGB", img.size, (255, 255, 255))
                background.paste(img, mask=img.convert("RGBA").split()[3])
                img = background
            else:
                img = img.convert("RGB")

            base_name = os.path.splitext(file_path)[0]
            new_path = f"{base_name}.jpg"

            img.save(new_path, "JPEG", quality=85)

        if file_path != new_path and os.path.exists(file_path):
            os.remove(file_path)

        return new_path

    except Exception as e:
        print(f"  [!] Failed to process image: {e}")
        return file_path


def create_video_preview(
    video_path: str,
    parts: int = 4,
    frames_per_part: int = 5,
    fps: int = 10,
    width: int = 480,
) -> str:
    if not os.path.isfile(video_path):
        print(f"  [!] File '{video_path}' not found.")
        return None

    cap = cv2.VideoCapture(video_path)
    total_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
    source_fps = cap.get(cv2.CAP_PROP_FPS)

    if total_frames == 0 or source_fps == 0:
        print(f"  [!] Could not read frames or FPS from '{video_path}'.")
        cap.release()
        return None

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

            if new_h % 2 != 0:
                new_h += 1

            frame_resized = cv2.resize(frame, (width, new_h))
            frame_rgb = cv2.cvtColor(frame_resized, cv2.COLOR_BGR2RGB)
            extracted_frames.append(frame_rgb)

    cap.release()

    if not extracted_frames:
        return None

    base_name = os.path.splitext(video_path)[0]
    out_name = f"{base_name}_preview.webm"

    try:
        imageio.mimsave(
            out_name, extracted_frames, fps=fps, codec="libvpx", macro_block_size=2
        )
        return out_name
    except Exception as e:
        print(f"  [!] Failed to process preview: {e}")
        return None


def process_full_video_to_webm(file_path: str, target_width: int = 1280) -> str:
    if not file_path or not os.path.exists(file_path):
        return file_path

    base_name = os.path.splitext(file_path)[0]
    new_path = f"{base_name}_full.webm"

    cmd = [
        "ffmpeg",
        "-y",
        "-i",
        file_path,
        "-vf",
        f"scale={target_width}:-2",
        "-c:v",
        "libvpx",
        "-crf",
        "30",
        "-b:v",
        "1M",
        "-c:a",
        "libvorbis",
        new_path,
    ]

    try:
        subprocess.run(
            cmd, check=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL
        )
        if os.path.exists(new_path):
            return new_path
    except subprocess.CalledProcessError as e:
        print(f"  [!] Failed to process full video: {e}")

    return file_path


def background_media_pipeline(
    capstone_id: int, process_poster: bool, process_video: bool
):
    print(f"[START] Media pipeline for Capstone {capstone_id}")
    db = SessionLocal()
    db_caps = db.query(CapstoneModel).filter(CapstoneModel.id == capstone_id).first()

    if not db_caps:
        db.close()
        return

    try:
        if process_poster and db_caps.poster:
            db_caps.poster = process_image_to_jpeg(db_caps.poster, target_width=800)

        if process_video and db_caps.video:
            original_video = db_caps.video

            preview_path = create_video_preview(original_video)
            if preview_path:
                db_caps.preview = preview_path
            full_path = process_full_video_to_webm(original_video, target_width=720)
            if full_path and full_path != original_video:
                db_caps.video = full_path
                if os.path.exists(original_video):
                    os.remove(original_video)

        db.commit()
        db.refresh(db_caps)
        print(f"[DONE] Media pipline for Capstone {capstone_id}")

    except Exception as e:
        print(f"[ERROR] Pipeline failed: {e}")
        db.rollback()

    finally:
        db.close()

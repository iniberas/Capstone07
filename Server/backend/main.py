import os

from database import Base, engine, get_db
from fastapi import (
    Depends,
    FastAPI,
    File,
    Form,
    HTTPException,
    UploadFile,
)
from media_utils import background_media_pipeline
from models import CapstoneModel, LikeLogModel
from pydantic import BaseModel
from sqlalchemy.orm import Session

Base.metadata.create_all(bind=engine)

app = FastAPI(title="Capstone Hub Dashboard")
os.makedirs("assets", exist_ok=True)


class LikeRequest(BaseModel):
    guest_id: str


@app.get("/capstones")
def get_all_objects(db: Session = Depends(get_db)):
    objects = db.query(CapstoneModel).all()
    return {"status": "success", "data": objects}


@app.get("/capstones/{capstone_id}")
def get_object(capstone_id: int, db: Session = Depends(get_db)):
    db_caps = db.query(CapstoneModel).filter(CapstoneModel.id == capstone_id).first()
    if not db_caps:
        raise HTTPException(status_code=404, detail="Object not found")
    return {"status": "success", "data": db_caps}


@app.put("/capstones/{capstone_id}")
async def update_object(
    capstone_id: int,
    title: str = Form(None),
    desc: str = Form(None),
    poster: UploadFile = File(None),
    video: UploadFile = File(None),
    db: Session = Depends(get_db),
):
    db_caps = db.query(CapstoneModel).filter(CapstoneModel.id == capstone_id).first()
    if not db_caps:
        db_caps = CapstoneModel(id=capstone_id)
        db.add(db_caps)

    if title:
        db_caps.title = title
    if desc:
        db_caps.desc = desc

    def remove_old_file(file_path: str):
        if file_path and os.path.exists(file_path):
            try:
                os.remove(file_path)
            except Exception:
                pass

    async def save_file(file: UploadFile, file_type: str, old_path: str) -> str:
        remove_old_file(old_path)
        
        safe_filename = file.filename.replace(" ", "_")
        file_location = f"assets/{file_type}_{capstone_id}_{safe_filename}"
        
        with open(file_location, "wb+") as file_object:
            file_object.write(await file.read())
        return file_location

    if poster:
        db_caps.poster = await save_file(poster, "poster", db_caps.poster)
    if video:
        remove_old_file(db_caps.preview)
        remove_old_file(db_caps.video)
        db_caps.video = await save_file(video, "video", db_caps.video)
        db_caps.preview = None

    db.commit()
    db.refresh(db_caps)

    background_media_pipeline.delay(
        capstone_id=db_caps.id,
        process_poster=bool(poster),
        process_video=bool(video),
    )

    return {
        "status": "success",
        "message": f"Object {capstone_id} updated. Background tasks added to queue.",
        "data": {
            "title": db_caps.title,
            "poster_path": db_caps.poster,
            "video_path": db_caps.video,
        },
    }


@app.post("/capstones/{capstone_id}/like")
def toggle_like_object(
    capstone_id: int, request_data: LikeRequest, db: Session = Depends(get_db)
):
    db_caps = db.query(CapstoneModel).filter(CapstoneModel.id == capstone_id).first()
    if not db_caps:
        raise HTTPException(status_code=404, detail="Object not found")

    guest_id = request_data.guest_id
    existing_like = (
        db.query(LikeLogModel)
        .filter(
            LikeLogModel.capstone_id == capstone_id, LikeLogModel.guest_id == guest_id
        )
        .first()
    )

    if existing_like:
        db.delete(existing_like)
        if db_caps.total_likes > 0:
            db_caps.total_likes -= 1
        action = "unliked"
    else:
        new_like_log = LikeLogModel(capstone_id=capstone_id, guest_id=guest_id)
        db.add(new_like_log)
        db_caps.total_likes += 1
        action = "liked"

    db.commit()
    db.refresh(db_caps)

    return {
        "status": "success",
        "action": action,
        "data": {"capstone_id": capstone_id, "total_likes": db_caps.total_likes},
    }
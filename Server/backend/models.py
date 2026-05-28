from database import Base
from sqlalchemy import Column, ForeignKey, Integer, String, Text


class CapstoneModel(Base):
    __tablename__ = "capstones"

    id = Column(Integer, primary_key=True, index=True)
    title = Column(String, nullable=True)
    desc = Column(Text, nullable=True)
    poster = Column(String, nullable=True)
    preview = Column(String, nullable=True)
    video = Column(String, nullable=True)
    total_likes = Column(Integer, default=0)


class LikeLogModel(Base):
    __tablename__ = "likes_log"

    id = Column(Integer, primary_key=True, index=True)
    capstone_id = Column(Integer, ForeignKey("capstones.id"))
    guest_id = Column(String, index=True)

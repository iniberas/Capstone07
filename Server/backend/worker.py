import os

from celery import Celery

redis_url = os.getenv("CELERY_BROKER_URL", "redis://redis:6379/0")

app = Celery(
    "worker",
    broker=redis_url,
    backend=redis_url,
    include=['media_utils']
)

app.conf.worker_concurrency = 1
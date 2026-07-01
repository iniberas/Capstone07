# CapstoneHub

A virtual exhibition designed to showcase student capstone projects. It features an admin dashboard for content management and an interactive like system for visitors.

## Features

- **3D/WebGL Exhibition**: Visitors can explore the virtual exhibition space and view the showcased projects.
- **Admin Dashboard**: Upload and edit posters, videos, titles, and descriptions for each project.
- **Like System**: Visitors can like their favorite projects (guest-based, toggleable like/unlike).
- **Background Media Processing**: Uploaded videos and posters are processed asynchronously (e.g., generating previews) without degrading API performance.

## Architecture

```text
Capstone07/
├── Assets/, Packages/, ProjectSettings/  -> Unity Project (virtual exhibition client)
└── Server/                               -> Backend & infrastructure
    ├── app/                              -> Served Unity WebGL build
    ├── admin/                            -> Admin dashboard
    ├── backend/                          -> REST API (FastAPI)
    ├── assets/                           -> Media storage (posters, videos, previews)
    ├── db/                               -> SQLite database
    ├── nginx/                            -> Reverse proxy + basic auth for dashboard
    └── docker-compose.yaml
```

### Workflow Overview

1. The admin logs into the dashboard (secured by basic auth via Nginx, accessible at `/admin/`) and uploads project posters/videos.
2. The FastAPI backend receives the request, saves the files, and dispatches media processing tasks to the Celery worker via Redis.
3. The background worker processes the media (generating previews, etc.) using OpenCV/ffmpeg.
4. The Unity (WebGL) client fetches the data from the API and displays it in the virtual exhibition, including the interaction states for the like system.

## Tech Stack

| Layer | Technology |
| --- | --- |
| Game Client | Unity (C#, ShaderLab, HLSL) |
| Backend API | Python, FastAPI |
| ORM / Database | SQLAlchemy |
| Task Queue | Celery |
| Message Broker | Redis |
| Media Processing | OpenCV, Pillow, ImageIO/ffmpeg |
| Web Server / Proxy | Nginx |
| Deployment | Docker Compose |

## Running the Server (Development)

1. Navigate to the `Server/` directory.
3. Copy `.env.example` to `.env` and configure the environment variables:
	```env
	DASHBOARD_USERNAME=admin
	DASHBOARD_PASSWORD=password
	DASHBOARD_PORT=8080
	```
3. Start all services using Docker Compose:
	```bash
	docker compose up --build
	```
4. The following services will be initialized:
	* `redis` — Message broker
	* `api` — FastAPI backend
	* `worker` — Celery worker for media processing
	* `asset_server` — Nginx serving the WebGL build, admin dashboard, and static assets
5. Access the admin dashboard at `http://localhost:${DASHBOARD_PORT}/admin/` and login using the basic auth credentials defined in your `.env` file.
6. Access the WebGL exhibition at `http://localhost:${DASHBOARD_PORT}/`.

## API Endpoints

All backend endpoints are accessed through Nginx with the `/api/` prefix (proxied to the `api` service).

| Method | Endpoint | Description |
| --- | --- | --- |
| GET | /api/capstones | Retrieve all project data |
| GET | /api/capstones/{id} | Retrieve details for a specific project |
| PUT | /api/capstones/{id} | Create or update a project (upload poster & video) |
| POST | /api/capstones/{id}/like | Toggle like/unlike for a project by a guest |

## Running the Unity Client

1. Open this project folder using Unity Hub (ensure you are using the Unity Editor version specified in [ProjectVersion.txt](https://github.com/iniberas/Capstone07/blob/main/ProjectSettings/ProjectVersion.txt)).
2. Open the main scene and press **Play** to test the client within the Unity Editor.
3. To build for WebGL, go to **File** -> **Build Settings** -> **WebGL** -> **Build**. Output the build files to the `Server/app/` directory so they can be served by Nginx.

## License

This project is licensed under the GNU General Public License v3.0 (GPL-3.0). See the [LICENSE](https://github.com/iniberas/Capstone07/blob/main/LICENSE) file for details.

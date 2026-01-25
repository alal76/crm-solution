# Docker Desktop Installation via Chocolatey

## Quick Start (Recommended - Manual Steps)

### Step 1: Open PowerShell as Administrator

1. Press `Win + X` on your keyboard
2. Select "Windows PowerShell (Admin)" or "Terminal (Admin)"
3. If prompted for User Account Control, click "Yes"

### Step 2: Run the Installation Command

Copy and paste this command into the administrator PowerShell window:

```powershell
choco install docker-desktop -y
```

Wait for the installation to complete (5-15 minutes depending on your internet speed).

### Step 3: Restart Your Computer

After installation finishes, restart your computer to complete the Docker setup.

### Step 4: Verify Installation

Open a new PowerShell window (regular, not admin) and run:

```powershell
docker --version
docker run hello-world
```

You should see output like:
```
Docker version 27.0.0, build xxxxx
...
Hello from Docker!
...
```

---

## What Gets Installed

- **Docker Desktop**: Full Docker environment for Windows
- **Docker Engine**: The core container runtime
- **Docker CLI**: Command-line interface
- **Docker Compose**: Multi-container orchestration tool

---

## After Installation

Once Docker is installed and running, you'll need:

### 1. Ensure WSL 2 is Enabled (Windows 10/11)
Docker Desktop automatically uses WSL 2 backend on Windows 10/11.

### 2. Start Docker Desktop
- Docker Desktop should start automatically after restart
- Look for the Docker whale icon in your system tray (bottom right)
- Wait until it shows "Docker is running"

### 3. Verify it's Running
```powershell
docker ps
```

Should show an empty list or running containers (not an error).

---

## Next Steps: Build and Push Images

Once Docker is confirmed running, execute in your CRM project directory:

### Build Backend Image
```powershell
cd "c:\Users\AbhishekLal\OneDrive - HSO\Documents\Work\Vibe\CRM"
docker build -f Dockerfile.backend -t ghcr.io/alal76/crm-api:latest -t ghcr.io/alal76/crm-api:v1.0.0 .
```

### Build Frontend Image
```powershell
docker build -f Dockerfile.frontend -t ghcr.io/alal76/crm-frontend:latest -t ghcr.io/alal76/crm-frontend:v1.0.0 .
```

### Verify Images Built
```powershell
docker images | Select-String "crm-api|crm-frontend"
```

---

## Troubleshooting

### Docker Daemon Not Running
- Check system tray for Docker whale icon
- If not present, manually start Docker Desktop from Start menu
- Wait 30 seconds for it to fully initialize

### "Cannot connect to Docker daemon" Error
1. Ensure Docker Desktop is running (check tray icon)
2. Restart Docker Desktop:
   - Right-click whale icon → Quit Docker Desktop
   - Wait 5 seconds
   - Start Docker Desktop again from Start menu

### Insufficient Disk Space
- Docker needs ~5-10GB free space for images
- Check your C: drive has enough space

### WSL 2 Issues (Windows 10)
- Install WSL 2 subsystem if prompted
- Docker Desktop will guide you through this automatically

### Still Having Issues?
- Restart your computer
- Uninstall and reinstall: `choco uninstall docker-desktop -y` then reinstall
- Check Docker Desktop logs: Help → Troubleshoot in Docker Desktop UI

---

## Once Docker is Ready

I can help you:
1. ✅ Build both Docker images (backend and frontend)
2. ✅ Push images to GitHub Container Registry
3. ✅ Verify images in your registry
4. ✅ Deploy to Kubernetes using the deployment script

Just let me know once Docker is installed and running!

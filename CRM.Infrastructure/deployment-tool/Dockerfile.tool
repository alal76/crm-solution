# ============================================================
# CRM Deployment Tool (CDT) — Self-Contained Docker Image
# Version: 0.611.0
# ============================================================
# Build:
#   docker build -f Dockerfile.tool -t crm-cdt:latest .
#
# Run (persistent data in a named volume):
#   docker run -d --name crm-cdt \
#     -p 5050:5050 \
#     -v /var/run/docker.sock:/var/run/docker.sock \
#     -v $HOME/.kube:/root/.kube:ro \
#     -v $HOME/.ssh:/root/.ssh:ro \
#     -v crm-cdt-data:/app/data \
#     crm-cdt:latest
#
# All CDT data (profiles, history, secrets, generated files) is stored in
# the /app/data volume. The Dockerfile symlinks /root/.crm-cdt → /app/data
# so all internal code paths automatically land in the persistent volume.
#
# Or use: docker-compose -f docker-compose.tool.yml up
# ============================================================

FROM python:3.12-slim

LABEL maintainer="CRM DevOps"
LABEL description="CRM Deployment Tool — unified discover/configure/deploy wizard"
LABEL version="0.611.0"

# ---- system dependencies + Docker CLI ----
RUN apt-get update -qq && apt-get install -y --no-install-recommends \
    ca-certificates curl git gnupg lsb-release openssh-client \
    && install -m 0755 -d /etc/apt/keyrings \
    && curl -fsSL https://download.docker.com/linux/debian/gpg -o /etc/apt/keyrings/docker.asc \
    && chmod a+r /etc/apt/keyrings/docker.asc \
    && echo \
       "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] \
        https://download.docker.com/linux/debian \
        $(. /etc/os-release && echo "$VERSION_CODENAME") stable" \
       > /etc/apt/sources.list.d/docker.list \
    && apt-get update -qq \
    && apt-get install -y --no-install-recommends docker-ce-cli docker-compose-plugin \
    && rm -rf /var/lib/apt/lists/*

# ---- kubectl ----
ARG KUBECTL_VERSION=v1.31.4
RUN ARCH=$(uname -m) && \
    [ "$ARCH" = "x86_64" ] && KARCH="amd64" || KARCH="arm64" ; \
    curl -sLf "https://dl.k8s.io/release/${KUBECTL_VERSION}/bin/linux/${KARCH}/kubectl" \
         -o /usr/local/bin/kubectl && \
    chmod +x /usr/local/bin/kubectl

# ---- helm ----
ARG HELM_VERSION=v3.17.0
RUN ARCH=$(uname -m) && \
    [ "$ARCH" = "x86_64" ] && HARCH="amd64" || HARCH="arm64" ; \
    curl -sLf "https://get.helm.sh/helm-${HELM_VERSION}-linux-${HARCH}.tar.gz" \
         -o /tmp/helm.tar.gz && \
    tar -xzf /tmp/helm.tar.gz -C /tmp && \
    mv /tmp/linux-"${HARCH}"/helm /usr/local/bin/helm && \
    chmod +x /usr/local/bin/helm && \
    rm -rf /tmp/helm* /tmp/linux-*

# ---- Python app ----
WORKDIR /app

# Install Python dependencies first (layer caching)
COPY requirements.txt .
RUN pip install --no-cache-dir --upgrade pip \
    && pip install --no-cache-dir -r requirements.txt \
    && pip install --no-cache-dir \
       azure-identity \
       azure-mgmt-compute \
       azure-mgmt-containerinstance \
       boto3 \
       google-cloud-compute \
       google-cloud-container

# Copy application source
COPY . .

# Ensure /app/data exists and symlink ~/.crm-cdt → /app/data so that all
# code paths (ProfileManager, VaultManager, RunHistoryManager) that write to
# Path.home() / ".crm-cdt" automatically land inside the mounted volume.
RUN mkdir -p /app/data/profiles \
             /app/data/configs \
             /app/data/snapshots \
             /app/data/generated \
             /app/data/secrets \
             /app/logs \
    && ln -sf /app/data /root/.crm-cdt

# Expose wizard port
EXPOSE 5050

# Health-check
HEALTHCHECK --interval=20s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:5050/health || exit 1

# Default: start the wizard (no browser-open in Docker)
ENV CDT_NO_BROWSER=true \
    CDT_PORT=5050 \
    PYTHONUNBUFFERED=1

ENTRYPOINT ["python", "gui/app.py"]
CMD ["--port", "5050", "--headless"]

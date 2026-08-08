# REV-STUB-008: crm-python-script-runner Docker image
# Single-stage build: standard-library-only server, nothing to compile.

FROM python:3.11-slim
WORKDIR /app

# No third-party runtime dependencies (see requirements.txt) — nothing to
# `pip install` for the production image, which keeps the image small and
# avoids adding a package-supply-chain attack surface to a code-execution
# component.
COPY server.py sandbox_runner.py denylist.py ./

EXPOSE 4001

# Run as non-root for security (mirrors crm-script-runner's Dockerfile).
RUN useradd --create-home --uid 10001 runner
USER runner

HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
    CMD python3 -c "import urllib.request,sys; sys.exit(0 if urllib.request.urlopen('http://localhost:4001/health', timeout=5).status == 200 else 1)"

CMD ["python3", "server.py"]

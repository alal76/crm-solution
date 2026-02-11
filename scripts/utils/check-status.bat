@echo off
setlocal enabledelayedexpansion

echo Checking Docker status on server...
echo.

set SSH_KEY=%USERPROFILE%\.ssh\crm-deploy-key
set SSH_HOST=root@192.168.0.9

REM WARNING: accept-new accepts on first connect but rejects if the host key changes (MITM protection).
REM Use known_hosts in production environments.
echo Checking Docker images:
ssh -i "%SSH_KEY%" -p 22 -o ConnectTimeout=30 -o StrictHostKeyChecking=accept-new %SSH_HOST% "docker images"

echo.
echo Checking all containers:
ssh -i "%SSH_KEY%" -p 22 -o ConnectTimeout=30 -o StrictHostKeyChecking=accept-new %SSH_HOST% "docker ps -a"

echo.
echo Checking docker compose status:
ssh -i "%SSH_KEY%" -p 22 -o ConnectTimeout=30 -o StrictHostKeyChecking=accept-new %SSH_HOST% "cd /opt/crm && docker compose ps -a 2>&1 || echo 'compose command failed'"

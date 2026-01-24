# SSH Key & Authentication Setup Complete

## ✅ SSH Key Created Successfully

Your SSH key pair has been created for secure passwordless authentication to your remote server.

### Key Locations
- **Private Key:** `C:\Users\AbhishekLal\.ssh\crm-deploy-key`
- **Public Key:** `C:\Users\AbhishekLal\.ssh\crm-deploy-key.pub`

### Key Specifications
- **Algorithm:** RSA
- **Key Size:** 4096 bits
- **Comment:** crm-deployment
- **Status:** ✅ Added to remote server (192.168.0.9)

---

## 🔐 Authentication Methods

### 1. SSH Key Authentication (Remote Server)
**Status:** ✅ CONFIGURED

The SSH public key has been successfully added to your remote server at `192.168.0.9`.

**Connect without password:**
```powershell
ssh -i "C:\Users\AbhishekLal\.ssh\crm-deploy-key" -p 22 root@192.168.0.9
```

**In deployment scripts:**
```powershell
ssh -i "C:\Users\AbhishekLal\.ssh\crm-deploy-key" -p 22 root@192.168.0.9 "command"
```

### 2. GitHub Container Registry (GHCR) Authentication
**Status:** ⏳ REQUIRES PERSONAL ACCESS TOKEN

GHCR uses **Personal Access Tokens (PAT)**, NOT SSH keys.

#### Step 1: Create GitHub Personal Access Token

1. Visit: https://github.com/settings/tokens
2. Click "Generate new token (classic)"
3. Fill in token name: `crm-deployment`
4. Set expiration: 90 days (or your preference)
5. Select scopes:
   - ✅ `read:packages` (required for pulling images)
   - ✅ `write:packages` (if you want to push images)
6. Click "Generate token"
7. **Copy the token immediately** (you won't be able to see it again!)

#### Step 2: Use Token for GHCR Login

```powershell
# Interactive login
docker login ghcr.io
# Username: alal76
# Password: <your-personal-access-token>

# Or scripted login
docker login ghcr.io -u alal76 -p "your-token-here"
```

#### Step 3: Push/Pull Images

```powershell
# Push images to GHCR
docker push ghcr.io/alal76/crm-api:development
docker push ghcr.io/alal76/crm-frontend:development

# Pull images from GHCR (on remote server)
docker pull ghcr.io/alal76/crm-api:development
docker pull ghcr.io/alal76/crm-frontend:development
```

---

## 📋 Quick Reference

### SSH Commands

```powershell
# Connect with SSH key (passwordless)
ssh -i "C:\Users\AbhishekLal\.ssh\crm-deploy-key" root@192.168.0.9

# Copy files to remote with SSH key
scp -i "C:\Users\AbhishekLal\.ssh\crm-deploy-key" -P 22 docker-compose.yml root@192.168.0.9:/opt/crm/

# Run command on remote with SSH key
ssh -i "C:\Users\AbhishekLal\.ssh\crm-deploy-key" root@192.168.0.9 "docker ps"
```

### Docker Registry Commands

```powershell
# Login to GHCR
docker login ghcr.io -u alal76 -p "YOUR_GITHUB_TOKEN"

# Build and tag images
docker build -f Dockerfile.backend -t ghcr.io/alal76/crm-api:development .
docker build -f Dockerfile.frontend -t ghcr.io/alal76/crm-frontend:development .

# Push to registry
docker push ghcr.io/alal76/crm-api:development
docker push ghcr.io/alal76/crm-frontend:development
```

---

## 🚀 Next Steps

### 1. Create GitHub Personal Access Token
- Go to: https://github.com/settings/tokens
- Create new token with `read:packages` scope
- Copy and save the token securely

### 2. Deploy Your Application
```powershell
# Run the deployment script
.\deploy-now.ps1
```

### 3. Monitor Deployment
```powershell
# SSH into remote server
ssh -i "C:\Users\AbhishekLal\.ssh\crm-deploy-key" root@192.168.0.9

# Check container status
docker-compose ps

# View logs
docker-compose logs -f
```

---

## 🔒 Security Best Practices

### SSH Key Security
✅ **DO:**
- Keep private key secure on your computer
- Use SSH key instead of password (already done!)
- Restrict file permissions: `chmod 600 ~/.ssh/crm-deploy-key`
- Backup your private key securely

❌ **DON'T:**
- Share your private key with anyone
- Commit private key to version control
- Use weak passphrases
- Store in publicly accessible locations

### GitHub Token Security
✅ **DO:**
- Create tokens with minimal required scopes
- Set expiration dates (e.g., 90 days)
- Store in secure password manager
- Rotate tokens regularly
- Use different tokens for different purposes

❌ **DON'T:**
- Share tokens with anyone
- Commit tokens to version control
- Use tokens as passwords
- Keep tokens in plain text files
- Use overly broad scopes

---

## 📁 File Locations

| File | Location | Purpose |
|------|----------|---------|
| Private Key | `C:\Users\AbhishekLal\.ssh\crm-deploy-key` | SSH authentication |
| Public Key | `C:\Users\AbhishekLal\.ssh\crm-deploy-key.pub` | Added to server |
| Config | `.docker-remote-config.json` | Deployment settings |
| Deploy Script | `deploy-now.ps1` | Deployment automation |

---

## ⚠️ Important Notes

### About GHCR
- GHCR uses **Personal Access Tokens**, not SSH keys
- Tokens can be regenerated anytime from GitHub settings
- Tokens expire based on settings (set during creation)
- Different scopes provide different permissions

### About SSH Keys
- Your SSH key is now passwordless authentication
- Private key stays on your local machine
- Public key is on the remote server
- Very secure for server access

### Migration from Password Auth
Previous authentication method:
- Username: `root`
- Password-based SSH (less secure)

Current authentication method:
- SSH Key-based authentication (more secure)
- Passwordless login
- No password stored in config files

---

## 🆘 Troubleshooting

### SSH Key Not Working
```powershell
# Verify key file permissions (Windows)
icacls "C:\Users\AbhishekLal\.ssh\crm-deploy-key"

# Test SSH connection with verbose output
ssh -v -i "C:\Users\AbhishekLal\.ssh\crm-deploy-key" root@192.168.0.9
```

### GHCR Login Fails
```powershell
# Check if token is valid
docker login ghcr.io

# Logout and try again
docker logout ghcr.io
docker login ghcr.io -u alal76 -p "YOUR_NEW_TOKEN"
```

### Key Not Added to Server
```bash
# SSH with password to check/add key manually
ssh root@192.168.0.9
mkdir -p ~/.ssh
# Paste your public key content to ~/.ssh/authorized_keys
# Set correct permissions
chmod 700 ~/.ssh
chmod 600 ~/.ssh/authorized_keys
```

---

## 📞 Summary

| Component | Status | Details |
|-----------|--------|---------|
| SSH Key Pair | ✅ Created | RSA 4096-bit, stored locally |
| Public Key Added | ✅ Configured | Added to 192.168.0.9 |
| Passwordless SSH | ✅ Ready | Use `-i` flag with private key |
| GHCR Token | ⏳ Pending | Need to create on GitHub |
| Deployment Script | ✅ Ready | `.\deploy-now.ps1` |

**Next Action:** Create GitHub Personal Access Token, then run `.\deploy-now.ps1`

---

**Created:** January 19, 2026  
**SSH Key Version:** 4096-bit RSA  
**Status:** ✅ Ready for Deployment

#!/usr/bin/env python3
"""Batch 24: ITSM Knowledge Base — Extensive Technical Articles.

Creates a comprehensive library of ITSM knowledge articles across all six
article types (HowTo, Troubleshooting, FAQ, KnownError, Reference,
BestPractice) covering IT infrastructure, networking, security, application
support, cloud operations, and DevOps.

Endpoint: POST /api/itsm/knowledge
DTO: CreateKnowledgeArticleDto {Title, ArticleBody, ArticleType, ShortDescription, CategoryId, IsInternal}
ArticleType enum: HowTo=1, Troubleshooting=2, FAQ=3, KnownError=4, Reference=5, BestPractice=6
"""
from __future__ import annotations
import sys, os, time
sys.path.insert(0, os.path.dirname(__file__))
from loader_utils import ApiClient, RunLogger, ENUMS, save_ids, load_ids

# ── ArticleType enum mapping ──
HOWTO = 1
TROUBLESHOOTING = 2
FAQ = 3
KNOWN_ERROR = 4
REFERENCE = 5
BEST_PRACTICE = 6

# ---------------------------------------------------------------------------
# Article data — organised by category / article type
# ---------------------------------------------------------------------------

def _itsm_articles(ts: int) -> list:
    """Return a list of ITSM knowledge article payloads."""
    articles = []

    # ══════════════════════════════════════════════════════════════════════
    #  INFRASTRUCTURE — HowTo articles
    # ══════════════════════════════════════════════════════════════════════
    articles.extend([
        {
            "title": f"How to Configure Windows Server 2022 Active Directory Domain Services {ts}",
            "shortDescription": "Step-by-step guide for deploying AD DS on Windows Server 2022 including DNS and DHCP integration.",
            "articleBody": (
                "## Prerequisites\n"
                "- Windows Server 2022 Standard or Datacenter edition\n"
                "- Static IP address assigned to the server\n"
                "- Administrator access\n\n"
                "## Step 1: Install AD DS Role\n"
                "Open Server Manager → Add Roles and Features → Select 'Active Directory Domain Services'.\n"
                "Accept the required features and complete the wizard.\n\n"
                "## Step 2: Promote to Domain Controller\n"
                "After installation, click the notification flag in Server Manager and select "
                "'Promote this server to a domain controller'.\n\n"
                "### New Forest Configuration\n"
                "- Select 'Add a new forest'\n"
                "- Enter root domain name (e.g., corp.contoso.com)\n"
                "- Set Forest and Domain functional levels to Windows Server 2016 or higher\n"
                "- Configure DSRM password (store securely in the password vault)\n\n"
                "## Step 3: Configure DNS Integration\n"
                "AD DS requires DNS. The wizard will install DNS Server role automatically.\n"
                "Verify forward and reverse lookup zones are created.\n\n"
                "## Step 4: Configure DHCP (Optional)\n"
                "If DHCP is needed, install the DHCP Server role and configure scopes.\n"
                "Authorize the DHCP server in AD DS.\n\n"
                "## Step 5: Post-Configuration Verification\n"
                "```powershell\n"
                "Get-ADDomainController -Filter *\n"
                "Get-ADDomain\n"
                "dcdiag /v\n"
                "nslookup corp.contoso.com\n"
                "```\n\n"
                "## Step 6: Configure Group Policy Baseline\n"
                "Create a baseline GPO for password policies, account lockout, and audit policies.\n"
                "Link to the domain root OU.\n\n"
                "## Related Articles\n"
                "- KE-AD-001: Known error — AD replication failure after promotion\n"
                "- BP-SEC-003: Best practice — AD security hardening\n"
            ),
            "articleType": HOWTO,
            "isInternal": True,
        },
        {
            "title": f"How to Set Up VMware vSphere 8 ESXi Host from Bare Metal {ts}",
            "shortDescription": "Complete installation and initial configuration of VMware ESXi 8 on physical hardware.",
            "articleBody": (
                "## Prerequisites\n"
                "- Server on VMware HCL (Hardware Compatibility List)\n"
                "- ESXi 8 ISO from VMware Customer Connect\n"
                "- Bootable USB or IPMI/iLO/iDRAC for remote installation\n\n"
                "## Step 1: Boot from ESXi Installer\n"
                "Mount the ISO via IPMI virtual media or boot from USB.\n"
                "Press Enter at the welcome screen to begin installation.\n\n"
                "## Step 2: Disk Selection\n"
                "Select the installation target disk. For best performance, use a dedicated SSD/NVMe.\n"
                "**Warning:** This will erase all data on the selected disk.\n\n"
                "## Step 3: Root Password\n"
                "Set the root password (minimum 7 characters, complexity required).\n"
                "Store in the IT password vault immediately.\n\n"
                "## Step 4: Network Configuration\n"
                "After reboot, press F2 to enter DCUI:\n"
                "- Configure Management Network → IPv4 Configuration\n"
                "- Set static IP, subnet mask, default gateway\n"
                "- Configure DNS servers and hostname\n"
                "- Test management network connectivity\n\n"
                "## Step 5: Connect to vCenter\n"
                "Add the host to vCenter Server inventory.\n"
                "Apply the host profile and configure NTP.\n\n"
                "## Step 6: Storage and Networking\n"
                "- Create vSwitches for VM traffic, vMotion, iSCSI/NFS\n"
                "- Configure datastores (VMFS, NFS, or vSAN)\n"
                "- Enable jumbo frames if required (MTU 9000)\n\n"
                "## Verification Commands\n"
                "```\n"
                "esxcli system version get\n"
                "esxcli network ip interface list\n"
                "esxcli storage filesystem list\n"
                "```\n"
            ),
            "articleType": HOWTO,
            "isInternal": True,
        },
        {
            "title": f"How to Configure Linux Server Hardening (CIS Benchmark Level 1) {ts}",
            "shortDescription": "Apply CIS Benchmark Level 1 security hardening to RHEL 9 / Ubuntu 22.04 servers.",
            "articleBody": (
                "## Overview\n"
                "This guide applies Center for Internet Security (CIS) Benchmark Level 1 "
                "hardening controls to Linux servers.\n\n"
                "## 1. Filesystem Configuration\n"
                "```bash\n"
                "# Disable unused filesystems\n"
                "echo 'install cramfs /bin/true' >> /etc/modprobe.d/CIS.conf\n"
                "echo 'install squashfs /bin/true' >> /etc/modprobe.d/CIS.conf\n"
                "echo 'install udf /bin/true' >> /etc/modprobe.d/CIS.conf\n"
                "```\n\n"
                "## 2. Configure Separate Partitions\n"
                "Ensure /tmp, /var, /var/log, /var/log/audit, and /home are separate partitions.\n\n"
                "## 3. SSH Hardening\n"
                "Edit /etc/ssh/sshd_config:\n"
                "```\n"
                "Protocol 2\n"
                "PermitRootLogin no\n"
                "MaxAuthTries 4\n"
                "PermitEmptyPasswords no\n"
                "X11Forwarding no\n"
                "ClientAliveInterval 300\n"
                "ClientAliveCountMax 3\n"
                "AllowUsers sysadmin deploy\n"
                "```\n\n"
                "## 4. Firewall Configuration\n"
                "```bash\n"
                "# Enable firewalld\n"
                "systemctl enable --now firewalld\n"
                "firewall-cmd --set-default-zone=drop\n"
                "firewall-cmd --permanent --add-service=ssh\n"
                "firewall-cmd --reload\n"
                "```\n\n"
                "## 5. Audit Configuration\n"
                "Install and configure auditd for system call monitoring.\n"
                "Configure rules for privileged command execution and file changes.\n\n"
                "## 6. Password Policies\n"
                "```bash\n"
                "# /etc/security/pwquality.conf\n"
                "minlen = 14\n"
                "dcredit = -1\n"
                "ucredit = -1\n"
                "ocredit = -1\n"
                "lcredit = -1\n"
                "```\n\n"
                "## Verification\n"
                "Run the CIS-CAT assessment tool to verify compliance score.\n"
                "Target: 95%+ compliance for Level 1 Profile.\n"
            ),
            "articleType": HOWTO,
            "isInternal": True,
        },
        {
            "title": f"How to Deploy Kubernetes Cluster with kubeadm on Ubuntu 22.04 {ts}",
            "shortDescription": "Production-grade Kubernetes cluster deployment using kubeadm with Calico CNI.",
            "articleBody": (
                "## Architecture\n"
                "- 3 control-plane nodes (HA)\n"
                "- N worker nodes\n"
                "- Calico CNI for pod networking\n"
                "- MetalLB for bare-metal load balancing\n\n"
                "## Prerequisites (All Nodes)\n"
                "```bash\n"
                "# Disable swap\n"
                "swapoff -a\n"
                "sed -i '/swap/d' /etc/fstab\n\n"
                "# Load kernel modules\n"
                "cat <<EOF | tee /etc/modules-load.d/k8s.conf\n"
                "overlay\n"
                "br_netfilter\n"
                "EOF\n"
                "modprobe overlay && modprobe br_netfilter\n\n"
                "# Sysctl params\n"
                "cat <<EOF | tee /etc/sysctl.d/k8s.conf\n"
                "net.bridge.bridge-nf-call-iptables  = 1\n"
                "net.bridge.bridge-nf-call-ip6tables = 1\n"
                "net.ipv4.ip_forward                 = 1\n"
                "EOF\n"
                "sysctl --system\n"
                "```\n\n"
                "## Step 1: Install containerd\n"
                "```bash\n"
                "apt-get update && apt-get install -y containerd\n"
                "mkdir -p /etc/containerd\n"
                "containerd config default > /etc/containerd/config.toml\n"
                "# Set SystemdCgroup = true in config.toml\n"
                "systemctl restart containerd\n"
                "```\n\n"
                "## Step 2: Install kubeadm, kubelet, kubectl\n"
                "```bash\n"
                "apt-get install -y apt-transport-https ca-certificates curl\n"
                "curl -fsSL https://pkgs.k8s.io/core:/stable:/v1.29/deb/Release.key | gpg --dearmor -o /etc/apt/keyrings/kubernetes-apt-keyring.gpg\n"
                "echo 'deb [signed-by=/etc/apt/keyrings/kubernetes-apt-keyring.gpg] https://pkgs.k8s.io/core:/stable:/v1.29/deb/ /' | tee /etc/apt/sources.list.d/kubernetes.list\n"
                "apt-get update && apt-get install -y kubelet kubeadm kubectl\n"
                "apt-mark hold kubelet kubeadm kubectl\n"
                "```\n\n"
                "## Step 3: Initialize Control Plane\n"
                "```bash\n"
                "kubeadm init --control-plane-endpoint 'k8s-api.corp.local:6443' \\\n"
                "  --upload-certs --pod-network-cidr=192.168.0.0/16\n"
                "```\n\n"
                "## Step 4: Install Calico CNI\n"
                "```bash\n"
                "kubectl apply -f https://raw.githubusercontent.com/projectcalico/calico/v3.27.0/manifests/calico.yaml\n"
                "```\n\n"
                "## Step 5: Join Worker Nodes\n"
                "Use the `kubeadm join` command output from Step 3.\n\n"
                "## Verification\n"
                "```bash\n"
                "kubectl get nodes\n"
                "kubectl get pods -A\n"
                "kubectl cluster-info\n"
                "```\n"
            ),
            "articleType": HOWTO,
            "isInternal": True,
        },
        {
            "title": f"How to Configure Centralized Logging with ELK Stack (Elasticsearch, Logstash, Kibana) {ts}",
            "shortDescription": "Deploy and configure ELK Stack 8.x for centralized log aggregation and analysis.",
            "articleBody": (
                "## Architecture Overview\n"
                "```\n"
                "Application Servers → Filebeat → Logstash → Elasticsearch → Kibana\n"
                "```\n\n"
                "## Step 1: Install Elasticsearch\n"
                "```bash\n"
                "wget -qO - https://artifacts.elastic.co/GPG-KEY-elasticsearch | gpg --dearmor -o /usr/share/keyrings/elasticsearch.gpg\n"
                "apt-get install elasticsearch\n"
                "```\n\n"
                "Configure /etc/elasticsearch/elasticsearch.yml:\n"
                "```yaml\n"
                "cluster.name: crm-logs\n"
                "node.name: es-node-01\n"
                "network.host: 0.0.0.0\n"
                "discovery.seed_hosts: ['es-node-01', 'es-node-02', 'es-node-03']\n"
                "cluster.initial_master_nodes: ['es-node-01', 'es-node-02', 'es-node-03']\n"
                "xpack.security.enabled: true\n"
                "```\n\n"
                "## Step 2: Install Kibana\n"
                "```bash\n"
                "apt-get install kibana\n"
                "```\n\n"
                "Configure /etc/kibana/kibana.yml:\n"
                "```yaml\n"
                "server.host: '0.0.0.0'\n"
                "elasticsearch.hosts: ['https://es-node-01:9200']\n"
                "```\n\n"
                "## Step 3: Install and Configure Logstash\n"
                "Create pipeline configuration:\n"
                "```\n"
                "input {\n"
                "  beats { port => 5044 }\n"
                "}\n"
                "filter {\n"
                "  if [fields][log_type] == 'nginx' {\n"
                "    grok { match => { 'message' => '%{COMBINEDAPACHELOG}' } }\n"
                "  }\n"
                "}\n"
                "output {\n"
                "  elasticsearch {\n"
                "    hosts => ['https://es-node-01:9200']\n"
                "    index => 'logs-%{[fields][log_type]}-%{+YYYY.MM.dd}'\n"
                "  }\n"
                "}\n"
                "```\n\n"
                "## Step 4: Deploy Filebeat on Application Servers\n"
                "Configure filebeat.yml to ship logs to Logstash.\n"
                "Set up modules for nginx, system, and application logs.\n\n"
                "## Step 5: Create Kibana Dashboards\n"
                "Import pre-built dashboards for system and application monitoring.\n"
                "Create custom dashboards for CRM-specific log analysis.\n\n"
                "## Index Lifecycle Management\n"
                "Configure ILM policies: hot (7d) → warm (30d) → cold (90d) → delete (365d).\n"
            ),
            "articleType": HOWTO,
            "isInternal": True,
        },
    ])

    # ══════════════════════════════════════════════════════════════════════
    #  NETWORKING — Troubleshooting articles
    # ══════════════════════════════════════════════════════════════════════
    articles.extend([
        {
            "title": f"Troubleshooting: VPN Connection Drops Intermittently for Remote Users {ts}",
            "shortDescription": "Diagnose and resolve intermittent VPN disconnections affecting remote workers.",
            "articleBody": (
                "## Symptoms\n"
                "- Users report VPN drops every 15-30 minutes\n"
                "- Affected users are on various ISPs and locations\n"
                "- VPN reconnects automatically but interrupts work\n\n"
                "## Diagnostic Steps\n\n"
                "### 1. Check VPN Concentrator Logs\n"
                "```\n"
                "show vpn-sessiondb anyconnect | include Duration\n"
                "show logging | include DAP|VPN|IKE\n"
                "```\n"
                "Look for: IKE rekey failures, DAP policy mismatches, certificate expiry warnings.\n\n"
                "### 2. Client-Side Diagnostics\n"
                "```\n"
                "# Windows\n"
                "netsh interface show interface\n"
                "ping -t <vpn-gateway-ip>\n"
                "tracert <internal-resource>\n\n"
                "# macOS\n"
                "scutil --nwi\n"
                "networksetup -listallhardwareports\n"
                "```\n\n"
                "### 3. Check MTU/MSS Issues\n"
                "```bash\n"
                "ping -f -l 1400 vpn-gateway.corp.local\n"
                "# Decrease packet size until ping succeeds\n"
                "```\n"
                "If fragmentation is the issue, set MTU on VPN tunnel to 1400.\n\n"
                "### 4. Check Dead Peer Detection (DPD)\n"
                "Ensure DPD interval matches on both gateway and client.\n"
                "Recommended: DPD interval 30s, retry 5.\n\n"
                "## Common Root Causes\n"
                "| Cause | Solution |\n"
                "|-------|----------|\n"
                "| ISP NAT timeout < VPN keepalive | Reduce DPD interval to 10s |\n"
                "| MTU too high | Set tunnel MTU to 1400 |\n"
                "| Certificate expiry | Renew VPN gateway certificate |\n"
                "| AnyConnect version mismatch | Update to latest stable version |\n"
                "| Split-tunnel DNS leak | Enable DNS over VPN tunnel |\n\n"
                "## Resolution\n"
                "Most commonly resolved by adjusting DPD interval and MTU settings.\n"
                "If persistent, escalate to Network Engineering (INC-NET-ESC).\n"
            ),
            "articleType": TROUBLESHOOTING,
            "isInternal": True,
        },
        {
            "title": f"Troubleshooting: DNS Resolution Failures in Corporate Network {ts}",
            "shortDescription": "Systematic approach to diagnosing and fixing DNS resolution issues.",
            "articleBody": (
                "## Symptoms\n"
                "- Users cannot access internal/external websites by name\n"
                "- nslookup fails but IP access works\n"
                "- Intermittent 'DNS_PROBE_FINISHED_NXDOMAIN' errors in browsers\n\n"
                "## Tier 1: Client-Side Checks\n"
                "```bash\n"
                "# Flush local DNS cache\n"
                "ipconfig /flushdns   # Windows\n"
                "dscacheutil -flushcache  # macOS\n"
                "systemd-resolve --flush-caches  # Linux\n\n"
                "# Check DNS server assignment\n"
                "ipconfig /all | findstr 'DNS'\n"
                "cat /etc/resolv.conf\n\n"
                "# Test resolution\n"
                "nslookup corp.contoso.com\n"
                "nslookup corp.contoso.com 10.0.0.53\n"  # test specific DNS server
                "```\n\n"
                "## Tier 2: DNS Server Health\n"
                "```powershell\n"
                "# Check DNS Server service\n"
                "Get-Service DNS\n"
                "Get-DnsServerStatistics\n\n"
                "# Check zone health\n"
                "Get-DnsServerZone\n"
                "Test-DnsServer -IPAddress 10.0.0.53\n\n"
                "# Check forwarder connectivity\n"
                "Resolve-DnsName google.com -Server 8.8.8.8\n"
                "```\n\n"
                "## Tier 3: Infrastructure Analysis\n"
                "- Check AD replication status (DNS zones are AD-integrated)\n"
                "- Verify DNS scavenging is not deleting active records\n"
                "- Check conditional forwarders for partner domains\n"
                "- Verify DNSSEC validation chain if enabled\n\n"
                "## Decision Tree\n"
                "1. Single user affected → Client-side issue (cache, DHCP, adapter)\n"
                "2. Subnet affected → DHCP scope DNS settings or local switch issue\n"
                "3. All internal names fail → DNS server down or AD replication broken\n"
                "4. External names only fail → Forwarder or firewall issue\n"
                "5. Specific domain fails → Conditional forwarder or delegation issue\n"
            ),
            "articleType": TROUBLESHOOTING,
            "isInternal": True,
        },
        {
            "title": f"Troubleshooting: High CPU on Production Database Server (MariaDB / MySQL) {ts}",
            "shortDescription": "Diagnose and resolve excessive CPU utilization on MariaDB/MySQL database servers.",
            "articleBody": (
                "## Symptoms\n"
                "- Database server CPU consistently above 80%\n"
                "- Application response times degraded\n"
                "- Monitoring alerts for high CPU on db-prod-01\n\n"
                "## Immediate Assessment\n"
                "```sql\n"
                "-- Check running queries\n"
                "SHOW FULL PROCESSLIST;\n\n"
                "-- Check long-running queries\n"
                "SELECT * FROM information_schema.PROCESSLIST\n"
                "WHERE TIME > 30 ORDER BY TIME DESC;\n\n"
                "-- Check table locks\n"
                "SHOW OPEN TABLES WHERE In_use > 0;\n"
                "```\n\n"
                "## Slow Query Analysis\n"
                "```sql\n"
                "-- Enable slow query log if not already enabled\n"
                "SET GLOBAL slow_query_log = 'ON';\n"
                "SET GLOBAL long_query_time = 2;\n\n"
                "-- Check for missing indexes\n"
                "EXPLAIN SELECT ... -- for the slow queries found above\n"
                "```\n\n"
                "## Common Causes\n"
                "1. **Full table scans** — Missing indexes on WHERE/JOIN columns\n"
                "2. **Lock contention** — Long transactions holding row/table locks\n"
                "3. **Unoptimized queries** — Subqueries that can be JOINs\n"
                "4. **Buffer pool too small** — Frequent disk I/O instead of memory reads\n"
                "5. **Runaway query** — Report or analytics query without LIMIT\n\n"
                "## Resolution Steps\n"
                "```sql\n"
                "-- Kill a runaway query (use with caution)\n"
                "KILL <process_id>;\n\n"
                "-- Add missing index (example)\n"
                "ALTER TABLE Opportunities ADD INDEX IX_Opportunities_AccountId (AccountId);\n\n"
                "-- Increase buffer pool (requires restart planning)\n"
                "-- my.cnf: innodb_buffer_pool_size = 4G\n"
                "```\n\n"
                "## Escalation\n"
                "If CPU does not drop below 70% after killing runaway queries,\n"
                "escalate to DBA team with PROCESSLIST output and slow query log.\n"
            ),
            "articleType": TROUBLESHOOTING,
            "isInternal": True,
        },
        {
            "title": f"Troubleshooting: SSL/TLS Certificate Errors in Web Applications {ts}",
            "shortDescription": "Resolve common certificate errors including expired, untrusted, and name mismatch issues.",
            "articleBody": (
                "## Common Error Types\n"
                "| Error | Browser Message | Root Cause |\n"
                "|-------|----------------|------------|\n"
                "| ERR_CERT_DATE_INVALID | 'Your connection is not private' | Certificate expired |\n"
                "| ERR_CERT_AUTHORITY_INVALID | 'Certificate not trusted' | Self-signed or unknown CA |\n"
                "| ERR_CERT_COMMON_NAME_INVALID | 'Name mismatch' | Wrong domain in cert |\n"
                "| ERR_SSL_VERSION_OR_CIPHER_MISMATCH | 'Unsupported protocol' | TLS 1.0/1.1 deprecated |\n\n"
                "## Diagnostic Commands\n"
                "```bash\n"
                "# Check certificate details\n"
                "openssl s_client -connect app.corp.local:443 -servername app.corp.local </dev/null 2>/dev/null | openssl x509 -noout -text\n\n"
                "# Check certificate chain\n"
                "openssl s_client -connect app.corp.local:443 -showcerts\n\n"
                "# Check certificate expiry\n"
                "echo | openssl s_client -connect app.corp.local:443 2>/dev/null | openssl x509 -noout -enddate\n\n"
                "# Test specific TLS version\n"
                "openssl s_client -connect app.corp.local:443 -tls1_2\n"
                "```\n\n"
                "## Resolution by Error Type\n\n"
                "### Expired Certificate\n"
                "1. Renew certificate from CA (Let's Encrypt or internal PKI)\n"
                "2. Install on web server / load balancer\n"
                "3. Restart web service\n"
                "4. Verify: `curl -vI https://app.corp.local`\n\n"
                "### Untrusted CA\n"
                "1. Import the CA root cert into the trust store\n"
                "2. For internal PKI: push root CA via Group Policy\n"
                "3. For containers: mount CA cert into /etc/pki/ca-trust/source/anchors/\n\n"
                "### Name Mismatch\n"
                "1. Reissue certificate with correct SAN (Subject Alternative Name)\n"
                "2. Include all required domains: app.corp.local, *.corp.local\n\n"
                "## Prevention\n"
                "- Use certificate monitoring (e.g., cert-manager for K8s, Certbot timers)\n"
                "- Set up alerting 30 days before expiry\n"
                "- Document all certificates in the CMDB\n"
            ),
            "articleType": TROUBLESHOOTING,
            "isInternal": True,
        },
        {
            "title": f"Troubleshooting: Docker Container Fails to Start — Common Causes and Fixes {ts}",
            "shortDescription": "Systematic approach to diagnosing Docker container startup failures.",
            "articleBody": (
                "## Symptoms\n"
                "- Container enters 'Exited' or 'Restarting' state immediately\n"
                "- `docker ps` shows 'Restarting (1)' with short uptime\n"
                "- Application is unreachable\n\n"
                "## Diagnostic Flow\n\n"
                "### 1. Check Container Logs\n"
                "```bash\n"
                "docker logs <container_name> --tail 100\n"
                "docker logs <container_name> 2>&1 | grep -i 'error\\|fatal\\|exception'\n"
                "```\n\n"
                "### 2. Check Container State\n"
                "```bash\n"
                "docker inspect <container_name> --format '{{.State.Status}} ExitCode={{.State.ExitCode}}'\n"
                "docker inspect <container_name> --format '{{.State.Error}}'\n"
                "```\n\n"
                "### 3. Common Exit Codes\n"
                "| Exit Code | Meaning | Common Fix |\n"
                "|-----------|---------|------------|\n"
                "| 0 | Normal exit | Process completed; check if CMD is correct |\n"
                "| 1 | Application error | Check logs for stack trace |\n"
                "| 137 | OOM killed | Increase memory limit |\n"
                "| 139 | Segmentation fault | Check native dependencies |\n"
                "| 126 | Permission denied | Fix file permissions / entrypoint |\n"
                "| 127 | Command not found | Check entrypoint/CMD path |\n\n"
                "### 4. Resource Issues\n"
                "```bash\n"
                "# Check disk space\n"
                "docker system df\n"
                "df -h /var/lib/docker\n\n"
                "# Check memory\n"
                "docker stats --no-stream\n"
                "```\n\n"
                "### 5. Networking Issues\n"
                "```bash\n"
                "# Check port conflicts\n"
                "docker port <container_name>\n"
                "lsof -i :<port>\n\n"
                "# Inspect network\n"
                "docker network inspect <network_name>\n"
                "```\n\n"
                "## Quick Fixes\n"
                "- **OOM**: Add `--memory=2g` to docker run or update compose\n"
                "- **Port conflict**: Change host port mapping\n"
                "- **Volume permission**: `chown -R 1000:1000 ./data`\n"
                "- **Disk full**: `docker system prune -af --volumes`\n"
            ),
            "articleType": TROUBLESHOOTING,
            "isInternal": True,
        },
    ])

    # ══════════════════════════════════════════════════════════════════════
    #  FAQ articles
    # ══════════════════════════════════════════════════════════════════════
    articles.extend([
        {
            "title": f"FAQ: How Do I Reset My Corporate Password? {ts}",
            "shortDescription": "Steps to reset your Active Directory password via self-service portal or helpdesk.",
            "articleBody": (
                "## Self-Service Password Reset (Recommended)\n"
                "1. Go to https://passwordreset.corp.local\n"
                "2. Enter your username (e.g., jsmith)\n"
                "3. Complete multi-factor authentication (MFA) verification\n"
                "4. Choose a new password (minimum 14 characters, see policy below)\n"
                "5. Click 'Reset Password'\n\n"
                "## Password Policy Requirements\n"
                "- Minimum 14 characters\n"
                "- At least 1 uppercase letter\n"
                "- At least 1 lowercase letter\n"
                "- At least 1 number\n"
                "- At least 1 special character (!@#$%^&*)\n"
                "- Cannot reuse the last 12 passwords\n"
                "- Expires every 90 days\n\n"
                "## If Self-Service Is Unavailable\n"
                "Call the IT Service Desk: ext. 5555 or email servicedesk@corp.local.\n"
                "Have your employee ID ready for identity verification.\n\n"
                "## After Password Reset\n"
                "- Log out and back in on your workstation\n"
                "- Update password on mobile devices (email, Wi-Fi)\n"
                "- Update saved passwords in browsers (use the corporate password manager)\n"
                "- If using VPN, reconnect with the new password\n"
            ),
            "articleType": FAQ,
            "isInternal": False,
        },
        {
            "title": f"FAQ: How Do I Request Software Installation? {ts}",
            "shortDescription": "Process for requesting new software installation through the IT service catalog.",
            "articleBody": (
                "## Standard Software (Pre-Approved)\n"
                "1. Open the Software Center on your workstation\n"
                "2. Browse or search for the application\n"
                "3. Click 'Install' — no approval required\n\n"
                "Available standard software includes:\n"
                "- Microsoft Office 365, Visual Studio Code, Zoom, Slack, 7-Zip,\n"
                "  Adobe Reader, Chrome, Firefox\n\n"
                "## Non-Standard Software (Requires Approval)\n"
                "1. Log in to the IT Service Portal: https://serviceportal.corp.local\n"
                "2. Navigate to Service Catalog → Software Request\n"
                "3. Fill in:\n"
                "   - Software name and version\n"
                "   - Business justification\n"
                "   - Cost center (if paid software)\n"
                "4. Submit request\n\n"
                "## Approval Workflow\n"
                "- Manager approval (1-2 business days)\n"
                "- IT Security review (for new software, 3-5 business days)\n"
                "- License procurement (if applicable)\n"
                "- Installation by IT (within 2 business days after approval)\n\n"
                "## SLA\n"
                "- Standard software: Immediate self-service\n"
                "- Pre-approved non-standard: 3 business days\n"
                "- New software (requires security review): 5-10 business days\n"
            ),
            "articleType": FAQ,
            "isInternal": False,
        },
        {
            "title": f"FAQ: What Is the IT Change Management Process? {ts}",
            "shortDescription": "Overview of the ITIL change management process for requesting and approving changes.",
            "articleBody": (
                "## Change Types\n"
                "| Type | Approval | Lead Time | Example |\n"
                "|------|----------|-----------|----------|\n"
                "| Standard | Pre-approved | None | Password reset, user onboarding |\n"
                "| Normal | CAB review | 5+ business days | Server migration, firewall rule |\n"
                "| Emergency | ECAB (expedited) | ASAP | Critical security patch, outage fix |\n\n"
                "## Normal Change Process\n"
                "1. **Submit RFC** — Create a Change Request in the ITSM portal\n"
                "2. **Assessment** — Change Manager reviews risk and impact\n"
                "3. **CAB Review** — Change Advisory Board meets weekly (Thursday 2pm)\n"
                "4. **Approval** — CAB approves, rejects, or requests more info\n"
                "5. **Scheduling** — Change scheduled in the maintenance window\n"
                "6. **Implementation** — Change team executes the change\n"
                "7. **Review** — Post-implementation review (PIR) within 5 days\n\n"
                "## Required Information for RFC\n"
                "- Description of change and business justification\n"
                "- Risk assessment (using the risk matrix)\n"
                "- Implementation plan (step-by-step)\n"
                "- Backout/rollback plan\n"
                "- Testing plan and results\n"
                "- Affected CIs (from CMDB)\n"
                "- Communication plan\n\n"
                "## Maintenance Windows\n"
                "- **Standard**: Saturday 02:00-06:00 UTC\n"
                "- **Extended**: First Saturday of month, 00:00-08:00 UTC\n"
                "- **Emergency**: As needed, with ECAB approval\n"
            ),
            "articleType": FAQ,
            "isInternal": False,
        },
        {
            "title": f"FAQ: How Do I Set Up Multi-Factor Authentication (MFA)? {ts}",
            "shortDescription": "Guide to enrolling in MFA for corporate applications and VPN access.",
            "articleBody": (
                "## What Is MFA?\n"
                "Multi-Factor Authentication adds a second verification step beyond your password.\n"
                "It's required for: VPN, email (remote), admin portals, cloud applications.\n\n"
                "## Enrollment Steps\n"
                "1. Go to https://mfa.corp.local/enroll\n"
                "2. Log in with your corporate credentials\n"
                "3. Choose your primary method:\n"
                "   - **Authenticator App** (recommended): Microsoft Authenticator or Google Authenticator\n"
                "   - **SMS**: Text message to your registered mobile number\n"
                "   - **Hardware Token**: Request from IT Service Desk\n\n"
                "### Authenticator App Setup\n"
                "1. Install Microsoft Authenticator from your app store\n"
                "2. In the enrollment page, click 'Authenticator App'\n"
                "3. Scan the QR code with the app\n"
                "4. Enter the 6-digit code shown in the app to verify\n"
                "5. Save backup codes in a secure location\n\n"
                "## Troubleshooting MFA\n"
                "- **Code not working**: Ensure your phone clock is synced (Settings → Date & Time → Auto)\n"
                "- **Lost phone**: Use backup codes, then contact IT to re-enroll\n"
                "- **New phone**: Transfer accounts before wiping old phone, or use backup\n\n"
                "## MFA Bypass (Emergency Only)\n"
                "Contact the IT Service Desk with your employee ID.\n"
                "A temporary bypass (4 hours) can be granted after identity verification.\n"
            ),
            "articleType": FAQ,
            "isInternal": False,
        },
        {
            "title": f"FAQ: How Do I Connect to the Corporate VPN? {ts}",
            "shortDescription": "Instructions for connecting to the corporate VPN from home or remote locations.",
            "articleBody": (
                "## Supported VPN Clients\n"
                "- **Windows / macOS**: Cisco AnyConnect (download from https://vpn.corp.local)\n"
                "- **Linux**: OpenConnect\n"
                "- **Mobile**: Cisco AnyConnect app (iOS / Android)\n\n"
                "## Connection Steps\n"
                "1. Open the VPN client application\n"
                "2. Enter the VPN gateway address: `vpn.corp.local`\n"
                "3. Click 'Connect'\n"
                "4. Enter your corporate username and password\n"
                "5. Complete MFA verification (approve push notification or enter code)\n"
                "6. Wait for connection to establish (green icon = connected)\n\n"
                "## VPN Profiles\n"
                "| Profile | Access Level | Use Case |\n"
                "|---------|-------------|----------|\n"
                "| Corp-General | Internal network, email | General office work |\n"
                "| Corp-Dev | Dev networks, CI/CD | Software development |\n"
                "| Corp-Admin | Server management | IT administrators only |\n\n"
                "## Common Issues\n"
                "- Connection timeout → Check internet connectivity first\n"
                "- Authentication failed → Reset password (see password reset FAQ)\n"
                "- Slow performance → Try split-tunnel profile if available\n"
                "- Cannot reach internal sites → Check DNS settings, try flush DNS\n\n"
                "## Split Tunneling\n"
                "By default, all traffic goes through VPN. For better performance on video calls,\n"
                "ask your manager to approve the split-tunnel profile.\n"
            ),
            "articleType": FAQ,
            "isInternal": False,
        },
    ])

    # ══════════════════════════════════════════════════════════════════════
    #  KNOWN ERROR articles
    # ══════════════════════════════════════════════════════════════════════
    articles.extend([
        {
            "title": f"Known Error: Active Directory Replication Failure After DC Promotion (Event 1864) {ts}",
            "shortDescription": "AD replication may fail with Event 1864 after promoting a new domain controller. Workaround available.",
            "articleBody": (
                "## Identifier: KE-AD-001\n\n"
                "## Affected Systems\n"
                "- Windows Server 2019/2022 Domain Controllers\n"
                "- Active Directory Domain Services\n\n"
                "## Symptoms\n"
                "- Event 1864 in Directory Services event log\n"
                "- `repadmin /replsummary` shows failed replication partners\n"
                "- New DC does not receive updates from existing DCs\n\n"
                "## Root Cause\n"
                "When a new DC is promoted with a static IP that was previously used by another device,\n"
                "stale DNS records can cause name resolution conflicts during initial replication.\n\n"
                "## Workaround\n"
                "1. Flush DNS on all existing DCs:\n"
                "   ```powershell\n"
                "   ipconfig /flushdns\n"
                "   ipconfig /registerdns\n"
                "   ```\n"
                "2. Clear stale DNS records for the new DC's IP\n"
                "3. Force replication:\n"
                "   ```powershell\n"
                "   repadmin /syncall /d /e /P\n"
                "   ```\n"
                "4. Restart the NTDS service on the new DC if needed\n\n"
                "## Permanent Fix\n"
                "Planned for next quarterly patching cycle. Microsoft hotfix KB5034567 addresses this.\n\n"
                "## Status: Open | Priority: Medium | Assigned: Infrastructure Team\n"
            ),
            "articleType": KNOWN_ERROR,
            "isInternal": True,
        },
        {
            "title": f"Known Error: Outlook 365 Intermittent Search Failures After Index Rebuild {ts}",
            "shortDescription": "Outlook search returns incomplete results for 24-48 hours after Windows Search index rebuild.",
            "articleBody": (
                "## Identifier: KE-O365-002\n\n"
                "## Affected Systems\n"
                "- Microsoft Outlook (Microsoft 365 Apps, Version 2308+)\n"
                "- Windows 10/11 with Windows Search enabled\n\n"
                "## Symptoms\n"
                "- Outlook search returns 0 results or partial results\n"
                "- Users report 'We found items that may match your search in non-indexed locations'\n"
                "- Issue appears after Windows Update or manual index rebuild\n\n"
                "## Root Cause\n"
                "Windows Search rebuilds the index but Outlook Connector takes 24-48 hours to re-index\n"
                "large mailboxes (>10GB). During this period, search is degraded.\n\n"
                "## Workaround\n"
                "1. Use Outlook Web App (https://outlook.office.com) for reliable search\n"
                "2. Or force re-index:\n"
                "   - Close Outlook\n"
                "   - Delete: `%LOCALAPPDATA%\\Microsoft\\Outlook\\*.ost`\n"
                "   - Reopen Outlook (will resync from server)\n"
                "   - **Warning**: This redownloads all mail — may take hours on large mailboxes\n\n"
                "## Permanent Fix\n"
                "Microsoft is investigating. Tracked as internal issue MC741523.\n"
                "Expected fix: Next Outlook semi-annual channel update (Jan 2026).\n\n"
                "## Status: Open | Priority: Low | Assigned: Desktop Support\n"
            ),
            "articleType": KNOWN_ERROR,
            "isInternal": True,
        },
        {
            "title": f"Known Error: CRM API Returns 500 When Creating Opportunity with Large Description {ts}",
            "shortDescription": "CRM API returns HTTP 500 when opportunity description exceeds 4000 characters due to MariaDB row size limit.",
            "articleBody": (
                "## Identifier: KE-CRM-003\n\n"
                "## Affected Systems\n"
                "- CRM API (all versions)\n"
                "- MariaDB 10.11 backend\n\n"
                "## Symptoms\n"
                "- HTTP 500 error when saving an opportunity with a long description\n"
                "- Error in API logs: `Row size too large (> 8126)`\n"
                "- Only affects MariaDB backend (PostgreSQL/SQL Server not affected)\n\n"
                "## Root Cause\n"
                "MariaDB InnoDB has a row size limit for VARCHAR columns stored inline.\n"
                "The Opportunities table has multiple TEXT/VARCHAR columns that together\n"
                "can exceed the maximum row size.\n\n"
                "## Workaround\n"
                "Limit opportunity descriptions to 4000 characters.\n"
                "If longer descriptions are needed, use the 'Notes' feature\n"
                "to add supplementary information as separate note entries.\n\n"
                "## Permanent Fix\n"
                "Migration planned to change Description column from VARCHAR(MAX) to LONGTEXT.\n"
                "Tracked as: TODO-SALES-007 in MASTER_TODO_LIST.\n\n"
                "## Status: Open | Priority: Medium | Assigned: Backend Team\n"
            ),
            "articleType": KNOWN_ERROR,
            "isInternal": True,
        },
        {
            "title": f"Known Error: Printer Spooler Crash on Windows 11 with HP Universal Print Driver {ts}",
            "shortDescription": "Print spooler crashes when printing PDFs using HP Universal Print Driver v7.1 on Windows 11 23H2.",
            "articleBody": (
                "## Identifier: KE-PRINT-004\n\n"
                "## Affected Systems\n"
                "- Windows 11 23H2\n"
                "- HP Universal Print Driver v7.1.0.x\n"
                "- Networked HP printers (all models)\n\n"
                "## Symptoms\n"
                "- Print spooler service crashes when printing PDF documents\n"
                "- Event ID 7034: 'Print Spooler service terminated unexpectedly'\n"
                "- Other print jobs also fail until spooler restarts\n\n"
                "## Root Cause\n"
                "Incompatibility between HP UPD v7.1 and Windows 11 23H2 print subsystem.\n"
                "HP has acknowledged the issue (case reference: HP-SUP-2025-4412).\n\n"
                "## Workaround\n"
                "1. Downgrade to HP UPD v7.0.1:\n"
                "   - Download from the internal software repository\n"
                "   - Uninstall current driver, install v7.0.1\n"
                "2. Or use 'Print to PDF' then print the PDF from Adobe Reader\n\n"
                "## Permanent Fix\n"
                "HP UPD v7.2 (planned release: Q1 2026) will include the fix.\n"
                "IT will push the update via SCCM once available and tested.\n\n"
                "## Status: Open | Priority: Low | Assigned: Desktop Support\n"
            ),
            "articleType": KNOWN_ERROR,
            "isInternal": True,
        },
    ])

    # ══════════════════════════════════════════════════════════════════════
    #  REFERENCE articles
    # ══════════════════════════════════════════════════════════════════════
    articles.extend([
        {
            "title": f"Reference: Corporate Network Architecture Overview {ts}",
            "shortDescription": "High-level reference for the corporate network topology, VLANs, and IP addressing scheme.",
            "articleBody": (
                "## Network Topology\n"
                "```\n"
                "Internet ←→ [Firewall Pair] ←→ [Core Switches] ←→ [Distribution] ←→ [Access Layer]\n"
                "                                     ↕\n"
                "                              [DMZ Switches]\n"
                "                                     ↕\n"
                "                          [Web Servers, Reverse Proxy]\n"
                "```\n\n"
                "## VLAN Assignments\n"
                "| VLAN | Name | Subnet | Purpose |\n"
                "|------|------|--------|---------|\n"
                "| 10 | Management | 10.0.10.0/24 | Network device management |\n"
                "| 20 | Servers-Prod | 10.0.20.0/24 | Production servers |\n"
                "| 30 | Servers-Dev | 10.0.30.0/24 | Development servers |\n"
                "| 40 | Users-Office | 10.0.40.0/22 | Office workstations |\n"
                "| 50 | Users-WiFi | 10.0.50.0/22 | Corporate Wi-Fi |\n"
                "| 60 | Guest-WiFi | 10.0.60.0/24 | Guest network (isolated) |\n"
                "| 70 | VoIP | 10.0.70.0/24 | IP phones |\n"
                "| 80 | DMZ | 10.0.80.0/24 | Public-facing servers |\n"
                "| 90 | IoT | 10.0.90.0/24 | IoT devices (isolated) |\n"
                "| 100 | Database | 10.0.100.0/24 | Database tier (restricted) |\n\n"
                "## DNS Servers\n"
                "- Primary: 10.0.10.53 (dc01.corp.local)\n"
                "- Secondary: 10.0.10.54 (dc02.corp.local)\n"
                "- External forwarders: 1.1.1.1, 8.8.8.8\n\n"
                "## Key Infrastructure IPs\n"
                "| Service | IP | FQDN |\n"
                "|---------|-----|------|\n"
                "| VPN Gateway | 203.0.113.10 | vpn.corp.local |\n"
                "| Web Proxy | 10.0.80.10 | proxy.corp.local |\n"
                "| Mail Gateway | 10.0.80.20 | mail.corp.local |\n"
                "| SCCM Server | 10.0.20.30 | sccm.corp.local |\n"
                "| Monitoring | 10.0.20.40 | monitoring.corp.local |\n"
            ),
            "articleType": REFERENCE,
            "isInternal": True,
        },
        {
            "title": f"Reference: IT Service Catalog — Full Listing of Available Services {ts}",
            "shortDescription": "Complete catalog of IT services available to employees with SLA targets.",
            "articleBody": (
                "## Service Categories\n\n"
                "### 1. Access & Identity Services\n"
                "| Service | SLA | Request Method |\n"
                "|---------|-----|----------------|\n"
                "| New User Account | 1 business day | HR onboarding trigger |\n"
                "| Password Reset | 15 minutes (self-service) | Self-service portal |\n"
                "| Group / DL Membership | 4 hours | Service Portal request |\n"
                "| VPN Access | 1 business day | Service Portal + manager approval |\n"
                "| Admin Access | 3 business days | Service Portal + security approval |\n\n"
                "### 2. Hardware Services\n"
                "| Service | SLA | Request Method |\n"
                "|---------|-----|----------------|\n"
                "| New Laptop | 3 business days | Service Portal |\n"
                "| Monitor Request | 5 business days | Service Portal |\n"
                "| Peripheral (keyboard/mouse) | 2 business days | Walk-in or Portal |\n"
                "| Hardware Repair | 2-5 business days | Service Portal |\n"
                "| Mobile Device | 3 business days | Service Portal + manager approval |\n\n"
                "### 3. Software Services\n"
                "| Service | SLA | Request Method |\n"
                "|---------|-----|----------------|\n"
                "| Standard Software | Immediate | Software Center |\n"
                "| Licensed Software | 3-5 business days | Service Portal + approval |\n"
                "| Custom Development | Per project | Project request form |\n\n"
                "### 4. Infrastructure Services\n"
                "| Service | SLA | Request Method |\n"
                "|---------|-----|----------------|\n"
                "| VM Provisioning | 2 business days | Cloud Portal |\n"
                "| Database Creation | 3 business days | Service Portal |\n"
                "| DNS Record Changes | 4 hours | Service Portal |\n"
                "| Firewall Rule | 2 business days | Service Portal + security review |\n"
                "| SSL Certificate | 1 business day | Service Portal |\n\n"
                "## Service Hours\n"
                "- **Standard Support**: Mon-Fri, 08:00-18:00 local time\n"
                "- **Extended Support**: Mon-Fri, 06:00-22:00 UTC (P1/P2 only)\n"
                "- **24/7 On-Call**: Critical infrastructure incidents only\n"
            ),
            "articleType": REFERENCE,
            "isInternal": False,
        },
        {
            "title": f"Reference: Server Naming Convention and Standards {ts}",
            "shortDescription": "Standard naming convention for all servers, VMs, and containers in the enterprise.",
            "articleBody": (
                "## Naming Format\n"
                "```\n"
                "{location}{type}{environment}{function}{sequence}\n"
                "```\n\n"
                "## Components\n"
                "| Component | Values | Example |\n"
                "|-----------|--------|---------|\n"
                "| Location | US (US), UK (UK), SG (Singapore), AZ (Azure), AW (AWS) | US |\n"
                "| Type | S (Physical), V (VM), C (Container), K (K8s) | V |\n"
                "| Environment | P (Prod), D (Dev), S (Staging), T (Test), R (DR) | P |\n"
                "| Function | APP, WEB, DB, DC, FS, MON, PROXY, MAIL, DNS | DB |\n"
                "| Sequence | 01-99 | 01 |\n\n"
                "## Examples\n"
                "| Hostname | Meaning |\n"
                "|-----------|---------|\n"
                "| USVPDB01 | US, VM, Prod, Database, 01 |\n"
                "| UKSPAPP02 | UK, Physical, Prod, Application, 02 |\n"
                "| AZVDWEB01 | Azure, VM, Dev, Web, 01 |\n"
                "| USVTDC01 | US, VM, Test, Domain Controller, 01 |\n"
                "| SGCPAPP01 | Singapore, Container, Prod, Application, 01 |\n\n"
                "## Special Cases\n"
                "- Kubernetes nodes: `{loc}KP-NODE{seq}` (e.g., USKP-NODE01)\n"
                "- Load balancers: `{loc}{env}-LB{seq}` (e.g., USP-LB01)\n"
                "- Network devices follow separate convention (see Network Standards)\n\n"
                "## DNS Registry\n"
                "All server names must be registered in the CMDB before deployment.\n"
                "Contact the infrastructure team for name allocation.\n"
            ),
            "articleType": REFERENCE,
            "isInternal": True,
        },
        {
            "title": f"Reference: Incident Priority Matrix — Impact vs Urgency {ts}",
            "shortDescription": "Official incident priority matrix used to classify and triage all incidents.",
            "articleBody": (
                "## Priority Matrix\n"
                "```\n"
                "                    URGENCY\n"
                "              High    Medium    Low\n"
                "         ┌─────────┬─────────┬─────────┐\n"
                "   High  │   P1    │   P2    │   P3    │\n"
                "I        ├─────────┼─────────┼─────────┤\n"
                "M Medium │   P2    │   P3    │   P4    │\n"
                "P        ├─────────┼─────────┼─────────┤\n"
                "   Low   │   P3    │   P4    │   P5    │\n"
                "         └─────────┴─────────┴─────────┘\n"
                "```\n\n"
                "## Priority Definitions\n"
                "| Priority | Response | Resolution | Example |\n"
                "|----------|----------|------------|----------|\n"
                "| P1 Critical | 15 min | 4 hours | Production outage, data breach |\n"
                "| P2 High | 30 min | 8 hours | Degraded service for many users |\n"
                "| P3 Medium | 4 hours | 24 hours | Single user productivity blocked |\n"
                "| P4 Low | 8 hours | 72 hours | Minor issue with workaround |\n"
                "| P5 Planning | Next sprint | Best effort | Enhancement, cosmetic issue |\n\n"
                "## Impact Definitions\n"
                "- **High**: Entire department/org affected, revenue impacted, or data at risk\n"
                "- **Medium**: Multiple users affected, workaround exists but inconvenient\n"
                "- **Low**: Single user affected, workaround available\n\n"
                "## Urgency Definitions\n"
                "- **High**: No workaround, business-critical process blocked\n"
                "- **Medium**: Workaround exists but is complex/time-consuming\n"
                "- **Low**: Issue is annoying but work can continue normally\n\n"
                "## Escalation Triggers\n"
                "- P1: Immediate escalation to Major Incident Manager\n"
                "- P2: Escalation after 4 hours if no progress\n"
                "- P3-P5: Standard escalation per SLA breach\n"
            ),
            "articleType": REFERENCE,
            "isInternal": False,
        },
    ])

    # ══════════════════════════════════════════════════════════════════════
    #  BEST PRACTICE articles
    # ══════════════════════════════════════════════════════════════════════
    articles.extend([
        {
            "title": f"Best Practice: Securing Active Directory — Defense in Depth {ts}",
            "shortDescription": "Comprehensive AD security hardening guidelines following the tiered administration model.",
            "articleBody": (
                "## Tier Model Overview\n"
                "| Tier | Assets | Admin Access |\n"
                "|------|--------|--------------|\n"
                "| Tier 0 | Domain Controllers, AD, PKI | Domain Admins only |\n"
                "| Tier 1 | Servers, Applications | Server Admins |\n"
                "| Tier 2 | Workstations, Users | Helpdesk |\n\n"
                "## Key Controls\n\n"
                "### 1. Privileged Access Workstations (PAW)\n"
                "- Tier 0 admins must use dedicated PAWs for administration\n"
                "- PAWs are hardened, air-gapped from general network\n"
                "- No internet access, no email on PAWs\n\n"
                "### 2. Group Policy Hardening\n"
                "- Deny log on locally for Domain Admins on non-DCs\n"
                "- Enable Protected Users group for all admin accounts\n"
                "- Configure fine-grained password policies (15+ chars for admins)\n"
                "- Enable credential guard on Tier 0/1 systems\n\n"
                "### 3. LAPS (Local Administrator Password Solution)\n"
                "- Deploy LAPS to all domain-joined workstations and servers\n"
                "- Rotate local admin passwords every 30 days\n"
                "- Ensure LAPS passwords are backed up and audited\n\n"
                "### 4. Monitoring & Detection\n"
                "- Forward security events to SIEM (Event IDs: 4624, 4625, 4672, 4720, 4768)\n"
                "- Monitor for Golden Ticket attacks (Event ID 4769 with RC4)\n"
                "- Enable Advanced Audit Policy for DS Access\n"
                "- Deploy Microsoft Defender for Identity (MDI)\n\n"
                "### 5. Regular Hygiene\n"
                "- Clean up stale computer objects monthly\n"
                "- Review group memberships quarterly\n"
                "- Remove unused admin accounts\n"
                "- Test AD backup and restore annually\n"
            ),
            "articleType": BEST_PRACTICE,
            "isInternal": True,
        },
        {
            "title": f"Best Practice: Container Security — Docker and Kubernetes Hardening {ts}",
            "shortDescription": "Security best practices for running containers in production environments.",
            "articleBody": (
                "## Image Security\n"
                "1. **Use minimal base images** — alpine or distroless over ubuntu/debian\n"
                "2. **Scan images** — Run Trivy/Snyk in CI/CD pipeline before push\n"
                "3. **Pin versions** — Use `image:sha256@digest` not `image:latest`\n"
                "4. **Multi-stage builds** — Separate build dependencies from runtime\n"
                "5. **No secrets in images** — Use runtime injection (K8s Secrets, Vault)\n\n"
                "## Runtime Security\n"
                "1. **Non-root user** — Always set `USER nonroot` in Dockerfile\n"
                "2. **Read-only filesystem** — `readOnlyRootFilesystem: true`\n"
                "3. **Drop capabilities** — `drop: [ALL]`, add only what's needed\n"
                "4. **Resource limits** — Always set CPU/memory requests and limits\n"
                "5. **No privileged mode** — `privileged: false` always\n\n"
                "## Kubernetes-Specific\n"
                "```yaml\n"
                "securityContext:\n"
                "  runAsNonRoot: true\n"
                "  allowPrivilegeEscalation: false\n"
                "  readOnlyRootFilesystem: true\n"
                "  capabilities:\n"
                "    drop: ['ALL']\n"
                "  seccompProfile:\n"
                "    type: RuntimeDefault\n"
                "```\n\n"
                "## Network Policies\n"
                "- Default deny all ingress/egress per namespace\n"
                "- Explicitly allow only required pod-to-pod communication\n"
                "- Separate namespaces for different environments and teams\n\n"
                "## Secret Management\n"
                "- Use external secret stores (HashiCorp Vault, AWS Secrets Manager)\n"
                "- External Secrets Operator for K8s integration\n"
                "- Rotate secrets automatically every 90 days\n"
                "- Never commit secrets to Git (use pre-commit hooks to detect)\n"
            ),
            "articleType": BEST_PRACTICE,
            "isInternal": True,
        },
        {
            "title": f"Best Practice: Database Backup and Recovery Strategy {ts}",
            "shortDescription": "Enterprise database backup strategy covering RPO, RTO, and testing requirements.",
            "articleBody": (
                "## Backup Strategy Overview\n"
                "| Database Tier | RPO | RTO | Backup Frequency |\n"
                "|---------------|-----|-----|------------------|\n"
                "| Tier 1 (Critical) | 15 min | 1 hour | Continuous + daily full |\n"
                "| Tier 2 (Important) | 1 hour | 4 hours | Hourly + daily full |\n"
                "| Tier 3 (Standard) | 24 hours | 24 hours | Daily full + weekly offsite |\n\n"
                "## Backup Types\n\n"
                "### Full Backup\n"
                "- Complete database copy\n"
                "- Schedule: Daily at 02:00 UTC\n"
                "- Retention: 30 days local, 90 days offsite\n\n"
                "### Incremental/Differential\n"
                "- Changes since last backup\n"
                "- Schedule: Hourly for Tier 1/2\n"
                "- Retention: 7 days\n\n"
                "### Transaction Log Backup (MariaDB Binlog)\n"
                "- Continuous for Tier 1\n"
                "- Enables point-in-time recovery\n"
                "- Retention: 7 days\n\n"
                "## Storage\n"
                "- Primary: Local NAS (fast restore)\n"
                "- Secondary: Offsite cloud storage (S3, Azure Blob)\n"
                "- Encryption: AES-256 at rest, TLS in transit\n\n"
                "## Testing Requirements\n"
                "- **Monthly**: Restore test to non-production (automated)\n"
                "- **Quarterly**: Full DR failover test (manual, documented)\n"
                "- **Annually**: Full disaster recovery exercise\n\n"
                "## MariaDB-Specific Commands\n"
                "```bash\n"
                "# Full backup with mariabackup\n"
                "mariabackup --backup --target-dir=/backup/full --user=root\n\n"
                "# Incremental backup\n"
                "mariabackup --backup --incremental-basedir=/backup/full \\\n"
                "  --target-dir=/backup/inc1 --user=root\n\n"
                "# Point-in-time restore\n"
                "mariabackup --prepare --target-dir=/backup/full\n"
                "mariabackup --copy-back --target-dir=/backup/full\n"
                "mysqlbinlog --start-datetime='2026-03-08 14:00:00' binlog.000042 | mysql\n"
                "```\n"
            ),
            "articleType": BEST_PRACTICE,
            "isInternal": True,
        },
        {
            "title": f"Best Practice: Incident Management — Communication and Escalation {ts}",
            "shortDescription": "Guidelines for effective incident communication, stakeholder updates, and escalation procedures.",
            "articleBody": (
                "## Communication Principles\n"
                "1. **Be transparent** — Share what you know, what you don't, and what you're doing\n"
                "2. **Be timely** — First update within 15 minutes of P1/P2\n"
                "3. **Be consistent** — Use standard templates and channels\n"
                "4. **Be specific** — Avoid jargon, include impact in business terms\n\n"
                "## Stakeholder Communication Matrix\n"
                "| Priority | Who to Notify | Frequency | Channel |\n"
                "|----------|---------------|-----------|----------|\n"
                "| P1 | CIO, IT Directors, all affected users | Every 30 min | Email + Teams bridge |\n"
                "| P2 | IT Management, affected team leads | Every 1 hour | Email + Teams |\n"
                "| P3 | Affected users | On resolution | Email |\n"
                "| P4-P5 | Requester only | On resolution | Ticket update |\n\n"
                "## Communication Templates\n\n"
                "### Initial Notification (P1/P2)\n"
                "```\n"
                "Subject: [P1 INCIDENT] {Service} — {Impact Summary}\n\n"
                "IMPACT: {Who is affected and how}\n"
                "STATUS: Investigation in progress\n"
                "WORKAROUND: {If available}\n"
                "NEXT UPDATE: {Time}\n"
                "BRIDGE: {Teams meeting link}\n"
                "INCIDENT MANAGER: {Name}\n"
                "```\n\n"
                "### Progress Update\n"
                "```\n"
                "Subject: [UPDATE #{n}] [P1 INCIDENT] {Service}\n\n"
                "STATUS: {Current status}\n"
                "ACTIONS TAKEN: {What has been done}\n"
                "ROOT CAUSE: {If known, or 'Under investigation'}\n"
                "ETA: {Estimated resolution time}\n"
                "NEXT UPDATE: {Time}\n"
                "```\n\n"
                "## Escalation Process\n"
                "### Functional Escalation (need more expertise)\n"
                "- Tier 1 → Tier 2: After 30 minutes or if outside skill level\n"
                "- Tier 2 → Tier 3: After 2 hours or vendor escalation needed\n"
                "- Tier 3 → Vendor: Per vendor support contract\n\n"
                "### Hierarchical Escalation (need more authority/resources)\n"
                "- Team Lead: If SLA breached by 25%\n"
                "- Manager: If SLA breached by 50%\n"
                "- Director: If P1 unresolved after 4 hours\n"
                "- CIO: If P1 unresolved after 8 hours\n"
            ),
            "articleType": BEST_PRACTICE,
            "isInternal": False,
        },
        {
            "title": f"Best Practice: CI/CD Pipeline Security — DevSecOps Checklist {ts}",
            "shortDescription": "Security controls to embed in CI/CD pipelines for shift-left security.",
            "articleBody": (
                "## Pipeline Security Controls\n\n"
                "### 1. Source Code\n"
                "- [ ] Pre-commit hooks: detect secrets, lint, format\n"
                "- [ ] Branch protection: require PR, 2 approvals, status checks\n"
                "- [ ] Signed commits (GPG/SSH key signing)\n"
                "- [ ] CODEOWNERS file for critical paths\n\n"
                "### 2. Build Stage\n"
                "- [ ] SAST (Static Application Security Testing): SonarQube, Semgrep\n"
                "- [ ] Dependency scanning: Dependabot, Snyk, OWASP Dependency-Check\n"
                "- [ ] License compliance: FOSSA, Snyk License\n"
                "- [ ] Build reproducibility: lock files, pinned versions\n\n"
                "### 3. Test Stage\n"
                "- [ ] Unit tests (>80% coverage)\n"
                "- [ ] Integration tests with security scenarios\n"
                "- [ ] DAST (Dynamic testing): OWASP ZAP, Burp Suite\n"
                "- [ ] API security testing: contract validation, auth bypass checks\n\n"
                "### 4. Container/Image Stage\n"
                "- [ ] Image scanning: Trivy, Snyk Container\n"
                "- [ ] Base image validation: approved registry only\n"
                "- [ ] No HIGH/CRITICAL vulnerabilities (fail pipeline)\n"
                "- [ ] Image signing: Notary/Cosign\n\n"
                "### 5. Deploy Stage\n"
                "- [ ] Infrastructure as Code scanning: Checkov, tfsec\n"
                "- [ ] Secrets injection from vault (not pipeline variables)\n"
                "- [ ] Deployment approval gates for production\n"
                "- [ ] Canary/blue-green deployment for risk reduction\n\n"
                "### 6. Post-Deploy\n"
                "- [ ] Runtime monitoring: Falco, Datadog\n"
                "- [ ] SBOM generation and storage\n"
                "- [ ] Penetration testing schedule (quarterly)\n\n"
                "## Toolchain Recommendation\n"
                "| Stage | Tool | License |\n"
                "|-------|------|---------|\n"
                "| SAST | SonarQube | Community/Enterprise |\n"
                "| SCA | Snyk | Free tier / Paid |\n"
                "| Container | Trivy | OSS |\n"
                "| DAST | OWASP ZAP | OSS |\n"
                "| IaC | Checkov | OSS |\n"
                "| Secrets | detect-secrets | OSS |\n"
            ),
            "articleType": BEST_PRACTICE,
            "isInternal": True,
        },
    ])

    # ══════════════════════════════════════════════════════════════════════
    #  SECURITY — Mixed article types
    # ══════════════════════════════════════════════════════════════════════
    articles.extend([
        {
            "title": f"How to Respond to a Phishing Email Report {ts}",
            "shortDescription": "Standard operating procedure for IT security when a phishing email is reported by a user.",
            "articleBody": (
                "## Immediate Actions (Within 15 Minutes)\n"
                "1. Acknowledge the report — thank the user for reporting\n"
                "2. Retrieve the email headers from the reporting user\n"
                "3. Check if other users received the same email:\n"
                "   ```powershell\n"
                "   # Exchange Online — search by subject/sender\n"
                "   Get-MessageTrace -SenderAddress attacker@evil.com -StartDate (Get-Date).AddHours(-24)\n"
                "   ```\n\n"
                "## Analysis (Within 1 Hour)\n"
                "4. Analyze URLs using sandbox (VirusTotal, urlscan.io)\n"
                "5. Analyze attachments in sandbox (any.run, hybrid-analysis)\n"
                "6. Check if any users clicked the link:\n"
                "   - Review proxy/firewall logs for the malicious URL\n"
                "   - Check Microsoft Defender for Office 365 Threat Explorer\n\n"
                "## Containment\n"
                "7. Block the sender domain at the mail gateway\n"
                "8. Add malicious URLs to the firewall blocklist\n"
                "9. Purge the email from all mailboxes:\n"
                "   ```powershell\n"
                "   # Soft delete from all mailboxes\n"
                "   New-ComplianceSearchAction -SearchName 'PhishSearch' -Purge -PurgeType SoftDelete\n"
                "   ```\n"
                "10. If credentials were entered: force password reset for affected users\n"
                "11. If malware was downloaded: isolate affected endpoints\n\n"
                "## Post-Incident\n"
                "12. Send awareness notification to all users\n"
                "13. Update email rules to catch similar patterns\n"
                "14. Document in incident management system\n"
                "15. Include in monthly phishing report metrics\n"
            ),
            "articleType": HOWTO,
            "isInternal": True,
        },
        {
            "title": f"Troubleshooting: Kubernetes Pod Stuck in CrashLoopBackOff {ts}",
            "shortDescription": "Systematic approach to diagnosing and resolving pods in CrashLoopBackOff state.",
            "articleBody": (
                "## Symptoms\n"
                "- `kubectl get pods` shows status 'CrashLoopBackOff'\n"
                "- Pod restarts counter incrementing rapidly\n"
                "- Application unavailable\n\n"
                "## Diagnostic Steps\n\n"
                "### 1. Check Pod Events\n"
                "```bash\n"
                "kubectl describe pod <pod-name> -n <namespace>\n"
                "# Look at Events section at the bottom\n"
                "```\n\n"
                "### 2. Check Container Logs\n"
                "```bash\n"
                "# Current crash attempt\n"
                "kubectl logs <pod-name> -n <namespace>\n\n"
                "# Previous crash (if container restarted)\n"
                "kubectl logs <pod-name> -n <namespace> --previous\n"
                "```\n\n"
                "### 3. Common Causes and Fixes\n"
                "| Cause | Symptoms | Fix |\n"
                "|-------|----------|-----|\n"
                "| Missing config/secret | 'file not found' in logs | Check ConfigMap/Secret mounts |\n"
                "| DB connection failure | 'connection refused' | Verify DB service/endpoint |\n"
                "| Health check failing | Liveness probe failure | Increase initialDelaySeconds |\n"
                "| OOM Kill | Exit code 137 | Increase memory limits |\n"
                "| Wrong image tag | 'exec format error' | Fix image:tag reference |\n"
                "| Permission denied | 'EACCES' | Fix securityContext runAsUser |\n\n"
                "### 4. Debug with Ephemeral Container\n"
                "```bash\n"
                "kubectl debug <pod-name> -it --image=busybox --target=<container-name>\n"
                "```\n\n"
                "### 5. Override Entrypoint for Investigation\n"
                "```yaml\n"
                "# Temporarily override command to prevent crash\n"
                "command: ['sleep', '3600']\n"
                "```\n"
                "Then exec into the pod to investigate the filesystem and configuration.\n"
            ),
            "articleType": TROUBLESHOOTING,
            "isInternal": True,
        },
        {
            "title": f"Reference: On-Call Rotation and Escalation Contacts {ts}",
            "shortDescription": "Current on-call rotation schedule and escalation contact information for IT teams.",
            "articleBody": (
                "## On-Call Teams\n"
                "| Team | Coverage | Rotation |\n"
                "|------|----------|----------|\n"
                "| Infrastructure | 24/7 | Weekly |\n"
                "| Network | 24/7 | Weekly |\n"
                "| Database | Mon-Fri extended, weekend on-call | Bi-weekly |\n"
                "| Application Support | Mon-Fri 06:00-22:00 UTC | Weekly |\n"
                "| Security | 24/7 | Weekly |\n\n"
                "## Escalation Order\n"
                "```\n"
                "1. On-Call Engineer (via PagerDuty)\n"
                "2. Team Lead (15 min no response)\n"
                "3. Manager (30 min no response or P1)\n"
                "4. Director (P1 > 2 hours unresolved)\n"
                "5. VP/CIO (P1 > 4 hours, major business impact)\n"
                "```\n\n"
                "## Contact Methods\n"
                "| Priority | Contact Method |\n"
                "|----------|----------------|\n"
                "| P1 Critical | PagerDuty alert → Phone call → SMS |\n"
                "| P2 High | PagerDuty alert → Slack #incidents |\n"
                "| P3-P5 | Ticket assignment → Email notification |\n\n"
                "## Key Contacts (Non-Rotational)\n"
                "- **Major Incident Manager**: incident-manager@corp.local\n"
                "- **IT Service Desk**: servicedesk@corp.local / ext. 5555\n"
                "- **Security Operations**: soc@corp.local / ext. 5599\n"
                "- **Change Manager**: change-manager@corp.local\n"
                "- **Problem Manager**: problem-manager@corp.local\n\n"
                "## Vendor Escalation\n"
                "| Vendor | Support Portal | Contract Level |\n"
                "|--------|----------------|----------------|\n"
                "| Microsoft | premier.microsoft.com | Premier |\n"
                "| VMware | my.vmware.com | Production |\n"
                "| Cisco | mycase.cloudapps.cisco.com | SmartNet |\n"
                "| Oracle | support.oracle.com | Standard |\n"
            ),
            "articleType": REFERENCE,
            "isInternal": True,
        },
        {
            "title": f"How to Perform a Root Cause Analysis (RCA) After a Major Incident {ts}",
            "shortDescription": "Step-by-step guide for conducting an effective blameless post-incident review.",
            "articleBody": (
                "## Timeline\n"
                "- RCA meeting must be held within 5 business days of incident resolution\n"
                "- Draft RCA document due within 3 business days after the meeting\n"
                "- Action items must have owners and due dates\n\n"
                "## Before the Meeting\n"
                "1. Gather all incident timeline data (ticket, alerts, chat logs)\n"
                "2. Identify all participants (responders, affected teams, management)\n"
                "3. Schedule 60-90 minute meeting\n"
                "4. Send pre-read: incident timeline, impact summary\n\n"
                "## Meeting Agenda\n"
                "1. **Ground Rules** (5 min) — Blameless, focus on systems not people\n"
                "2. **Timeline Review** (20 min) — Walk through events chronologically\n"
                "3. **5 Whys Analysis** (15 min) — Drill into root causes\n"
                "4. **Contributing Factors** (10 min) — What made it worse\n"
                "5. **What Went Well** (5 min) — Celebrate good responses\n"
                "6. **Action Items** (15 min) — Concrete, assignable, time-bound\n\n"
                "## 5 Whys Example\n"
                "1. Why did the website go down? → Database server ran out of disk\n"
                "2. Why did the disk fill up? → Log files grew uncontrolled\n"
                "3. Why weren't logs rotated? → Log rotation was disabled during migration\n"
                "4. Why wasn't it re-enabled? → Not in the migration checklist\n"
                "5. Why wasn't it in the checklist? → Checklist not reviewed/updated since 2023\n\n"
                "**Root Cause**: Change management checklist not maintained.\n"
                "**Action**: Review and update all operational checklists quarterly.\n\n"
                "## RCA Document Template\n"
                "Use the template at: `templates/RCA-TEMPLATE.docx`\n"
                "Store completed RCAs in: `SharePoint/IT/RCA/{Year}/`\n"
            ),
            "articleType": HOWTO,
            "isInternal": True,
        },
        {
            "title": f"FAQ: What Are the Standard Maintenance Windows? {ts}",
            "shortDescription": "Schedule and rules for planned maintenance windows that may affect services.",
            "articleBody": (
                "## Regular Maintenance Windows\n"
                "| Window | Schedule | Duration | Scope |\n"
                "|--------|----------|----------|-------|\n"
                "| Weekly | Saturday 02:00-06:00 UTC | 4 hours | Minor patches, config changes |\n"
                "| Monthly | 1st Saturday, 00:00-08:00 UTC | 8 hours | OS patches, major updates |\n"
                "| Quarterly | As planned (Change Calendar) | 12 hours | Major upgrades, migrations |\n\n"
                "## Rules\n"
                "- All maintenance requires an approved Change Request\n"
                "- Maintenance notifications sent 5 business days in advance\n"
                "- Emergency maintenance: 4-hour notice minimum when possible\n"
                "- No planned maintenance during business-critical periods:\n"
                "  - Month-end close (last 3 business days)\n"
                "  - Year-end close (Dec 15 - Jan 5)\n"
                "  - Major product launch dates\n\n"
                "## Impact During Maintenance\n"
                "- Services may be unavailable or degraded during the window\n"
                "- Redundant services (email, VPN) should remain available\n"
                "- Status updates posted to: https://status.corp.local\n\n"
                "## How to Request a Maintenance Window\n"
                "1. Submit a Normal or Emergency Change Request\n"
                "2. Include: impact assessment, implementation plan, rollback plan\n"
                "3. Get CAB approval (for Normal changes)\n"
                "4. Schedule within the next available window\n"
            ),
            "articleType": FAQ,
            "isInternal": False,
        },
    ])

    # ══════════════════════════════════════════════════════════════════════
    #  CLOUD & DEVOPS — Additional technical articles
    # ══════════════════════════════════════════════════════════════════════
    articles.extend([
        {
            "title": f"How to Set Up AWS IAM Roles for ECS Task Definitions {ts}",
            "shortDescription": "Configure IAM roles and policies for ECS Fargate tasks with least-privilege access.",
            "articleBody": (
                "## Overview\n"
                "ECS tasks need two IAM roles:\n"
                "1. **Task Execution Role** — For ECS agent (pull images, write logs)\n"
                "2. **Task Role** — For your application code (access S3, SQS, etc.)\n\n"
                "## Step 1: Create Task Execution Role\n"
                "```json\n"
                "{\n"
                '  "Version": "2012-10-17",\n'
                '  "Statement": [{\n'
                '    "Effect": "Allow",\n'
                '    "Principal": {"Service": "ecs-tasks.amazonaws.com"},\n'
                '    "Action": "sts:AssumeRole"\n'
                "  }]\n"
                "}\n"
                "```\n"
                "Attach policy: `AmazonECSTaskExecutionRolePolicy`\n\n"
                "## Step 2: Create Task Role\n"
                "Same trust policy as above. Attach custom policy for your app needs:\n"
                "```json\n"
                "{\n"
                '  "Version": "2012-10-17",\n'
                '  "Statement": [\n'
                "    {\n"
                '      "Effect": "Allow",\n'
                '      "Action": ["s3:GetObject", "s3:PutObject"],\n'
                '      "Resource": "arn:aws:s3:::crm-prod-uploads/*"\n'
                "    },\n"
                "    {\n"
                '      "Effect": "Allow",\n'
                '      "Action": ["sqs:SendMessage", "sqs:ReceiveMessage"],\n'
                '      "Resource": "arn:aws:sqs:us-east-1:123456789:crm-*"\n'
                "    }\n"
                "  ]\n"
                "}\n"
                "```\n\n"
                "## Step 3: Reference in Task Definition\n"
                "```json\n"
                "{\n"
                '  "taskRoleArn": "arn:aws:iam::123456789:role/crm-task-role",\n'
                '  "executionRoleArn": "arn:aws:iam::123456789:role/crm-task-execution-role"\n'
                "}\n"
                "```\n\n"
                "## Best Practices\n"
                "- Scope permissions to specific resources (not `*`)\n"
                "- Use condition keys for extra security\n"
                "- Enable CloudTrail for role usage auditing\n"
                "- Review permissions quarterly with IAM Access Analyzer\n"
            ),
            "articleType": HOWTO,
            "isInternal": True,
        },
        {
            "title": f"Troubleshooting: Terraform Apply Fails with State Lock Error {ts}",
            "shortDescription": "Resolve Terraform state lock issues when apply or plan operations fail.",
            "articleBody": (
                "## Error Message\n"
                "```\n"
                "Error: Error acquiring the state lock\n"
                "Lock Info:\n"
                "  ID:        xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx\n"
                "  Path:      s3://terraform-state-bucket/prod/terraform.tfstate\n"
                "  Operation: OperationTypeApply\n"
                "  Created:   2026-03-08T10:30:00Z\n"
                "```\n\n"
                "## Diagnosis\n"
                "1. **Check if another apply is running** — Ask team members\n"
                "2. **Check if previous run crashed** — Often caused by interrupted pipelines\n"
                "3. **Check DynamoDB lock table**:\n"
                "   ```bash\n"
                "   aws dynamodb scan --table-name terraform-locks \\\n"
                "     --filter-expression 'LockID = :id' \\\n"
                "     --expression-attribute-values '{\":id\":{\"S\":\"s3://terraform-state-bucket/prod/terraform.tfstate\"}}'\n"
                "   ```\n\n"
                "## Resolution\n\n"
                "### If No Other Apply Is Running (Stale Lock)\n"
                "```bash\n"
                "terraform force-unlock <LOCK-ID>\n"
                "# Confirm with 'yes'\n"
                "```\n\n"
                "### If Pipeline Crashed\n"
                "1. Check CI/CD pipeline logs for the failed run\n"
                "2. Verify no partial changes were applied\n"
                "3. Force unlock\n"
                "4. Run `terraform plan` to verify state consistency\n\n"
                "## Prevention\n"
                "- Set CI/CD timeouts to prevent hung jobs\n"
                "- Use `-lock-timeout=5m` for automatic retry\n"
                "- Notify team in #infrastructure before applies\n"
                "- Use workspace-per-env to avoid contention\n"
            ),
            "articleType": TROUBLESHOOTING,
            "isInternal": True,
        },
        {
            "title": f"Known Error: .NET 10 Application Intermittent 502 Behind Nginx Reverse Proxy {ts}",
            "shortDescription": "ASP.NET Core 10 apps may return 502 errors under high concurrency when behind Nginx with keepalive.",
            "articleBody": (
                "## Identifier: KE-DOTNET-005\n\n"
                "## Affected Systems\n"
                "- ASP.NET Core 10.0 applications\n"
                "- Nginx reverse proxy with upstream keepalive\n"
                "- Linux containers (Docker/Kubernetes)\n\n"
                "## Symptoms\n"
                "- Intermittent HTTP 502 Bad Gateway errors (0.1-0.5% of requests)\n"
                "- Occurs under medium-to-high concurrency (>100 concurrent connections)\n"
                "- Nginx error log: `upstream prematurely closed connection`\n\n"
                "## Root Cause\n"
                "Race condition between Kestrel's keepalive timeout and Nginx's upstream "
                "keepalive. When Kestrel closes a connection that Nginx is about to reuse, "
                "the request fails with 502.\n\n"
                "## Workaround\n"
                "Set Kestrel's keepalive timeout higher than Nginx's:\n\n"
                "**Kestrel (Program.cs or config):**\n"
                "```csharp\n"
                "builder.WebHost.ConfigureKestrel(options =>\n"
                "{\n"
                "    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5);\n"
                "});\n"
                "```\n\n"
                "**Nginx:**\n"
                "```nginx\n"
                "upstream backend {\n"
                "    server crm-api:5000;\n"
                "    keepalive 32;\n"
                "    keepalive_timeout 60s;  # Must be less than Kestrel's\n"
                "}\n"
                "```\n\n"
                "## Permanent Fix\n"
                "Under investigation by the .NET team. Tracked as dotnet/aspnetcore#54321.\n\n"
                "## Status: Open | Priority: Medium | Assigned: Platform Team\n"
            ),
            "articleType": KNOWN_ERROR,
            "isInternal": True,
        },
    ])

    return articles


# ---------------------------------------------------------------------------
# Main batch runner
# ---------------------------------------------------------------------------

def run(api: ApiClient, log: RunLogger) -> None:
    log.section("BATCH 24: ITSM Knowledge Base — Extensive Technical Articles")
    ts = int(time.time())

    articles = _itsm_articles(ts)
    article_ids = []

    log.log(f"Loading {len(articles)} ITSM knowledge articles ...")

    for i, article in enumerate(articles):
        payload = {k: v for k, v in article.items() if v is not None}
        eid = api.create_and_track("knowledgearticles", "/api/itsm/knowledge", payload)
        if eid:
            article_ids.append(eid)

    save_ids("itsm_knowledge_articles", article_ids)
    log.log(f"Created {len(article_ids)} of {len(articles)} ITSM knowledge articles")

    # ── Read-back verification ──
    log.section("ITSM KB — Verification Queries")
    api.get("/api/itsm/knowledge/articles")
    api.get("/api/itsm/knowledge/popular")
    api.get("/api/itsm/knowledge/recent")
    api.get("/api/itsm/knowledge/search?searchTerm=VPN")
    api.get("/api/itsm/knowledge/search?searchTerm=Active+Directory")
    api.get("/api/itsm/knowledge/search?searchTerm=Kubernetes")
    api.get("/api/itsm/knowledge/search?searchTerm=backup")
    api.get("/api/itsm/knowledge/categories")

    # Publish a subset of articles (first 10)
    log.section("ITSM KB — Publishing Articles")
    for aid in article_ids[:10]:
        api.patch(f"/api/itsm/knowledge/{aid}/publish")

    # Submit feedback on first article
    if article_ids:
        api.post(f"/api/itsm/knowledge/{article_ids[0]}/feedback",
                 {"helpful": True, "comments": "Very detailed and well-structured guide."})

    log.log(f"BATCH 24 complete. IDs saved: {len(article_ids)}")

#!/usr/bin/env python3
"""SSL certificate management routes for CDT."""

import os
import subprocess
import tempfile
from pathlib import Path

from flask import Blueprint, request, jsonify

ssl_bp = Blueprint("ssl", __name__)

# Default storage dir for generated certs
_SSL_DIR = Path.home() / ".crm-cdt" / "ssl"


def _ensure_ssl_dir() -> Path:
    _SSL_DIR.mkdir(parents=True, exist_ok=True)
    return _SSL_DIR


@ssl_bp.route("/api/ssl/generate", methods=["POST"])
def generate_self_signed():
    """Generate a self-signed SSL certificate + key pair.

    Body:
        domain (str): Common Name / domain (default: localhost)
        days (int): Validity in days (default: 365)
        org (str): Organisation name (default: CRM Solution)
    """
    body = request.json or {}
    domain = body.get("domain", "localhost")
    days = int(body.get("days", 365))
    org = body.get("org", "CRM Solution")

    ssl_dir = _ensure_ssl_dir()
    cert_path = ssl_dir / f"{domain}.crt"
    key_path = ssl_dir / f"{domain}.key"

    # Build openssl command
    subject = f"/C=US/ST=State/L=City/O={org}/CN={domain}"
    cmd = [
        "openssl", "req", "-x509", "-newkey", "rsa:4096",
        "-keyout", str(key_path),
        "-out", str(cert_path),
        "-days", str(days),
        "-nodes",
        "-subj", subject,
        "-addext", f"subjectAltName=DNS:{domain},DNS:*.{domain},IP:127.0.0.1",
    ]

    try:
        result = subprocess.run(
            cmd, capture_output=True, text=True, timeout=30,
        )
        if result.returncode != 0:
            return jsonify({
                "error": f"openssl failed: {result.stderr.strip()}",
            }), 500

        return jsonify({
            "success": True,
            "domain": domain,
            "cert_path": str(cert_path),
            "key_path": str(key_path),
            "valid_days": days,
            "message": f"Self-signed certificate generated for {domain}",
        })
    except FileNotFoundError:
        return jsonify({
            "error": "openssl is not installed or not in PATH",
        }), 500
    except subprocess.TimeoutExpired:
        return jsonify({"error": "Certificate generation timed out"}), 500
    except Exception as exc:
        return jsonify({"error": str(exc)}), 500


@ssl_bp.route("/api/ssl/upload", methods=["POST"])
def upload_certificate():
    """Upload SSL certificate and key files.

    Expects multipart/form-data with 'cert' and 'key' file fields.
    Optional form field: 'domain' (used for naming).
    """
    cert_file = request.files.get("cert")
    key_file = request.files.get("key")

    if not cert_file:
        return jsonify({"error": "Certificate file ('cert') is required."}), 400
    if not key_file:
        return jsonify({"error": "Key file ('key') is required."}), 400

    domain = request.form.get("domain", "custom")
    ssl_dir = _ensure_ssl_dir()

    cert_path = ssl_dir / f"{domain}.crt"
    key_path = ssl_dir / f"{domain}.key"

    cert_file.save(str(cert_path))
    key_file.save(str(key_path))

    # Basic validation: try to read the cert with openssl
    try:
        result = subprocess.run(
            ["openssl", "x509", "-in", str(cert_path), "-noout", "-subject", "-enddate"],
            capture_output=True, text=True, timeout=10,
        )
        cert_info = result.stdout.strip() if result.returncode == 0 else "Could not parse certificate"
    except Exception:
        cert_info = "Could not validate certificate"

    return jsonify({
        "success": True,
        "domain": domain,
        "cert_path": str(cert_path),
        "key_path": str(key_path),
        "cert_info": cert_info,
        "message": f"Certificate uploaded for {domain}",
    })


@ssl_bp.route("/api/ssl/status", methods=["GET"])
def ssl_status():
    """Return info about existing SSL certificates."""
    ssl_dir = _ensure_ssl_dir()
    certs = []

    for cert_file in sorted(ssl_dir.glob("*.crt")):
        domain = cert_file.stem
        key_file = ssl_dir / f"{domain}.key"
        info = {"domain": domain, "cert_path": str(cert_file), "has_key": key_file.exists()}

        # Get cert details
        try:
            result = subprocess.run(
                ["openssl", "x509", "-in", str(cert_file), "-noout", "-subject", "-enddate", "-issuer"],
                capture_output=True, text=True, timeout=10,
            )
            if result.returncode == 0:
                for line in result.stdout.strip().split("\n"):
                    if line.startswith("subject="):
                        info["subject"] = line.split("=", 1)[1].strip()
                    elif line.startswith("notAfter="):
                        info["expires"] = line.split("=", 1)[1].strip()
                    elif line.startswith("issuer="):
                        info["issuer"] = line.split("=", 1)[1].strip()
        except Exception:
            pass

        certs.append(info)

    return jsonify({"certificates": certs, "ssl_dir": str(ssl_dir)})


@ssl_bp.route("/api/ssl/delete/<domain>", methods=["DELETE"])
def delete_certificate(domain: str):
    """Delete a certificate pair by domain name."""
    ssl_dir = _ensure_ssl_dir()
    cert_path = ssl_dir / f"{domain}.crt"
    key_path = ssl_dir / f"{domain}.key"

    deleted = []
    for p in (cert_path, key_path):
        if p.exists():
            p.unlink()
            deleted.append(p.name)

    if not deleted:
        return jsonify({"error": f"No certificate found for '{domain}'"}), 404

    return jsonify({"deleted": deleted, "domain": domain})

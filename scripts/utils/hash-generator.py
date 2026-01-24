#!/usr/bin/env python3
import hashlib
import base64

password = "Microsoft@1"
sha256_hash = hashlib.sha256(password.encode('utf-8')).digest()
base64_hash = base64.b64encode(sha256_hash).decode('utf-8')

print(f"Password: {password}")
print(f"SHA256 (hex): {sha256_hash.hex()}")
print(f"Base64: {base64_hash}")

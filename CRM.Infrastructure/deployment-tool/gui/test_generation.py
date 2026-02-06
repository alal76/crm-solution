import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent.parent))

from app import generate_docker_compose, generate_env_file, generate_deployment_script

config = {
    'database_type': 'mariadb',
    'search_provider': 'meilisearch', 
    'chat_provider': 'chatwoot',
    'notification_provider': 'novu',
    'analytics_provider': 'superset',
    'signature_provider': 'docuseal',
    'ai_provider': 'ollama'
}

print("Testing generate_docker_compose...")
docker_compose = generate_docker_compose(config)
print(f"Type: {type(docker_compose)}")
print(f"Length: {len(docker_compose)}")
print("First 300 chars:")
print(docker_compose[:300])
print("\nContains 'version:':", 'version:' in docker_compose)

print("\nTesting generate_env_file...")
env_file = generate_env_file(config)
print(f"Type: {type(env_file)}")
print(f"Length: {len(env_file)}")
print("First 200 chars:")
print(env_file[:200])

print("\nTesting generate_deployment_script...")
script = generate_deployment_script(config)
print(f"Type: {type(script)}")
print(f"Length: {len(script)}")
print("First 200 chars:")
print(script[:200])

# Write files
output_dir = Path(__file__).parent.parent / "generated"
output_dir.mkdir(exist_ok=True)

with open(output_dir / "docker-compose.yml", "w") as f:
    f.write(docker_compose)

with open(output_dir / "test.env", "w") as f:
    f.write(env_file)

with open(output_dir / "test-deploy.sh", "w") as f:
    f.write(script)

print(f"\nFiles written to {output_dir}")
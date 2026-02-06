import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent.parent))
from app import generate_docker_compose
config = {'database_type': 'mariadb', 'search_provider': 'meilisearch', 'chat_provider': 'chatwoot', 'notification_provider': 'novu', 'analytics_provider': 'superset', 'signature_provider': 'docuseal', 'ai_provider': 'ollama'}
result = generate_docker_compose(config)
print('Result type:', type(result))
print('First 200 chars:')
print(result[:200])
print('Contains version:', 'version:' in result)

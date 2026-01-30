#!/bin/bash
# Launch CRM Solution Deployment Tool
# Version: 2.0.0

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo "==========================================="
echo "   CRM Solution Deployment Tool v2.0.0    "
echo "==========================================="
echo ""

# Check Python version
find_python() {
    # Try homebrew Python first (usually has better tkinter support on macOS)
    if [ -x "/opt/homebrew/bin/python3" ]; then
        echo "/opt/homebrew/bin/python3"
        return
    fi
    if [ -x "/usr/local/bin/python3" ]; then
        echo "/usr/local/bin/python3"
        return
    fi
    # Try system Python
    if command -v python3 &> /dev/null; then
        echo "python3"
        return
    fi
    if command -v python &> /dev/null; then
        echo "python"
        return
    fi
    echo ""
}

PYTHON_CMD=$(find_python)

if [ -z "$PYTHON_CMD" ]; then
    echo "ERROR: Python 3 is required but not found."
    echo ""
    echo "Please install Python 3.9+ using one of these methods:"
    echo "  • macOS: brew install python"
    echo "  • Or download from https://python.org"
    exit 1
fi

# Check Python version is 3.9+
PY_VERSION=$($PYTHON_CMD -c "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')")
echo "Found Python $PY_VERSION at: $PYTHON_CMD"

# Check tkinter is available and works
echo "Checking tkinter..."
if ! $PYTHON_CMD -c "import tkinter" 2>/dev/null; then
    echo "ERROR: tkinter is not available."
    echo ""
    echo "Install tkinter using:"
    echo "  • macOS: brew install python-tk@3.11"
    echo "  • Ubuntu/Debian: sudo apt-get install python3-tk"
    echo "  • Fedora: sudo dnf install python3-tkinter"
    exit 1
fi

# Test if tkinter actually works (macOS Sonoma issue check)
if ! $PYTHON_CMD -c "import tkinter; tkinter.Tcl()" 2>/dev/null; then
    echo ""
    echo "WARNING: tkinter has a compatibility issue on this system."
    echo ""
    echo "This is often caused by an outdated Tcl/Tk on macOS."
    echo "To fix this, install Python via Homebrew:"
    echo ""
    echo "  brew install python-tk@3.11"
    echo "  brew link python@3.11"
    echo ""
    echo "Or use the Python installer from python.org"
    echo ""
    
    # Offer CLI fallback
    echo "Would you like to use command-line mode instead? (y/n)"
    read -r response
    if [ "$response" = "y" ]; then
        cd "$SCRIPT_DIR/deployment-tool"
        $PYTHON_CMD -c "
import sys
sys.path.insert(0, '.')
from main import DeploymentConfig, DeploymentEngine
print('CLI mode - generating scripts only...')
config = DeploymentConfig()
engine = DeploymentEngine(config, lambda t, m: print(f'[{t}] {m}'))
print('Environment file content:')
print(engine.generate_env_file())
print('\\n---\\nDocker Compose content:')
print(engine.generate_docker_compose())
"
        exit 0
    fi
    exit 1
fi

echo "tkinter is working ✓"
echo ""

# Run the tool
cd "$SCRIPT_DIR/deployment-tool"
$PYTHON_CMD main.py

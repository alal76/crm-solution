#!/usr/bin/env python3
"""
CRM CDT – SocketIO helper module (singleton pattern).

Usage
-----
In app.py (after creating the SocketIO instance):
    from core.socket_helpers import init_socketio
    init_socketio(socketio)

In any route / service that needs to push log events:
    from core.socket_helpers import emit_log, start_background_task
    emit_log(run_id, "INFO", "Step 1: pulling images...")
"""
from __future__ import annotations
from typing import Callable, Any

_socketio = None  # Set once via init_socketio()


def init_socketio(sio: Any) -> None:
    """Register the Flask-SocketIO instance (called once from app.py)."""
    global _socketio
    _socketio = sio


def is_available() -> bool:
    """Return True if SocketIO has been initialised."""
    return _socketio is not None


def emit_log(run_id: str, level: str, message: str, extra: dict | None = None) -> None:
    """
    Emit a structured log event to the room ``deploy_<run_id>``.

    Parameters
    ----------
    run_id:  Deployment run identifier (matches the job started by deploy_routes).
    level:   Severity string – DEBUG | INFO | WARN | ERROR | SUCCESS.
    message: Human-readable log line.
    extra:   Optional dict of additional fields merged into the payload.
    """
    if _socketio is None:
        return
    payload: dict = {"run_id": run_id, "level": level.upper(), "message": message}
    if extra:
        payload.update(extra)
    try:
        _socketio.emit("log", payload, room=f"deploy_{run_id}")
    except Exception:  # noqa: BLE001
        pass


def emit_step(run_id: str, step_index: int, state: str, label: str = "") -> None:
    """
    Emit a step-state change for the animated deploy step cards in the wizard.

    Parameters
    ----------
    step_index: 0-based index matching DEPLOY_STEP_LABELS in wizard.html.
    state:      'pending' | 'running' | 'done' | 'error' | 'skip'.
    label:      Optional override label for the step card.
    """
    if _socketio is None:
        return
    try:
        _socketio.emit(
            "deploy_step",
            {"run_id": run_id, "step": step_index, "state": state, "label": label},
            room=f"deploy_{run_id}",
        )
    except Exception:  # noqa: BLE001
        pass


def emit_progress(run_id: str, pct: int, status: str = "") -> None:
    """Emit an overall percentage progress update (0–100)."""
    if _socketio is None:
        return
    try:
        _socketio.emit(
            "deploy_progress",
            {"run_id": run_id, "pct": max(0, min(100, pct)), "status": status},
            room=f"deploy_{run_id}",
        )
    except Exception:  # noqa: BLE001
        pass


def emit_done(run_id: str, success: bool, result: dict | None = None) -> None:
    """Emit the final deployment-complete event."""
    if _socketio is None:
        return
    try:
        _socketio.emit(
            "deploy_done",
            {"run_id": run_id, "success": success, "result": result or {}},
            room=f"deploy_{run_id}",
        )
    except Exception:  # noqa: BLE001
        pass


def start_background_task(fn: Callable, *args: Any, **kwargs: Any) -> Any:
    """
    Start a background task using SocketIO's thread-safe scheduler when available,
    falling back to a plain daemon thread.

    Returns the thread / greenlet handle.
    """
    if _socketio is not None:
        return _socketio.start_background_task(fn, *args, **kwargs)
    import threading  # noqa: PLC0415
    t = threading.Thread(target=fn, args=args, kwargs=kwargs, daemon=True)
    t.start()
    return t

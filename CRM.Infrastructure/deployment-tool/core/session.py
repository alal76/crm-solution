#!/usr/bin/env python3
"""
core/session.py — Wizard session management for the CRM Deployment Tool.

A ``WizardSession`` holds all answers collected by the multi-step wizard.
Each step's answers are stored under a dict keyed by step ID.  The session
can be serialised to a full profile dict (``to_profile()``) that can be
saved by ``ProfileManager``, or populated from an existing profile
(``from_profile()``).

The ``SessionStore`` provides a simple in-memory store with TTL-based cleanup.
"""

from __future__ import annotations

import uuid
from dataclasses import dataclass, field
from datetime import datetime, timedelta, timezone
from typing import Optional

# ---------------------------------------------------------------------------
# Total number of wizard steps (used for percentage calculation)
# ---------------------------------------------------------------------------
_TOTAL_STEPS = 13


# ---------------------------------------------------------------------------
# WizardSession
# ---------------------------------------------------------------------------


@dataclass
class WizardSession:
    """Holds wizard state for a single browser session.

    Attributes
    ----------
    session_id:
        Unique identifier assigned by :class:`SessionStore`.
    profile_name:
        Name of the profile being edited (populated from the ``profile`` step).
    current_step:
        ID of the step currently displayed.
    completed_steps:
        Ordered list of step IDs that have been submitted.
    data:
        Dict mapping step ID → form data dict submitted for that step.
    _created_at:
        Internal timestamp used for TTL eviction (not serialised).
    """

    session_id: str
    profile_name: str = ""
    current_step: str = "welcome"
    completed_steps: list[str] = field(default_factory=list)
    data: dict = field(default_factory=dict)
    _created_at: datetime = field(
        default_factory=lambda: datetime.now(timezone.utc), compare=False, repr=False
    )

    # ------------------------------------------------------------------
    # Step helpers
    # ------------------------------------------------------------------

    def step_complete(self, step_id: str) -> bool:
        """Return ``True`` if *step_id* has already been completed."""
        return step_id in self.completed_steps

    def get_step_data(self, step_id: str) -> dict:
        """Return form data for *step_id*, or ``{}`` if not yet submitted."""
        return self.data.get(step_id, {})

    def set_step_data(self, step_id: str, form_data: dict) -> None:
        """Store *form_data* for *step_id* and mark the step as completed."""
        self.data[step_id] = form_data
        if step_id not in self.completed_steps:
            self.completed_steps.append(step_id)
        # Update profile_name shortcut if this is the profile step
        if step_id == "profile":
            self.profile_name = form_data.get("profile_name", self.profile_name)

    # ------------------------------------------------------------------
    # Profile serialisation
    # ------------------------------------------------------------------

    def to_profile(self) -> dict:
        """Build and return a full profile dict from all collected step data.

        Returns
        -------
        dict
            A profile dict with sections: ``meta``, ``target``,
            ``architecture``, ``database``, ``network``, ``security``,
            ``providers``, ``seed``.
        """
        now = datetime.now(timezone.utc).isoformat()
        return {
            "meta": {
                "profile_name": self.profile_name or self.get_step_data("profile").get("profile_name", ""),
                "created_at": now,
                "updated_at": now,
                "crm_version": "0.608.1",
            },
            "target": self.get_step_data("target"),
            "architecture": self.get_step_data("architecture"),
            "database": self.get_step_data("database"),
            "network": self.get_step_data("network"),
            "security": self.get_step_data("security"),
            "providers": self.get_step_data("providers"),
            "seed": self.get_step_data("seed"),
        }

    def from_profile(self, data: dict) -> None:
        """Populate session step data from an existing profile dict.

        Marks each top-level section (step) as completed so the wizard shows
        the steps as already filled in.

        Parameters
        ----------
        data:
            A profile dict as returned by ``ProfileManager.load()``.
        """
        section_to_step = {
            "target": "target",
            "architecture": "architecture",
            "database": "database",
            "network": "network",
            "security": "security",
            "providers": "providers",
            "seed": "seed",
        }
        # Restore profile metadata into the profile step
        meta = data.get("meta", {})
        profile_name = meta.get("profile_name", "")
        if profile_name:
            self.set_step_data("profile", {"profile_name": profile_name})
            self.profile_name = profile_name

        for section, step_id in section_to_step.items():
            section_data = data.get(section, {})
            if section_data:
                self.set_step_data(step_id, section_data)

    def percent_complete(self) -> int:
        """Return a 0–100 integer representing wizard completion percentage."""
        if _TOTAL_STEPS == 0:
            return 0
        return min(100, int(len(self.completed_steps) / _TOTAL_STEPS * 100))


# ---------------------------------------------------------------------------
# SessionStore
# ---------------------------------------------------------------------------


class SessionStore:
    """Simple in-memory session store with TTL eviction.

    Sessions are stored in a plain Python dict.  The store is **not**
    thread-safe by itself; for production use wrap calls in a lock or use
    Flask-Session with a real backend.
    """

    def __init__(self) -> None:
        self._sessions: dict[str, WizardSession] = {}

    def create(self) -> WizardSession:
        """Create and return a new :class:`WizardSession` with a unique ID."""
        session = WizardSession(session_id=str(uuid.uuid4()))
        self._sessions[session.session_id] = session
        return session

    def get(self, session_id: str) -> Optional[WizardSession]:
        """Return the session for *session_id*, or ``None`` if not found."""
        return self._sessions.get(session_id)

    def update(self, session: WizardSession) -> None:
        """Store (or overwrite) *session* in the store."""
        self._sessions[session.session_id] = session

    def cleanup_expired(self, max_age_hours: int = 24) -> int:
        """Remove sessions older than *max_age_hours*.

        Returns
        -------
        int
            Number of sessions removed.
        """
        cutoff = datetime.now(timezone.utc) - timedelta(hours=max_age_hours)
        expired = [
            sid
            for sid, sess in self._sessions.items()
            if sess._created_at < cutoff
        ]
        for sid in expired:
            del self._sessions[sid]
        return len(expired)

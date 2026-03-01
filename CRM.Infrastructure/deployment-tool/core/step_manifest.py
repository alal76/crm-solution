#!/usr/bin/env python3
"""
core/step_manifest.py — Wizard step manifest loader for the CRM Deployment Tool.

Loads a YAML file (``steps.yaml``) that defines all wizard steps and their
fields.  Provides helpers for step ordering, lookup by ID, and conditional
field evaluation.

YAML format example::

    steps:
      - id: target
        title: "Target"
        template: target.html
        description: "Select deployment target"
        fields:
          - id: provider
            type: select
            label: "Cloud / Platform"
            required: true
            default: local_docker
            options:
              - value: local_docker
                label: "Local Docker"
            conditional: ""
            validators: [required]
"""

from __future__ import annotations

import re
from dataclasses import dataclass, field
from pathlib import Path
from typing import Any, Optional

import yaml


# ---------------------------------------------------------------------------
# Dataclasses
# ---------------------------------------------------------------------------


@dataclass
class StepField:
    """Descriptor for a single form field within a wizard step.

    Attributes
    ----------
    id:
        Unique field identifier within the step (e.g. ``provider``).
    type:
        HTML input type hint: ``text``, ``select``, ``password``, ``toggle``,
        ``number``, ``textarea``, ``email``.
    label:
        Human-readable label shown next to the field.
    required:
        Whether the field must be filled before proceeding.
    default:
        Default value pre-populated in the field.
    options:
        For ``select`` fields — list of ``{value, label}`` dicts.
    help_text:
        Instructional text rendered below the field.
    conditional:
        A simple expression string that determines whether the field is shown,
        e.g. ``"provider != local_docker"``.  Empty string means always shown.
    validators:
        List of validator names to apply on the backend
        (e.g. ``["required", "min_length_32_or_empty"]``).
    """

    id: str
    type: str
    label: str
    required: bool = False
    default: Any = None
    options: Optional[list[dict]] = None
    help_text: str = ""
    conditional: str = ""
    validators: Optional[list[str]] = None


@dataclass
class WizardStep:
    """Descriptor for a single wizard step.

    Attributes
    ----------
    id:
        Unique step identifier (e.g. ``target``).
    title:
        Short title shown in the progress bar.
    template:
        Jinja2 template filename (relative to ``gui/templates/steps/``).
    fields:
        Ordered list of :class:`StepField` objects for this step.
    description:
        Longer description shown at the top of the step.
    """

    id: str
    title: str
    template: str
    fields: list[StepField] = field(default_factory=list)
    description: str = ""


# ---------------------------------------------------------------------------
# StepManifestLoader
# ---------------------------------------------------------------------------


class StepManifestLoader:
    """Loads and indexes wizard steps from a YAML manifest file.

    Usage::

        loader = StepManifestLoader()
        steps = loader.load("steps.yaml")
        target_step = loader.get_step("target")
        all_ids = loader.get_ordered_ids()
    """

    def __init__(self) -> None:
        self._steps: list[WizardStep] = []
        self._index: dict[str, WizardStep] = {}

    # ------------------------------------------------------------------
    # Loading
    # ------------------------------------------------------------------

    def load(self, path: str) -> list[WizardStep]:
        """Parse *path* (YAML file) and return ordered :class:`WizardStep` list.

        Also populates the internal index so that :meth:`get_step` and
        :meth:`get_ordered_ids` work after this call.

        Parameters
        ----------
        path:
            Absolute or relative path to the ``steps.yaml`` manifest.

        Returns
        -------
        list[WizardStep]
            Ordered list of all steps defined in the manifest.
        """
        yaml_path = Path(path)
        raw = yaml_path.read_text(encoding="utf-8")
        doc = yaml.safe_load(raw)

        steps_raw: list[dict] = doc.get("steps", [])
        self._steps = [self._parse_step(s) for s in steps_raw]
        self._index = {s.id: s for s in self._steps}
        return self._steps

    def get_step(self, step_id: str) -> Optional[WizardStep]:
        """Return the :class:`WizardStep` with *step_id*, or ``None``."""
        return self._index.get(step_id)

    def get_ordered_ids(self) -> list[str]:
        """Return the step IDs in manifest order."""
        return [s.id for s in self._steps]

    # ------------------------------------------------------------------
    # Conditional evaluation
    # ------------------------------------------------------------------

    def evaluate_conditional(self, condition_str: str, session_data: dict) -> bool:
        """Evaluate a simple conditional expression against *session_data*.

        Supported operators:
        - ``field == value``
        - ``field != value``
        - ``field in [val1, val2, ...]``
        - ``field not_in [val1, val2, ...]``

        Empty *condition_str* always returns ``True`` (field is shown).

        Parameters
        ----------
        condition_str:
            Expression string, e.g. ``"provider != local_docker"`` or
            ``"provider in [aws, azure, gcp]"``.
        session_data:
            Flat dict of current wizard answers (all steps merged).

        Returns
        -------
        bool
            Whether the condition is currently satisfied (i.e. field visible).
        """
        if not condition_str or not condition_str.strip():
            return True

        condition_str = condition_str.strip()

        # Handle: field in [val1, val2, ...]
        in_match = re.match(r"^(\w+)\s+in\s+\[([^\]]*)\]$", condition_str)
        if in_match:
            field_name = in_match.group(1)
            values = [v.strip() for v in in_match.group(2).split(",") if v.strip()]
            return str(session_data.get(field_name, "")) in values

        # Handle: field not_in [val1, val2, ...]
        not_in_match = re.match(r"^(\w+)\s+not_in\s+\[([^\]]*)\]$", condition_str)
        if not_in_match:
            field_name = not_in_match.group(1)
            values = [v.strip() for v in not_in_match.group(2).split(",") if v.strip()]
            return str(session_data.get(field_name, "")) not in values

        # Handle: field == value
        eq_match = re.match(r"^(\w+)\s*==\s*(.+)$", condition_str)
        if eq_match:
            field_name = eq_match.group(1)
            expected = eq_match.group(2).strip().strip("\"'")
            actual = session_data.get(field_name)
            # Support boolean toggle: "true"/"false"
            if isinstance(actual, bool):
                actual = str(actual).lower()
            return str(actual) == expected

        # Handle: field != value
        ne_match = re.match(r"^(\w+)\s*!=\s*(.+)$", condition_str)
        if ne_match:
            field_name = ne_match.group(1)
            expected = ne_match.group(2).strip().strip("\"'")
            actual = session_data.get(field_name)
            if isinstance(actual, bool):
                actual = str(actual).lower()
            return str(actual) != expected

        # Unknown expression — default to visible
        return True

    # ------------------------------------------------------------------
    # Internal helpers
    # ------------------------------------------------------------------

    @staticmethod
    def _parse_step(raw: dict) -> WizardStep:
        fields_raw: list[dict] = raw.get("fields") or []
        parsed_fields = [StepManifestLoader._parse_field(f) for f in fields_raw]
        return WizardStep(
            id=raw["id"],
            title=raw.get("title", raw["id"]),
            template=raw.get("template", f"{raw['id']}.html"),
            fields=parsed_fields,
            description=raw.get("description", ""),
        )

    @staticmethod
    def _parse_field(raw: dict) -> StepField:
        return StepField(
            id=raw["id"],
            type=raw.get("type", "text"),
            label=raw.get("label", raw["id"]),
            required=bool(raw.get("required", False)),
            default=raw.get("default"),
            options=raw.get("options"),
            help_text=raw.get("help_text", ""),
            conditional=raw.get("conditional", ""),
            validators=raw.get("validators"),
        )

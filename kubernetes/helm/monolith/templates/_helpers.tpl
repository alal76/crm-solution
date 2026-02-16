{{- define "crm-monolith.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "crm-monolith.fullname" -}}
{{- if .Values.fullnameOverride -}}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- include "crm-monolith.name" . -}}
{{- end -}}
{{- end -}}

{{- define "crm-monolith.labels" -}}
app.kubernetes.io/name: {{ include "crm-monolith.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/version: {{ .Chart.AppVersion }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end -}}

{{- define "crm-monolith.selectorLabels" -}}
app.kubernetes.io/name: {{ include "crm-monolith.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end -}}

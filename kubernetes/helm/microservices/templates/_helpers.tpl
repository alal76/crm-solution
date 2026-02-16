{{- define "crm-microservices.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "crm-microservices.fullname" -}}
{{- if .Values.fullnameOverride -}}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- include "crm-microservices.name" . -}}
{{- end -}}
{{- end -}}

{{- define "crm-microservices.labels" -}}
app.kubernetes.io/name: {{ include "crm-microservices.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/version: {{ .Chart.AppVersion }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end -}}

{{- define "crm-microservices.selectorLabels" -}}
app.kubernetes.io/name: {{ include "crm-microservices.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end -}}

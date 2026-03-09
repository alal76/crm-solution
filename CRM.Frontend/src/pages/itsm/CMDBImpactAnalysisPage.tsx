import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import {
  Box,
  Typography,
  Paper,
  CircularProgress,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Chip,
  Alert,
} from '@mui/material';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';
import WarningAmberIcon from '@mui/icons-material/WarningAmber';
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined';
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';
import apiClient from '../../services/apiClient';

type SeverityLevel = 'critical' | 'high' | 'medium' | 'low';

interface ImpactItem {
  description: string;
  severity: SeverityLevel;
}

const severityConfig: Record<SeverityLevel, { color: 'error' | 'warning' | 'info' | 'success'; icon: React.ReactElement; label: string }> = {
  critical: { color: 'error', icon: <ErrorOutlineIcon color="error" />, label: 'Critical' },
  high: { color: 'warning', icon: <WarningAmberIcon color="warning" />, label: 'High' },
  medium: { color: 'info', icon: <InfoOutlinedIcon color="info" />, label: 'Medium' },
  low: { color: 'success', icon: <CheckCircleOutlineIcon color="success" />, label: 'Low' },
};

const parseSeverity = (impact: string): SeverityLevel => {
  const lower = impact.toLowerCase();
  if (lower.includes('critical') || lower.includes('outage') || lower.includes('down')) return 'critical';
  if (lower.includes('high') || lower.includes('major') || lower.includes('severe')) return 'high';
  if (lower.includes('low') || lower.includes('minor') || lower.includes('minimal')) return 'low';
  return 'medium';
};

const CMDBImpactAnalysisPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [impacts, setImpacts] = useState<ImpactItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await apiClient.get(`/cmdb/${id}/impact-analysis`);
        const data = response.data ?? [];
        // Normalize: API may return string[] or object[]
        const parsed: ImpactItem[] = data.map((item: any) =>
          typeof item === 'string'
            ? { description: item, severity: parseSeverity(item) }
            : { description: item.description ?? String(item), severity: item.severity ?? parseSeverity(item.description ?? '') }
        );
        setImpacts(parsed);
      } catch (error) {
        console.error('Failed to load impact analysis', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [id]);

  const criticalCount = impacts.filter(i => i.severity === 'critical').length;
  const highCount = impacts.filter(i => i.severity === 'high').length;

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" fontWeight="bold" gutterBottom>
        Impact Analysis
      </Typography>

      {/* Summary bar */}
      {!loading && impacts.length > 0 && (
        <Box sx={{ display: 'flex', gap: 1, mb: 2, flexWrap: 'wrap' }}>
          <Chip label={`${impacts.length} Total`} variant="outlined" />
          {criticalCount > 0 && <Chip label={`${criticalCount} Critical`} color="error" />}
          {highCount > 0 && <Chip label={`${highCount} High`} color="warning" />}
          <Chip label={`${impacts.filter(i => i.severity === 'medium').length} Medium`} color="info" />
          <Chip label={`${impacts.filter(i => i.severity === 'low').length} Low`} color="success" />
        </Box>
      )}

      {criticalCount > 0 && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {criticalCount} critical impact{criticalCount > 1 ? 's' : ''} detected — review before making changes.
        </Alert>
      )}

      <Paper sx={{ p: 3 }}>
        {loading ? (
          <CircularProgress />
        ) : impacts.length === 0 ? (
          <Typography color="text.secondary">No impacts found.</Typography>
        ) : (
          <List>
            {impacts.map((impact, index) => {
              const cfg = severityConfig[impact.severity];
              return (
                <ListItem
                  key={index}
                  sx={{
                    borderLeft: 4,
                    borderColor: `${cfg.color}.main`,
                    mb: 1,
                    bgcolor: 'action.hover',
                    borderRadius: 1,
                  }}
                >
                  <ListItemIcon sx={{ minWidth: 40 }}>{cfg.icon}</ListItemIcon>
                  <ListItemText
                    primary={impact.description}
                    secondary={
                      <Chip label={cfg.label} size="small" color={cfg.color} sx={{ mt: 0.5 }} />
                    }
                  />
                </ListItem>
              );
            })}
          </List>
        )}
      </Paper>
    </Box>
  );
};

export default CMDBImpactAnalysisPage;

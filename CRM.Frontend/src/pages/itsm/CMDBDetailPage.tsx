import React, { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Box, Typography, Paper, Button, Grid, CircularProgress } from '@mui/material';
import apiClient from '../../services/apiClient';
import { RelationshipDiagram, ServiceMap } from '../../components/itsm';
import { CIRelationshipDiagram } from '../../components/itsm/CIRelationshipDiagram';
import type { ConfigurationItem, CIRelationship } from '../../components/itsm/CIRelationshipDiagram';
import type { ConfigurationItemNode, ServiceNode } from '../../components/itsm';

interface ConfigurationItemDetail {
  ciId: number;
  ciName: string;
  ciNumber: string;
  ciType: number;
  ciSubtype?: string;
  operationalStatus: number;
  description?: string;
}

const CMDBDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [ci, setCi] = useState<ConfigurationItemDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [relatedCIs, setRelatedCIs] = useState<ConfigurationItemNode[]>([]);
  const [ciRelationships, setCiRelationships] = useState<any[]>([]);
  const [serviceNodes, setServiceNodes] = useState<ServiceNode[]>([]);

  useEffect(() => {
    const load = async () => {
      try {
        const response = await apiClient.get(`/cmdb/${id}`);
        setCi(response.data);
        // Load relationships and service map (best-effort)
        const [relResp, svcResp] = await Promise.allSettled([
          apiClient.get(`/cmdb/${id}/relationships`),
          apiClient.get(`/cmdb/${id}/services`),
        ]);
        if (relResp.status === 'fulfilled') {
          setRelatedCIs(relResp.value.data?.relatedCIs ?? []);
          setCiRelationships(relResp.value.data?.relationships ?? []);
        }
        if (svcResp.status === 'fulfilled') setServiceNodes(svcResp.value.data ?? []);
      } catch (error) {
        console.error('Failed to load CI', error);
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [id]);

  if (loading) return <Box sx={{ p: 3 }}><CircularProgress /></Box>;
  if (!ci) return <Box sx={{ p: 3 }}><Typography>Configuration item not found</Typography></Box>;

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h4" component="h1" fontWeight="bold">{ci.ciName}</Typography>
          <Typography color="text.secondary">{ci.ciNumber}</Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button variant="outlined" onClick={() => navigate(`/itsm/cmdb/${ci.ciId}/relationships`)}>
            Relationships
          </Button>
          <Button variant="outlined" onClick={() => navigate(`/itsm/cmdb/${ci.ciId}/impact`)}>
            Impact Analysis
          </Button>
        </Box>
      </Box>

      <Paper sx={{ p: 3 }}>
        <Box sx={{ mb: 2 }}>
          <Typography variant="subtitle2" color="text.secondary">Description</Typography>
          <Typography sx={{ whiteSpace: 'pre-wrap' }}>{ci.description || '—'}</Typography>
        </Box>
        <Grid container spacing={2}>
          <Grid item xs={12} md={4}>
            <Typography variant="subtitle2" color="text.secondary">Type</Typography>
            <Typography>Type {ci.ciType}</Typography>
          </Grid>
          <Grid item xs={12} md={4}>
            <Typography variant="subtitle2" color="text.secondary">Subtype</Typography>
            <Typography>{ci.ciSubtype || '—'}</Typography>
          </Grid>
          <Grid item xs={12} md={4}>
            <Typography variant="subtitle2" color="text.secondary">Status</Typography>
            <Typography>Status {ci.operationalStatus}</Typography>
          </Grid>
        </Grid>
      </Paper>

      {/* CI Relationship Diagram */}
      <Box sx={{ mt: 3 }}>
        <RelationshipDiagram
          centerCI={{
            id: ci.ciId,
            name: ci.ciName,
            ciNumber: ci.ciNumber,
            ciType: 'server',
            status: 'operational',
            criticality: 'medium',
          }}
          relatedCIs={relatedCIs}
          relationships={ciRelationships}
        />
      </Box>

      {/* Advanced CI Dependency Graph */}
      {relatedCIs.length > 0 && (
        <Box sx={{ mt: 3 }}>
          <Typography variant="h6" fontWeight="bold" gutterBottom>Dependency Graph</Typography>
          <CIRelationshipDiagram
            configItems={relatedCIs.map((node: ConfigurationItemNode) => ({
              id: String(node.id),
              name: node.name,
              type: (node.ciType as any) || 'server',
              status: (node.status as any) || 'active',
              environment: undefined,
            } as ConfigurationItem))}
            relationships={ciRelationships.map((rel: any, idx: number) => ({
              id: String(rel.id ?? idx),
              sourceId: String(rel.sourceId ?? rel.fromCIId),
              targetId: String(rel.targetId ?? rel.toCIId),
              type: (rel.type as any) || 'depends_on',
              label: rel.label ?? rel.relationshipType ?? '',
            } as CIRelationship))}
            selectedCIId={String(ci.ciId)}
            onCISelect={(ciId) => {
              if (ciId && ciId !== String(ci.ciId)) {
                navigate(`/itsm/cmdb/${ciId}`);
              }
            }}
            onCIDoubleClick={(ciId) => navigate(`/itsm/cmdb/${ciId}`)}
            highlightImpact
          />
        </Box>
      )}

      {/* Service Map */}
      {serviceNodes.length > 0 && (
        <Box sx={{ mt: 3 }}>
          <ServiceMap
            services={serviceNodes}
            selectedServiceId={String(ci.ciId)}
            showLegend
          />
        </Box>
      )}
    </Box>
  );
};

export default CMDBDetailPage;

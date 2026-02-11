import React, { useEffect, useState, useMemo, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Box, Typography, Paper, CircularProgress, Grid } from '@mui/material';
import apiClient from '../../services/apiClient';

interface ConfigurationItem {
  ciId: number;
  ciName: string;
  ciNumber: string;
  ciType: number;
  ciSubtype?: string;
  operationalStatus: number;
  ownerName?: string;
}

interface NodePosition {
  x: number;
  y: number;
  item: ConfigurationItem;
  isCenter: boolean;
}

const STATUS_COLORS: Record<number, string> = {
  0: '#10b981', // Operational - green
  1: '#f59e0b', // Degraded - amber
  2: '#ef4444', // Down - red
  3: '#6b7280', // Retired - gray
};

const TYPE_COLORS: Record<number, string> = {
  0: '#6366f1', // Server - indigo
  1: '#8b5cf6', // Network - violet
  2: '#ec4899', // Application - pink
  3: '#14b8a6', // Database - teal
  4: '#f97316', // Storage - orange
};

function getStatusLabel(status: number): string {
  switch (status) {
    case 0: return 'Operational';
    case 1: return 'Degraded';
    case 2: return 'Down';
    case 3: return 'Retired';
    default: return 'Unknown';
  }
}

function truncateText(text: string, maxLen: number): string {
  return text.length > maxLen ? text.slice(0, maxLen - 1) + '…' : text;
}

const CMDBRelationshipMapPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [ci, setCi] = useState<ConfigurationItem | null>(null);
  const [related, setRelated] = useState<ConfigurationItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [hoveredNode, setHoveredNode] = useState<number | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        const [ciResponse, relatedResponse] = await Promise.all([
          apiClient.get(`/api/cmdb/${id}`),
          apiClient.get(`/api/cmdb/${id}/related`),
        ]);
        setCi(ciResponse.data ?? null);
        setRelated(relatedResponse.data ?? []);
      } catch (error) {
        console.error('Failed to load relationship map', error);
      } finally {
        setLoading(false);
      }
    };

    if (id) {
      load();
    } else {
      setLoading(false);
    }
  }, [id]);

  const SVG_WIDTH = 500;
  const SVG_HEIGHT = 420;
  const CENTER_X = SVG_WIDTH / 2;
  const CENTER_Y = SVG_HEIGHT / 2;
  const RADIUS = 150;
  const NODE_RADIUS = 32;

  const nodePositions: NodePosition[] = useMemo(() => {
    if (!ci) return [];
    const positions: NodePosition[] = [
      { x: CENTER_X, y: CENTER_Y, item: ci, isCenter: true },
    ];
    const count = related.length;
    if (count === 0) return positions;

    const angleStep = (2 * Math.PI) / count;
    const startAngle = -Math.PI / 2; // start from top
    related.forEach((item, i) => {
      const angle = startAngle + i * angleStep;
      positions.push({
        x: CENTER_X + RADIUS * Math.cos(angle),
        y: CENTER_Y + RADIUS * Math.sin(angle),
        item,
        isCenter: false,
      });
    });
    return positions;
  }, [ci, related, CENTER_X, CENTER_Y, RADIUS]);

  const handleNodeClick = useCallback((ciId: number) => {
    navigate(`/itsm/cmdb/${ciId}/relationships`);
  }, [navigate]);

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" component="h1" fontWeight="bold" sx={{ mb: 3 }}>CI Relationship Map</Typography>
      <Paper sx={{ p: 3 }}>
        {loading ? (
          <CircularProgress />
        ) : !ci ? (
          <Typography color="text.secondary">CI not found.</Typography>
        ) : (
          <>
            <Box sx={{ mb: 2 }}>
              <Typography variant="body2" color="text.secondary">{ci.ciNumber}</Typography>
              <Typography variant="h6" fontWeight="bold">{ci.ciName}</Typography>
              <Typography variant="body2" color="text.secondary">
                Type {ci.ciType}{ci.ciSubtype ? ` • ${ci.ciSubtype}` : ''}
              </Typography>
            </Box>
            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <Paper variant="outlined" sx={{ p: 2 }}>
                  <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1.5 }}>Related configuration items</Typography>
                  {related.length === 0 ? (
                    <Typography variant="body2" color="text.secondary">No related items found.</Typography>
                  ) : (
                    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
                      {related.map((item) => (
                        <Paper key={item.ciId} variant="outlined" sx={{ p: 1.5 }}>
                          <Typography variant="body2" color="text.secondary">{item.ciNumber}</Typography>
                          <Typography variant="body2" fontWeight="bold">{item.ciName}</Typography>
                          <Typography variant="caption" color="text.secondary">Type {item.ciType}</Typography>
                        </Paper>
                      ))}
                    </Box>
                  )}
                </Paper>
              </Grid>

              {/* SVG Relationship Graph */}
              <Grid item xs={12} md={6}>
                <Paper variant="outlined" sx={{ p: 1, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                  <Typography variant="subtitle2" color="text.secondary" sx={{ mb: 1 }}>Relationship Graph</Typography>
                  {related.length === 0 ? (
                    <Box sx={{ height: SVG_HEIGHT, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                      <Typography variant="body2" color="text.disabled">No relationships to display</Typography>
                    </Box>
                  ) : (
                    <svg
                      width="100%"
                      height={SVG_HEIGHT}
                      viewBox={`0 0 ${SVG_WIDTH} ${SVG_HEIGHT}`}
                      style={{ userSelect: 'none' }}
                    >
                      <defs>
                        <filter id="cmdb-shadow" x="-20%" y="-20%" width="140%" height="140%">
                          <feDropShadow dx="0" dy="1" stdDeviation="2" floodOpacity="0.15" />
                        </filter>
                        <marker id="cmdb-arrow" viewBox="0 0 10 10" refX="10" refY="5"
                          markerWidth="6" markerHeight="6" orient="auto-start-reverse">
                          <path d="M 0 0 L 10 5 L 0 10 z" fill="#9ca3af" />
                        </marker>
                      </defs>

                      {/* Edges — lines from center to each related node */}
                      {nodePositions.slice(1).map((node) => {
                        const center = nodePositions[0];
                        const dx = node.x - center.x;
                        const dy = node.y - center.y;
                        const dist = Math.sqrt(dx * dx + dy * dy);
                        const ux = dx / dist;
                        const uy = dy / dist;
                        const x1 = center.x + ux * (NODE_RADIUS + 2);
                        const y1 = center.y + uy * (NODE_RADIUS + 2);
                        const x2 = node.x - ux * (NODE_RADIUS + 2);
                        const y2 = node.y - uy * (NODE_RADIUS + 2);

                        return (
                          <line
                            key={`edge-${node.item.ciId}`}
                            x1={x1} y1={y1} x2={x2} y2={y2}
                            stroke={hoveredNode === node.item.ciId ? '#6366f1' : '#d1d5db'}
                            strokeWidth={hoveredNode === node.item.ciId ? 2.5 : 1.5}
                            markerEnd="url(#cmdb-arrow)"
                            style={{ transition: 'stroke 0.2s, stroke-width 0.2s' }}
                          />
                        );
                      })}

                      {/* Nodes */}
                      {nodePositions.map((node) => {
                        const isHovered = hoveredNode === node.item.ciId;
                        const fillColor = node.isCenter ? '#4f46e5' : (TYPE_COLORS[node.item.ciType] || '#6b7280');
                        const statusColor = STATUS_COLORS[node.item.operationalStatus] || '#6b7280';
                        const r = node.isCenter ? NODE_RADIUS + 4 : NODE_RADIUS;

                        return (
                          <g
                            key={`node-${node.item.ciId}`}
                            transform={`translate(${node.x}, ${node.y})`}
                            onClick={() => handleNodeClick(node.item.ciId)}
                            onMouseEnter={() => setHoveredNode(node.item.ciId)}
                            onMouseLeave={() => setHoveredNode(null)}
                            style={{ cursor: 'pointer' }}
                          >
                            <circle
                              r={r + 3}
                              fill="none"
                              stroke={statusColor}
                              strokeWidth={2.5}
                              opacity={isHovered ? 1 : 0.7}
                              style={{ transition: 'opacity 0.2s' }}
                            />
                            <circle
                              r={r}
                              fill={fillColor}
                              opacity={isHovered ? 1 : 0.85}
                              filter="url(#cmdb-shadow)"
                              style={{ transition: 'opacity 0.2s' }}
                            />
                            <text
                              textAnchor="middle"
                              dy="-0.1em"
                              fill="#fff"
                              fontSize={node.isCenter ? 11 : 10}
                              fontWeight={node.isCenter ? 700 : 600}
                            >
                              {truncateText(node.item.ciName, 12)}
                            </text>
                            <text
                              textAnchor="middle"
                              dy="1.2em"
                              fill="rgba(255,255,255,0.8)"
                              fontSize={8}
                            >
                              {node.item.ciSubtype || `Type ${node.item.ciType}`}
                            </text>

                            {isHovered && (
                              <g>
                                <rect
                                  x={-70} y={r + 8} width={140} height={44}
                                  rx={6} fill="white" stroke="#e5e7eb" strokeWidth={1}
                                  filter="url(#cmdb-shadow)"
                                />
                                <text x={0} y={r + 24} textAnchor="middle" fontSize={10} fill="#111827" fontWeight={600}>
                                  {truncateText(node.item.ciName, 22)}
                                </text>
                                <text x={0} y={r + 38} textAnchor="middle" fontSize={9} fill="#6b7280">
                                  {node.item.ciNumber} • {getStatusLabel(node.item.operationalStatus)}
                                </text>
                              </g>
                            )}
                          </g>
                        );
                      })}
                    </svg>
                  )}
                  {/* Legend */}
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1.5, mt: 1, justifyContent: 'center' }}>
                    {[
                      { color: '#10b981', label: 'Operational' },
                      { color: '#f59e0b', label: 'Degraded' },
                      { color: '#ef4444', label: 'Down' },
                      { color: '#6b7280', label: 'Retired' },
                    ].map(({ color, label }) => (
                      <Box key={label} sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                        <Box sx={{ width: 10, height: 10, borderRadius: '50%', bgcolor: color }} />
                        <Typography variant="caption" color="text.secondary">{label}</Typography>
                      </Box>
                    ))}
                  </Box>
                </Paper>
              </Grid>
            </Grid>
          </>
        )}
      </Paper>
    </Box>
  );
};

export default CMDBRelationshipMapPage;

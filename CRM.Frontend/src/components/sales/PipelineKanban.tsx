import React, { useState, useCallback } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Chip,
  Avatar,
  IconButton,
  Tooltip,
  CircularProgress,
  Paper,
} from '@mui/material';
import {
  Edit as EditIcon,
  Person as PersonIcon,
  Business as BusinessIcon,
  AttachMoney as MoneyIcon,
  CalendarToday as CalendarIcon,
} from '@mui/icons-material';

// Base opportunity interface - components can pass objects with more properties
interface OpportunityBase {
  id: number;
  name: string;
  stage: number;
  probability: number;
  amount: number;
  currency?: string;
  expectedCloseDate?: string;
  accountName?: string;
  salesOwnerName?: string;
}

interface PipelineStage {
  value: number;
  label: string;
  color: string;
}

interface PipelineKanbanProps {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  opportunities: any[];
  stages: PipelineStage[];
  onStageChange: (opportunityId: number, newStage: number) => Promise<void>;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  onEdit: (opportunity?: any) => void;
  loading?: boolean;
}

const formatCurrency = (value: number, currency = 'USD') => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency,
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(value);
};

const formatDate = (dateString?: string) => {
  if (!dateString) return '—';
  return new Date(dateString).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
  });
};

const OpportunityCard: React.FC<{
  opportunity: OpportunityBase;
  onEdit: (opportunity: OpportunityBase) => void;
  onDragStart: (e: React.DragEvent, opportunity: OpportunityBase) => void;
}> = ({ opportunity, onEdit, onDragStart }) => {
  const isOverdue =
    opportunity.expectedCloseDate &&
    new Date(opportunity.expectedCloseDate) < new Date() &&
    opportunity.stage < 4;

  return (
    <Card
      draggable
      onDragStart={(e) => onDragStart(e, opportunity)}
      sx={{
        mb: 1.5,
        cursor: 'grab',
        transition: 'all 0.2s ease',
        border: isOverdue ? '2px solid #f44336' : '1px solid #e0e0e0',
        '&:hover': {
          boxShadow: 3,
          transform: 'translateY(-2px)',
        },
        '&:active': {
          cursor: 'grabbing',
        },
      }}
    >
      <CardContent sx={{ p: 2, '&:last-child': { pb: 2 } }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
          <Typography
            variant="subtitle2"
            sx={{
              fontWeight: 600,
              fontSize: '0.875rem',
              lineHeight: 1.3,
              flex: 1,
              mr: 1,
            }}
          >
            {opportunity.name}
          </Typography>
          <IconButton size="small" onClick={() => onEdit(opportunity)} sx={{ p: 0.5 }}>
            <EditIcon fontSize="small" />
          </IconButton>
        </Box>

        {opportunity.accountName && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mb: 1 }}>
            <BusinessIcon sx={{ fontSize: 14, color: 'text.secondary' }} />
            <Typography variant="caption" color="text.secondary" noWrap>
              {opportunity.accountName}
            </Typography>
          </Box>
        )}

        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <MoneyIcon sx={{ fontSize: 16, color: '#4caf50' }} />
            <Typography variant="body2" sx={{ fontWeight: 600, color: '#2e7d32' }}>
              {formatCurrency(opportunity.amount, opportunity.currency)}
            </Typography>
          </Box>
          <Chip
            label={`${opportunity.probability}%`}
            size="small"
            sx={{
              height: 20,
              fontSize: '0.7rem',
              bgcolor: opportunity.probability >= 70 ? '#e8f5e9' : opportunity.probability >= 40 ? '#fff3e0' : '#ffebee',
              color: opportunity.probability >= 70 ? '#2e7d32' : opportunity.probability >= 40 ? '#e65100' : '#c62828',
            }}
          />
        </Box>

        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <CalendarIcon sx={{ fontSize: 14, color: isOverdue ? '#f44336' : 'text.secondary' }} />
            <Typography
              variant="caption"
              sx={{ color: isOverdue ? '#f44336' : 'text.secondary', fontWeight: isOverdue ? 600 : 400 }}
            >
              {formatDate(opportunity.expectedCloseDate)}
            </Typography>
          </Box>
          {opportunity.salesOwnerName && (
            <Tooltip title={opportunity.salesOwnerName}>
              <Avatar sx={{ width: 24, height: 24, fontSize: '0.7rem', bgcolor: '#6750A4' }}>
                {opportunity.salesOwnerName
                  .split(' ')
                  .map((n) => n[0])
                  .join('')
                  .toUpperCase()
                  .slice(0, 2)}
              </Avatar>
            </Tooltip>
          )}
        </Box>
      </CardContent>
    </Card>
  );
};

const PipelineKanban: React.FC<PipelineKanbanProps> = ({
  opportunities,
  stages,
  onStageChange,
  onEdit,
  loading = false,
}) => {
  const [draggedOpportunity, setDraggedOpportunity] = useState<OpportunityBase | null>(null);
  const [dragOverStage, setDragOverStage] = useState<number | null>(null);
  const [updating, setUpdating] = useState<number | null>(null);

  const handleDragStart = useCallback((e: React.DragEvent, opportunity: OpportunityBase) => {
    setDraggedOpportunity(opportunity);
    e.dataTransfer.effectAllowed = 'move';
    e.dataTransfer.setData('text/plain', opportunity.id.toString());
  }, []);

  const handleDragOver = useCallback((e: React.DragEvent, stageValue: number) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'move';
    setDragOverStage(stageValue);
  }, []);

  const handleDragLeave = useCallback(() => {
    setDragOverStage(null);
  }, []);

  const handleDrop = useCallback(
    async (e: React.DragEvent, stageValue: number) => {
      e.preventDefault();
      setDragOverStage(null);

      if (!draggedOpportunity || draggedOpportunity.stage === stageValue) {
        setDraggedOpportunity(null);
        return;
      }

      setUpdating(draggedOpportunity.id);
      try {
        await onStageChange(draggedOpportunity.id, stageValue);
      } catch (error) {
        console.error('Failed to update opportunity stage', error);
      } finally {
        setUpdating(null);
        setDraggedOpportunity(null);
      }
    },
    [draggedOpportunity, onStageChange]
  );

  const handleDragEnd = useCallback(() => {
    setDraggedOpportunity(null);
    setDragOverStage(null);
  }, []);

  const getOpportunitiesByStage = (stageValue: number) =>
    opportunities.filter((opp) => opp.stage === stageValue);

  const getStageTotal = (stageValue: number) =>
    getOpportunitiesByStage(stageValue).reduce((sum, opp) => sum + opp.amount, 0);

  const getWeightedTotal = (stageValue: number) =>
    getOpportunitiesByStage(stageValue).reduce((sum, opp) => sum + opp.amount * (opp.probability / 100), 0);

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 400 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box
      sx={{
        display: 'flex',
        gap: 2,
        overflowX: 'auto',
        pb: 2,
        minHeight: 500,
      }}
    >
      {stages.map((stage) => {
        const stageOpportunities = getOpportunitiesByStage(stage.value);
        const stageTotal = getStageTotal(stage.value);
        const weightedTotal = getWeightedTotal(stage.value);
        const isDragOver = dragOverStage === stage.value;

        return (
          <Paper
            key={stage.value}
            onDragOver={(e) => handleDragOver(e, stage.value)}
            onDragLeave={handleDragLeave}
            onDrop={(e) => handleDrop(e, stage.value)}
            sx={{
              minWidth: 280,
              maxWidth: 320,
              flex: '1 0 280px',
              bgcolor: isDragOver ? '#f3e5f5' : '#fafafa',
              borderRadius: 2,
              border: isDragOver ? '2px dashed #6750A4' : '1px solid #e0e0e0',
              transition: 'all 0.2s ease',
              display: 'flex',
              flexDirection: 'column',
            }}
          >
            {/* Stage Header */}
            <Box
              sx={{
                p: 2,
                borderBottom: '1px solid #e0e0e0',
                bgcolor: stage.color,
                borderTopLeftRadius: 8,
                borderTopRightRadius: 8,
              }}
            >
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
                <Typography variant="subtitle1" sx={{ fontWeight: 600, color: '#fff' }}>
                  {stage.label}
                </Typography>
                <Chip
                  label={stageOpportunities.length}
                  size="small"
                  sx={{
                    bgcolor: 'rgba(255,255,255,0.3)',
                    color: '#fff',
                    fontWeight: 600,
                    height: 24,
                  }}
                />
              </Box>
              <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                <Typography variant="caption" sx={{ color: 'rgba(255,255,255,0.9)' }}>
                  Total: {formatCurrency(stageTotal)}
                </Typography>
                <Typography variant="caption" sx={{ color: 'rgba(255,255,255,0.9)' }}>
                  Weighted: {formatCurrency(weightedTotal)}
                </Typography>
              </Box>
            </Box>

            {/* Opportunities */}
            <Box
              sx={{
                p: 1.5,
                flex: 1,
                overflowY: 'auto',
                minHeight: 200,
              }}
            >
              {stageOpportunities.length === 0 ? (
                <Box
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    height: '100%',
                    color: 'text.secondary',
                    fontStyle: 'italic',
                  }}
                >
                  <Typography variant="body2">No opportunities</Typography>
                </Box>
              ) : (
                stageOpportunities.map((opportunity) => (
                  <Box
                    key={opportunity.id}
                    sx={{
                      opacity: updating === opportunity.id ? 0.5 : 1,
                      position: 'relative',
                    }}
                  >
                    {updating === opportunity.id && (
                      <CircularProgress
                        size={20}
                        sx={{
                          position: 'absolute',
                          top: '50%',
                          left: '50%',
                          transform: 'translate(-50%, -50%)',
                          zIndex: 1,
                        }}
                      />
                    )}
                    <OpportunityCard
                      opportunity={opportunity}
                      onEdit={onEdit}
                      onDragStart={handleDragStart}
                    />
                  </Box>
                ))
              )}
            </Box>
          </Paper>
        );
      })}
    </Box>
  );
};

export default PipelineKanban;

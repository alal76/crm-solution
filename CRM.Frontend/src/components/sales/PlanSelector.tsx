/**
 * PlanSelector - Pricing cards layout for selecting subscription plans
 */
import React from 'react';
import {
  Card,
  CardContent,
  CardActions,
  Typography,
  Button,
  Grid,
  Box,
  Chip,
  List,
  ListItem,
  ListItemIcon,
  ListItemText,
  Skeleton,
  Stack,
} from '@mui/material';
import {
  Check as CheckIcon,
  Star as StarIcon,
} from '@mui/icons-material';

export interface Plan {
  id: number;
  name: string;
  description: string;
  price: number;
  billingCycle: string;
  features: string[];
  isPopular?: boolean;
}

interface PlanSelectorProps {
  plans: Plan[];
  currentPlanId?: number;
  onSelectPlan: (planId: number) => void;
  loading?: boolean;
}

const formatPrice = (price: number): string => {
  if (price === 0) return 'Free';
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(price);
};

const PlanSelector: React.FC<PlanSelectorProps> = ({
  plans,
  currentPlanId,
  onSelectPlan,
  loading = false,
}) => {
  if (loading) {
    return (
      <Grid container spacing={3}>
        {[1, 2, 3].map((i) => (
          <Grid item xs={12} sm={6} md={4} key={i}>
            <Card variant="outlined" sx={{ height: '100%' }}>
              <CardContent>
                <Skeleton width="60%" height={32} />
                <Skeleton width="40%" height={48} sx={{ mt: 1 }} />
                <Skeleton width="80%" height={20} sx={{ mt: 1 }} />
                {[1, 2, 3, 4].map((j) => (
                  <Skeleton key={j} width="90%" height={24} sx={{ mt: 0.5 }} />
                ))}
              </CardContent>
              <CardActions sx={{ p: 2 }}>
                <Skeleton width="100%" height={36} />
              </CardActions>
            </Card>
          </Grid>
        ))}
      </Grid>
    );
  }

  if (plans.length === 0) {
    return (
      <Box sx={{ textAlign: 'center', py: 6 }}>
        <Typography variant="body1" color="text.secondary">
          No plans available
        </Typography>
      </Box>
    );
  }

  return (
    <Grid container spacing={3} alignItems="stretch">
      {plans.map((plan) => {
        const isCurrent = plan.id === currentPlanId;
        const isPopular = plan.isPopular === true;

        return (
          <Grid item xs={12} sm={6} md={plans.length <= 3 ? 4 : 3} key={plan.id}>
            <Card
              variant="outlined"
              sx={{
                height: '100%',
                display: 'flex',
                flexDirection: 'column',
                border: isPopular ? 2 : 1,
                borderColor: isPopular ? 'primary.main' : 'divider',
                position: 'relative',
              }}
            >
              {isPopular && (
                <Chip
                  icon={<StarIcon />}
                  label="Most Popular"
                  color="primary"
                  size="small"
                  sx={{
                    position: 'absolute',
                    top: -12,
                    left: '50%',
                    transform: 'translateX(-50%)',
                    fontWeight: 600,
                  }}
                />
              )}
              <CardContent sx={{ flexGrow: 1, pt: isPopular ? 3 : 2 }}>
                <Typography variant="h6" fontWeight={700} gutterBottom>
                  {plan.name}
                </Typography>

                <Stack direction="row" alignItems="baseline" spacing={0.5} sx={{ mb: 1 }}>
                  <Typography variant="h4" fontWeight={700} color="primary.main">
                    {formatPrice(plan.price)}
                  </Typography>
                  {plan.price > 0 && (
                    <Typography variant="body2" color="text.secondary">
                      / {plan.billingCycle}
                    </Typography>
                  )}
                </Stack>

                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                  {plan.description}
                </Typography>

                <List dense disablePadding>
                  {plan.features.map((feature, idx) => (
                    <ListItem key={idx} disableGutters sx={{ py: 0.25 }}>
                      <ListItemIcon sx={{ minWidth: 28 }}>
                        <CheckIcon fontSize="small" color="success" />
                      </ListItemIcon>
                      <ListItemText
                        primary={feature}
                        primaryTypographyProps={{ variant: 'body2' }}
                      />
                    </ListItem>
                  ))}
                </List>
              </CardContent>

              <CardActions sx={{ p: 2, pt: 0 }}>
                <Button
                  fullWidth
                  variant={isCurrent ? 'outlined' : 'contained'}
                  disabled={isCurrent}
                  onClick={() => onSelectPlan(plan.id)}
                  size="large"
                >
                  {isCurrent ? 'Current Plan' : 'Select Plan'}
                </Button>
              </CardActions>
            </Card>
          </Grid>
        );
      })}
    </Grid>
  );
};

export default PlanSelector;

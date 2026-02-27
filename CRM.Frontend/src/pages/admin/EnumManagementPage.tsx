/**
 * CRM Solution - Enum Management Page
 * Phase 3: Frontend Implementation (SPEC-GEN-002)
 * Lists all enum categories with navigation to editor
 */

import React, { useEffect, useState } from 'react';
import { Container, Typography, Box } from '@mui/material';
import { EnumCategory } from '../../types/enums';
import enumService from '../../services/enumService';
import EnumCategoryGrid from '../../components/admin/enums/EnumCategoryGrid';

const EnumManagementPage: React.FC = () => {
  const [categories, setCategories] = useState<EnumCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadCategories();
  }, []);

  const loadCategories = async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await enumService.getAllCategories();
      setCategories(response.data);
    } catch (err: any) {
      setError(err.message || 'Failed to load enum categories');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container maxWidth="xl">
      <Box sx={{ mt: 4, mb: 2 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Enum Management
        </Typography>
        <Typography variant="body1" color="text.secondary" gutterBottom>
          Manage configurable enum values for status fields, priorities, and other dropdown options.
        </Typography>
      </Box>

      <EnumCategoryGrid 
        categories={categories} 
        loading={loading} 
        error={error}
        onRefresh={loadCategories}
      />
    </Container>
  );
};

export default EnumManagementPage;

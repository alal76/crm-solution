/**
 * CRM Solution - Enum Editor Page
 * Phase 3: Frontend Implementation (SPEC-GEN-002)
 * Edit values within a specific enum category
 */

import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Container, Typography, Box, Button, Alert } from '@mui/material';
import { ArrowBack as ArrowBackIcon } from '@mui/icons-material';
import { EnumCategory, EnumValue, CreateEnumValueDto, UpdateEnumValueDto } from '../../types/enums';
import enumService from '../../services/enumService';
import enumCacheService from '../../services/enumCacheService';
import EnumValueTable from '../../components/admin/enums/EnumValueTable';

const EnumEditorPage: React.FC = () => {
  const { categoryName } = useParams<{ categoryName: string }>();
  const navigate = useNavigate();
  
  const [category, setCategory] = useState<EnumCategory | null>(null);
  const [values, setValues] = useState<EnumValue[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (categoryName) {
      loadData();
    }
  }, [categoryName]);

  const loadData = async () => {
    if (!categoryName) return;
    
    setLoading(true);
    setError(null);
    try {
      const [catResponse, valuesResponse] = await Promise.all([
        enumService.getCategoryByName(categoryName),
        enumService.getValuesByCategoryName(categoryName, true) // include inactive
      ]);
      setCategory(catResponse.data);
      setValues(valuesResponse.data);
    } catch (err: any) {
      setError(err.message || 'Failed to load enum data');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateValue = async (dto: CreateEnumValueDto) => {
    if (!category) return;
    try {
      const response = await enumService.createValue(category.id, dto);
      setValues([...values, response.data]);
      enumCacheService.invalidate(categoryName);
    } catch (err: any) {
      throw new Error(err.response?.data?.message || 'Failed to create value');
    }
  };

  const handleUpdateValue = async (valueId: number, dto: UpdateEnumValueDto) => {
    try {
      const response = await enumService.updateValue(valueId, dto);
      setValues(values.map(v => v.id === valueId ? response.data : v));
      enumCacheService.invalidate(categoryName);
    } catch (err: any) {
      throw new Error(err.response?.data?.message || 'Failed to update value');
    }
  };

  const handleDeleteValue = async (valueId: number) => {
    try {
      await enumService.deleteValue(valueId);
      setValues(values.filter(v => v.id !== valueId));
      enumCacheService.invalidate(categoryName);
    } catch (err: any) {
      throw new Error(err.response?.data?.message || 'Failed to delete value');
    }
  };

  const handleReorderValues = async (newSortOrders: Record<number, number>) => {
    if (!category) return;
    try {
      const response = await enumService.reorderValues(category.id, newSortOrders);
      setValues(response.data);
      enumCacheService.invalidate(categoryName);
    } catch (err: any) {
      throw new Error(err.response?.data?.message || 'Failed to reorder values');
    }
  };

  return (
    <Container maxWidth="xl">
      <Box sx={{ mt: 4, mb: 2 }}>
        <Button 
          startIcon={<ArrowBackIcon />} 
          onClick={() => navigate('/admin/master-data/enums')}
          sx={{ mb: 2 }}
        >
          Back to Enum Management
        </Button>

        {category && (
          <>
            <Typography variant="h4" component="h1" gutterBottom>
              {category.displayName || category.name}
            </Typography>
            {category.description && (
              <Typography variant="body1" color="text.secondary" gutterBottom>
                {category.description}
              </Typography>
            )}
            {category.entityType && category.propertyName && (
              <Typography variant="body2" color="text.disabled" gutterBottom>
                Entity: {category.entityType}.{category.propertyName}
              </Typography>
            )}
          </>
        )}
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {category && (
        <EnumValueTable 
          category={category}
          values={values}
          loading={loading}
          onCreateValue={handleCreateValue}
          onUpdateValue={handleUpdateValue}
          onDeleteValue={handleDeleteValue}
          onReorderValues={handleReorderValues}
        />
      )}
    </Container>
  );
};

export default EnumEditorPage;

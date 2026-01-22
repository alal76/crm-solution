import React, { useEffect, useState, useContext } from 'react';
import { FormControl, InputLabel, Select, MenuItem } from '@mui/material';
import lookupService, { LookupItem } from '../services/lookupService';
import { LookupContext } from '../context/LookupContext';

interface Props {
  category: string;
  name: string;
  value: any;
  onChange: (e: any) => void;
  fallback?: Array<{ value: any; label: string }>;
  label?: string;
}

const LookupSelect: React.FC<Props> = ({ category, name, value, onChange, fallback = [], label }) => {
  const ctx = useContext(LookupContext);
  const [items, setItems] = useState<LookupItem[] | null>(null);

  const getFromCtx = ctx?.get;
  const setToCtx = ctx?.set;

  useEffect(() => {
    let mounted = true;
    const load = async () => {
      try {
        const list = getFromCtx ? getFromCtx(category) : undefined;
        if (list) {
          if (mounted) setItems(list);
          return;
        }
        const res = await lookupService.getLookupItems(category);
        setToCtx?.(category, res);
        if (mounted) setItems(res);
      } catch (err) {
        if (mounted) setItems([]);
      }
    };
    load();
    return () => { mounted = false; };
  }, [category, getFromCtx, setToCtx]);

  const renderOptions = () => {
    if (items && items.length) return items.map(i => <MenuItem key={i.key || i.id} value={i.key || i.value}>{i.value}</MenuItem>);
    return fallback.map(f => <MenuItem key={f.value} value={f.value}>{f.label}</MenuItem>);
  };

  return (
    <FormControl fullWidth>
      <InputLabel>{label || category}</InputLabel>
      <Select name={name} value={value} onChange={onChange} label={label || category}>
        {renderOptions()}
      </Select>
    </FormControl>
  );
};

export default LookupSelect;

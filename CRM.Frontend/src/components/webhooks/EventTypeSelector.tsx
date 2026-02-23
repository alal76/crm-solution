/**
 * EventTypeSelector - Multi-select component for webhook event types
 * Provides grouped selection with "Select All" per category
 */

import React, { useCallback, useMemo } from 'react';
import {
  Autocomplete,
  Checkbox,
  TextField,
  Chip,
  Box,
  Typography,
  Button,
} from '@mui/material';
import {
  CheckBoxOutlineBlank as UncheckedIcon,
  CheckBox as CheckedIcon,
} from '@mui/icons-material';

interface EventTypeSelectorProps {
  selectedEvents: string[];
  onChange: (events: string[]) => void;
  disabled?: boolean;
}

interface EventOption {
  category: string;
  event: string;
  label: string;
}

const EVENT_CATEGORIES: Record<string, { event: string; label: string }[]> = {
  Account: [
    { event: 'account.created', label: 'Account Created' },
    { event: 'account.updated', label: 'Account Updated' },
    { event: 'account.deleted', label: 'Account Deleted' },
  ],
  Contact: [
    { event: 'contact.created', label: 'Contact Created' },
    { event: 'contact.updated', label: 'Contact Updated' },
    { event: 'contact.deleted', label: 'Contact Deleted' },
  ],
  Opportunity: [
    { event: 'opportunity.created', label: 'Opportunity Created' },
    { event: 'opportunity.updated', label: 'Opportunity Updated' },
    { event: 'opportunity.won', label: 'Opportunity Won' },
    { event: 'opportunity.lost', label: 'Opportunity Lost' },
  ],
  Lead: [
    { event: 'lead.created', label: 'Lead Created' },
    { event: 'lead.updated', label: 'Lead Updated' },
    { event: 'lead.converted', label: 'Lead Converted' },
  ],
  Ticket: [
    { event: 'incident.created', label: 'Ticket Created' },
    { event: 'incident.updated', label: 'Ticket Updated' },
    { event: 'incident.status_changed', label: 'Ticket Status Changed' },
    { event: 'incident.closed', label: 'Ticket Closed' },
  ],
  Invoice: [
    { event: 'invoice.created', label: 'Invoice Created' },
    { event: 'invoice.paid', label: 'Invoice Paid' },
    { event: 'invoice.overdue', label: 'Invoice Overdue' },
  ],
  Payment: [
    { event: 'payment.received', label: 'Payment Received' },
    { event: 'payment.refunded', label: 'Payment Refunded' },
    { event: 'payment.failed', label: 'Payment Failed' },
  ],
  Custom: [
    { event: 'custom', label: 'Custom Event' },
  ],
};

const ALL_OPTIONS: EventOption[] = Object.entries(EVENT_CATEGORIES).flatMap(
  ([category, events]) =>
    events.map((e) => ({ category, event: e.event, label: e.label }))
);

const EventTypeSelector: React.FC<EventTypeSelectorProps> = ({
  selectedEvents,
  onChange,
  disabled = false,
}) => {
  const selectedOptions = useMemo(
    () => ALL_OPTIONS.filter((opt) => selectedEvents.includes(opt.event)),
    [selectedEvents]
  );

  const handleChange = useCallback(
    (_event: React.SyntheticEvent, value: EventOption[]) => {
      onChange(value.map((v) => v.event));
    },
    [onChange]
  );

  const handleSelectCategory = useCallback(
    (category: string) => {
      const categoryEvents = EVENT_CATEGORIES[category]?.map((e) => e.event) || [];
      const allSelected = categoryEvents.every((ev) => selectedEvents.includes(ev));

      if (allSelected) {
        onChange(selectedEvents.filter((ev) => !categoryEvents.includes(ev)));
      } else {
        const merged = new Set([...selectedEvents, ...categoryEvents]);
        onChange(Array.from(merged));
      }
    },
    [selectedEvents, onChange]
  );

  const isCategoryFullySelected = useCallback(
    (category: string): boolean => {
      const categoryEvents = EVENT_CATEGORIES[category]?.map((e) => e.event) || [];
      return categoryEvents.every((ev) => selectedEvents.includes(ev));
    },
    [selectedEvents]
  );

  return (
    <Box>
      <Autocomplete
        multiple
        disabled={disabled}
        options={ALL_OPTIONS}
        value={selectedOptions}
        onChange={handleChange}
        groupBy={(option) => option.category}
        getOptionLabel={(option) => option.label}
        isOptionEqualToValue={(option, value) => option.event === value.event}
        disableCloseOnSelect
        renderOption={(props, option, { selected }) => (
          <li {...props} key={option.event}>
            <Checkbox
              icon={<UncheckedIcon fontSize="small" />}
              checkedIcon={<CheckedIcon fontSize="small" />}
              style={{ marginRight: 8 }}
              checked={selected}
            />
            {option.label}
          </li>
        )}
        renderGroup={(params) => (
          <li key={params.key}>
            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                px: 2,
                py: 0.5,
                backgroundColor: 'action.hover',
              }}
            >
              <Typography variant="subtitle2" color="text.secondary">
                {params.group}
              </Typography>
              <Button
                size="small"
                onClick={() => handleSelectCategory(params.group)}
                disabled={disabled}
              >
                {isCategoryFullySelected(params.group) ? 'Deselect All' : 'Select All'}
              </Button>
            </Box>
            <ul style={{ padding: 0 }}>{params.children}</ul>
          </li>
        )}
        renderTags={(value, getTagProps) =>
          value.map((option, index) => {
            const tagProps = getTagProps({ index });
            return (
              <Chip
                {...tagProps}
                key={option.event}
                label={option.label}
                size="small"
                color="primary"
                variant="outlined"
              />
            );
          })
        }
        renderInput={(params) => (
          <TextField
            {...params}
            label="Event Types"
            placeholder={selectedEvents.length === 0 ? 'Select event types...' : ''}
            helperText={`${selectedEvents.length} event(s) selected`}
          />
        )}
      />
    </Box>
  );
};

export default EventTypeSelector;

import React, { useState } from 'react';
import {
  Box,
  Typography,
  Paper,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Chip,
  Stack,
  Tooltip as MuiTooltip,
  useTheme,
} from '@mui/material';
import { ComposableMap, Geographies, Geography, Marker, ZoomableGroup } from 'react-simple-maps';

interface GeographicDataPoint {
  country?: string;
  state?: string;
  city?: string;
  latitude?: number;
  longitude?: number;
  value: number;
  label?: string;
}

interface GeographicVisualizationProps {
  data: GeographicDataPoint[];
  title?: string;
  metric?: string;
  mapType?: 'world' | 'us';
  height?: number;
}

// Simplified US map data (state centroids)
const usStateCentroids: Record<string, { lat: number; lon: number }> = {
  'California': { lat: 36.7783, lon: -119.4179 },
  'Texas': { lat: 31.9686, lon: -99.9018 },
  'Florida': { lat: 27.6648, lon: -81.5158 },
  'New York': { lat: 40.7128, lon: -74.0060 },
  'Illinois': { lat: 40.6331, lon: -89.3985 },
  'Pennsylvania': { lat: 41.2033, lon: -77.1945 },
  'Ohio': { lat: 40.4173, lon: -82.9071 },
  'Georgia': { lat: 32.1656, lon: -82.9001 },
  'North Carolina': { lat: 35.7596, lon: -79.0193 },
  'Michigan': { lat: 44.3148, lon: -85.6024 },
};

const GeographicVisualization: React.FC<GeographicVisualizationProps> = ({
  data,
  title = 'Geographic Distribution',
  metric = 'Revenue',
  mapType = 'us',
  height = 500,
}) => {
  const theme = useTheme();
  const [viewType, setViewType] = useState<'map' | 'table'>('map');
  
  // Enrich data with coordinates
  const enrichedData = data.map((point) => {
    if (point.latitude && point.longitude) {
      return point;
    }
    
    // Try to find coordinates from state
    if (point.state && usStateCentroids[point.state]) {
      return {
        ...point,
        latitude: usStateCentroids[point.state].lat,
        longitude: usStateCentroids[point.state].lon,
      };
    }
    
    return point;
  });

  const validMarkers = enrichedData.filter((d) => d.latitude && d.longitude);
  
  // Calculate color intensity based on value
  const maxValue = Math.max(...enrichedData.map((d) => d.value));
  const getMarkerSize = (value: number) => {
    const minSize = 4;
    const maxSize = 20;
    return minSize + ((value / maxValue) * (maxSize - minSize));
  };
  
  const getMarkerColor = (value: number) => {
    const intensity = value / maxValue;
    if (intensity > 0.75) return theme.palette.error.main;
    if (intensity > 0.5) return theme.palette.warning.main;
    if (intensity > 0.25) return theme.palette.info.main;
    return theme.palette.success.main;
  };

  const formatValue = (value: number) => {
    if (value >= 1000000) return `$${(value / 1000000).toFixed(1)}M`;
    if (value >= 1000) return `$${(value / 1000).toFixed(1)}K`;
    return `$${value}`;
  };

  // Sort data for table view
  const sortedData = [...enrichedData].sort((a, b) => b.value - a.value);

  return (
    <Paper sx={{ p: 3 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h6" fontWeight={600}>
          {title}
        </Typography>
        <FormControl size="small" sx={{ minWidth: 120 }}>
          <InputLabel>View</InputLabel>
          <Select
            value={viewType}
            label="View"
            onChange={(e) => setViewType(e.target.value as 'map' | 'table')}
          >
            <MenuItem value="map">Map</MenuItem>
            <MenuItem value="table">Table</MenuItem>
          </Select>
        </FormControl>
      </Box>

      {viewType === 'map' && (
        <Box sx={{ width: '100%', height }}>
          <ComposableMap
            projection="geoAlbersUsa"
            projectionConfig={{
              scale: 1000,
            }}
            style={{
              width: '100%',
              height: '100%',
            }}
          >
            <ZoomableGroup>
              <Geographies geography="/maps/us-states.json">
                {({ geographies }) =>
                  geographies.map((geo) => (
                    <Geography
                      key={geo.rsmKey}
                      geography={geo}
                      fill="#DDD"
                      stroke="#FFF"
                      strokeWidth={0.5}
                      style={{
                        default: { outline: 'none' },
                        hover: { outline: 'none', fill: '#CCC' },
                        pressed: { outline: 'none' },
                      }}
                    />
                  ))
                }
              </Geographies>
              
              {validMarkers.map((marker, index) => (
                <Marker
                  key={`marker-${index}`}
                  coordinates={[marker.longitude!, marker.latitude!]}
                >
                  <MuiTooltip
                    title={`${marker.state || marker.city}: ${formatValue(marker.value)}`}
                    arrow
                  >
                    <circle
                      r={getMarkerSize(marker.value)}
                      fill={getMarkerColor(marker.value)}
                      stroke="#fff"
                      strokeWidth={1}
                      style={{ cursor: 'pointer' }}
                      opacity={0.8}
                    />
                  </MuiTooltip>
                </Marker>
              ))}
            </ZoomableGroup>
          </ComposableMap>

          {/* Legend */}
          <Box sx={{ mt: 2, display: 'flex', justifyContent: 'center', gap: 2 }}>
            <Stack direction="row" spacing={1} alignItems="center">
              <Box
                sx={{
                  width: 16,
                  height: 16,
                  borderRadius: '50%',
                  bgcolor: theme.palette.success.main,
                }}
              />
              <Typography variant="caption">Low</Typography>
            </Stack>
            <Stack direction="row" spacing={1} alignItems="center">
              <Box
                sx={{
                  width: 16,
                  height: 16,
                  borderRadius: '50%',
                  bgcolor: theme.palette.info.main,
                }}
              />
              <Typography variant="caption">Medium</Typography>
            </Stack>
            <Stack direction="row" spacing={1} alignItems="center">
              <Box
                sx={{
                  width: 16,
                  height: 16,
                  borderRadius: '50%',
                  bgcolor: theme.palette.warning.main,
                }}
              />
              <Typography variant="caption">High</Typography>
            </Stack>
            <Stack direction="row" spacing={1} alignItems="center">
              <Box
                sx={{
                  width: 16,
                  height: 16,
                  borderRadius: '50%',
                  bgcolor: theme.palette.error.main,
                }}
              />
              <Typography variant="caption">Very High</Typography>
            </Stack>
          </Box>
        </Box>
      )}

      {viewType === 'table' && (
        <Stack spacing={1}>
          {sortedData.map((point, index) => (
            <Box
              key={index}
              sx={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'center',
                p: 2,
                bgcolor: 'grey.50',
                borderRadius: 1,
                borderLeft: `4px solid ${getMarkerColor(point.value)}`,
              }}
            >
              <Box>
                <Typography variant="body2" fontWeight={500}>
                  {point.label || point.state || point.city || point.country}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  Rank #{index + 1}
                </Typography>
              </Box>
              <Box sx={{ textAlign: 'right' }}>
                <Typography variant="body2" fontWeight={600}>
                  {formatValue(point.value)}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {metric}
                </Typography>
              </Box>
            </Box>
          ))}
        </Stack>
      )}

      <Box sx={{ mt: 2, display: 'flex', gap: 1, flexWrap: 'wrap' }}>
        <Chip
          label={`Total ${metric}: ${formatValue(enrichedData.reduce((sum, d) => sum + d.value, 0))}`}
          color="primary"
        />
        <Chip label={`${enrichedData.length} Locations`} />
      </Box>
    </Paper>
  );
};

export default GeographicVisualization;

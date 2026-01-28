# Navigation Configuration Guide

## Overview

The CRM uses a centralized navigation configuration system. All navigation items are defined in a single source of truth: `CRM.Frontend/src/config/navigationConfig.ts`.

## Architecture

### Single Source of Truth

The `navigationConfig.ts` file contains:
- **NAV_ITEMS_CONFIG**: Array of all navigation items with icons, paths, and categories
- **CATEGORIES**: Array of category definitions for grouping nav items
- **Helper functions**: Utilities for accessing nav items and generating configurations

### Three-Layer Configuration

1. **Static Config** (`navigationConfig.ts`): Defines all available navigation items
2. **Database Defaults** (`DbSeed.cs`): Seeds initial nav configuration for new installations
3. **User Preferences**: Stored in localStorage (`crm_nav_order`) and database (SystemSettings.NavOrderConfig)

## Adding a New Navigation Item

### Step 1: Add the Route (App.tsx)

```tsx
<Route path="/my-new-feature" element={<PrivateRoute><MyNewFeaturePage /></PrivateRoute>} />
```

### Step 2: Add to Navigation Config

Edit `CRM.Frontend/src/config/navigationConfig.ts`:

1. Import the icon:
```tsx
import { NewIcon as MyFeatureIcon } from '@mui/icons-material';
```

2. Add to NAV_ITEMS_CONFIG:
```tsx
{
  id: 'my-new-feature',
  label: 'My New Feature',
  path: '/my-new-feature',
  icon: MyFeatureIcon,
  order: 25, // Choose appropriate order
  category: 'main', // Or sales, support, productivity, info, admin
  description: 'Description of the feature',
},
```

### Step 3: Update Navigation.tsx

Add the icon import and navItemsConfig entry:
```tsx
import { NewIcon as MyFeatureIcon } from '@mui/icons-material';

// In navItemsConfig:
'my-new-feature': {
  label: 'My New Feature',
  path: '/my-new-feature',
  icon: <MyFeatureIcon />,
  category: 'main',
},
```

### Step 4: Update NavigationSettingsTab.tsx

Add to DEFAULT_NAV_ITEMS and iconMap:
```tsx
{ id: 'my-new-feature', label: 'My New Feature', icon: 'MyFeatureIcon', order: 25, category: 'main' },

// In iconMap:
MyFeatureIcon: <MyFeatureIcon />,
```

### Step 5: Update DbSeed.cs (for new installations)

Add to the defaultNavConfig JSON:
```csharp
{""id"":""my-new-feature"",""order"":25,""visible"":true,""category"":""main""},
```

## Categories

| ID | Label | Description |
|---|---|---|
| main | Main | Core CRM functionality |
| sales | Sales & Marketing | Sales pipeline, campaigns, quotes |
| support | Customer Support | Services and service requests |
| productivity | Productivity | Tasks, notes, communications |
| info | Help & Info | About, help, licenses |
| admin | Administration | Settings, workflows, channel config |

## User Customization

Users can customize their navigation through:
- **Navigation Settings** (Settings → Navigation): Reorder items, hide/show, organize by category
- **Drag & Drop**: Reorder items in the sidebar directly

## Database Storage

Navigation preferences are stored in:
- **SystemSettings table**: `NavOrderConfig` field (JSON)
- **localStorage**: `crm_nav_order` key (fallback)

## Troubleshooting

### New item not appearing?

1. Check that the item is in `navItemsConfig` in Navigation.tsx
2. Verify the route exists in App.tsx
3. Clear localStorage: `localStorage.removeItem('crm_nav_order')`
4. Check browser console for errors

### Item appears but icon missing?

1. Verify icon is imported in both Navigation.tsx and NavigationSettingsTab.tsx
2. Check that iconMap includes the icon in NavigationSettingsTab.tsx

### Item in wrong category?

1. Update the `category` field in all config locations
2. Clear localStorage to reset user preferences

## Future Improvements

The goal is to have navigation items auto-discovered from routes. This could be achieved by:
1. Using route metadata to define nav properties
2. Build-time generation of navigation config from routes
3. Database-driven navigation with admin UI for adding items

Currently, manual updates are required in the locations listed above.

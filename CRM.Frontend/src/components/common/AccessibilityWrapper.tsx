/**
 * AccessibilityWrapper - Higher-Order Component and wrapper component
 * that enhances any interactive element with ARIA attributes.
 *
 * TODO-UX-01: ARIA labels for all interactive components
 *
 * Usage as HOC:
 *   const AccessibleButton = withAccessibility(Button);
 *   <AccessibleButton label="Delete account" .../>
 *
 * Usage as wrapper:
 *   <AccessibilityWrapper label="Filter panel" role="group">
 *     <Select .../>
 *   </AccessibilityWrapper>
 */

import React, { forwardRef } from 'react';
import { Box } from '@mui/material';
import type { BoxProps } from '@mui/material';

// --------------------------------------------------------------------------
// Types
// --------------------------------------------------------------------------

export interface AccessibilityProps {
  /** Human-readable label announced by screen readers (maps to aria-label) */
  label?: string;
  /** ID of an element that labels this component (maps to aria-labelledby) */
  labelledBy?: string;
  /** ID of an element that describes this component (maps to aria-describedby) */
  describedBy?: string;
  /** ARIA role override */
  role?: string;
  /** Whether the element is hidden from assistive technology */
  ariaHidden?: boolean;
  /** Live region politeness for dynamic content */
  live?: 'polite' | 'assertive' | 'off';
  /** aria-expanded for disclosure widgets */
  expanded?: boolean;
  /** aria-selected for selectable items */
  selected?: boolean;
  /** aria-disabled */
  disabled?: boolean;
  /** aria-busy for loading states */
  busy?: boolean;
  /** aria-current for navigation items */
  current?: boolean | 'page' | 'step' | 'location' | 'date' | 'time';
}

export interface AccessibilityWrapperProps extends AccessibilityProps {
  children: React.ReactNode;
  /** HTML tag to use for the wrapper element. Defaults to 'div'. */
  as?: React.ElementType;
  /** Additional Box/HTML props forwarded to the wrapper element */
  sx?: BoxProps['sx'];
  className?: string;
  id?: string;
  /** If true, renders children inline without a wrapper element */
  transparent?: boolean;
}

// --------------------------------------------------------------------------
// Build ARIA attribute object from props
// --------------------------------------------------------------------------

export function buildAriaProps(props: AccessibilityProps): Record<string, unknown> {
  const attrs: Record<string, unknown> = {};
  if (props.label !== undefined) attrs['aria-label'] = props.label;
  if (props.labelledBy !== undefined) attrs['aria-labelledby'] = props.labelledBy;
  if (props.describedBy !== undefined) attrs['aria-describedby'] = props.describedBy;
  if (props.role !== undefined) attrs['role'] = props.role;
  if (props.ariaHidden !== undefined) attrs['aria-hidden'] = props.ariaHidden;
  if (props.live !== undefined) attrs['aria-live'] = props.live;
  if (props.expanded !== undefined) attrs['aria-expanded'] = props.expanded;
  if (props.selected !== undefined) attrs['aria-selected'] = props.selected;
  if (props.disabled !== undefined) attrs['aria-disabled'] = props.disabled;
  if (props.busy !== undefined) attrs['aria-busy'] = props.busy;
  if (props.current !== undefined) attrs['aria-current'] = props.current;
  return attrs;
}

// --------------------------------------------------------------------------
// Wrapper Component
// --------------------------------------------------------------------------

export const AccessibilityWrapper: React.FC<AccessibilityWrapperProps> = ({
  children,
  as: Component = 'div',
  sx,
  className,
  id,
  transparent = false,
  // Accessibility props
  label,
  labelledBy,
  describedBy,
  role,
  ariaHidden,
  live,
  expanded,
  selected,
  disabled,
  busy,
  current,
}) => {
  const ariaProps = buildAriaProps({
    label,
    labelledBy,
    describedBy,
    role,
    ariaHidden,
    live,
    expanded,
    selected,
    disabled,
    busy,
    current,
  });

  if (transparent) {
    // Clone children to pass ARIA attrs without an extra wrapper element
    const child = React.Children.only(children) as React.ReactElement;
    return React.cloneElement(child, ariaProps);
  }

  return (
    <Box
      component={Component as React.ElementType}
      id={id}
      className={className}
      sx={sx}
      {...ariaProps}
    >
      {children}
    </Box>
  );
};

// --------------------------------------------------------------------------
// Higher-Order Component factory
// --------------------------------------------------------------------------

export type WithAccessibilityProps<P> = P & AccessibilityProps;

/**
 * withAccessibility<P>
 * Wraps a component so that any AccessibilityProps passed to it are forwarded
 * as proper aria-* attributes, while all original props still work.
 *
 * @example
 *   const AccessibleTextField = withAccessibility(TextField);
 *   <AccessibleTextField label="Search" inputProps={{ 'aria-label': 'Search accounts' }} />
 */
export function withAccessibility<P extends object>(
  WrappedComponent: React.ComponentType<P>,
  displayName?: string,
) {
  const Accessible = forwardRef<unknown, WithAccessibilityProps<P>>(
    (
      {
        label,
        labelledBy,
        describedBy,
        role,
        ariaHidden,
        live,
        expanded,
        selected,
        disabled,
        busy,
        current,
        ...rest
      },
      ref,
    ) => {
      const ariaAttrs = buildAriaProps({
        label,
        labelledBy,
        describedBy,
        role,
        ariaHidden,
        live,
        expanded,
        selected,
        disabled,
        busy,
        current,
      });

      return (
        <WrappedComponent
          ref={ref as React.Ref<unknown>}
          {...(rest as P)}
          {...ariaAttrs}
        />
      );
    },
  );

  Accessible.displayName = displayName ?? `withAccessibility(${WrappedComponent.displayName ?? WrappedComponent.name ?? 'Component'})`;
  return Accessible;
}

// --------------------------------------------------------------------------
// Pre-built accessible wrappers for common action patterns
// --------------------------------------------------------------------------

export interface AccessibleRegionProps {
  label: string;
  children: React.ReactNode;
  sx?: BoxProps['sx'];
  id?: string;
}

/** Semantic landmark wrapper for page regions */
export const AccessibleRegion: React.FC<AccessibleRegionProps> = ({
  label,
  children,
  sx,
  id,
}) => (
  <Box component="section" aria-label={label} id={id} sx={sx}>
    {children}
  </Box>
);

/** Live-region that announces dynamic status changes to screen readers */
export interface LiveRegionProps {
  message: string;
  politeness?: 'polite' | 'assertive';
  /** Hide visually but keep accessible */
  visuallyHidden?: boolean;
}

export const LiveRegion: React.FC<LiveRegionProps> = ({
  message,
  politeness = 'polite',
  visuallyHidden = true,
}) => (
  <Box
    aria-live={politeness}
    aria-atomic="true"
    sx={
      visuallyHidden
        ? {
            position: 'absolute',
            width: 1,
            height: 1,
            margin: -1,
            padding: 0,
            overflow: 'hidden',
            clip: 'rect(0,0,0,0)',
            whiteSpace: 'nowrap',
            border: 0,
          }
        : undefined
    }
  >
    {message}
  </Box>
);

export default AccessibilityWrapper;

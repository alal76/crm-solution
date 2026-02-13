/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Condition Builder - wrapper around RuleBuilder for workflow conditions
 */

import React from 'react';
import { RuleBuilder, type RuleBuilderProps } from './RuleBuilder';

export type ConditionBuilderProps = RuleBuilderProps;

const ConditionBuilder: React.FC<ConditionBuilderProps> = (props) => {
  return <RuleBuilder {...props} />;
};

export default ConditionBuilder;

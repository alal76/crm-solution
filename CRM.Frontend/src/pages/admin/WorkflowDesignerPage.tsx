/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * Visual Workflow Designer - Node-based canvas for designing workflows
 */

import { useState, useEffect, useCallback, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Button,
  IconButton,
  Paper,
  Drawer,
  Divider,
  CircularProgress,
  Chip,
} from '@mui/material';
import {
  Delete as DeleteIcon,
  PlayCircle as TriggerIcon,
  CallSplit as ConditionIcon,
  FlashOn as ActionIcon,
  Person as HumanTaskIcon,
  Schedule as WaitIcon,
  Psychology as LLMIcon,
  StopCircle as EndIcon,
  ArrowBack as BackIcon,
  Settings as SettingsIcon,
  PanTool as PanIcon,
  Publish as PublishIcon,
  ContentCopy as CloneIcon,
  Close as CloseIcon,
  Hub as ParallelIcon,
  Merge as JoinIcon,
  AccountTree as SubprocessIcon,
  CheckCircle as CheckIcon,
  // AI-Enhanced Workflow Node Icons
  Route as AIDecisionIcon,
  SmartToy as AIAgentIcon,
  AutoAwesome as AIContentGeneratorIcon,
  DataObject as AIDataExtractorIcon,
  Category as AIClassifierIcon,
  SentimentSatisfied as AISentimentIcon,
  RateReview as HumanReviewIcon,
  BuildCircle as AIToolIcon,
} from '@mui/icons-material';
import {
  RuleBuilder,
  WorkflowSimulator,
  VersionDiffViewer,
  WorkflowCanvas,
  WorkflowToolbar,
  NodePalette,
  NodeEditor,
  TransitionEditor,
} from '../../components/workflow';
import {
  workflowService,
  WorkflowDefinitionDetail,
  WorkflowVersionDetail,
  WorkflowNode,
  WorkflowTransition,
  CreateNodeDto,
  CreateTransitionDto,
  UpdateNodeDto,
  UpdateTransitionDto,
  nodeTypeInfo,
  WorkflowNodeType,
} from '../../services/workflowService';

const DRAWER_WIDTH = 280;
const PROPERTIES_WIDTH = 420;
const GRID_SIZE = 20;
const DEFAULT_NODE_WIDTH = 180;
const DEFAULT_NODE_HEIGHT = 72;

// Icon mapping for node types (icons must be local React components)
const nodeTypeIcons: Record<string, React.ComponentType> = {
  Trigger: TriggerIcon,
  Condition: ConditionIcon,
  Action: ActionIcon,
  HumanTask: HumanTaskIcon,
  Wait: WaitIcon,
  LLMAction: LLMIcon,
  ParallelGateway: ParallelIcon,
  JoinGateway: JoinIcon,
  Subprocess: SubprocessIcon,
  End: EndIcon,
  // AI-Enhanced Node Types
  AIDecision: AIDecisionIcon,
  AIAgent: AIAgentIcon,
  AIContentGenerator: AIContentGeneratorIcon,
  AIDataExtractor: AIDataExtractorIcon,
  AIClassifier: AIClassifierIcon,
  AISentimentAnalyzer: AISentimentIcon,
  HumanReview: HumanReviewIcon,
};

// Default node type list (will be enhanced from backend config when available)
const defaultNodeTypeList = [
  { type: 'Trigger', label: 'Trigger', description: 'Start the workflow', category: 'flow' },
  { type: 'Condition', label: 'Condition', description: 'Branch based on rules', category: 'flow' },
  { type: 'Action', label: 'Action', description: 'Perform automated action', category: 'actions' },
  { type: 'HumanTask', label: 'Human Task', description: 'Require user input', category: 'actions' },
  { type: 'Wait', label: 'Wait/Timer', description: 'Delay execution', category: 'flow' },
  { type: 'LLMAction', label: 'AI/LLM Action', description: 'AI-powered processing', category: 'ai' },
  { type: 'ParallelGateway', label: 'Parallel Split', description: 'Split into parallel paths', category: 'flow' },
  { type: 'JoinGateway', label: 'Parallel Join', description: 'Merge parallel paths', category: 'flow' },
  { type: 'Subprocess', label: 'Subprocess', description: 'Call another workflow', category: 'flow' },
  { type: 'End', label: 'End', description: 'End the workflow', category: 'flow' },
  // AI-Enhanced Node Types
  { type: 'AIDecision', label: 'AI Decision', description: 'Route based on AI analysis', category: 'ai' },
  { type: 'AIAgent', label: 'AI Agent', description: 'Autonomous agent with tools', category: 'ai' },
  { type: 'AIContentGenerator', label: 'AI Content Generator', description: 'Generate emails, summaries, reports', category: 'ai' },
  { type: 'AIDataExtractor', label: 'AI Data Extractor', description: 'Extract structured data from text', category: 'ai' },
  { type: 'AIClassifier', label: 'AI Classifier', description: 'Categorize and tag content', category: 'ai' },
  { type: 'AISentimentAnalyzer', label: 'AI Sentiment Analyzer', description: 'Analyze sentiment and emotion', category: 'ai' },
  { type: 'HumanReview', label: 'Human Review', description: 'Human-in-the-loop review', category: 'ai' },
];

// Helper to get icon component for a node type
const getNodeTypeIcon = (type: string): React.ComponentType<{ fontSize?: 'small' | 'medium' | 'large' | 'inherit' }> => {
  return nodeTypeIcons[type] || ActionIcon;
};

interface CanvasNode extends WorkflowNode {
  selected?: boolean;
}

interface CanvasTransition extends WorkflowTransition {
  selected?: boolean;
}

interface DragState {
  isDragging: boolean;
  nodeId?: number;
  startX: number;
  startY: number;
  offsetX: number;
  offsetY: number;
}

interface ConnectState {
  isConnecting: boolean;
  sourceNodeId?: number;
  sourceHandle?: string;
  tempX?: number;
  tempY?: number;
}

function WorkflowDesignerPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const canvasRef = useRef<HTMLDivElement>(null);

  // Node type list - can be overridden by backend config
  const [nodeTypeList, setNodeTypeList] = useState(
    defaultNodeTypeList.map(n => ({ ...n, icon: getNodeTypeIcon(n.type) }))
  );

  // Load config from backend
  useEffect(() => {
    workflowService.getConfig().then(config => {
      if (config.nodeTypes?.length) {
        setNodeTypeList(config.nodeTypes.map(nt => ({
          type: nt.value,
          label: nt.label,
          description: nt.description || '',
          icon: getNodeTypeIcon(nt.value),
          category: 'flow', // Default category
        })));
      }
    }).catch(() => {
      // Use defaults on error
    });
  }, []);

  // State
  const [workflow, setWorkflow] = useState<WorkflowDefinitionDetail | null>(null);
  const [version, setVersion] = useState<WorkflowVersionDetail | null>(null);
  const [nodes, setNodes] = useState<CanvasNode[]>([]);
  const [transitions, setTransitions] = useState<CanvasTransition[]>([]);
  const [selectedNode, setSelectedNode] = useState<CanvasNode | null>(null);
  const [selectedTransition, setSelectedTransition] = useState<CanvasTransition | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [hasChanges, setHasChanges] = useState(false);

  // Canvas state
  const [zoom, setZoom] = useState(1);
  const [pan, setPan] = useState({ x: 0, y: 0 });
  const [showGrid, setShowGrid] = useState(true);
  const [dragState, setDragState] = useState<DragState>({
    isDragging: false,
    startX: 0,
    startY: 0,
    offsetX: 0,
    offsetY: 0,
  });
  const [connectState, setConnectState] = useState<ConnectState>({
    isConnecting: false,
  });
  const [isPanning, setIsPanning] = useState(false);
  const [panStart, setPanStart] = useState({ x: 0, y: 0 });

  // UI state
  const [propertiesOpen, setPropertiesOpen] = useState(false);
  const [paletteExpanded, setPaletteExpanded] = useState<string | false>('nodes');
  const [simulatorOpen, setSimulatorOpen] = useState(false);
  const [versionDiffOpen, setVersionDiffOpen] = useState(false);

  // Undo/Redo history
  const MAX_HISTORY = 50;
  const [history, setHistory] = useState<{ nodes: CanvasNode[]; transitions: CanvasTransition[] }[]>([]);
  const [historyIndex, setHistoryIndex] = useState(-1);
  const isUndoRedoRef = useRef(false);

  const pushHistory = useCallback((currentNodes: CanvasNode[], currentTransitions: CanvasTransition[]) => {
    if (isUndoRedoRef.current) return;
    setHistory(prev => {
      const truncated = prev.slice(0, historyIndex + 1);
      const entry = {
        nodes: currentNodes.map(n => ({ ...n })),
        transitions: currentTransitions.map(t => ({ ...t })),
      };
      const newHistory = [...truncated, entry].slice(-MAX_HISTORY);
      setHistoryIndex(newHistory.length - 1);
      return newHistory;
    });
  }, [historyIndex]);

  const canUndo = historyIndex > 0;
  const canRedo = historyIndex < history.length - 1;

  const handleUndo = useCallback(() => {
    if (!canUndo) return;
    isUndoRedoRef.current = true;
    const prevIndex = historyIndex - 1;
    const snapshot = history[prevIndex];
    setNodes(snapshot.nodes.map(n => ({ ...n })));
    setTransitions(snapshot.transitions.map(t => ({ ...t })));
    setHistoryIndex(prevIndex);
    setHasChanges(true);
    setSelectedNode(null);
    setSelectedTransition(null);
    setPropertiesOpen(false);
    setTimeout(() => { isUndoRedoRef.current = false; }, 0);
  }, [canUndo, historyIndex, history]);

  const handleRedo = useCallback(() => {
    if (!canRedo) return;
    isUndoRedoRef.current = true;
    const nextIndex = historyIndex + 1;
    const snapshot = history[nextIndex];
    setNodes(snapshot.nodes.map(n => ({ ...n })));
    setTransitions(snapshot.transitions.map(t => ({ ...t })));
    setHistoryIndex(nextIndex);
    setHasChanges(true);
    setSelectedNode(null);
    setSelectedTransition(null);
    setPropertiesOpen(false);
    setTimeout(() => { isUndoRedoRef.current = false; }, 0);
  }, [canRedo, historyIndex, history]);

  // Keyboard shortcuts for undo/redo and delete
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      const isMac = navigator.platform.toUpperCase().indexOf('MAC') >= 0;
      const mod = isMac ? e.metaKey : e.ctrlKey;

      if (mod && e.key === 'z' && !e.shiftKey) {
        e.preventDefault();
        handleUndo();
      } else if ((mod && e.key === 'y') || (mod && e.shiftKey && e.key === 'z') || (mod && e.shiftKey && e.key === 'Z')) {
        e.preventDefault();
        handleRedo();
      } else if (e.key === 'Delete' || e.key === 'Backspace') {
        // Delete selected node/transition (only if not in an input)
        const tag = (e.target as HTMLElement).tagName;
        if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') return;
        if (selectedNode) {
          deleteNode(selectedNode);
        } else if (selectedTransition) {
          deleteTransition(selectedTransition);
        }
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [handleUndo, handleRedo, selectedNode, selectedTransition]);

  // Load workflow data
  const loadWorkflow = useCallback(async () => {
    if (!id) return;
    try {
      setLoading(true);
      const workflowData = await workflowService.getWorkflow(parseInt(id));
      setWorkflow(workflowData);

      // Get the draft version or latest
      const draftVersion = workflowData.versions.find(v => v.status === 'Draft');
      if (draftVersion) {
        const versionData = await workflowService.getVersion(draftVersion.id);
        setVersion(versionData);
        setNodes(versionData.nodes.map(n => ({ ...n, selected: false })));
        setTransitions(versionData.transitions.map(t => ({ ...t, selected: false })));
      } else if (workflowData.versions.length > 0) {
        // Create a new draft version from the latest
        const latestVersion = workflowData.versions[0];
        const newVersion = await workflowService.createVersion(parseInt(id), latestVersion.id);
        const versionData = await workflowService.getVersion(newVersion.id);
        setVersion(versionData);
        setNodes(versionData.nodes.map(n => ({ ...n, selected: false })));
        setTransitions(versionData.transitions.map(t => ({ ...t, selected: false })));
      }
      setError('');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load workflow');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    loadWorkflow();
  }, [loadWorkflow]);

  // Snap to grid
  const snapToGrid = (value: number) => Math.round(value / GRID_SIZE) * GRID_SIZE;

  // Handle node drag
  const handleNodeMouseDown = (e: React.MouseEvent, node: CanvasNode) => {
    if (e.button !== 0) return; // Only left click
    e.stopPropagation();

    // Check if clicking on a handle
    const target = e.target as HTMLElement;
    if (target.classList.contains('node-handle')) {
      // Start connection
      const handle = target.dataset.handle || 'right';
      setConnectState({
        isConnecting: true,
        sourceNodeId: node.id,
        sourceHandle: handle,
        tempX: e.clientX,
        tempY: e.clientY,
      });
      return;
    }

    // Select node
    selectNode(node);

    // Start dragging
    const rect = canvasRef.current?.getBoundingClientRect();
    if (!rect) return;

    setDragState({
      isDragging: true,
      nodeId: node.id,
      startX: node.positionX,
      startY: node.positionY,
      offsetX: e.clientX - rect.left - pan.x - node.positionX * zoom,
      offsetY: e.clientY - rect.top - pan.y - node.positionY * zoom,
    });
  };

  const handleCanvasMouseMove = (e: React.MouseEvent) => {
    const rect = canvasRef.current?.getBoundingClientRect();
    if (!rect) return;

    if (dragState.isDragging && dragState.nodeId) {
      // Drag node
      const newX = (e.clientX - rect.left - pan.x - dragState.offsetX) / zoom;
      const newY = (e.clientY - rect.top - pan.y - dragState.offsetY) / zoom;

      setNodes(prev =>
        prev.map(n =>
          n.id === dragState.nodeId
            ? { ...n, positionX: snapToGrid(newX), positionY: snapToGrid(newY) }
            : n
        )
      );
      setHasChanges(true);
    } else if (connectState.isConnecting) {
      // Update temp connection line
      setConnectState(prev => ({
        ...prev,
        tempX: e.clientX,
        tempY: e.clientY,
      }));
    } else if (isPanning) {
      // Pan canvas
      setPan({
        x: e.clientX - panStart.x,
        y: e.clientY - panStart.y,
      });
    }
  };

  const handleCanvasMouseUp = async (e: React.MouseEvent) => {
    if (dragState.isDragging && dragState.nodeId) {
      // Save node position
      const node = nodes.find(n => n.id === dragState.nodeId);
      if (node) {
        try {
          await workflowService.updateNode(node.id, {
            positionX: node.positionX,
            positionY: node.positionY,
          });
        } catch (err) {
          console.error('Failed to save node position:', err);
        }
      }
    }

    if (connectState.isConnecting && connectState.sourceNodeId) {
      // Check if dropped on a node
      const target = e.target as HTMLElement;
      const targetHandle = target.closest('.node-handle') as HTMLElement;
      const targetNode = target.closest('.workflow-node') as HTMLElement;

      if (targetNode && targetHandle) {
        const targetNodeId = parseInt(targetNode.dataset.nodeId || '0');
        if (targetNodeId && targetNodeId !== connectState.sourceNodeId) {
          // Create transition
          await createTransition({
            sourceNodeId: connectState.sourceNodeId,
            targetNodeId,
            sourceHandle: connectState.sourceHandle || 'right',
            targetHandle: targetHandle.dataset.handle || 'left',
          });
        }
      }
    }

    setDragState({ isDragging: false, startX: 0, startY: 0, offsetX: 0, offsetY: 0 });
    setConnectState({ isConnecting: false });
    setIsPanning(false);
  };

  const handleCanvasMouseDown = (e: React.MouseEvent) => {
    if (e.button === 1 || (e.button === 0 && e.shiftKey)) {
      // Middle click or Shift+Left click to pan
      setIsPanning(true);
      setPanStart({ x: e.clientX - pan.x, y: e.clientY - pan.y });
      e.preventDefault();
    } else if (e.target === canvasRef.current || (e.target as HTMLElement).classList.contains('canvas-grid')) {
      // Click on empty canvas to deselect
      setSelectedNode(null);
      setSelectedTransition(null);
      setPropertiesOpen(false);
    }
  };

  const selectNode = (node: CanvasNode) => {
    setNodes(prev => prev.map(n => ({ ...n, selected: n.id === node.id })));
    setTransitions(prev => prev.map(t => ({ ...t, selected: false })));
    setSelectedNode(node);
    setSelectedTransition(null);
    setPropertiesOpen(true);
  };

  const selectTransition = (transition: CanvasTransition) => {
    setNodes(prev => prev.map(n => ({ ...n, selected: false })));
    setTransitions(prev => prev.map(t => ({ ...t, selected: t.id === transition.id })));
    setSelectedNode(null);
    setSelectedTransition(transition);
    setPropertiesOpen(true);
  };

  // Add node
  const addNode = async (nodeType: string) => {
    if (!version) return;

    // Find a good position for the new node
    const canvasRect = canvasRef.current?.getBoundingClientRect();
    const centerX = canvasRect ? (canvasRect.width / 2 - pan.x) / zoom : 400;
    const centerY = canvasRect ? (canvasRect.height / 2 - pan.y) / zoom : 300;

    const newNode: CreateNodeDto = {
      name: `${nodeTypeInfo[nodeType]?.label || nodeType} ${nodes.length + 1}`,
      nodeType,
      positionX: snapToGrid(centerX),
      positionY: snapToGrid(centerY),
      width: DEFAULT_NODE_WIDTH,
      height: DEFAULT_NODE_HEIGHT,
      iconName: nodeTypeInfo[nodeType]?.icon,
      color: nodeTypeInfo[nodeType]?.color,
      isStartNode: nodeType === 'Trigger',
      isEndNode: nodeType === 'End',
    };

    try {
      setSaving(true);
      const result = await workflowService.addNode(version.id, newNode);
      const addedNode: CanvasNode = {
        id: result.id,
        nodeKey: result.nodeKey,
        ...newNode,
        positionX: newNode.positionX,
        positionY: newNode.positionY,
        width: newNode.width || DEFAULT_NODE_WIDTH,
        height: newNode.height || DEFAULT_NODE_HEIGHT,
        isStartNode: newNode.isStartNode || false,
        isEndNode: newNode.isEndNode || false,
        timeoutMinutes: 0,
        retryCount: 0,
        executionOrder: nodes.length,
        selected: true,
      };
      setNodes(prev => [...prev.map(n => ({ ...n, selected: false })), addedNode]);
      setSelectedNode(addedNode);
      setPropertiesOpen(true);
      setSuccess('Node added');
      setHasChanges(true);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to add node');
    } finally {
      setSaving(false);
    }
  };

  // Create transition
  const createTransition = async (dto: CreateTransitionDto) => {
    if (!version) return;

    try {
      setSaving(true);
      const result = await workflowService.addTransition(version.id, dto);
      const addedTransition: CanvasTransition = {
        id: result.id,
        sourceNodeId: dto.sourceNodeId,
        targetNodeId: dto.targetNodeId,
        transitionKey: dto.transitionKey,
        label: dto.label,
        conditionType: dto.conditionType || 'Always',
        conditionExpression: dto.conditionExpression,
        isDefault: dto.isDefault || false,
        priority: dto.priority || 100,
        sourceHandle: dto.sourceHandle || 'right',
        targetHandle: dto.targetHandle || 'left',
        lineStyle: dto.lineStyle || 'solid',
        color: dto.color || '#888888',
        animationStyle: dto.animationStyle || 'none',
        selected: false,
      };
      setTransitions(prev => [...prev, addedTransition]);
      setSuccess('Connection created');
      setHasChanges(true);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create connection');
    } finally {
      setSaving(false);
    }
  };

  // Delete node
  const deleteNode = async (node: CanvasNode) => {
    try {
      setSaving(true);
      await workflowService.deleteNode(node.id);
      setNodes(prev => prev.filter(n => n.id !== node.id));
      setTransitions(prev =>
        prev.filter(t => t.sourceNodeId !== node.id && t.targetNodeId !== node.id)
      );
      setSelectedNode(null);
      setPropertiesOpen(false);
      setSuccess('Node deleted');
      setHasChanges(true);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete node');
    } finally {
      setSaving(false);
    }
  };

  // Delete transition
  const deleteTransition = async (transition: CanvasTransition) => {
    try {
      setSaving(true);
      await workflowService.deleteTransition(transition.id);
      setTransitions(prev => prev.filter(t => t.id !== transition.id));
      setSelectedTransition(null);
      setPropertiesOpen(false);
      setSuccess('Connection deleted');
      setHasChanges(true);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete connection');
    } finally {
      setSaving(false);
    }
  };

  const handleTransitionUpdate = async (updates: UpdateTransitionDto) => {
    if (!selectedTransition) return;

    await workflowService.updateTransition(selectedTransition.id, updates);
    setTransitions(prev =>
      prev.map(t => t.id === selectedTransition.id ? { ...t, ...updates } : t)
    );
    setSelectedTransition(prev => prev ? { ...prev, ...updates } : null);
  };

  // Update node properties
  const updateNodeProperty = async (property: keyof UpdateNodeDto, value: any) => {
    if (!selectedNode) return;

    try {
      await workflowService.updateNode(selectedNode.id, { [property]: value });
      setNodes(prev =>
        prev.map(n => (n.id === selectedNode.id ? { ...n, [property]: value } : n))
      );
      setSelectedNode(prev => (prev ? { ...prev, [property]: value } : null));
      setHasChanges(true);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to update node');
    }
  };

  // Get node position
  const getNodeCenter = (node: CanvasNode) => ({
    x: node.positionX + (node.width || DEFAULT_NODE_WIDTH) / 2,
    y: node.positionY + (node.height || DEFAULT_NODE_HEIGHT) / 2,
  });

  // Get handle position
  const getHandlePosition = (node: CanvasNode, handle: string) => {
    const width = node.width || DEFAULT_NODE_WIDTH;
    const height = node.height || DEFAULT_NODE_HEIGHT;

    switch (handle) {
      case 'top':
        return { x: node.positionX + width / 2, y: node.positionY };
      case 'right':
        return { x: node.positionX + width, y: node.positionY + height / 2 };
      case 'bottom':
        return { x: node.positionX + width / 2, y: node.positionY + height };
      case 'left':
      default:
        return { x: node.positionX, y: node.positionY + height / 2 };
    }
  };

  // Render transition line
  const renderTransitionPath = (t: CanvasTransition) => {
    const sourceNode = nodes.find(n => n.id === t.sourceNodeId);
    const targetNode = nodes.find(n => n.id === t.targetNodeId);
    if (!sourceNode || !targetNode) return null;

    const start = getHandlePosition(sourceNode, t.sourceHandle || 'right');
    const end = getHandlePosition(targetNode, t.targetHandle || 'left');

    // Create curved path
    const dx = end.x - start.x;
    const dy = end.y - start.y;
    const controlOffset = Math.min(Math.abs(dx), Math.abs(dy), 80);

    const path =
      t.sourceHandle === 'right' || t.sourceHandle === 'left'
        ? `M ${start.x} ${start.y} C ${start.x + controlOffset * (t.sourceHandle === 'right' ? 1 : -1)} ${start.y}, ${end.x - controlOffset * (t.targetHandle === 'left' ? 1 : -1)} ${end.y}, ${end.x} ${end.y}`
        : `M ${start.x} ${start.y} C ${start.x} ${start.y + controlOffset * (t.sourceHandle === 'bottom' ? 1 : -1)}, ${end.x} ${end.y - controlOffset * (t.targetHandle === 'top' ? 1 : -1)}, ${end.x} ${end.y}`;

    return (
      <g key={t.id} onClick={() => selectTransition(t)} style={{ cursor: 'pointer' }}>
        <path
          d={path}
          fill="none"
          stroke={t.selected ? '#1976d2' : t.color || '#888888'}
          strokeWidth={t.selected ? 3 : 2}
          strokeDasharray={t.lineStyle === 'dashed' ? '5,5' : t.lineStyle === 'dotted' ? '2,2' : undefined}
          markerEnd="url(#arrowhead)"
        />
        {t.label && (
          <text
            x={(start.x + end.x) / 2}
            y={(start.y + end.y) / 2 - 8}
            textAnchor="middle"
            fill="#666"
            fontSize={12}
          >
            {t.label}
          </text>
        )}
      </g>
    );
  };

  // Get icon component for node type
  const getNodeIcon = (nodeType: string) => {
    const iconInfo = nodeTypeList.find(n => n.type === nodeType);
    if (iconInfo) {
      const IconComponent = iconInfo.icon;
      return <IconComponent />;
    }
    return <ActionIcon />;
  };

  // Zoom controls
  const handleZoomIn = () => setZoom(prev => Math.min(prev + 0.1, 2));
  const handleZoomOut = () => setZoom(prev => Math.max(prev - 0.1, 0.3));
  const handleFitScreen = () => {
    setZoom(1);
    setPan({ x: 0, y: 0 });
  };

  if (loading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box sx={{ display: 'flex', height: 'calc(100vh - 64px)', overflow: 'hidden' }}>
      {/* Left Drawer - Node Palette */}
      <Drawer
        variant="permanent"
        sx={{
          width: DRAWER_WIDTH,
          flexShrink: 0,
          '& .MuiDrawer-paper': {
            width: DRAWER_WIDTH,
            position: 'relative',
            height: '100%',
          },
        }}
      >
        <Box sx={{ p: 2 }}>
          <Button
            startIcon={<BackIcon />}
            onClick={() => navigate('/admin/workflows')}
            sx={{ mb: 2 }}
          >
            Back to Workflows
          </Button>
          <Typography variant="h6" gutterBottom>
            {workflow?.name}
          </Typography>
          <Chip
            label={version?.status || 'Draft'}
            size="small"
            color={version?.status === 'Active' ? 'success' : 'default'}
            sx={{ mb: 2 }}
          />
        </Box>

        <Divider />

        <NodePalette
          nodeTypes={nodeTypeList}
          expanded={paletteExpanded === 'nodes'}
          onToggle={() => setPaletteExpanded(paletteExpanded === 'nodes' ? false : 'nodes')}
          onAddNode={addNode}
          showGrid={showGrid}
          onToggleGrid={setShowGrid}
        />
      </Drawer>

      {/* Main Canvas Area */}
      <Box sx={{ flexGrow: 1, display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
        <WorkflowToolbar
          error={error}
          success={success}
          onClearError={() => setError('')}
          onClearSuccess={() => setSuccess('')}
          zoom={zoom}
          onZoomOut={handleZoomOut}
          onZoomIn={handleZoomIn}
          onFitScreen={handleFitScreen}
          showGrid={showGrid}
          onToggleGrid={() => setShowGrid(!showGrid)}
          onOpenSimulator={() => setSimulatorOpen(true)}
          onOpenVersionHistory={() => setVersionDiffOpen(true)}
          saving={saving}
        />

        {/* Canvas */}
        <WorkflowCanvas
          canvasRef={canvasRef}
          showGrid={showGrid}
          zoom={zoom}
          pan={pan}
          isPanning={isPanning}
          onMouseDown={handleCanvasMouseDown}
          onMouseMove={handleCanvasMouseMove}
          onMouseUp={handleCanvasMouseUp}
          onMouseLeave={handleCanvasMouseUp}
          gridSize={GRID_SIZE}
        >

          {/* SVG Layer for Transitions */}
          <svg
            style={{
              position: 'absolute',
              inset: 0,
              width: '100%',
              height: '100%',
              pointerEvents: 'none',
            }}
          >
            <defs>
              <marker
                id="arrowhead"
                markerWidth="10"
                markerHeight="7"
                refX="9"
                refY="3.5"
                orient="auto"
              >
                <polygon points="0 0, 10 3.5, 0 7" fill="#888888" />
              </marker>
            </defs>
            <g
              transform={`translate(${pan.x}, ${pan.y}) scale(${zoom})`}
              style={{ pointerEvents: 'all' }}
            >
              {transitions.map(t => renderTransitionPath(t))}
              {/* Temp connection line */}
              {connectState.isConnecting && connectState.sourceNodeId && (
                <line
                  x1={(() => {
                    const node = nodes.find(n => n.id === connectState.sourceNodeId);
                    return node ? getHandlePosition(node, connectState.sourceHandle || 'right').x : 0;
                  })()}
                  y1={(() => {
                    const node = nodes.find(n => n.id === connectState.sourceNodeId);
                    return node ? getHandlePosition(node, connectState.sourceHandle || 'right').y : 0;
                  })()}
                  x2={(connectState.tempX! - pan.x) / zoom}
                  y2={(connectState.tempY! - pan.y) / zoom}
                  stroke="#1976d2"
                  strokeWidth={2}
                  strokeDasharray="5,5"
                />
              )}
            </g>
          </svg>

          {/* Nodes Layer */}
          <Box
            sx={{
              position: 'absolute',
              inset: 0,
              transform: `translate(${pan.x}px, ${pan.y}px) scale(${zoom})`,
              transformOrigin: '0 0',
            }}
          >
            {nodes.map(node => (
              <Paper
                key={node.id}
                data-node-id={node.id}
                className="workflow-node"
                elevation={node.selected ? 8 : 2}
                onMouseDown={(e) => handleNodeMouseDown(e, node)}
                sx={{
                  position: 'absolute',
                  left: node.positionX,
                  top: node.positionY,
                  width: node.width || DEFAULT_NODE_WIDTH,
                  height: node.height || DEFAULT_NODE_HEIGHT,
                  borderRadius: 2,
                  border: node.selected ? '2px solid #1976d2' : `2px solid ${node.color || '#6750A4'}`,
                  backgroundColor: 'white',
                  cursor: 'move',
                  userSelect: 'none',
                  display: 'flex',
                  alignItems: 'center',
                  px: 1.5,
                  gap: 1,
                  '&:hover': {
                    boxShadow: 4,
                  },
                }}
              >
                {/* Left Handle */}
                <Box
                  className="node-handle"
                  data-handle="left"
                  sx={{
                    position: 'absolute',
                    left: -6,
                    top: '50%',
                    transform: 'translateY(-50%)',
                    width: 12,
                    height: 12,
                    borderRadius: '50%',
                    backgroundColor: '#1976d2',
                    border: '2px solid white',
                    cursor: 'crosshair',
                  }}
                />

                {/* Node Content */}
                <Box
                  sx={{
                    width: 32,
                    height: 32,
                    borderRadius: 1,
                    backgroundColor: node.color || '#6750A4',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    color: 'white',
                    flexShrink: 0,
                  }}
                >
                  {getNodeIcon(node.nodeType)}
                </Box>
                <Box sx={{ flex: 1, overflow: 'hidden' }}>
                  <Typography
                    variant="body2"
                    fontWeight="medium"
                    noWrap
                    title={node.name}
                  >
                    {node.name}
                  </Typography>
                  <Typography variant="caption" color="text.secondary" noWrap>
                    {nodeTypeInfo[node.nodeType]?.label || node.nodeType}
                  </Typography>
                </Box>
                {node.isStartNode && (
                  <CheckIcon fontSize="small" sx={{ color: 'success.main' }} />
                )}

                {/* Right Handle */}
                <Box
                  className="node-handle"
                  data-handle="right"
                  sx={{
                    position: 'absolute',
                    right: -6,
                    top: '50%',
                    transform: 'translateY(-50%)',
                    width: 12,
                    height: 12,
                    borderRadius: '50%',
                    backgroundColor: '#1976d2',
                    border: '2px solid white',
                    cursor: 'crosshair',
                  }}
                />
              </Paper>
            ))}
          </Box>
        </WorkflowCanvas>
      </Box>

      {/* Right Drawer - Properties Panel */}
      <Drawer
        anchor="right"
        open={propertiesOpen}
        variant="persistent"
        sx={{
          width: PROPERTIES_WIDTH,
          flexShrink: 0,
          '& .MuiDrawer-paper': {
            width: PROPERTIES_WIDTH,
            position: 'relative',
          },
        }}
      >
        <Box sx={{ p: 2 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h6">Properties</Typography>
            <IconButton size="small" onClick={() => setPropertiesOpen(false)}>
              <CloseIcon />
            </IconButton>
          </Box>

          {selectedNode && (
            <NodeEditor
              selectedNode={selectedNode}
              entityType={workflow?.entityType || 'Account'}
              versionStatus={version?.status}
              onUpdateProperty={(property, value) => updateNodeProperty(property, value)}
              onDelete={() => deleteNode(selectedNode)}
            />
          )}

          {selectedTransition && (
            <TransitionEditor
              selectedTransition={selectedTransition}
              onUpdate={handleTransitionUpdate}
              onDelete={() => deleteTransition(selectedTransition)}
            />
          )}
        </Box>
      </Drawer>

      {/* Workflow Simulator Dialog */}
      {workflow && (
        <WorkflowSimulator
          open={simulatorOpen}
          onClose={() => setSimulatorOpen(false)}
          workflowId={workflow.id}
          workflowName={workflow.name}
          entityType={workflow.entityType}
          nodes={nodes}
          transitions={transitions}
        />
      )}

      {/* Version Diff Viewer Dialog */}
      {workflow && workflow.versions.length > 0 && (
        <VersionDiffViewer
          open={versionDiffOpen}
          onClose={() => setVersionDiffOpen(false)}
          versions={workflow.versions.map(v => ({
            id: v.id,
            versionNumber: v.versionNumber,
            label: `v${v.versionNumber}`,
            status: v.status,
            createdAt: v.createdAt,
          }))}
          loadVersion={async (versionId: number) => {
            const versionData = await workflowService.getVersion(versionId);
            return versionData;
          }}
        />
      )}
    </Box>
  );
}

export default WorkflowDesignerPage;

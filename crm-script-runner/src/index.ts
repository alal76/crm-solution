import express from 'express';
import { executeScript } from './engine';
import type { ScriptExecutionRequest } from './types/context';

const PORT = process.env['PORT'] ? parseInt(process.env['PORT'], 10) : 4000;
const app = express();
app.use(express.json({ limit: '10mb' }));

// Health check — used by Docker healthcheck and Kubernetes readiness probe
app.get('/health', (_req, res) => {
  res.json({ status: 'ok', version: process.env['npm_package_version'] ?? '1.0.0' });
});

// Execute TypeScript script in an isolated-vm V8 sandbox
app.post('/execute', async (req, res) => {
  const request = req.body as ScriptExecutionRequest;
  if (!request?.source) {
    res.status(400).json({ success: false, error: 'source is required' });
    return;
  }
  try {
    const result = await executeScript(request);
    res.json(result);
  } catch (err) {
    res.status(500).json({ success: false, error: (err as Error).message });
  }
});

app.listen(PORT, () => {
  console.log(`[crm-script-runner] Listening on port ${PORT}`);
});

export default app;

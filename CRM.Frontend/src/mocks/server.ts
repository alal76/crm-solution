/**
 * MSW server setup for Node.js environment (Jest tests).
 */

import { setupServer } from 'msw/node';
import { handlers } from './handlers';

// Create the server with our handlers
export const server = setupServer(...handlers);

export default server;

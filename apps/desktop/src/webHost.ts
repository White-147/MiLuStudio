import fs from 'node:fs';
import http, { type Server } from 'node:http';
import path from 'node:path';

const mimeTypes: Record<string, string> = {
  '.css': 'text/css; charset=utf-8',
  '.html': 'text/html; charset=utf-8',
  '.ico': 'image/x-icon',
  '.js': 'text/javascript; charset=utf-8',
  '.json': 'application/json; charset=utf-8',
  '.png': 'image/png',
  '.svg': 'image/svg+xml',
  '.txt': 'text/plain; charset=utf-8',
  '.webp': 'image/webp'
};

export interface WebHost {
  port: number;
  url: string;
  close: () => Promise<void>;
}

export async function startWebHost(webRoot: string): Promise<WebHost> {
  const indexPath = path.join(webRoot, 'index.html');
  if (!fs.existsSync(indexPath)) {
    throw new Error(`MiLuStudio Web dist was not found at ${webRoot}. Run the web build first.`);
  }

  const server = http.createServer((request, response) => {
    const requestUrl = new URL(request.url ?? '/', 'http://127.0.0.1');
    const filePath = resolveStaticPath(webRoot, requestUrl.pathname);

    response.setHeader('Content-Security-Policy', [
      "default-src 'self'",
      "script-src 'self'",
      "style-src 'self' 'unsafe-inline'",
      "img-src 'self' data:",
      "font-src 'self' data:",
      "connect-src 'self' http://127.0.0.1:* http://localhost:*",
      "object-src 'none'",
      "base-uri 'self'",
      "form-action 'self'",
      "frame-ancestors 'none'"
    ].join('; '));
    response.setHeader('X-Content-Type-Options', 'nosniff');

    fs.readFile(filePath, (error, content) => {
      if (error) {
        response.writeHead(error.code === 'ENOENT' ? 404 : 500);
        response.end(error.code === 'ENOENT' ? 'Not found' : 'MiLuStudio desktop web host error');
        return;
      }

      response.writeHead(200, {
        'Content-Type': mimeTypes[path.extname(filePath).toLowerCase()] ?? 'application/octet-stream'
      });
      response.end(content);
    });
  });

  const port = await listen(server);
  return {
    port,
    url: `http://127.0.0.1:${port}`,
    close: () => close(server)
  };
}

function resolveStaticPath(webRoot: string, pathname: string): string {
  const decoded = decodeURIComponent(pathname);
  const relativePath = decoded === '/' ? 'index.html' : decoded.replace(/^\/+/, '');
  const candidate = path.resolve(webRoot, relativePath);
  const root = path.resolve(webRoot);

  if (!candidate.startsWith(root)) {
    return path.join(webRoot, 'index.html');
  }

  if (!fs.existsSync(candidate) || fs.statSync(candidate).isDirectory()) {
    return path.join(webRoot, 'index.html');
  }

  return candidate;
}

function listen(server: Server): Promise<number> {
  return new Promise((resolve, reject) => {
    server.once('error', reject);
    server.listen(0, '127.0.0.1', () => {
      const address = server.address();
      if (!address || typeof address === 'string') {
        reject(new Error('MiLuStudio web host did not receive a TCP port.'));
        return;
      }

      resolve(address.port);
    });
  });
}

function close(server: Server): Promise<void> {
  return new Promise((resolve, reject) => {
    server.close(error => {
      if (error) {
        reject(error);
        return;
      }

      resolve();
    });
  });
}

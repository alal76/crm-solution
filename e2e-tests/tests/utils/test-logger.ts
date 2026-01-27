/**
 * CRM Solution - Test Logger
 * 
 * Creates human-readable test logs for all test executions.
 * Logs are stored in the test container and copied to the CRM source root.
 */

import * as fs from 'fs';
import * as path from 'path';

export interface TestLogEntry {
  timestamp: string;
  testId: string;
  testName: string;
  module: string;
  status: 'PASS' | 'FAIL' | 'SKIP' | 'RUNNING' | 'ERROR';
  duration?: number;
  message?: string;
  error?: string;
  screenshot?: string;
}

export interface TestSummary {
  startTime: string;
  endTime?: string;
  totalTests: number;
  passed: number;
  failed: number;
  skipped: number;
  duration?: number;
  modules: Record<string, {
    total: number;
    passed: number;
    failed: number;
    skipped: number;
  }>;
}

class TestLogger {
  private logs: TestLogEntry[] = [];
  private summary: TestSummary;
  private logDir: string;
  private startTime: Date;

  constructor() {
    this.startTime = new Date();
    this.logDir = path.resolve(__dirname, '../../test-logs');
    
    // Ensure log directory exists
    if (!fs.existsSync(this.logDir)) {
      fs.mkdirSync(this.logDir, { recursive: true });
    }

    this.summary = {
      startTime: this.startTime.toISOString(),
      totalTests: 0,
      passed: 0,
      failed: 0,
      skipped: 0,
      modules: {}
    };
  }

  /**
   * Log a test start
   */
  testStart(testId: string, testName: string, module: string): void {
    const entry: TestLogEntry = {
      timestamp: new Date().toISOString(),
      testId,
      testName,
      module,
      status: 'RUNNING',
      message: `Starting test: ${testName}`
    };
    this.logs.push(entry);
    console.log(`[TEST START] [${testId}] ${testName}`);
  }

  /**
   * Log a test pass
   */
  testPass(testId: string, testName: string, module: string, duration: number, message?: string): void {
    const entry: TestLogEntry = {
      timestamp: new Date().toISOString(),
      testId,
      testName,
      module,
      status: 'PASS',
      duration,
      message: message || `Test passed successfully`
    };
    this.logs.push(entry);
    this.summary.passed++;
    this.summary.totalTests++;
    this.updateModuleSummary(module, 'passed');
    console.log(`[✓ PASS] [${testId}] ${testName} (${duration}ms)`);
  }

  /**
   * Log a test failure
   */
  testFail(testId: string, testName: string, module: string, duration: number, error: string, screenshot?: string): void {
    const entry: TestLogEntry = {
      timestamp: new Date().toISOString(),
      testId,
      testName,
      module,
      status: 'FAIL',
      duration,
      error,
      screenshot
    };
    this.logs.push(entry);
    this.summary.failed++;
    this.summary.totalTests++;
    this.updateModuleSummary(module, 'failed');
    console.log(`[✗ FAIL] [${testId}] ${testName} (${duration}ms)`);
    console.log(`         Error: ${error}`);
  }

  /**
   * Log a test skip
   */
  testSkip(testId: string, testName: string, module: string, reason?: string): void {
    const entry: TestLogEntry = {
      timestamp: new Date().toISOString(),
      testId,
      testName,
      module,
      status: 'SKIP',
      message: reason || 'Test skipped'
    };
    this.logs.push(entry);
    this.summary.skipped++;
    this.summary.totalTests++;
    this.updateModuleSummary(module, 'skipped');
    console.log(`[- SKIP] [${testId}] ${testName}`);
  }

  /**
   * Log an info message
   */
  info(message: string): void {
    console.log(`[INFO] ${message}`);
  }

  /**
   * Log a warning
   */
  warn(message: string): void {
    console.log(`[WARN] ${message}`);
  }

  /**
   * Log an error
   */
  error(message: string): void {
    console.log(`[ERROR] ${message}`);
  }

  /**
   * Update module summary
   */
  private updateModuleSummary(module: string, status: 'passed' | 'failed' | 'skipped'): void {
    if (!this.summary.modules[module]) {
      this.summary.modules[module] = { total: 0, passed: 0, failed: 0, skipped: 0 };
    }
    this.summary.modules[module].total++;
    this.summary.modules[module][status]++;
  }

  /**
   * Finalize and save all logs
   */
  finalize(): void {
    const endTime = new Date();
    this.summary.endTime = endTime.toISOString();
    this.summary.duration = endTime.getTime() - this.startTime.getTime();

    // Generate log files
    this.generateTextLog();
    this.generateJsonLog();
    this.generateHtmlReport();
    this.generateMarkdownReport();

    console.log('\n===========================================');
    console.log('           TEST EXECUTION SUMMARY          ');
    console.log('===========================================');
    console.log(`Total Tests: ${this.summary.totalTests}`);
    console.log(`Passed:      ${this.summary.passed} (${this.getPercentage(this.summary.passed)}%)`);
    console.log(`Failed:      ${this.summary.failed} (${this.getPercentage(this.summary.failed)}%)`);
    console.log(`Skipped:     ${this.summary.skipped} (${this.getPercentage(this.summary.skipped)}%)`);
    console.log(`Duration:    ${this.formatDuration(this.summary.duration)}`);
    console.log('===========================================\n');
  }

  /**
   * Generate human-readable text log
   */
  private generateTextLog(): void {
    const filename = `test-log-${this.formatDate(this.startTime)}.txt`;
    const filepath = path.join(this.logDir, filename);
    
    let content = '═══════════════════════════════════════════════════════════════════════════════\n';
    content += '                    CRM SOLUTION - E2E TEST EXECUTION LOG                       \n';
    content += '═══════════════════════════════════════════════════════════════════════════════\n\n';
    
    content += `Test Run Started:  ${this.summary.startTime}\n`;
    content += `Test Run Ended:    ${this.summary.endTime || 'In Progress'}\n`;
    content += `Total Duration:    ${this.formatDuration(this.summary.duration)}\n\n`;
    
    content += '───────────────────────────────────────────────────────────────────────────────\n';
    content += '                              EXECUTION SUMMARY                                 \n';
    content += '───────────────────────────────────────────────────────────────────────────────\n';
    content += `  Total Tests:     ${this.summary.totalTests}\n`;
    content += `  Passed:          ${this.summary.passed} (${this.getPercentage(this.summary.passed)}%)\n`;
    content += `  Failed:          ${this.summary.failed} (${this.getPercentage(this.summary.failed)}%)\n`;
    content += `  Skipped:         ${this.summary.skipped} (${this.getPercentage(this.summary.skipped)}%)\n\n`;
    
    // Module breakdown
    content += '───────────────────────────────────────────────────────────────────────────────\n';
    content += '                              MODULE BREAKDOWN                                  \n';
    content += '───────────────────────────────────────────────────────────────────────────────\n';
    
    Object.entries(this.summary.modules).forEach(([module, stats]) => {
      const passRate = stats.total > 0 ? Math.round((stats.passed / stats.total) * 100) : 0;
      content += `\n  ${module.toUpperCase()}\n`;
      content += `  ├─ Total:   ${stats.total}\n`;
      content += `  ├─ Passed:  ${stats.passed}\n`;
      content += `  ├─ Failed:  ${stats.failed}\n`;
      content += `  ├─ Skipped: ${stats.skipped}\n`;
      content += `  └─ Pass Rate: ${passRate}%\n`;
    });
    
    // Detailed test log
    content += '\n───────────────────────────────────────────────────────────────────────────────\n';
    content += '                              DETAILED TEST LOG                                 \n';
    content += '───────────────────────────────────────────────────────────────────────────────\n\n';
    
    this.logs.forEach(log => {
      const statusIcon = log.status === 'PASS' ? '✓' : log.status === 'FAIL' ? '✗' : log.status === 'SKIP' ? '○' : '►';
      content += `[${log.timestamp}] [${statusIcon} ${log.status}] ${log.testId}\n`;
      content += `   Test: ${log.testName}\n`;
      content += `   Module: ${log.module}\n`;
      if (log.duration) content += `   Duration: ${log.duration}ms\n`;
      if (log.message) content += `   Message: ${log.message}\n`;
      if (log.error) content += `   Error: ${log.error}\n`;
      if (log.screenshot) content += `   Screenshot: ${log.screenshot}\n`;
      content += '\n';
    });
    
    // Failed tests summary
    const failedTests = this.logs.filter(l => l.status === 'FAIL');
    if (failedTests.length > 0) {
      content += '───────────────────────────────────────────────────────────────────────────────\n';
      content += '                              FAILED TESTS                                     \n';
      content += '───────────────────────────────────────────────────────────────────────────────\n\n';
      
      failedTests.forEach((test, idx) => {
        content += `${idx + 1}. ${test.testId} - ${test.testName}\n`;
        content += `   Error: ${test.error}\n\n`;
      });
    }
    
    content += '\n═══════════════════════════════════════════════════════════════════════════════\n';
    content += '                              END OF LOG                                        \n';
    content += '═══════════════════════════════════════════════════════════════════════════════\n';
    
    fs.writeFileSync(filepath, content);
    console.log(`[LOG] Text log saved: ${filepath}`);
  }

  /**
   * Generate JSON log
   */
  private generateJsonLog(): void {
    const filename = `test-log-${this.formatDate(this.startTime)}.json`;
    const filepath = path.join(this.logDir, filename);
    
    const data = {
      summary: this.summary,
      logs: this.logs
    };
    
    fs.writeFileSync(filepath, JSON.stringify(data, null, 2));
    console.log(`[LOG] JSON log saved: ${filepath}`);
  }

  /**
   * Generate HTML report
   */
  private generateHtmlReport(): void {
    const filename = `test-report-${this.formatDate(this.startTime)}.html`;
    const filepath = path.join(this.logDir, filename);
    
    const passRate = this.summary.totalTests > 0 
      ? Math.round((this.summary.passed / this.summary.totalTests) * 100) 
      : 0;
    
    const statusColor = passRate >= 80 ? '#4caf50' : passRate >= 50 ? '#ff9800' : '#f44336';
    
    let html = `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>CRM E2E Test Report</title>
  <style>
    * { box-sizing: border-box; margin: 0; padding: 0; }
    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; background: #f5f5f5; color: #333; }
    .container { max-width: 1200px; margin: 0 auto; padding: 20px; }
    .header { background: linear-gradient(135deg, #1976d2, #1565c0); color: white; padding: 40px 20px; text-align: center; border-radius: 8px 8px 0 0; }
    .header h1 { font-size: 28px; margin-bottom: 10px; }
    .header .date { opacity: 0.9; font-size: 14px; }
    
    .summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; padding: 20px; background: white; }
    .summary-card { padding: 20px; border-radius: 8px; text-align: center; }
    .summary-card.total { background: #e3f2fd; border-left: 4px solid #1976d2; }
    .summary-card.passed { background: #e8f5e9; border-left: 4px solid #4caf50; }
    .summary-card.failed { background: #ffebee; border-left: 4px solid #f44336; }
    .summary-card.skipped { background: #fff3e0; border-left: 4px solid #ff9800; }
    .summary-card .number { font-size: 36px; font-weight: bold; }
    .summary-card .label { font-size: 14px; color: #666; margin-top: 5px; }
    
    .progress-bar { height: 30px; background: #e0e0e0; border-radius: 15px; overflow: hidden; margin: 20px; display: flex; }
    .progress-bar .segment { height: 100%; display: flex; align-items: center; justify-content: center; color: white; font-size: 12px; font-weight: bold; }
    .progress-bar .passed { background: #4caf50; }
    .progress-bar .failed { background: #f44336; }
    .progress-bar .skipped { background: #ff9800; }
    
    .section { background: white; margin-top: 20px; border-radius: 8px; overflow: hidden; }
    .section-header { padding: 15px 20px; background: #f5f5f5; border-bottom: 1px solid #e0e0e0; font-weight: bold; }
    
    .module-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 15px; padding: 20px; }
    .module-card { background: #fafafa; border-radius: 8px; padding: 15px; border: 1px solid #e0e0e0; }
    .module-card h3 { font-size: 16px; margin-bottom: 10px; color: #1976d2; }
    .module-stats { display: flex; gap: 15px; font-size: 13px; }
    .module-stats span { padding: 3px 8px; border-radius: 4px; }
    .module-stats .passed { background: #e8f5e9; color: #2e7d32; }
    .module-stats .failed { background: #ffebee; color: #c62828; }
    .module-stats .skipped { background: #fff3e0; color: #e65100; }
    
    .test-list { max-height: 600px; overflow-y: auto; }
    .test-item { padding: 12px 20px; border-bottom: 1px solid #f0f0f0; display: flex; align-items: center; gap: 15px; }
    .test-item:hover { background: #f9f9f9; }
    .test-item .icon { width: 24px; height: 24px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 12px; color: white; }
    .test-item .icon.pass { background: #4caf50; }
    .test-item .icon.fail { background: #f44336; }
    .test-item .icon.skip { background: #ff9800; }
    .test-item .details { flex: 1; }
    .test-item .test-id { font-size: 12px; color: #666; }
    .test-item .test-name { font-weight: 500; }
    .test-item .test-module { font-size: 12px; color: #1976d2; }
    .test-item .duration { font-size: 12px; color: #666; }
    .test-item .error { background: #ffebee; color: #c62828; font-size: 12px; padding: 8px; margin-top: 8px; border-radius: 4px; }
    
    .footer { text-align: center; padding: 20px; color: #666; font-size: 12px; }
  </style>
</head>
<body>
  <div class="container">
    <div class="header">
      <h1>🧪 CRM E2E Test Report</h1>
      <div class="date">Generated: ${new Date().toLocaleString()}</div>
      <div class="date">Duration: ${this.formatDuration(this.summary.duration)}</div>
    </div>
    
    <div class="summary">
      <div class="summary-card total">
        <div class="number">${this.summary.totalTests}</div>
        <div class="label">Total Tests</div>
      </div>
      <div class="summary-card passed">
        <div class="number">${this.summary.passed}</div>
        <div class="label">Passed</div>
      </div>
      <div class="summary-card failed">
        <div class="number">${this.summary.failed}</div>
        <div class="label">Failed</div>
      </div>
      <div class="summary-card skipped">
        <div class="number">${this.summary.skipped}</div>
        <div class="label">Skipped</div>
      </div>
    </div>
    
    <div class="progress-bar">
      <div class="segment passed" style="width: ${this.getPercentage(this.summary.passed)}%">
        ${this.getPercentage(this.summary.passed) > 10 ? this.summary.passed : ''}
      </div>
      <div class="segment failed" style="width: ${this.getPercentage(this.summary.failed)}%">
        ${this.getPercentage(this.summary.failed) > 10 ? this.summary.failed : ''}
      </div>
      <div class="segment skipped" style="width: ${this.getPercentage(this.summary.skipped)}%">
        ${this.getPercentage(this.summary.skipped) > 10 ? this.summary.skipped : ''}
      </div>
    </div>
    
    <div class="section">
      <div class="section-header">📁 Module Breakdown</div>
      <div class="module-grid">
        ${Object.entries(this.summary.modules).map(([module, stats]) => `
          <div class="module-card">
            <h3>${module}</h3>
            <div class="module-stats">
              <span class="passed">✓ ${stats.passed}</span>
              <span class="failed">✗ ${stats.failed}</span>
              <span class="skipped">○ ${stats.skipped}</span>
            </div>
          </div>
        `).join('')}
      </div>
    </div>
    
    <div class="section">
      <div class="section-header">📋 Test Results</div>
      <div class="test-list">
        ${this.logs.map(log => `
          <div class="test-item">
            <div class="icon ${log.status.toLowerCase()}">${log.status === 'PASS' ? '✓' : log.status === 'FAIL' ? '✗' : '○'}</div>
            <div class="details">
              <div class="test-id">${log.testId}</div>
              <div class="test-name">${log.testName}</div>
              <div class="test-module">${log.module}</div>
              ${log.error ? `<div class="error">${log.error}</div>` : ''}
            </div>
            ${log.duration ? `<div class="duration">${log.duration}ms</div>` : ''}
          </div>
        `).join('')}
      </div>
    </div>
    
    <div class="footer">
      CRM Solution E2E Test Suite • Powered by Playwright
    </div>
  </div>
</body>
</html>`;
    
    fs.writeFileSync(filepath, html);
    console.log(`[LOG] HTML report saved: ${filepath}`);
  }

  /**
   * Generate Markdown report
   */
  private generateMarkdownReport(): void {
    const filename = `test-report-${this.formatDate(this.startTime)}.md`;
    const filepath = path.join(this.logDir, filename);
    
    let md = `# 🧪 CRM E2E Test Report\n\n`;
    md += `**Generated:** ${new Date().toLocaleString()}\n`;
    md += `**Duration:** ${this.formatDuration(this.summary.duration)}\n\n`;
    
    md += `## 📊 Summary\n\n`;
    md += `| Metric | Count | Percentage |\n`;
    md += `|--------|-------|------------|\n`;
    md += `| ✅ Passed | ${this.summary.passed} | ${this.getPercentage(this.summary.passed)}% |\n`;
    md += `| ❌ Failed | ${this.summary.failed} | ${this.getPercentage(this.summary.failed)}% |\n`;
    md += `| ⏭️ Skipped | ${this.summary.skipped} | ${this.getPercentage(this.summary.skipped)}% |\n`;
    md += `| **Total** | **${this.summary.totalTests}** | 100% |\n\n`;
    
    md += `## 📁 Module Breakdown\n\n`;
    Object.entries(this.summary.modules).forEach(([module, stats]) => {
      const passRate = stats.total > 0 ? Math.round((stats.passed / stats.total) * 100) : 0;
      md += `### ${module}\n`;
      md += `- Total: ${stats.total}\n`;
      md += `- Passed: ${stats.passed}\n`;
      md += `- Failed: ${stats.failed}\n`;
      md += `- Skipped: ${stats.skipped}\n`;
      md += `- Pass Rate: ${passRate}%\n\n`;
    });
    
    const failedTests = this.logs.filter(l => l.status === 'FAIL');
    if (failedTests.length > 0) {
      md += `## ❌ Failed Tests\n\n`;
      failedTests.forEach(test => {
        md += `### ${test.testId}\n`;
        md += `- **Test:** ${test.testName}\n`;
        md += `- **Module:** ${test.module}\n`;
        md += `- **Error:** ${test.error}\n\n`;
      });
    }
    
    md += `## 📋 All Tests\n\n`;
    md += `| Status | Test ID | Test Name | Module | Duration |\n`;
    md += `|--------|---------|-----------|--------|----------|\n`;
    this.logs.forEach(log => {
      const icon = log.status === 'PASS' ? '✅' : log.status === 'FAIL' ? '❌' : '⏭️';
      md += `| ${icon} | ${log.testId} | ${log.testName} | ${log.module} | ${log.duration || '-'}ms |\n`;
    });
    
    md += `\n---\n*CRM Solution E2E Test Suite • Powered by Playwright*\n`;
    
    fs.writeFileSync(filepath, md);
    console.log(`[LOG] Markdown report saved: ${filepath}`);
  }

  /**
   * Copy logs to destination directory
   */
  copyLogsTo(destination: string): void {
    if (!fs.existsSync(destination)) {
      fs.mkdirSync(destination, { recursive: true });
    }
    
    const files = fs.readdirSync(this.logDir);
    files.forEach(file => {
      const src = path.join(this.logDir, file);
      const dest = path.join(destination, file);
      fs.copyFileSync(src, dest);
      console.log(`[LOG] Copied ${file} to ${destination}`);
    });
  }

  /**
   * Helper: Format date for filename
   */
  private formatDate(date: Date): string {
    return date.toISOString().replace(/[:.]/g, '-').slice(0, 19);
  }

  /**
   * Helper: Format duration
   */
  private formatDuration(ms?: number): string {
    if (!ms) return 'N/A';
    const seconds = Math.floor(ms / 1000);
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);
    
    if (hours > 0) {
      return `${hours}h ${minutes % 60}m ${seconds % 60}s`;
    }
    if (minutes > 0) {
      return `${minutes}m ${seconds % 60}s`;
    }
    return `${seconds}s`;
  }

  /**
   * Helper: Get percentage
   */
  private getPercentage(count: number): number {
    if (this.summary.totalTests === 0) return 0;
    return Math.round((count / this.summary.totalTests) * 100);
  }
}

// Export singleton instance
export const testLogger = new TestLogger();

// Export class for custom instances
export { TestLogger };

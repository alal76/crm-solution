/**
 * CRM Solution - Custom Playwright Reporter
 * 
 * Generates human-readable logs for all test executions.
 */

import type {
  FullConfig,
  FullResult,
  Reporter,
  Suite,
  TestCase,
  TestResult,
} from '@playwright/test/reporter';
import * as fs from 'fs';
import * as path from 'path';

interface TestLogEntry {
  timestamp: string;
  testId: string;
  testName: string;
  module: string;
  status: 'PASS' | 'FAIL' | 'SKIP' | 'ERROR';
  duration: number;
  error?: string;
  screenshot?: string;
}

interface TestSummary {
  startTime: string;
  endTime?: string;
  totalTests: number;
  passed: number;
  failed: number;
  skipped: number;
  duration: number;
  modules: Record<string, {
    total: number;
    passed: number;
    failed: number;
    skipped: number;
  }>;
}

class CRMTestReporter implements Reporter {
  private logs: TestLogEntry[] = [];
  private summary: TestSummary;
  private logDir: string;
  private startTime: Date;
  private outputDir: string;

  constructor(options: { outputDir?: string } = {}) {
    this.startTime = new Date();
    this.outputDir = options.outputDir || 'test-logs';
    this.logDir = path.resolve(process.cwd(), this.outputDir);
    
    this.summary = {
      startTime: this.startTime.toISOString(),
      totalTests: 0,
      passed: 0,
      failed: 0,
      skipped: 0,
      duration: 0,
      modules: {}
    };
  }

  onBegin(config: FullConfig, suite: Suite) {
    console.log('\n');
    console.log('═══════════════════════════════════════════════════════════════════════════════');
    console.log('                    CRM SOLUTION - E2E TEST EXECUTION                          ');
    console.log('═══════════════════════════════════════════════════════════════════════════════');
    console.log(`Started: ${this.summary.startTime}`);
    console.log(`Workers: ${config.workers}`);
    console.log('───────────────────────────────────────────────────────────────────────────────\n');

    // Ensure log directory exists
    if (!fs.existsSync(this.logDir)) {
      fs.mkdirSync(this.logDir, { recursive: true });
    }
  }

  onTestBegin(test: TestCase, result: TestResult) {
    const testId = this.extractTestId(test.title);
    const module = this.extractModule(test);
    console.log(`[▶ START] ${testId} - ${test.title}`);
  }

  onTestEnd(test: TestCase, result: TestResult) {
    const testId = this.extractTestId(test.title);
    const module = this.extractModule(test);
    const duration = result.duration;

    let status: 'PASS' | 'FAIL' | 'SKIP' | 'ERROR';
    let statusIcon: string;

    switch (result.status) {
      case 'passed':
        status = 'PASS';
        statusIcon = '✓';
        this.summary.passed++;
        break;
      case 'failed':
        status = 'FAIL';
        statusIcon = '✗';
        this.summary.failed++;
        break;
      case 'skipped':
        status = 'SKIP';
        statusIcon = '○';
        this.summary.skipped++;
        break;
      default:
        status = 'ERROR';
        statusIcon = '!';
        this.summary.failed++;
    }

    this.summary.totalTests++;
    this.updateModuleSummary(module, status);

    const entry: TestLogEntry = {
      timestamp: new Date().toISOString(),
      testId,
      testName: test.title,
      module,
      status,
      duration,
      error: result.error?.message,
    };

    this.logs.push(entry);

    console.log(`[${statusIcon} ${status}] ${testId} - ${test.title} (${duration}ms)`);
    if (result.error) {
      console.log(`        └─ Error: ${result.error.message?.split('\n')[0]}`);
    }
  }

  async onEnd(result: FullResult) {
    const endTime = new Date();
    this.summary.endTime = endTime.toISOString();
    this.summary.duration = endTime.getTime() - this.startTime.getTime();

    console.log('\n───────────────────────────────────────────────────────────────────────────────');
    console.log('                              EXECUTION SUMMARY                                 ');
    console.log('───────────────────────────────────────────────────────────────────────────────');
    console.log(`  Total Tests:     ${this.summary.totalTests}`);
    console.log(`  Passed:          ${this.summary.passed} (${this.getPercentage(this.summary.passed)}%)`);
    console.log(`  Failed:          ${this.summary.failed} (${this.getPercentage(this.summary.failed)}%)`);
    console.log(`  Skipped:         ${this.summary.skipped} (${this.getPercentage(this.summary.skipped)}%)`);
    console.log(`  Duration:        ${this.formatDuration(this.summary.duration)}`);
    console.log('───────────────────────────────────────────────────────────────────────────────');
    
    console.log('\n  Module Breakdown:');
    Object.entries(this.summary.modules).forEach(([module, stats]) => {
      const passRate = stats.total > 0 ? Math.round((stats.passed / stats.total) * 100) : 0;
      console.log(`    ${module}: ${stats.passed}/${stats.total} passed (${passRate}%)`);
    });

    if (this.summary.failed > 0) {
      console.log('\n  Failed Tests:');
      this.logs.filter(l => l.status === 'FAIL').forEach(test => {
        console.log(`    ✗ ${test.testId}: ${test.testName}`);
        if (test.error) {
          console.log(`      └─ ${test.error.split('\n')[0]}`);
        }
      });
    }

    console.log('\n═══════════════════════════════════════════════════════════════════════════════');
    console.log(`  Overall Result: ${result.status.toUpperCase()}`);
    console.log('═══════════════════════════════════════════════════════════════════════════════\n');

    // Generate reports
    await this.generateReports();
  }

  private extractTestId(title: string): string {
    const match = title.match(/TC-[A-Z]+-\d+/);
    return match ? match[0] : 'TC-UNKNOWN';
  }

  private extractModule(test: TestCase): string {
    const file = test.location.file;
    const parts = file.split('/');
    const testsIndex = parts.indexOf('tests');
    if (testsIndex !== -1 && parts[testsIndex + 1]) {
      return parts[testsIndex + 1].replace('.spec.ts', '');
    }
    return 'unknown';
  }

  private updateModuleSummary(module: string, status: 'PASS' | 'FAIL' | 'SKIP' | 'ERROR') {
    if (!this.summary.modules[module]) {
      this.summary.modules[module] = { total: 0, passed: 0, failed: 0, skipped: 0 };
    }
    this.summary.modules[module].total++;
    if (status === 'PASS') {
      this.summary.modules[module].passed++;
    } else if (status === 'SKIP') {
      this.summary.modules[module].skipped++;
    } else {
      this.summary.modules[module].failed++;
    }
  }

  private getPercentage(count: number): number {
    if (this.summary.totalTests === 0) return 0;
    return Math.round((count / this.summary.totalTests) * 100);
  }

  private formatDuration(ms: number): string {
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

  private formatDate(date: Date): string {
    return date.toISOString().replace(/[:.]/g, '-').slice(0, 19);
  }

  private async generateReports() {
    const dateStr = this.formatDate(this.startTime);
    
    // Generate text log
    await this.generateTextLog(dateStr);
    
    // Generate JSON log
    await this.generateJsonLog(dateStr);
    
    // Generate HTML report
    await this.generateHtmlReport(dateStr);
    
    // Generate Markdown report
    await this.generateMarkdownReport(dateStr);
    
    console.log(`\n📁 Reports saved to: ${this.logDir}`);
  }

  private async generateTextLog(dateStr: string) {
    const filename = `test-log-${dateStr}.txt`;
    const filepath = path.join(this.logDir, filename);
    
    let content = '═══════════════════════════════════════════════════════════════════════════════\n';
    content += '                    CRM SOLUTION - E2E TEST EXECUTION LOG                       \n';
    content += '═══════════════════════════════════════════════════════════════════════════════\n\n';
    
    content += `Test Run Started:  ${this.summary.startTime}\n`;
    content += `Test Run Ended:    ${this.summary.endTime}\n`;
    content += `Total Duration:    ${this.formatDuration(this.summary.duration)}\n\n`;
    
    content += '───────────────────────────────────────────────────────────────────────────────\n';
    content += '                              EXECUTION SUMMARY                                 \n';
    content += '───────────────────────────────────────────────────────────────────────────────\n';
    content += `  Total Tests:     ${this.summary.totalTests}\n`;
    content += `  Passed:          ${this.summary.passed} (${this.getPercentage(this.summary.passed)}%)\n`;
    content += `  Failed:          ${this.summary.failed} (${this.getPercentage(this.summary.failed)}%)\n`;
    content += `  Skipped:         ${this.summary.skipped} (${this.getPercentage(this.summary.skipped)}%)\n\n`;
    
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
    
    content += '\n───────────────────────────────────────────────────────────────────────────────\n';
    content += '                              DETAILED TEST LOG                                 \n';
    content += '───────────────────────────────────────────────────────────────────────────────\n\n';
    
    this.logs.forEach(log => {
      const statusIcon = log.status === 'PASS' ? '✓' : log.status === 'FAIL' ? '✗' : '○';
      content += `[${log.timestamp}] [${statusIcon} ${log.status}] ${log.testId}\n`;
      content += `   Test: ${log.testName}\n`;
      content += `   Module: ${log.module}\n`;
      content += `   Duration: ${log.duration}ms\n`;
      if (log.error) content += `   Error: ${log.error}\n`;
      content += '\n';
    });
    
    const failedTests = this.logs.filter(l => l.status === 'FAIL');
    if (failedTests.length > 0) {
      content += '───────────────────────────────────────────────────────────────────────────────\n';
      content += '                              FAILED TESTS                                     \n';
      content += '───────────────────────────────────────────────────────────────────────────────\n\n';
      
      failedTests.forEach((test, idx) => {
        content += `${idx + 1}. ${test.testId} - ${test.testName}\n`;
        content += `   Error: ${test.error || 'Unknown error'}\n\n`;
      });
    }
    
    content += '\n═══════════════════════════════════════════════════════════════════════════════\n';
    content += '                              END OF LOG                                        \n';
    content += '═══════════════════════════════════════════════════════════════════════════════\n';
    
    fs.writeFileSync(filepath, content);
  }

  private async generateJsonLog(dateStr: string) {
    const filename = `test-log-${dateStr}.json`;
    const filepath = path.join(this.logDir, filename);
    
    const data = {
      summary: this.summary,
      logs: this.logs
    };
    
    fs.writeFileSync(filepath, JSON.stringify(data, null, 2));
  }

  private async generateHtmlReport(dateStr: string) {
    const filename = `test-report-${dateStr}.html`;
    const filepath = path.join(this.logDir, filename);
    
    const passRate = this.summary.totalTests > 0 
      ? Math.round((this.summary.passed / this.summary.totalTests) * 100) 
      : 0;
    
    let html = `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>CRM E2E Test Report - ${dateStr}</title>
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
    .section { background: white; margin-top: 20px; border-radius: 8px; overflow: hidden; }
    .section-header { padding: 15px 20px; background: #f5f5f5; border-bottom: 1px solid #e0e0e0; font-weight: bold; }
    .test-list { max-height: 600px; overflow-y: auto; }
    .test-item { padding: 12px 20px; border-bottom: 1px solid #f0f0f0; display: flex; align-items: flex-start; gap: 15px; }
    .test-item:hover { background: #f9f9f9; }
    .test-item .icon { width: 24px; height: 24px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 12px; color: white; flex-shrink: 0; }
    .test-item .icon.pass { background: #4caf50; }
    .test-item .icon.fail { background: #f44336; }
    .test-item .icon.skip { background: #ff9800; }
    .test-item .details { flex: 1; }
    .test-item .test-id { font-size: 12px; color: #666; }
    .test-item .test-name { font-weight: 500; }
    .test-item .test-module { font-size: 12px; color: #1976d2; }
    .test-item .duration { font-size: 12px; color: #666; flex-shrink: 0; }
    .test-item .error { background: #ffebee; color: #c62828; font-size: 12px; padding: 8px; margin-top: 8px; border-radius: 4px; word-break: break-word; }
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
              ${log.error ? `<div class="error">${log.error.replace(/</g, '&lt;').replace(/>/g, '&gt;')}</div>` : ''}
            </div>
            <div class="duration">${log.duration}ms</div>
          </div>
        `).join('')}
      </div>
    </div>
    <div class="footer">CRM Solution E2E Test Suite • Powered by Playwright</div>
  </div>
</body>
</html>`;
    
    fs.writeFileSync(filepath, html);
  }

  private async generateMarkdownReport(dateStr: string) {
    const filename = `test-report-${dateStr}.md`;
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
      md += `- Total: ${stats.total}, Passed: ${stats.passed}, Failed: ${stats.failed}, Skipped: ${stats.skipped}\n`;
      md += `- Pass Rate: ${passRate}%\n\n`;
    });
    
    const failedTests = this.logs.filter(l => l.status === 'FAIL');
    if (failedTests.length > 0) {
      md += `## ❌ Failed Tests\n\n`;
      failedTests.forEach(test => {
        md += `- **${test.testId}**: ${test.testName}\n`;
        md += `  - Error: ${test.error || 'Unknown'}\n`;
      });
      md += `\n`;
    }
    
    md += `---\n*CRM Solution E2E Test Suite • Powered by Playwright*\n`;
    
    fs.writeFileSync(filepath, md);
  }
}

export default CRMTestReporter;

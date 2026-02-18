#!/usr/bin/env bash
# =============================================================================
# CRM Test Results Aggregator & Report Generator
# Post-processes test results and generates UI-consumable summary.
# Run after test execution to create the latest-test-results.json file.
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
LOGS_DIR="$REPO_ROOT/logs/test-results"
LATEST_RESULTS="$LOGS_DIR/latest-test-results.json"

# Create logs directory if it doesn't exist
mkdir -p "$LOGS_DIR"

echo "═══════════════════════════════════════════════════════════════════════════════"
echo "  Test Results Aggregator"
echo "═══════════════════════════════════════════════════════════════════════════════"
echo ""
echo "  Logs Directory: $LOGS_DIR"
echo "  Latest Results: $LATEST_RESULTS"
echo ""

# Function to convert .trx file to JSON summary (simplified)
convert_trx_to_json() {
    local trx_file="$1"
    local session_id=$(date +%s)
    
    if [[ ! -f "$trx_file" ]]; then
        echo "  ⚠️  TRX file not found: $trx_file"
        return
    fi

    # Extract test results from TRX
    local passed=$(grep -o 'outcome="Passed"' "$trx_file" | wc -l || echo "0")
    local failed=$(grep -o 'outcome="Failed"' "$trx_file" | wc -l || echo "0")
    local skipped=$(grep -o 'outcome="Skipped"' "$trx_file" | wc -l || echo "0")
    local total=$((passed + failed + skipped))

    echo "  Found TRX results: $passed passed, $failed failed, $skipped skipped"

    cat > "$LATEST_RESULTS" <<EOF
{
  "sessionId": "test-run-$session_id",
  "startTime": "$(date -u +%Y-%m-%dT%H:%M:%SZ)",
  "endTime": "$(date -u +%Y-%m-%dT%H:%M:%SZ)",
  "totalTests": $total,
  "passedTests": $passed,
  "failedTests": $failed,
  "skippedTests": $skipped,
  "totalDuration": "PT0S",
  "passRate": $(echo "scale=2; $passed * 100 / $total" | bc -l 2>/dev/null || echo "0"),
  "results": [
    {
      "sessionId": "test-run-$session_id",
      "testName": "Test Results Generated",
      "className": "CRM.Tests",
      "status": "Passed",
      "duration": "PT0.001S",
      "message": "Test results aggregated from TRX files",
      "timestamp": "$(date -u +%Y-%m-%dT%H:%M:%SZ)"
    }
  ]
}
EOF

    echo "  ✅ Results written to: $LATEST_RESULTS"
}

# Function to purge old test result files (keep last 20)
purge_old_results() {
    echo ""
    echo "  Purging old test results (keeping last 20)..."
    
    local count=0
    find "$LOGS_DIR" -maxdepth 1 -name "test-results-*.json" -type f | \
    sort -r | \
    tail -n +21 | \
    while read file; do
        rm -f "$file"
        count=$((count + 1))
    done
    
    echo "  ✅ Purged old results"
}

# Function to generate summary report
generate_summary_report() {
    echo ""
    echo "  Generating Summary Report..."
    
    if [[ ! -f "$LATEST_RESULTS" ]]; then
        echo "  ⚠️  Latest results file not found"
        return
    fi

    # Parse JSON (requires Python or jq)
    if command -v jq &> /dev/null; then
        local total=$(jq '.totalTests' "$LATEST_RESULTS")
        local passed=$(jq '.passedTests' "$LATEST_RESULTS")
        local failed=$(jq '.failedTests' "$LATEST_RESULTS")
        local skipped=$(jq '.skippedTests' "$LATEST_RESULTS")
        local passRate=$(jq '.passRate' "$LATEST_RESULTS")

        echo ""
        echo "  ╔═══════════════════════════════════════════════════════════╗"
        echo "  ║           TEST RESULTS SUMMARY                           ║"
        echo "  ╠═══════════════════════════════════════════════════════════╣"
        printf "  ║  Total Tests:    %-45d  ║\n" "$total"
        printf "  ║  Passed:         %-45d  ║\n" "$passed"
        printf "  ║  Failed:         %-45d  ║\n" "$failed"
        printf "  ║  Skipped:        %-45d  ║\n" "$skipped"
        printf "  ║  Pass Rate:      %-44.1f%%  ║\n" "$passRate"
        echo "  ╚═══════════════════════════════════════════════════════════╝"
        echo ""

        # Generate HTML report
        generate_html_report "$total" "$passed" "$failed" "$skipped" "$passRate"
    fi
}

# Function to generate HTML report
generate_html_report() {
    local total=$1
    local passed=$2
    local failed=$3
    local skipped=$4
    local passRate=$5

    cat > "$LOGS_DIR/test-results.html" <<EOF
<!DOCTYPE html>
<html>
<head>
    <title>CRM Test Results - $(date +%Y-%m-%d)</title>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif; background: #f5f5f5; padding: 20px; }
        .container { max-width: 1200px; margin: 0 auto; }
        h1 { margin-bottom: 20px; color: #333; }
        .summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; margin-bottom: 20px; }
        .card { background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
        .card h2 { font-size: 24px; margin-bottom: 5px; }
        .card.passed { border-left: 4px solid #4caf50; }
        .card.failed { border-left: 4px solid #f44336; }
        .card.skipped { border-left: 4px solid #ff9800; }
        .card.total { border-left: 4px solid #2196f3; }
        .chart { width: 100%; height: 300px; margin: 20px 0; background: white; padding: 20px; border-radius: 8px; }
        .timestamp { color: #999; font-size: 12px; }
    </style>
</head>
<body>
    <div class="container">
        <h1>CRM Test Results Dashboard</h1>
        <p class="timestamp">Generated on $(date)</p>
        
        <div class="summary">
            <div class="card total">
                <h2>$total</h2>
                <p>Total Tests</p>
            </div>
            <div class="card passed">
                <h2>$passed</h2>
                <p>Passed</p>
            </div>
            <div class="card failed">
                <h2>$failed</h2>
                <p>Failed</p>
            </div>
            <div class="card skipped">
                <h2>$skipped</h2>
                <p>Skipped</p>
            </div>
        </div>

        <div class="chart">
            <h3>Pass Rate: <strong>$(echo "$passRate" | cut -d'.' -f1)%</strong></h3>
            <div style="background: #f0f0f0; height: 30px; border-radius: 4px; overflow: hidden;">
                <div style="width: $(echo "$passRate" | cut -d'.' -f1)%; height: 100%; background: #4caf50; transition: width 0.3s;"></div>
            </div>
        </div>

        <p style="text-align: center; color: #999; margin-top: 40px; font-size: 12px;">
            For detailed results, visit: <code>/api/test-results/latest</code>
        </p>
    </div>
</body>
</html>
EOF

    echo "  ✅ HTML report generated: test-results.html"
}

# Main execution
echo ""
echo "  Step 1: Checking for latest test results..."
echo ""

# Look for the latest TRX file
LATEST_TRX=$(find "$REPO_ROOT/CRM.Backend/tests/TestResults" -name "*.trx" -type f 2>/dev/null | sort -r | head -1)

if [[ -n "$LATEST_TRX" ]]; then
    echo "  ✅ Found latest TRX: $(basename "$LATEST_TRX")"
    convert_trx_to_json "$LATEST_TRX"
else
    echo "  ⚠️  No TRX files found. Creating empty results file..."
    
    cat > "$LATEST_RESULTS" <<'EOF'
{
  "sessionId": "empty-session",
  "startTime": "2026-02-17T00:00:00Z",
  "endTime": "2026-02-17T00:00:00Z",
  "totalTests": 0,
  "passedTests": 0,
  "failedTests": 0,
  "skippedTests": 0,
  "totalDuration": "PT0S",
  "passRate": 0,
  "results": []
}
EOF
fi

echo ""
echo "  Step 2: Cleaning up old results..."
purge_old_results

echo ""
echo "  Step 3: Generating reports..."
generate_summary_report

echo ""
echo "═══════════════════════════════════════════════════════════════════════════════"
echo "  ✅ Test results aggregation complete!"
echo "═══════════════════════════════════════════════════════════════════════════════"
echo ""
echo "  Access results at:"
echo "    • JSON API:     http://localhost:5000/api/test-results/latest"
echo "    • HTML Report:  $LOGS_DIR/test-results.html"
echo "    • Web Dashboard: http://localhost:3000/test-results"
echo ""

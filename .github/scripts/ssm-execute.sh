#!/bin/bash
# ssm-execute.sh - Execute PowerShell commands on EC2 instances via SSM
# Usage: ./ssm-execute.sh <instance_id> <region> <commands_json_file> [timeout_seconds]
#
# This script wraps AWS SSM send-command + polling for use in CI/CD pipelines.
# It sends a PowerShell command via SSM, polls for completion, and returns
# the output. Exit code 0 on success, 1 on failure/timeout.

set -euo pipefail

# --- Parameters ---
INSTANCE_ID="${1:?Usage: $0 <instance_id> <region> <commands_json_file> [timeout_seconds]}"
REGION="${2:?Usage: $0 <instance_id> <region> <commands_json_file> [timeout_seconds]}"
COMMANDS_FILE="${3:?Usage: $0 <instance_id> <region> <commands_json_file> [timeout_seconds]}"
TIMEOUT="${4:-300}"

# Calculate poll iterations (poll every 10 seconds)
POLL_INTERVAL=10
MAX_POLLS=$(( TIMEOUT / POLL_INTERVAL ))

# --- Validate inputs ---
if [ ! -f "$COMMANDS_FILE" ]; then
  echo "ERROR: Commands file not found: $COMMANDS_FILE" >&2
  exit 1
fi

echo "=== SSM Execute ==="
echo "Instance:  $INSTANCE_ID"
echo "Region:    $REGION"
echo "Commands:  $COMMANDS_FILE"
echo "Timeout:   ${TIMEOUT}s (${MAX_POLLS} polls @ ${POLL_INTERVAL}s)"
echo "==================="

# --- Send command ---
COMMAND_ID=$(aws ssm send-command \
  --instance-ids "$INSTANCE_ID" \
  --document-name "AWS-RunPowerShellScript" \
  --parameters "file://$COMMANDS_FILE" \
  --region "$REGION" \
  --timeout-seconds "$TIMEOUT" \
  --query "Command.CommandId" \
  --output text)

if [ -z "$COMMAND_ID" ]; then
  echo "ERROR: Failed to send SSM command" >&2
  exit 1
fi

echo "SSM Command ID: $COMMAND_ID"

# --- Poll for completion ---
STATUS="InProgress"
for i in $(seq 1 "$MAX_POLLS"); do
  STATUS=$(aws ssm get-command-invocation \
    --command-id "$COMMAND_ID" \
    --instance-id "$INSTANCE_ID" \
    --region "$REGION" \
    --query "Status" \
    --output text 2>/dev/null || echo "InProgress")

  echo "  Poll $i/$MAX_POLLS: $STATUS"

  case "$STATUS" in
    Success)
      echo ""
      echo "=== Command Output ==="
      aws ssm get-command-invocation \
        --command-id "$COMMAND_ID" \
        --instance-id "$INSTANCE_ID" \
        --region "$REGION" \
        --query "StandardOutputContent" \
        --output text
      echo "======================"
      exit 0
      ;;
    Failed|TimedOut|Cancelled)
      echo ""
      echo "=== Command FAILED (Status: $STATUS) ==="
      echo "--- STDERR ---"
      aws ssm get-command-invocation \
        --command-id "$COMMAND_ID" \
        --instance-id "$INSTANCE_ID" \
        --region "$REGION" \
        --query "StandardErrorContent" \
        --output text
      echo "--- STDOUT ---"
      aws ssm get-command-invocation \
        --command-id "$COMMAND_ID" \
        --instance-id "$INSTANCE_ID" \
        --region "$REGION" \
        --query "StandardOutputContent" \
        --output text
      echo "============================================"
      exit 1
      ;;
    InProgress|Pending|Delayed)
      # Still running, continue polling
      ;;
    *)
      echo "  Unexpected status: $STATUS"
      ;;
  esac

  sleep "$POLL_INTERVAL"
done

# --- Timeout ---
echo ""
echo "ERROR: SSM command timed out after ${TIMEOUT}s (status: $STATUS)" >&2
echo "Command ID: $COMMAND_ID" >&2
exit 1

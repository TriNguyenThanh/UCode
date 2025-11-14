#!/bin/bash
# Script to run API submission test

cd "$(dirname "$0")"

echo "🚀 Starting API Submission Test..."
echo ""

# Activate virtual environment if exists
if [ -d ".venv" ]; then
    echo "📦 Activating virtual environment..."
    source .venv/bin/activate
fi

# Run the test script
python3 test_submit_api.py

echo ""
echo "✅ Test completed!"

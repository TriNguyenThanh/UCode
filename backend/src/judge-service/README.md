# Judge Service - README

## 🚀 Quick Start

### 1. Choose Execution Mode

The service supports two execution modes:

- **Async Mode** (default): Processes multiple submissions in parallel - **4-16x faster**
- **Sync Mode**: Processes one submission at a time - stable, lower resource usage

### 2. Set Environment Variables

```bash
# Copy example configuration
cp .env.example .env

# Edit .env file
nano .env
```

**Quick configurations:**

```bash
# For async mode (recommended - 4x faster)
export EXECUTION_MODE=async
export MAX_CONCURRENT_SUBMISSIONS=4
export MAX_PARALLEL_TESTCASES=4

# For sync mode (stable, low resource)
export EXECUTION_MODE=sync
export MAX_CONCURRENT_SUBMISSIONS=1
```

### 3. Run the Service

#### Option A: Using Docker (Recommended)

```bash
# Build image
docker build -f Dockerfile.dev -t judge-service .

# Run with async mode
docker run -e EXECUTION_MODE=async \
           -e MAX_CONCURRENT_SUBMISSIONS=4 \
           -e MAX_PARALLEL_TESTCASES=4 \
           --privileged \
           judge-service

# Run with sync mode
docker run -e EXECUTION_MODE=sync \
           --privileged \
           judge-service
```

#### Option B: Using Virtual Environment

```bash
# Create virtual environment
python3 -m venv venv

# Activate it
source venv/bin/activate

# Install dependencies
pip install -r requirements.txt

# Run the service
python app/main.py
```

> ⚠️ On Ubuntu 24.04+ / Debian 12+, you MUST use venv or add `--break-system-packages` flag

---

## 📊 Performance Comparison

| Mode | Submissions/min | Testcases/min | Resource Usage |
|------|----------------|---------------|----------------|
| Sync | ~15 | ~150 | Low (1 box) |
| Async (2×2) | ~60 | ~600 | Medium (4 boxes) |
| Async (4×4) | ~200 | ~2400 | High (16 boxes) |

---

## ⚙️ Configuration Guide

### Execution Modes

#### Async Mode (Parallel Processing)
```bash
EXECUTION_MODE=async
MAX_CONCURRENT_SUBMISSIONS=4    # Process 4 submissions at once
MAX_PARALLEL_TESTCASES=4        # Run 4 testcases per submission in parallel
```

**Benefits:**
- ✅ 4-16x faster throughput
- ✅ Better CPU utilization
- ✅ Handles high load efficiently

**Requirements:**
- More CPU cores (4+ recommended)
- More RAM (~4GB for 16 concurrent boxes)
- `aio-pika` library (`pip install aio-pika`)

#### Sync Mode (Sequential Processing)
```bash
EXECUTION_MODE=sync
MAX_CONCURRENT_SUBMISSIONS=1    # Always 1 for sync mode
```

**Benefits:**
- ✅ Stable and predictable
- ✅ Low resource usage
- ✅ Simple debugging

**Requirements:**
- Minimal (2 cores, 2GB RAM)
- Works on any server

### Resource Calculation

**Total concurrent isolate boxes** = `MAX_CONCURRENT_SUBMISSIONS × MAX_PARALLEL_TESTCASES`

**RAM estimation**: `boxes × 256MB`

Examples:
- Sync: 1 box = ~256MB
- Async (2×2): 4 boxes = ~1GB
- Async (4×4): 16 boxes = ~4GB

---

## 🧪 Testing

### Test Async Executor
```bash
python app/test_async_executor.py
```

### Visual Demo (Sync vs Async)
```bash
python app/demo_async_vs_sync.py
```

### Integration Tests
```bash
python app/test_integration.py
```

---

## 📚 Documentation

- **[ASYNC_SUMMARY.md](../ASYNC_SUMMARY.md)** - Overview of async mode
- **[QUICK_REFERENCE.md](../QUICK_REFERENCE.md)** - Command reference
- **[ASYNC_MIGRATION_GUIDE.md](ASYNC_MIGRATION_GUIDE.md)** - Detailed migration guide
- **[ISOLATE_GUIDE.md](ISOLATE_GUIDE.md)** - Isolate sandbox documentation

---

## 🐛 Troubleshooting

### "ImportError: No module named 'aio_pika'"
```bash
pip install aio-pika
```

### "Too many isolate boxes"
Reduce concurrency:
```bash
export MAX_PARALLEL_TESTCASES=2
export MAX_CONCURRENT_SUBMISSIONS=2
```

### "externally-managed-environment" error (Ubuntu 24.04+)

**Option 1: Use Virtual Environment (Recommended)**
```bash
python3 -m venv venv
source venv/bin/activate
pip install -r requirements.txt
```

**Option 2: Use Docker**
```bash
docker build -f Dockerfile.dev -t judge-service .
docker run --privileged judge-service
```

**Option 3: Break System Packages (Not Recommended)**
```bash
pip install -r requirements.txt --break-system-packages
```

---

## 🔄 Switching Between Modes

You can switch modes without code changes:

```bash
# Switch to async
export EXECUTION_MODE=async
python app/main.py

# Switch to sync
export EXECUTION_MODE=sync
python app/main.py
```

---

## 📈 Monitoring

Watch logs for performance indicators:

**Async mode:**
```
[DEBUG] Running 10 testcases in parallel (max 4 concurrent)
[✓] Testcase #2 passed  ← Not sequential!
[✓] Testcase #0 passed
```

**Sync mode:**
```
[✓] Testcase #0 passed
[✓] Testcase #1 passed  ← Sequential order
[✓] Testcase #2 passed
```

---

## 🎯 Recommendations

**Small Server (2 cores, 4GB RAM):**
```bash
EXECUTION_MODE=async
MAX_CONCURRENT_SUBMISSIONS=2
MAX_PARALLEL_TESTCASES=2
```

**Medium Server (4 cores, 8GB RAM):**
```bash
EXECUTION_MODE=async
MAX_CONCURRENT_SUBMISSIONS=4
MAX_PARALLEL_TESTCASES=4
```

**Large Server (8+ cores, 16GB+ RAM):**
```bash
EXECUTION_MODE=async
MAX_CONCURRENT_SUBMISSIONS=8
MAX_PARALLEL_TESTCASES=8
```

**Production (Stable):**
```bash
EXECUTION_MODE=sync
MAX_CONCURRENT_SUBMISSIONS=1
```

---

## 💡 Tips

- Start with conservative settings and scale up gradually
- Monitor CPU and memory usage with `htop`
- Use async mode for high-volume periods
- Use sync mode for debugging or low-traffic periods
- Both modes work with the same RabbitMQ setup (no backend changes needed)

---

For more details, see the comprehensive guides in the `/docs` folder!

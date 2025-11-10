import sys
import json
import asyncio
from executor_isolate_async import execute_in_sandbox

async def main():
    """Entry point - chạy async executor"""
    payload = json.loads(sys.argv[1])
    language = payload["language"]
    code = payload["code"]
    testcases = payload["testcases"]
    timelimit = payload.get("timelimit", 3)
    memorylimit = payload.get("memorylimit", 256)

    # Gọi async executor
    results = await execute_in_sandbox(language, code, testcases, timelimit, memorylimit)
    print(json.dumps(results))

if __name__ == "__main__":
    asyncio.run(main())

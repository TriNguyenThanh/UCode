import sys
import json
import asyncio
from executor_isolate_async import execute_in_sandbox

async def main():
    payload = json.loads(sys.argv[1])
    
    language = payload["language"]
    code = payload["code"]
    testcases = payload["testcases"]
    timelimit = payload.get("timelimit", 2)
    memorylimit = payload.get("memorylimit", 262144)
    slot_id = payload.get("slot_id")

    if slot_id is None:
        # Fallback or error if slot_id is missing (should not happen with updated message_handler)
        # Assuming single slot 0 if running manually without manager? Or raising error?
        # Let's set default to 0 for backward compatibility manual tests
        slot_id = 0

    # Gọi async executor
    results = await execute_in_sandbox(language, code, testcases, timelimit, memorylimit, slot_id=slot_id)
    print(json.dumps(results))

if __name__ == "__main__":
    asyncio.run(main())

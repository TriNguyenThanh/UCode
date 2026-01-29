import asyncio
from adaptive_consumer import AsyncAdaptiveConsumer

async def main():
    consumer = AsyncAdaptiveConsumer()
    await consumer.start()

if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print("\n[*] Consumer stopped by user.")
    except Exception as e:
        print(f"[FATAL] Unhandled exception in main: {e}")
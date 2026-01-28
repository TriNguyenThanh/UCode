import asyncio
import logging
import os

logger = logging.getLogger(__name__)

class BoxManager:
    def __init__(self, max_slots=4):
        self.max_slots = max_slots
        self.slots = asyncio.Queue()
        # Initialize slots
        for i in range(max_slots):
            self.slots.put_nowait(i)
        logger.info(f"BoxManager initialized with {max_slots} slots")

    async def acquire_slot(self):
        # Log waiting state if queue is empty
        if self.slots.empty():
            logger.info(f"All {self.max_slots} slots busy. Waiting for a slot...")
            
        slot_id = await self.slots.get()
        logger.info(f"Acquired Slot {slot_id}")
        return slot_id

    def release_slot(self, slot_id):
        if 0 <= slot_id < self.max_slots:
            self.slots.put_nowait(slot_id)
            logger.info(f"Released Slot {slot_id}. Available: {self.slots.qsize()}/{self.max_slots}")
        else:
            logger.error(f"Attempted to release invalid slot {slot_id}. Ignored.")

# Create global instance
# MAX_CONCURRENT_SUBMISSIONS = int(os.getenv("MAX_CONCURRENT_SUBMISSIONS", "4"))
MAX_PARALLEL_TESTCASES = int(os.getenv("MAX_PARALLEL_TESTCASES", "5"))
box_manager = BoxManager(int(100/MAX_PARALLEL_TESTCASES))
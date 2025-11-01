# Import thư viện os để đọc biến môi trường
import os

# =============================================================================
# CẤU HÌNH CHO SERVER YẾU (2 cores, 4-8GB RAM)
# =============================================================================
# Tính toán concurrent containers an toàn:
# - RAM available: 6GB (để lại 2GB cho OS + services)
# - Memory per container: 256MB
# - Max containers: 6GB / 256MB = ~23 containers
# 
# Chiến lược: Conservative để tránh OOM (Out of Memory)
# - Prefetch: 1 submission tại một thời điểm
# - Max workers: 4 testcases song song (fit với 2 cores + hyperthreading)
# - Total concurrent: 1 × 4 = 4 containers (4 × 256MB = 1GB RAM)
# =============================================================================

# Số lượng submissions xử lý đồng thời (an toàn với RAM thấp)
MAX_CONCURRENT_SUBMISSIONS = int(os.getenv("MAX_CONCURRENT_SUBMISSIONS", "1"))

# Số lượng testcases chạy song song trong 1 submission (phù hợp với CPU cores)
MAX_WORKERS_PER_SUBMISSION = int(os.getenv("MAX_WORKERS_PER_SUBMISSION", "4"))

# def test():
#     """Test function để thử nghiệm execute_in_sandbox"""
#     "SubmissionId": "b8409ab4-3934-435a-8b55-834b020fa46b",
    # "Code": "a=int(input())\nprint(a*2)",
    # "Language": "python",
    # "TimeLimit": 2,
    # "MemoryLimit": 128,
    # "Testcases": [
    #     {
    #         "TestCaseId": "fce99b62-1e09-4c2d-a470-8eba2e89fb91",
    #         "InputRef": "11",
    #         "OutputRef": "22",
    #         "IndexNo": 0,
    #         "Weight": 1.0,
    #         "InputChecksum": "",
    #         "OutputChecksum": ""
    #     },
    #     {
    #         "TestCaseId": "bdb8c631-60e4-4c1e-a42d-0d578bf72b37",
    #         "InputRef": "12",
    #         "OutputRef": "24",
    #         "IndexNo": 2,
    #         "Weight": 1.0,
    #         "InputChecksum": "",
    #         "OutputChecksum": ""
    #     }
    # ]
#     result = execute_in_sandbox(
#         testcaseId=sample_submission["id"],
#         language=sample_submission["language"],  # Lấy ngôn ngữ từ sample
#         code=sample_submission["code"],  # Lấy code từ sample
#         stdin=sample_submission["stdin"],  # Lấy input từ sample
#         timelimit=sample_submission["timelimit"],  # Lấy time limit từ sample
#         memorylimit=sample_submission["memorylimit"]  # Lấy memory limit từ sample
#     )
#     # In kết quả test dạng JSON với định dạng đẹp (indent 4 spaces)
#     print("Test Result:", json.dumps(result, indent=4))

def main():
    """
    Entry point chính cho ExecutionService.
    Chạy AdaptiveConsumer để xử lý submissions từ RabbitMQ.
    """
    from app.adaptive_consumer import AdaptiveConsumer
    
    print("[*] Starting ExecutionService with Adaptive Consumer...")
    consumer = AdaptiveConsumer()
    consumer.start()

if __name__ == "__main__":
    # Chạy main() (production mode)
    main()
    
    # Hoặc chạy test (development mode)
    # test()
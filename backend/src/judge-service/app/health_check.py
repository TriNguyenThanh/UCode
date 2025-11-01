# Module kiểm tra tình trạng server trước khi chạy testcases
import psutil
import os

# Ngưỡng an toàn
MEMORY_THRESHOLD = float(os.getenv("MEMORY_THRESHOLD", "85"))  # 85%
CPU_THRESHOLD = float(os.getenv("CPU_THRESHOLD", "90"))  # 90%

def check_system_health():
    """
    Kiểm tra tình trạng server trước khi chạy testcases.
    
    Returns:
        tuple: (is_healthy, error_message)
        - is_healthy: True nếu server OK, False nếu quá tải
        - error_message: Thông báo lỗi nếu có
    """
    try:
        # Kiểm tra RAM
        memory = psutil.virtual_memory()
        memory_percent = memory.percent
        
        if memory_percent > MEMORY_THRESHOLD:
            return False, f"Server RAM quá tải: {memory_percent:.1f}% (ngưỡng: {MEMORY_THRESHOLD}%)"
        
        # Kiểm tra Swap (nếu swap được dùng = nguy hiểm)
        swap = psutil.swap_memory()
        if swap.percent > 10:  # Nếu dùng > 10% swap
            return False, f"Server đang dùng Swap: {swap.percent:.1f}% - RAM không đủ"
        
        # Kiểm tra CPU (optional - ít quan trọng hơn RAM)
        cpu_percent = psutil.cpu_percent(interval=1)
        if cpu_percent > CPU_THRESHOLD:
            return False, f"Server CPU quá tải: {cpu_percent:.1f}% (ngưỡng: {CPU_THRESHOLD}%)"
        
        return True, "OK"
        
    except Exception as e:
        # Nếu không check được, cho phép chạy (fail-open)
        return True, "OK"

def get_available_memory_mb():
    """
    Lấy số MB RAM còn available.
    
    Returns:
        int: MB RAM còn lại
    """
    try:
        memory = psutil.virtual_memory()
        return memory.available // (1024 * 1024)  # Convert to MB
    except:
        return 0

def should_accept_submission():
    """
    Quyết định có nên nhận submission mới hay không.
    Dựa trên tình trạng tài nguyên hiện tại.
    
    Returns:
        tuple: (accept, reason)
    """
    is_healthy, message = check_system_health()
    
    if not is_healthy:
        return False, f"Server quá tải: {message}"
    
    # Kiểm tra RAM còn đủ cho ít nhất 2 containers nữa không
    available_mb = get_available_memory_mb()
    min_required_mb = 512  # Tối thiểu 512MB free
    
    if available_mb < min_required_mb:
        return False, f"RAM không đủ: Còn {available_mb}MB (cần tối thiểu {min_required_mb}MB)"
    
    return True, "OK"

def log_system_stats():
    """In ra thông tin tài nguyên hiện tại (for debugging)"""
    try:
        memory = psutil.virtual_memory()
        swap = psutil.swap_memory()
        cpu = psutil.cpu_percent(interval=0.1)
        
        print(f"[HEALTH] RAM: {memory.percent:.1f}% | "
              f"Swap: {swap.percent:.1f}% | "
              f"CPU: {cpu:.1f}% | "
              f"Available: {memory.available // (1024*1024)}MB")
    except:
        pass

def get_system_stats():
    """
    Lấy thông tin chi tiết về tài nguyên hệ thống.
    
    Returns:
        dict: System stats
    """
    try:
        memory = psutil.virtual_memory()
        swap = psutil.swap_memory()
        cpu = psutil.cpu_percent(interval=0.1)
        
        return {
            "cpu_percent": cpu,
            "memory_percent": memory.percent,
            "memory_available_mb": memory.available // (1024 * 1024),
            "memory_used_mb": memory.used // (1024 * 1024),
            "memory_total_mb": memory.total // (1024 * 1024),
            "swap_percent": swap.percent
        }
    except Exception as e:
        return {
            "cpu_percent": 0,
            "memory_percent": 0,
            "memory_available_mb": 0,
            "memory_used_mb": 0,
            "memory_total_mb": 0,
            "swap_percent": 0,
            "error": str(e)
        }

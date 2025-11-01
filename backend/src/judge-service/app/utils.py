# Import subprocess để chạy các lệnh shell và command line
import subprocess

# Hàm chạy lệnh shell với giới hạn thời gian
def run_command(cmd, timeout=3):
    """Chạy lệnh trong subprocess, giới hạn thời gian"""
    # Khối try-except để bắt lỗi timeout
    try:
        # Chạy lệnh với các tham số:
        # shell=True: cho phép chạy lệnh qua shell (hỗ trợ pipe, redirect...)
        # capture_output=True: bắt stdout và stderr
        # text=True: trả về output dạng string thay vì bytes
        # timeout: giới hạn thời gian thực thi (giây)
        result = subprocess.run(
            cmd, shell=True, capture_output=True, text=True, timeout=timeout
        )
        # Trả về dictionary chứa output, error và exit code
        return {
            "stdout": result.stdout,  # Output chuẩn (kết quả chương trình)
            "stderr": result.stderr,  # Output lỗi
            "exit_code": result.returncode  # Mã thoát của chương trình (0 = thành công)
        }
    # Bắt exception khi lệnh chạy quá thời gian cho phép
    except subprocess.TimeoutExpired:
        # Trả về kết quả với thông báo timeout và exit code -1
        return {"stdout": "", "stderr": "Time limit exceeded", "exit_code": -1}
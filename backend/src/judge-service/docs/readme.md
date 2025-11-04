💡 Rất tốt — đây là lỗi rất phổ biến trên **Ubuntu 24.04+ hoặc Debian 12+** với Python 3.12 trở lên,
khi bạn cố chạy `pip install -r requirements.txt` ngoài **virtual environment (venv)**.

---

## ⚙️ Nguyên nhân

Ubuntu (và các bản mới của Debian) **kích hoạt cơ chế “externally-managed environment” (PEP 668)**,
nghĩa là:

> Bạn **không thể dùng `pip install` trực tiếp vào Python hệ thống**, để tránh ghi đè package hệ thống.

Nên hệ thống báo lỗi:

```
error: externally-managed-environment
```

---

## ✅ Cách khắc phục (3 cách, tuỳ bạn chọn)

---

### 🥇 **Cách khuyến nghị (an toàn, chuẩn nhất): dùng Virtual Environment (venv)**

1. Tạo môi trường ảo:

   ```bash
   python3 -m venv venv
   ```

2. Kích hoạt môi trường:

   ```bash
   source venv/bin/activate
   ```

   Bạn sẽ thấy dòng lệnh đổi thành:

   ```
   (venv) trislord@TrisComputer:~/Code/ExecuteCode$
   ```

3. Cài thư viện:

   ```bash
   pip install -r requirements.txt
   ```

4. Chạy chương trình:

   ```bash
   python main.py
   ```

> ✅ Ưu điểm: an toàn, cô lập, không ảnh hưởng hệ thống
> ⚠️ Lưu ý: Mỗi khi mở terminal mới, bạn cần `source venv/bin/activate` lại.

---

### 🥈 **Cách nhanh (tạm thời, nhưng không khuyến nghị)**

Nếu bạn chỉ muốn cài nhanh (dù có thể ảnh hưởng Python hệ thống),
thì thêm flag `--break-system-packages`:

```bash
pip install -r requirements.txt --break-system-packages
```

> ⚠️ Cách này sẽ ghi package trực tiếp vào môi trường hệ thống của Ubuntu.
> Dùng tạm khi test ngắn hạn, không nên dùng cho production.

---

### 🥉 **Cách dành cho devops (toàn cục, sandbox riêng)**

Nếu bạn đã cài Docker, bạn có thể chạy Python và cài pip bên trong container:

```bash
docker run -it --rm -v $(pwd):/app -w /app python:3.11 bash
pip install -r requirements.txt
python main.py
```

> ✅ Cách này cực kỳ an toàn vì mọi thứ chạy trong container riêng.

---

## ✅ Gợi ý nhanh (nên dùng)

Tạo và kích hoạt venv một lần:

```bash
python3 -m venv venv && source venv/bin/activate
pip install -r requirements.txt
```

Sau đó mỗi lần vào project:

```bash
source venv/bin/activate
python main.py
```

---

Nếu bạn muốn, mình có thể thêm **Dockerfile** cho project này
(để bạn chỉ cần `docker build . && docker run` mà không cần cài pip gì hết).
👉 Bạn có muốn mình tạo Dockerfile đó cho bạn luôn không?

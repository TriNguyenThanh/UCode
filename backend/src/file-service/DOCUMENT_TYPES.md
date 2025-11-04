# File Types Support - File Service

## 📄 Category 7: Document (NEW)

**Mục đích**: Tài liệu văn phòng chung (PDF, Word, Excel, PowerPoint)

**Cấu hình**:
- Thư mục: `documents/`
- Kích thước tối đa: **50MB**
- Loại file được hỗ trợ:

### PDF Files
| Extension | MIME Type | Description |
|-----------|-----------|-------------|
| `.pdf` | `application/pdf` | Portable Document Format |

### Microsoft Word
| Extension | MIME Type | Description |
|-----------|-----------|-------------|
| `.doc` | `application/msword` | Word 97-2003 Document |
| `.docx` | `application/vnd.openxmlformats-officedocument.wordprocessingml.document` | Word Document |
| `.dot` | `application/msword` | Word Template |
| `.dotx` | `application/vnd.openxmlformats-officedocument.wordprocessingml.template` | Word Template |
| `.docm` | `application/vnd.ms-word.document.macroEnabled.12` | Word Macro-Enabled Document |

### Microsoft Excel
| Extension | MIME Type | Description |
|-----------|-----------|-------------|
| `.xls` | `application/vnd.ms-excel` | Excel 97-2003 Workbook |
| `.xlsx` | `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` | Excel Workbook |
| `.xlsm` | `application/vnd.ms-excel.sheet.macroEnabled.12` | Excel Macro-Enabled Workbook |
| `.xlt` | `application/vnd.ms-excel` | Excel Template |
| `.xltx` | `application/vnd.openxmlformats-officedocument.spreadsheetml.template` | Excel Template |
| `.xlsb` | `application/vnd.ms-excel.sheet.binary.macroEnabled.12` | Excel Binary Workbook |

### Microsoft PowerPoint
| Extension | MIME Type | Description |
|-----------|-----------|-------------|
| `.ppt` | `application/vnd.ms-powerpoint` | PowerPoint 97-2003 Presentation |
| `.pptx` | `application/vnd.openxmlformats-officedocument.presentationml.presentation` | PowerPoint Presentation |
| `.pptm` | `application/vnd.ms-powerpoint.presentation.macroEnabled.12` | PowerPoint Macro-Enabled Presentation |
| `.pps` | `application/vnd.ms-powerpoint` | PowerPoint Slideshow |
| `.ppsx` | `application/vnd.openxmlformats-officedocument.presentationml.slideshow` | PowerPoint Slideshow |

### OpenDocument Formats
| Extension | MIME Type | Description |
|-----------|-----------|-------------|
| `.odt` | `application/vnd.oasis.opendocument.text` | OpenDocument Text |
| `.ods` | `application/vnd.oasis.opendocument.spreadsheet` | OpenDocument Spreadsheet |
| `.odp` | `application/vnd.oasis.opendocument.presentation` | OpenDocument Presentation |

### Rich Text & Plain Text
| Extension | MIME Type | Description |
|-----------|-----------|-------------|
| `.rtf` | `application/rtf`, `text/rtf` | Rich Text Format |
| `.txt` | `text/plain` | Plain Text |
| `.md` | `text/markdown` | Markdown |

---

## 📊 Tổng hợp File Types theo Category

### Category 1: Assignment Document
- **Max Size**: 10MB
- **Files**: PDF, Word (.doc, .docx), Text (.txt, .md)

### Category 2: Code Submission
- **Max Size**: 5MB
- **Files**: ZIP, RAR, 7Z, Source code files (C, C++, Java, Python, JavaScript, C#, Go, Ruby, PHP)

### Category 3: Image
- **Max Size**: 5MB
- **Files**: JPG, JPEG, PNG, GIF, SVG, WEBP
- **Security**: Magic bytes validation ✅

### Category 4: Avatar
- **Max Size**: 2MB
- **Files**: JPG, JPEG, PNG, WEBP
- **Security**: Magic bytes validation ✅

### Category 5: Test Case
- **Max Size**: 1MB
- **Files**: TXT, IN, OUT

### Category 6: Reference
- **Max Size**: 20MB
- **Files**: PDF, Word, PowerPoint, Excel

### Category 7: Document (NEW) ⭐
- **Max Size**: 50MB
- **Files**: All office documents (30+ formats)
  - PDF
  - Microsoft Office (Word, Excel, PowerPoint) - All versions
  - OpenDocument formats (ODT, ODS, ODP)
  - Rich Text Format (RTF)
  - Plain Text, Markdown

---

## 🆚 So sánh Categories

| Category | Use Case | Max Size | PDF | Word | Excel | PPT | Code | Images |
|----------|----------|----------|-----|------|-------|-----|------|--------|
| Assignment (1) | Đề bài | 10MB | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| Code (2) | Bài làm | 5MB | ❌ | ❌ | ❌ | ❌ | ✅ | ❌ |
| Image (3) | Hình minh họa | 5MB | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| Avatar (4) | Ảnh đại diện | 2MB | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| TestCase (5) | Test data | 1MB | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Reference (6) | Tài liệu tham khảo | 20MB | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Document (7)** | **Tài liệu chung** | **50MB** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |

---

## 💡 Khi nào dùng Category nào?

### Dùng Category 1 (Assignment Document)
- Upload đề bài assignment
- Hướng dẫn làm bài
- File nhỏ, đơn giản (PDF, Word, Text)

### Dùng Category 6 (Reference)
- Tài liệu học tập
- Slide bài giảng
- Tài liệu tham khảo cho khóa học
- File vừa phải (< 20MB)

### Dùng Category 7 (Document) ⭐ **NEW**
- Tài liệu văn phòng bất kỳ
- File lớn (lên đến 50MB)
- Nhiều định dạng (30+ formats)
- Hỗ trợ cả Microsoft Office và OpenDocument
- Báo cáo, kế hoạch, biểu đồ phức tạp

---

## 🔧 API Usage

### Upload Document
```http
POST /api/files/upload
Content-Type: multipart/form-data

file: report.pdf
category: 7
```

### Get Document Category Info
```http
GET /api/files/categories/7
```

**Response**:
```json
{
  "success": true,
  "data": {
    "id": 7,
    "name": "Document",
    "folderPath": "documents",
    "maxFileSizeMB": 50.0,
    "allowedExtensions": [
      ".pdf", ".doc", ".docx", ".dot", ".dotx", ".docm",
      ".xls", ".xlsx", ".xlsm", ".xlt", ".xltx", ".xlsb",
      ".ppt", ".pptx", ".pptm", ".pps", ".ppsx",
      ".odt", ".ods", ".odp", ".rtf", ".txt", ".md"
    ]
  }
}
```

---

## 📝 Client Integration

### JavaScript Example
```javascript
async function uploadDocument(file) {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('category', '7'); // Document category
  
  const response = await fetch('/api/files/upload', {
    method: 'POST',
    body: formData
  });
  
  return await response.json();
}

// Usage
const fileInput = document.getElementById('fileInput');
const file = fileInput.files[0];

// Validate file size (50MB)
if (file.size > 50 * 1024 * 1024) {
  alert('File too large. Max 50MB');
  return;
}

// Validate extension
const allowedExts = ['.pdf', '.doc', '.docx', '.xls', '.xlsx', '.ppt', '.pptx', ...];
const ext = '.' + file.name.split('.').pop().toLowerCase();
if (!allowedExts.includes(ext)) {
  alert('File type not supported');
  return;
}

// Upload
const result = await uploadDocument(file);
if (result.success) {
  console.log('Uploaded:', result.data.fileUrl);
}
```

### React Example with File Type Detection
```typescript
const DocumentUploader: React.FC = () => {
  const [file, setFile] = useState<File | null>(null);
  
  const getFileIcon = (filename: string) => {
    const ext = filename.split('.').pop()?.toLowerCase();
    const icons = {
      'pdf': '📄',
      'doc': '📝', 'docx': '📝',
      'xls': '📊', 'xlsx': '📊',
      'ppt': '📊', 'pptx': '📊',
      'odt': '📄', 'ods': '📊', 'odp': '📊'
    };
    return icons[ext as keyof typeof icons] || '📄';
  };
  
  const handleUpload = async () => {
    if (!file) return;
    
    const formData = new FormData();
    formData.append('file', file);
    formData.append('category', '7');
    
    try {
      const response = await fetch('/api/files/upload', {
        method: 'POST',
        body: formData
      });
      
      const result = await response.json();
      
      if (result.success) {
        alert('Upload successful!');
      } else {
        alert(`Error: ${result.message}`);
      }
    } catch (error) {
      alert('Upload failed');
    }
  };
  
  return (
    <div>
      <input 
        type="file" 
        accept=".pdf,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.odt,.ods,.odp,.rtf,.txt,.md"
        onChange={(e) => setFile(e.target.files?.[0] || null)}
      />
      {file && (
        <div>
          <span>{getFileIcon(file.name)}</span>
          <span>{file.name}</span>
          <span>({(file.size / (1024 * 1024)).toFixed(2)} MB)</span>
        </div>
      )}
      <button onClick={handleUpload} disabled={!file}>
        Upload Document
      </button>
    </div>
  );
};
```

---

## ✅ Benefits của Category 7

1. **Comprehensive**: Hỗ trợ 30+ file formats
2. **Large Files**: Lên đến 50MB
3. **Cross-platform**: Support cả Microsoft Office và OpenDocument
4. **Legacy Support**: Support cả file formats cũ (.doc, .xls, .ppt)
5. **Modern Formats**: Support file mới (.docx, .xlsx, .pptx)
6. **Open Standards**: Support OpenDocument formats (.odt, .ods, .odp)

---

## 🔒 Security Notes

- Tất cả file types đều được validate extension và MIME type
- File size limits được enforce nghiêm ngặt
- Filename sanitization được apply tự động
- Server xác định folder path, client không thể tự chọn
- Metadata tracking cho audit trail

---

## 🧪 Testing

```bash
# Test upload PDF
curl -X POST http://localhost:5073/api/files/upload \
  -F "file=@report.pdf" \
  -F "category=7"

# Test upload Word
curl -X POST http://localhost:5073/api/files/upload \
  -F "file=@document.docx" \
  -F "category=7"

# Test upload Excel
curl -X POST http://localhost:5073/api/files/upload \
  -F "file=@spreadsheet.xlsx" \
  -F "category=7"

# Get category info
curl http://localhost:5073/api/files/categories/7
```

---

## 📈 File Format Support Summary

| Format Type | Extensions | Count |
|-------------|------------|-------|
| PDF | .pdf | 1 |
| Word | .doc, .docx, .dot, .dotx, .docm | 5 |
| Excel | .xls, .xlsx, .xlsm, .xlt, .xltx, .xlsb | 6 |
| PowerPoint | .ppt, .pptx, .pptm, .pps, .ppsx | 5 |
| OpenDocument | .odt, .ods, .odp | 3 |
| Rich Text | .rtf | 1 |
| Plain Text | .txt, .md | 2 |
| **TOTAL** | | **23 formats** |

Plus all corresponding MIME types validated! 🎉

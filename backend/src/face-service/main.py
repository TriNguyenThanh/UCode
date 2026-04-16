
"""
Face Recognition API Service sử dụng UniFace trên Google Colab
Cài đặt:
    !pip install flask flask-cors pyngrok pillow numpy uniface pymongo faiss-cpu
"""

!pip install flask flask-cors pyngrok pillow numpy uniface pymongo faiss-cpu

from flask import Flask, request, jsonify
from flask_cors import CORS
from pyngrok import ngrok
import base64
import io
import numpy as np
from PIL import Image
import os
from datetime import datetime
from pymongo import MongoClient
import requests
import faiss

# Import UniFace
from uniface import RetinaFace, ArcFace, compute_similarity

app = Flask(__name__)
CORS(app)

# Khởi tạo models
print("Initializing UniFace models...")
detector = RetinaFace()  # Face detection
recognizer = ArcFace()   # Face recognition

# MongoDB Configuration
MONGO_URI = os.environ.get("MONGO_URI", "mongodb+srv://hieutest420_db_user:Mk1vhw89JtbTUYUB@ucode.e8mznkg.mongodb.net/?appName=UCode")
DB_NAME = "uface_recognition"
COLLECTION_NAME = "face_embeddings"

# Webhook Configuration
USER_SERVICE_URL = os.environ.get("USER_SERVICE_URL", "https://api.ucode.io.vn")
INTERNAL_API_KEY = os.environ.get("INTERNAL_API_KEY", "ucode-internal-service-key-2024")

# MongoDB Connection
mongo_client = None
db = None
collection = None

# FAISS Index
faiss_index = None
user_id_list = []  # Map index position -> user_id

def init_mongodb():
    """Khởi tạo kết nối MongoDB"""
    global mongo_client, db, collection
    try:
        mongo_client = MongoClient(MONGO_URI)
        db = mongo_client[DB_NAME]
        collection = db[COLLECTION_NAME]
        # Tạo index cho user_id
        collection.create_index("user_id", unique=True)
        print(f"✅ Connected to MongoDB")
        print(f"📦 Database: {DB_NAME}, Collection: {COLLECTION_NAME}")
        
        # Khởi tạo FAISS index
        init_faiss_index()
    except Exception as e:
        print(f"❌ Failed to connect to MongoDB: {str(e)}")
        raise e

def init_faiss_index():
    """Khởi tạo FAISS index từ MongoDB"""
    global faiss_index, user_id_list
    
    # ArcFace embedding dimension = 512
    dimension = 512
    
    # Sử dụng IndexFlatIP (Inner Product) cho cosine similarity
    # Hoặc IndexFlatL2 cho L2 distance
    faiss_index = faiss.IndexFlatIP(dimension)
    
    # Load tất cả embeddings từ MongoDB
    docs = collection.find({})
    embeddings = []
    user_ids = []
    
    for doc in docs:
        embedding = np.array(doc["embedding"], dtype=np.float32)
        # Normalize embedding cho cosine similarity
        faiss.normalize_L2(embedding.reshape(1, -1))
        embeddings.append(embedding)
        user_ids.append(doc["user_id"])
    
    if embeddings:
        embeddings_matrix = np.vstack(embeddings)
        faiss_index.add(embeddings_matrix)
        user_id_list = user_ids
        print(f"🔍 FAISS index initialized with {len(user_ids)} embeddings")
    else:
        user_id_list = []
        print(f"🔍 FAISS index initialized (empty)")

def add_to_faiss_index(user_id, embedding):
    """Thêm embedding vào FAISS index"""
    global faiss_index, user_id_list
    
    # Normalize embedding
    embedding_normalized = embedding.astype(np.float32).reshape(1, -1)
    faiss.normalize_L2(embedding_normalized)
    
    # Kiểm tra xem user_id đã tồn tại chưa
    if user_id in user_id_list:
        # Update: xóa cũ và thêm mới
        idx = user_id_list.index(user_id)
        # FAISS không hỗ trợ update trực tiếp, phải rebuild index
        init_faiss_index()
    else:
        # Add new
        faiss_index.add(embedding_normalized)
        user_id_list.append(user_id)

def remove_from_faiss_index(user_id):
    """Xóa embedding khỏi FAISS index"""
    global faiss_index, user_id_list
    
    if user_id in user_id_list:
        # FAISS không hỗ trợ xóa trực tiếp, phải rebuild index
        init_faiss_index()

def search_similar_face(embedding, threshold=0.6, k=1):
    """Tìm kiếm khuôn mặt tương tự trong FAISS index
    
    Returns:
        List of (user_id, similarity) nếu tìm thấy match >= threshold
    """
    global faiss_index, user_id_list
    
    if faiss_index.ntotal == 0:
        return []
    
    # Normalize query embedding
    query_embedding = embedding.astype(np.float32).reshape(1, -1)
    faiss.normalize_L2(query_embedding)
    
    # Search k nearest neighbors
    # D = distances (similarity scores for IP index)
    # I = indices
    D, I = faiss_index.search(query_embedding, min(k, faiss_index.ntotal))
    
    results = []
    for i, (distance, idx) in enumerate(zip(D[0], I[0])):
        if idx != -1:  # Valid index
            similarity = float(distance)  # For IP index, distance = similarity
            if similarity >= threshold:
                results.append((user_id_list[idx], similarity))
    
    return results

def get_embedding_from_db(user_id):
    """Lấy embedding từ MongoDB"""
    doc = collection.find_one({"user_id": user_id})
    if doc:
        return np.array(doc["embedding"])
    return None

def save_embedding_to_db(user_id, embedding, image_filename=None):
    """Lưu embedding vào MongoDB và FAISS index"""
    doc = {
        "user_id": user_id,
        "embedding": embedding.tolist(),
        "updated_at": datetime.utcnow()
    }
    
    # Thêm image_filename nếu có (chỉ để backup, không dùng cho xác thực)
    if image_filename:
        doc["image_filename"] = image_filename
    
    collection.update_one(
        {"user_id": user_id},
        {"$set": doc},
        upsert=True
    )
    
    # Cập nhật FAISS index
    add_to_faiss_index(user_id, embedding)

def delete_embedding_from_db(user_id):
    """Xóa embedding từ MongoDB và FAISS index"""
    result = collection.delete_one({"user_id": user_id})
    if result.deleted_count > 0:
        remove_from_faiss_index(user_id)
        return True
    return False

def get_all_embeddings():
    """Lấy tất cả embeddings từ MongoDB"""
    docs = collection.find({})
    return {doc["user_id"]: np.array(doc["embedding"]) for doc in docs}

def get_all_user_ids():
    """Lấy danh sách tất cả user_id"""
    docs = collection.find({}, {"user_id": 1})
    return [doc["user_id"] for doc in docs]

def count_users():
    """Đếm số lượng users"""
    return collection.count_documents({})


def notify_user_service(user_id, is_face_auth):
    """Gọi webhook sang user-service để cập nhật IsFaceAuth"""
    try:
        webhook_url = f"{USER_SERVICE_URL}/api/v1/webhooks/face-auth-status"
        headers = {
            "Content-Type": "application/json",
            "X-Internal-Api-Key": INTERNAL_API_KEY
        }
        payload = {
            "userId": user_id,
            "isFaceAuth": is_face_auth
        }
        
        print(f"Calling webhook: {webhook_url}")
        response = requests.post(webhook_url, json=payload, headers=headers, timeout=5)
        
        if response.status_code == 200:
            print(f"✅ Webhook success: Updated IsFaceAuth={is_face_auth} for user {user_id}")
            return True
        else:
            print(f"⚠️ Webhook failed: {response.status_code} - {response.text}")
            return False
    except Exception as e:
        print(f"❌ Webhook error: {str(e)}")
        return False


def api_response(success, message, data=None):
    """Chuẩn hóa response format"""
    return jsonify({
        'success': success,
        'message': message,
        'data': data
    })

def base64_to_image(base64_string):
    """Chuyển base64 string thành numpy array"""
    if ',' in base64_string:
        base64_string = base64_string.split(',')[1]

    image_data = base64.b64decode(base64_string)
    image = Image.open(io.BytesIO(image_data))
    # Convert to RGB if needed
    if image.mode != 'RGB':
        image = image.convert('RGB')
    # Convert to numpy array (BGR for OpenCV compatibility)
    image_array = np.array(image)
    return image_array[:, :, ::-1]  # RGB to BGR

@app.route('/health', methods=['GET'])
def health_check():
    """Kiểm tra trạng thái service"""
    return api_response(True, "Service is healthy", {
        'status': 'healthy',
        'detector': 'RetinaFace',
        'recognizer': 'ArcFace',
        'registered_faces': count_users()
    })

@app.route('/register', methods=['POST'])
def register_face():
    """
    Đăng ký khuôn mặt mới
    Body: {
        "user_id": "string",
        "image": "base64_string"
    }
    """
    try:
        data = request.json
        user_id = data.get('user_id')
        image_base64 = data.get('image')

        if not user_id or not image_base64:
            return api_response(False, "Missing user_id or image", None), 400

        # Chuyển đổi ảnh
        image = base64_to_image(image_base64)

        # Detect faces
        faces = detector.detect(image)

        if not faces or len(faces) == 0:
            return api_response(False, "No face detected in image", None), 400

        # Lấy khuôn mặt đầu tiên
        face = faces[0]
        landmarks = face.landmarks  # 5-point landmarks

        # Trích xuất embedding sử dụng ArcFace
        embedding = recognizer.get_normalized_embedding(image, landmarks)

        # Đảm bảo embedding là numpy array và flatten nếu cần
        if isinstance(embedding, np.ndarray):
            if embedding.ndim > 1:
                embedding = embedding.flatten()
        else:
            embedding = np.array(embedding).flatten()

        # Kiểm tra xem user_id đã đăng ký chưa
        existing_embedding = get_embedding_from_db(user_id)
        if existing_embedding is not None:
            return api_response(False, "User already registered. Please delete first to re-register.", {
                'user_id': user_id
            }), 400
        
        # Kiểm tra xem khuôn mặt này đã được đăng ký bởi user khác chưa
        # Sử dụng FAISS để tìm kiếm nhanh O(log n) thay vì O(n)
        threshold = 0.4  # ArcFace similarity threshold
        
        similar_faces = search_similar_face(embedding, threshold=threshold, k=5)
        
        # Nếu có bất kỳ match nào → reject
        if similar_faces:
            matched_user_id, similarity = similar_faces[0]
            return api_response(False, f"Face already registered for user {matched_user_id}", {
                'matched_user_id': matched_user_id,
                'similarity': similarity,
                'threshold': threshold
            }), 400

        # Tự động tạo image_filename dựa trên user_id và timestamp (chỉ để backup)
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        image_filename = f"{user_id}_{timestamp}.jpg"

        # Lưu vào MongoDB (kèm image_filename)
        save_embedding_to_db(user_id, embedding, image_filename)
        
        # NOTE: Không cần gọi webhook nữa vì user-service sẽ tự cập nhật IsFaceAuth
        # sau khi gọi API này thành công

        response_data = {
            'user_id': user_id,
            'face_bbox': face.bbox.tolist(),
            'confidence': float(face.confidence),
            'embedding_shape': list(embedding.shape),
            'image_filename': image_filename,
            'timestamp': datetime.now().isoformat()
        }

        return api_response(True, "Face registered successfully", response_data)

    except Exception as e:
        import traceback
        print(f"Error in register_face: {str(e)}")
        print(traceback.format_exc())
        return api_response(False, str(e), None), 500

@app.route('/verify', methods=['POST'])
def verify_face():
    """
    Xác thực khuôn mặt (1:1 verification)
    Body: {
        "user_id": "string",
        "image": "base64_string",
        "threshold": 0.4 (optional, default 0.4 for ArcFace)
    }
    """
    try:
        data = request.json
        user_id = data.get('user_id')
        image_base64 = data.get('image')
        threshold = data.get('threshold', 0.4)  # ArcFace threshold thường 0.3-0.5

        if not user_id or not image_base64:
            return api_response(False, "Missing user_id or image", None), 400

        stored_embedding = get_embedding_from_db(user_id)
        if stored_embedding is None:
            return api_response(False, "User not registered", None), 404

        image = base64_to_image(image_base64)

        faces = detector.detect(image)

        if not faces or len(faces) == 0:
            return api_response(False, "No face detected in image", None), 400

        face = faces[0]
        landmarks = face.landmarks

        # Trích xuất embedding
        embedding = recognizer.get_normalized_embedding(image, landmarks)

        # Đảm bảo embedding là numpy array 1D
        if isinstance(embedding, np.ndarray):
            if embedding.ndim > 1:
                embedding = embedding.flatten()
        else:
            embedding = np.array(embedding).flatten()

        # So sánh với embedding đã lưu
        if stored_embedding.ndim > 1:
            stored_embedding = stored_embedding.flatten()

        # Tính similarity (cosine similarity)
        similarity = compute_similarity(
            embedding.reshape(1, -1),
            stored_embedding.reshape(1, -1)
        )

        # Xử lý kết quả similarity
        if isinstance(similarity, np.ndarray):
            if similarity.ndim > 0:
                similarity = float(similarity.flatten()[0])
            else:
                similarity = float(similarity)
        else:
            similarity = float(similarity)

        is_match = similarity >= threshold

        return api_response(True, "Face verification completed", {
            'user_id': user_id,
            'is_match': is_match,
            'similarity': similarity,
            'threshold': threshold,
            'face_bbox': face.bbox.tolist(),
            'confidence': float(face.confidence),
            'timestamp': datetime.now().isoformat()
        })

    except Exception as e:
        import traceback
        print(f"Error in verify_face: {str(e)}")
        print(traceback.format_exc())
        return api_response(False, str(e), None), 500

@app.route('/identify', methods=['POST'])
def identify_face():
    """
    Nhận diện khuôn mặt (1:N identification)
    Body: {
        "image": "base64_string",
        "threshold": 0.4 (optional),
        "top_k": 5 (optional)
    }
    """
    try:
        data = request.json
        image_base64 = data.get('image')
        threshold = data.get('threshold', 0.4)
        top_k = data.get('top_k', 5)

        if not image_base64:
            return api_response(False, "Missing image", None), 400

        face_database = get_all_embeddings()
        if not face_database:
            return api_response(False, "No faces registered in database", None), 400

        # Chuyển đổi ảnh
        image = base64_to_image(image_base64)

        # Detect faces
        faces = detector.detect(image)

        if not faces or len(faces) == 0:
            return api_response(False, "No face detected in image", None), 400

        face = faces[0]
        landmarks = face.landmarks

        # Trích xuất embedding
        embedding = recognizer.get_normalized_embedding(image, landmarks)

        # Đảm bảo embedding là numpy array 1D
        if isinstance(embedding, np.ndarray):
            if embedding.ndim > 1:
                embedding = embedding.flatten()
        else:
            embedding = np.array(embedding).flatten()

        # So sánh với tất cả embeddings
        matches = []
        for user_id, stored_embedding in face_database.items():
            if stored_embedding.ndim > 1:
                stored_embedding = stored_embedding.flatten()

            similarity = compute_similarity(
                embedding.reshape(1, -1),
                stored_embedding.reshape(1, -1)
            )

            # Xử lý kết quả similarity
            if isinstance(similarity, np.ndarray):
                if similarity.ndim > 0:
                    similarity = float(similarity.flatten()[0])
                else:
                    similarity = float(similarity)
            else:
                similarity = float(similarity)

            if similarity >= threshold:
                matches.append({
                    'user_id': user_id,
                    'similarity': similarity
                })

        # Sắp xếp theo similarity
        matches.sort(key=lambda x: x['similarity'], reverse=True)
        matches = matches[:top_k]

        return api_response(True, "Face identification completed", {
            'matches': matches,
            'match_found': len(matches) > 0,
            'threshold': threshold,
            'face_bbox': face.bbox.tolist(),
            'confidence': float(face.confidence),
            'timestamp': datetime.now().isoformat()
        })

    except Exception as e:
        import traceback
        print(f"Error in identify_face: {str(e)}")
        print(traceback.format_exc())
        return api_response(False, str(e), None), 500

@app.route('/detect', methods=['POST'])
def detect_faces():
    """
    Chỉ detect faces, không recognition
    Body: {
        "image": "base64_string"
    }
    """
    try:
        data = request.json
        image_base64 = data.get('image')

        if not image_base64:
            return api_response(False, "Missing image", None), 400

        # Chuyển đổi ảnh
        image = base64_to_image(image_base64)

        # Detect faces
        faces = detector.detect(image)

        # Format response
        detected_faces = []
        for face in faces:
            detected_faces.append({
                'bbox': face.bbox.tolist(),
                'confidence': float(face.confidence),
                'landmarks': face.landmarks.tolist()
            })

        return api_response(True, "Face detection completed", {
            'num_faces': len(faces),
            'faces': detected_faces,
            'timestamp': datetime.now().isoformat()
        })

    except Exception as e:
        return api_response(False, str(e), None), 500

@app.route('/delete', methods=['DELETE'])
def delete_face():
    """
    Xóa khuôn mặt đã đăng ký
    Body: {
        "user_id": "string"
    }
    """
    try:
        data = request.json
        user_id = data.get('user_id')

        if not user_id:
            return api_response(False, "Missing user_id", None), 400

        deleted = delete_embedding_from_db(user_id)
        if not deleted:
            return api_response(False, "User not found", None), 404

        return api_response(True, "Face deleted successfully", {
            'user_id': user_id,
            'timestamp': datetime.now().isoformat()
        })

    except Exception as e:
        return api_response(False, str(e), None), 500

@app.route('/list', methods=['GET'])
def list_users():
    """Liệt kê tất cả user đã đăng ký"""
    users = get_all_user_ids()
    return api_response(True, "Users retrieved successfully", {
        'users': users,
        'total': len(users)
    })

def start_server(port=5000, use_ngrok=True):
    """Khởi động server"""
    # Khởi tạo MongoDB
    init_mongodb()

    if use_ngrok:
        # Cấu hình ngrok token (lấy từ https://dashboard.ngrok.com/get-started/your-authtoken)
        # Uncomment và thay "YOUR_NGROK_TOKEN" bằng token của bạn.
        # ngrok.set_auth_token("YOUR_NGROK_TOKEN")
        public_url = ngrok.connect(port)
        print(f"\n{'='*70}")
        print(f"🚀 UniFace Face Recognition API đang chạy!")
        print(f"{'='*70}")
        print(f"📍 Local URL:  http://127.0.0.1:{port}")
        print(f"🌐 Public URL: {public_url}")
        print(f"{'='*70}\n")
        print("📡 API Endpoints:")
        print(f"  GET    {public_url}/health    - Kiểm tra trạng thái")
        print(f"  POST   {public_url}/detect    - Detect faces")
        print(f"  POST   {public_url}/register  - Đăng ký khuôn mặt")
        print(f"  POST   {public_url}/verify    - Xác thực 1:1")
        print(f"  POST   {public_url}/identify  - Nhận diện 1:N")
        print(f"  GET    {public_url}/list      - Danh sách users")
        print(f"  DELETE {public_url}/delete    - Xóa user")
        print(f"\n{'='*70}\n")

    app.run(port=port, threaded=True)

if __name__ == '__main__':
    # Cấu hình ngrok token (lấy từ https://dashboard.ngrok.com/get-started/your-authtoken)
    ngrok.set_auth_token("36mhcqflXxDtl7d6NMc0hk2Mlz4_5BZ4BNYf3dSUFPZPLgskP")

    start_server(port=5000, use_ngrok=True)

import { useNavigate, useLocation, redirect } from 'react-router'

export function meta() {
  return [{ title: '404 - Không tìm thấy trang | UCode' }]
}

// Handle POST requests to 404 routes
export async function action() {
  throw new Response('Not Found', { status: 404 })
}

export default function NotFound() {
  const navigate = useNavigate()
  const location = useLocation()

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center px-4">
      <div className="max-w-md w-full text-center">
        <div className="mb-8">
          <h1 className="text-9xl font-bold text-blue-600">404</h1>
          <div className="text-6xl mb-4">🔍</div>
        </div>

        <h2 className="text-3xl font-bold text-gray-900 mb-4">
          Không tìm thấy trang
        </h2>

        <p className="text-gray-600 mb-2">
          Trang bạn đang tìm kiếm không tồn tại hoặc đã bị di chuyển.
        </p>

        <p className="text-sm text-gray-500 font-mono mb-8 break-all">
          {location.pathname}
        </p>

        <div className="space-y-3">
          <button
            onClick={() => navigate(-1)}
            className="w-full py-3 px-4 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-100 transition-colors cursor-pointer"
          >
            ← Quay lại
          </button>

          <button
            onClick={() => navigate('/')}
            className="w-full py-3 px-4 bg-blue-600 text-white font-semibold rounded-lg hover:bg-blue-700 transition-colors cursor-pointer"
          >
            Về trang chủ
          </button>
        </div>
      </div>
    </div>
  )
}

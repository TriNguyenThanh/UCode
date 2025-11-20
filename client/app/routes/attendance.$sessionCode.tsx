import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router'
import { getAttendanceSessionByCode, checkInAttendance, checkAttendanceStatus } from '~/services/attendanceService'
import type { AttendanceSession, AttendanceRecord, GeolocationPosition } from '~/types'
import { auth } from '~/auth'

export default function AttendanceCheckIn() {
  const { sessionCode } = useParams()
  const navigate = useNavigate()
  const [session, setSession] = useState<AttendanceSession | null>(null)
  const [attendanceRecord, setAttendanceRecord] = useState<AttendanceRecord | null>(null)
  const [loading, setLoading] = useState(true)
  const [checking, setChecking] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [ipAddress, setIpAddress] = useState<string>('Đang tải...')
  const [location, setLocation] = useState<{ latitude: number; longitude: number; accuracy?: number } | null>(null)
  const [locationError, setLocationError] = useState<string | null>(null)

  useEffect(() => {
    // Kiểm tra đăng nhập
    if (!auth.isAuthenticated()) {
      // Lưu URL hiện tại để redirect về sau khi đăng nhập
      const returnUrl = `/attendance/${sessionCode}`
      navigate(`/login?returnUrl=${encodeURIComponent(returnUrl)}`)
      return
    }

    if (!sessionCode) return

    const loadSession = async () => {
      try {
        setLoading(true)
        const [sessionData, statusData] = await Promise.all([
          getAttendanceSessionByCode(sessionCode),
          checkAttendanceStatus(sessionCode)
        ])
        setSession(sessionData)
        setAttendanceRecord(statusData)

        // Lấy IP address
        fetchIpAddress()

        // Lấy GPS nếu session yêu cầu
        if (sessionData.requireGpsCheck) {
          requestLocation()
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Không thể tải thông tin phiên điểm danh')
      } finally {
        setLoading(false)
      }
    }

    loadSession()
  }, [sessionCode, navigate])

  const fetchIpAddress = async () => {
    try {
      const response = await fetch('https://api.ipify.org?format=json')
      const data = await response.json()
      setIpAddress(data.ip)
    } catch {
      setIpAddress('Không xác định')
    }
  }

  const requestLocation = async () => {
    if (!navigator.geolocation) {
      setLocationError('Trình duyệt không hỗ trợ GPS')
      return
    }

    setLocationError(null)
    setLocation(null)

    try {
      // Sử dụng watchPosition để lấy vị trí liên tục cho đến khi đủ chính xác
      let watchId: number | null = null
      let bestAccuracy = Infinity

      const promise = new Promise<GeolocationPosition>((resolve, reject) => {
        const timeout = setTimeout(() => {
          if (watchId !== null) {
            navigator.geolocation.clearWatch(watchId)
          }
          reject(new Error('Timeout'))
        }, 20000) // 20 giây timeout

        watchId = navigator.geolocation.watchPosition(
          (pos) => {
            const accuracy = pos.coords.accuracy

            // Cập nhật nếu độ chính xác tốt hơn
            if (accuracy < bestAccuracy) {
              bestAccuracy = accuracy
              setLocation({
                latitude: pos.coords.latitude,
                longitude: pos.coords.longitude,
                accuracy: accuracy
              })

              // Nếu đủ chính xác (< 50m), dừng lại
              if (accuracy < 50) {
                clearTimeout(timeout)
                if (watchId !== null) {
                  navigator.geolocation.clearWatch(watchId)
                }
                resolve(pos.coords)
              }
            }
          },
          (err) => {
            clearTimeout(timeout)
            if (watchId !== null) {
              navigator.geolocation.clearWatch(watchId)
            }
            reject(err)
          },
          {
            enableHighAccuracy: true,
            timeout: 20000,
            maximumAge: 0
          }
        )
      })

      await promise
      setLocationError(null)
    } catch (err: any) {
      if (err.code === 1) {
        setLocationError('Bạn đã từ chối cấp quyền truy cập vị trí')
        setLocation(null)
      } else if (err.code === 2) {
        setLocationError('Không thể xác định vị trí')
        setLocation(null)
      } else if (err.code === 3 || err.message === 'Timeout') {
        // Nếu timeout nhưng đã có vị trí, kiểm tra độ chính xác
        const currentLoc = location
        if (currentLoc && currentLoc.accuracy && currentLoc.accuracy > 100) {
          setLocationError(`Độ chính xác kém (±${currentLoc.accuracy.toFixed(0)}m). Vui lòng thử lại hoặc ra ngoài trời.`)
        } else if (!currentLoc) {
          setLocationError('Hết thời gian chờ lấy vị trí')
        }
      } else {
        setLocationError('Lỗi khi lấy vị trí GPS')
        setLocation(null)
      }
    }
  }

  const handleCheckIn = async () => {
    if (!sessionCode || !session) return

    // Kiểm tra GPS nếu yêu cầu
    if (session.requireGpsCheck && !location) {
      setError('Vui lòng cấp quyền truy cập vị trí để điểm danh')
      return
    }

    try {
      setChecking(true)
      setError(null)

      const record = await checkInAttendance({
        sessionCode,
        latitude: location?.latitude,
        longitude: location?.longitude
      })
      setAttendanceRecord(record)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Điểm danh thất bại')
    } finally {
      setChecking(false)
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Đang tải...</p>
        </div>
      </div>
    )
  }

  if (error && !session) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="text-red-600 text-xl mb-4">❌</div>
          <p className="text-red-600">{error}</p>
          <button
            onClick={() => navigate('/')}
            className="mt-4 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 cursor-pointer"
          >
            Quay lại
          </button>
        </div>
      </div>
    )
  }

  if (!session) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <p className="text-gray-600">Không tìm thấy phiên điểm danh</p>
        </div>
      </div>
    )
  }

  const isExpired = new Date(session.endTime) < new Date()
  const hasCheckedIn = !!attendanceRecord

  return (
    <div className="min-h-screen bg-gray-50 py-8 px-4">
      <div className="max-w-2xl mx-auto">
        <div className="bg-white rounded-lg shadow-md p-6">
          <h1 className="text-2xl font-bold text-gray-900 mb-6">Điểm danh</h1>

          <div className="space-y-4 mb-6">
            <div>
              <label className="text-sm font-medium text-gray-500">Lớp học</label>
              <p className="text-lg text-gray-900">{session.className}</p>
            </div>

            <div>
              <label className="text-sm font-medium text-gray-500">Tiêu đề</label>
              <p className="text-lg text-gray-900">{session.title}</p>
            </div>

            <div>
              <label className="text-sm font-medium text-gray-500">Mã phiên</label>
              <p className="text-lg font-mono text-gray-900">{session.sessionCode}</p>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="text-sm font-medium text-gray-500">Bắt đầu</label>
                <p className="text-gray-900">
                  {new Date(session.startTime).toLocaleString('vi-VN')}
                </p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-500">Kết thúc</label>
                <p className="text-gray-900">
                  {new Date(session.endTime).toLocaleString('vi-VN')}
                </p>
              </div>
            </div>

            <div className="border-t pt-4 mt-4">
              <div className="mb-3">
                <label className="text-sm font-medium text-gray-500">Địa chỉ IP của bạn</label>
                <p className="text-gray-900 font-mono">{ipAddress}</p>
              </div>

              {session.requireGpsCheck && (
                <div>
                  <label className="text-sm font-medium text-gray-500">Vị trí GPS</label>
                  {location ? (
                    <div className="text-gray-900">
                      <p className="font-mono text-sm">
                        Vĩ độ: {location.latitude.toFixed(6)}
                      </p>
                      <p className="font-mono text-sm">
                        Kinh độ: {location.longitude.toFixed(6)}
                      </p>
                      {location.accuracy && (
                        <p className={`text-xs mt-1 ${location.accuracy < 50 ? 'text-green-600' : location.accuracy < 100 ? 'text-yellow-600' : 'text-red-600'}`}>
                          Độ chính xác: ±{location.accuracy.toFixed(0)}m
                          {location.accuracy >= 100 && ' (Kém - Nên thử lại)'}
                        </p>
                      )}
                      {location.accuracy && location.accuracy < 50 ? (
                        <p className="text-green-600 text-sm mt-1">✓ Đã lấy vị trí chính xác</p>
                      ) : (
                        <p className="text-yellow-600 text-sm mt-1">⚠️ Đang cải thiện độ chính xác...</p>
                      )}
                    </div>
                  ) : locationError ? (
                    <div className="text-red-600">
                      <p className="text-sm">{locationError}</p>
                      <button
                        onClick={requestLocation}
                        className="mt-2 text-sm text-blue-600 hover:text-blue-700 underline cursor-pointer"
                      >
                        Thử lại
                      </button>
                    </div>
                  ) : (
                    <p className="text-yellow-600 text-sm">⏳ Đang lấy vị trí...</p>
                  )}
                </div>
              )}
            </div>
          </div>

          {error && (
            <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded">
              <p className="text-red-600">{error}</p>
            </div>
          )}

          {hasCheckedIn ? (
            <div className="bg-green-50 border border-green-200 rounded-lg p-6 text-center">
              <div className="text-green-600 text-4xl mb-2">✓</div>
              <h2 className="text-xl font-semibold text-green-900 mb-2">
                Đã điểm danh thành công
              </h2>
              <p className="text-green-700">
                Thời gian: {new Date(attendanceRecord.attendedAt).toLocaleString('vi-VN')}
              </p>
              {!attendanceRecord.isValid && attendanceRecord.invalidReason && (
                <p className="text-red-600 mt-2">
                  Lưu ý: {attendanceRecord.invalidReason}
                </p>
              )}
            </div>
          ) : isExpired ? (
            <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-6 text-center">
              <div className="text-yellow-600 text-4xl mb-2">⚠️</div>
              <h2 className="text-xl font-semibold text-yellow-900 mb-2">
                Phiên điểm danh đã kết thúc
              </h2>
              <p className="text-yellow-700">
                Bạn không thể điểm danh cho phiên này nữa
              </p>
            </div>
          ) : (
            <button
              onClick={handleCheckIn}
              disabled={checking}
              className="w-full py-3 px-4 bg-blue-600 text-white font-semibold rounded-lg hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors cursor-pointer"
            >
              {checking ? 'Đang điểm danh...' : 'Điểm danh ngay'}
            </button>
          )}

          <button
            onClick={() => navigate('/')}
            className="w-full mt-4 py-2 px-4 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors cursor-pointer"
          >
            Quay lại
          </button>
        </div>
      </div>
    </div>
  )
}
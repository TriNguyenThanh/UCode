namespace AssignmentService.Application.DTOs.Common;

    public static class EmailTemplates
    {
        /// <summary>
        /// Email: Thông báo lớp học mới
        /// </summary>
        public static string NewClass(
            string className,
            string teacherName,
            string startDate,
            string? classUrl = null)
        {
            return $@"
<div style=""font-family: Arial, sans-serif; max-width: 540px; margin:auto; padding: 24px; border:1px solid #eee; border-radius:10px;"">
    <h2 style=""text-align:center; color:#3b82f6; margin-bottom: 8px;"">
        Lớp học mới dành cho bạn!
    </h2>

    <p>Xin chào bạn,</p>

    <p>Bạn vừa được thêm vào lớp học:</p>

    <div style=""background:#f1f5f9; padding:14px 18px; border-radius:6px; margin:14px 0;"">
        <p><b>Tên lớp:</b> {className}</p>
        <p><b>Giảng viên:</b> {teacherName}</p>
        <p><b>Thời gian bắt đầu:</b> {startDate}</p>
    </div>

    <p>Bạn có thể truy cập lớp ngay tại liên kết bên dưới:</p>

    <div style=""text-align:center; margin: 20px 0;"">
        <a href=""{classUrl}"" 
           style=""background:#3b82f6; color:white; padding:12px 20px; text-decoration:none; border-radius:6px;"">
           Truy cập lớp học
        </a>
    </div>

    <p>Nếu có sai sót, vui lòng liên hệ với giảng viên <b>{teacherName}</b>.</p>

    <p style=""margin-top:24px;"">Trân trọng,<br><b>UCode Team</b></p>
</div>";
        }

        /// <summary>
        /// Email: Thông báo bài tập mới
        /// </summary>
        public static string NewAssignment(
            string assignmentTitle,
            string deadline,
            string description,
            string? assignmentUrl = null)
        {
            return $@"
<div style=""font-family:Arial, sans-serif; max-width:540px; margin:auto; padding:24px; border:1px solid #eee; border-radius:10px;"">
    <h2 style=""text-align:center; color:#16a34a; margin-bottom:8px;"">
        Bài tập mới được giao!
    </h2>

    <p>Xin chào bạn,</p>

    <p>Bạn vừa được giao bài tập mới:</p>

    <div style=""background:#ecfdf5; padding:14px 18px; border-radius:6px; margin:14px 0;"">
        <p><b>Tên bài tập:</b> {assignmentTitle}</p>
        <p><b>Hạn nộp:</b> {deadline}</p>
        <p><b>Mô tả:</b> {description}</p>
    </div>

    <div style=""text-align:center; margin: 20px 0;"">
        <a href=""{assignmentUrl}"" 
           style=""background:#16a34a; color:white; padding:12px 20px; text-decoration:none; border-radius:6px;"">
           Xem bài tập
        </a>
    </div>

    <p>Hãy hoàn thành bài tập trước thời hạn để tránh bị trễ hạn nhé!</p>

    <p style=""margin-top:24px;"">Chúc bạn học tốt!<br><b>UCode Team</b></p>
</div>";
        }

        /// <summary>
        /// Email: Thông báo có bài kiểm tra
        /// </summary>
        public static string NewExam(
            string studentName,
            string className,
            string examTitle,
            string examDate,
            string duration,
            string description,
            string examUrl)
        {
            return $@"
<div style=""font-family:Arial, sans-serif; max-width:540px; margin:auto; padding:24px; border:1px solid #eee; border-radius:10px;"">
    <h2 style=""text-align:center; color:#ef4444; margin-bottom:8px;"">
        Bài kiểm tra sắp diễn ra
    </h2>

    <p>Xin chào <b>{studentName}</b>,</p>

    <p>Bạn có bài kiểm tra mới trong lớp <b>{className}</b>:</p>

    <div style=""background:#fee2e2; padding:14px 18px; border-radius:6px; margin:14px 0;"">
        <p><b>Tiêu đề kiểm tra:</b> {examTitle}</p>
        <p><b>Ngày diễn ra:</b> {examDate}</p>
        <p><b>Thời lượng:</b> {duration}</p>
        <p><b>Mô tả:</b> {description}</p>
    </div>

    <div style=""text-align:center; margin: 20px 0;"">
        <a href=""{examUrl}"" 
           style=""background:#ef4444; color:white; padding:12px 20px; text-decoration:none; border-radius:6px;"">
           Xem bài kiểm tra
        </a>
    </div>

    <p>Hãy chuẩn bị đầy đủ và tham gia đúng giờ để đạt kết quả tốt nhất.</p>

    <p style=""margin-top:24px;"">Chúc bạn thành công!<br><b>UCode Team</b></p>
</div>";
        }

        /// <summary>
        /// Email: Tài khoản mới được tạo (dùng link kích hoạt / đặt mật khẩu)
        /// </summary>
        public static string NewAccountActivation(
            string fullName,
            string email,
            string role,
            string activationLink)
        {
            return $@"
<div style=""font-family: Arial, sans-serif; max-width: 540px; margin:auto; padding:24px; border:1px solid #eee; border-radius:10px;"">
    <h2 style=""text-align:center; color:#3b82f6; margin-bottom:8px;"">
        Chào mừng bạn đến với UCode!
    </h2>

    <p>Xin chào <b>{fullName}</b>,</p>

    <p>Tài khoản của bạn trên hệ thống <b>UCode</b> đã được tạo.</p>

    <div style=""background:#f1f5f9; padding:14px 18px; border-radius:6px; margin:14px 0;"">
        <p><b>Email đăng nhập:</b> {email}</p>
        <p><b>Vai trò:</b> {role}</p>
    </div>

    <p>Để bắt đầu sử dụng, vui lòng nhấn vào nút bên dưới để kích hoạt tài khoản và thiết lập mật khẩu:</p>

    <div style=""text-align:center; margin: 20px 0;"">
        <a href=""{activationLink}""
           style=""background:#3b82f6; color:white; padding:12px 20px; text-decoration:none; border-radius:6px;"">
            Kích hoạt tài khoản
        </a>
    </div>

    <p style=""font-size: 13px; color:#6b7280; margin-top:8px;"">
        Nếu bạn không tạo tài khoản trên UCode, vui lòng bỏ qua email này.
    </p>

    <p style=""margin-top:24px;"">Trân trọng,<br><b>UCode Team</b></p>
</div>";
        }

        /// <summary>
        /// Email: Tài khoản mới được tạo (gửi kèm mật khẩu tạm thời)
        /// </summary>
        public static string NewAccountWithTempPassword(
            string fullName,
            string email,
            string tempPassword,
            string role,
            string? loginUrl = null)
        {
        return $@"
<div style=""font-family: Arial, sans-serif; max-width: 540px; margin:auto; padding:24px; border:1px solid #eee; border-radius:10px;"">
    <h2 style=""text-align:center; color:#3b82f6; margin-bottom:8px;"">
        Tài khoản UCode của bạn đã sẵn sàng
    </h2>

    <p>Xin chào <b>{fullName}</b>,</p>

    <p>Tài khoản của bạn trên <b>UCode</b> đã được tạo với thông tin:</p>

    <div style=""background:#f1f5f9; padding:14px 18px; border-radius:6px; margin:14px 0;"">
        <p><b>Email đăng nhập:</b> {email}</p>
        <p><b>Mật khẩu tạm thời:</b> {tempPassword}</p>
        <p><b>Vai trò:</b> {role}</p>
    </div>

    <p>Vui lòng đăng nhập và đổi mật khẩu ngay sau lần đăng nhập đầu tiên để đảm bảo an toàn.</p>

    <p style=""font-size: 13px; color:#6b7280; margin-top:8px;"">
        Nếu bạn không yêu cầu tạo tài khoản này, hãy liên hệ với bộ phận hỗ trợ hoặc giảng viên phụ trách.
    </p>

    <p style=""margin-top:24px;"">Trân trọng,<br><b>UCode Team</b></p>
</div>";
    // <div style=""text-align:center; margin:20px 0;"">
    //     <a href=""{loginUrl}""
    //        style=""background:#3b82f6; color:white; padding:12px 20px; text-decoration:none; border-radius:6px;"">
    //         Đăng nhập UCode
    //     </a>
    // </div>
        }
    }

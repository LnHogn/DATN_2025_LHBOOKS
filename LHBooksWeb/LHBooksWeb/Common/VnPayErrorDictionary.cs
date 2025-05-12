namespace LHBooksWeb.Common
{
    public static class VnPayErrorDictionary
    {
        public static readonly Dictionary<string, string> ErrorMessages = new()
        {
            { "09", "Giao dịch không thành công: Thẻ/Tài khoản chưa đăng ký Internet Banking." },
            { "10", "Giao dịch không thành công: Xác thực không đúng quá 3 lần." },
            { "11", "Giao dịch không thành công: Hết hạn chờ thanh toán. Vui lòng thực hiện lại." },
            { "12", "Giao dịch không thành công: Thẻ/Tài khoản bị khóa." },
            { "13", "Giao dịch không thành công: Nhập sai OTP. Vui lòng thử lại." },
            { "24", "Giao dịch không thành công: Khách hàng đã hủy giao dịch." },
            { "51", "Giao dịch không thành công: Tài khoản không đủ số dư." },
            { "65", "Giao dịch không thành công: Vượt quá hạn mức giao dịch trong ngày." },
            { "75", "Giao dịch không thành công: Ngân hàng đang bảo trì." },
            { "79", "Giao dịch không thành công: Nhập sai mật khẩu thanh toán quá số lần quy định." }
        };
    }
}


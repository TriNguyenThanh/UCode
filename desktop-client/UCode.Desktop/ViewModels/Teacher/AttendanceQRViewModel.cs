using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using QRCoder;
using UCode.Desktop.Helpers;

namespace UCode.Desktop.ViewModels
{
    public class AttendanceQRViewModel : ViewModelBase
    {
        private string _sessionTitle = string.Empty;
        private string _attendanceUrl = string.Empty;
        private BitmapImage? _qrCodeImage;

        public string SessionTitle
        {
            get => _sessionTitle;
            set => SetProperty(ref _sessionTitle, value);
        }

        public string AttendanceUrl
        {
            get => _attendanceUrl;
            set => SetProperty(ref _attendanceUrl, value);
        }

        public BitmapImage? QRCodeImage
        {
            get => _qrCodeImage;
            set => SetProperty(ref _qrCodeImage, value);
        }

        public ICommand CopyUrlCommand { get; }

        public AttendanceQRViewModel(string sessionCode, string sessionTitle)
        {
            SessionTitle = sessionTitle;
            AttendanceUrl = $"https://ucode.edu.vn/attendance/{sessionCode}";

            CopyUrlCommand = new RelayCommand(_ => CopyUrl());

            GenerateQRCode();
        }

        private void GenerateQRCode()
        {
            try
            {
                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrCodeData = qrGenerator.CreateQrCode(AttendanceUrl, QRCodeGenerator.ECCLevel.Q);
                    using (var qrCode = new QRCode(qrCodeData))
                    {
                        var qrCodeBitmap = qrCode.GetGraphic(20);

                        // Convert System.Drawing.Bitmap to BitmapImage
                        using (var memory = new System.IO.MemoryStream())
                        {
                            qrCodeBitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                            memory.Position = 0;

                            var bitmapImage = new BitmapImage();
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = memory;
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.EndInit();
                            bitmapImage.Freeze();

                            QRCodeImage = bitmapImage;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error generating QR code: {ex.Message}");
            }
        }

        private void CopyUrl()
        {
            try
            {
                Clipboard.SetText(AttendanceUrl);
                // Show success message
                System.Windows.MessageBox.Show(
                    "Đã sao chép link vào clipboard!",
                    "Thành công",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Không thể sao chép: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}

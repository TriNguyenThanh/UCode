using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace UCode.Desktop.Services
{
    public class StoredTokenData
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public long ExpiresAt { get; set; }
        public string UserJson { get; set; } // Store entire user object as JSON
    }

    public class TokenStorageService
    {
        private readonly string _tokenFilePath;
        private const string EncryptionKey = "UCode-Desktop-2024-Secret-Key-32"; // 32 chars for AES-256

        public TokenStorageService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appDataPath, "UCode");
            Directory.CreateDirectory(appFolder);
            _tokenFilePath = Path.Combine(appFolder, "token.dat");
        }

        public void SaveToken(string accessToken, string refreshToken, long expiresAt, string userJson)
        {
            try
            {
                var tokenData = new StoredTokenData
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt,
                    UserJson = userJson
                };

                var json = JsonSerializer.Serialize(tokenData);
                var encrypted = Encrypt(json);
                File.WriteAllText(_tokenFilePath, encrypted);
                
                System.Diagnostics.Debug.WriteLine($"Token saved successfully to: {_tokenFilePath}");
                System.Diagnostics.Debug.WriteLine($"ExpiresAt: {expiresAt}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving token: {ex.Message}");
            }
        }

        public StoredTokenData LoadToken()
        {
            try
            {
                if (!File.Exists(_tokenFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Token file not found: {_tokenFilePath}");
                    return null;
                }

                var encrypted = File.ReadAllText(_tokenFilePath);
                var json = Decrypt(encrypted);
                var tokenData = JsonSerializer.Deserialize<StoredTokenData>(json);
                
                System.Diagnostics.Debug.WriteLine($"Token loaded successfully from: {_tokenFilePath}");
                System.Diagnostics.Debug.WriteLine($"ExpiresAt: {tokenData?.ExpiresAt}");
                
                return tokenData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading token: {ex.Message}");
                return null;
            }
        }

        public void ClearToken()
        {
            try
            {
                if (File.Exists(_tokenFilePath))
                    File.Delete(_tokenFilePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing token: {ex.Message}");
            }
        }

        public bool IsTokenValid(StoredTokenData tokenData)
        {
            if (tokenData == null || string.IsNullOrEmpty(tokenData.AccessToken))
                return false;

            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            // Handle both seconds and milliseconds timestamps
            var expiresAt = tokenData.ExpiresAt;
            if (expiresAt > 10000000000) // If timestamp is in milliseconds
            {
                expiresAt = expiresAt / 1000;
            }
            
            System.Diagnostics.Debug.WriteLine($"Token validation - Current: {currentTimestamp}, Expires: {expiresAt}, Valid: {expiresAt > currentTimestamp}");
            return expiresAt > currentTimestamp;
        }

        private string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
            aes.IV = new byte[16]; // Use zero IV for simplicity

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using var sw = new StreamWriter(cs);
            sw.Write(plainText);
            sw.Flush();
            cs.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray());
        }

        private string Decrypt(string cipherText)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
            aes.IV = new byte[16]; // Use zero IV for simplicity

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}

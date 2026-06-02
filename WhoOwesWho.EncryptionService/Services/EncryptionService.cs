using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using System.Text;
using WhoOwesWho.EncryptionService.Services.Base;
using WhoOwesWho.EncryptionService.Validators;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.EncryptionService.Services
{

    public interface IEncryptionService
    {
        Task<ProtectionResponseModel> Encrypt(string plainText);
        Task<ProtectionResponseModel> Decrypt(string cipherText);
        //Task<EncryptedCookiesResponseModel> EncryptCookies(CookiesRequestModel request);
    }

    public class EncryptionService(IConfiguration configuration)
        : ServiceBase(configuration), IEncryptionService
    {
        public async Task<ProtectionResponseModel> Encrypt(string plainText)
        {
            var iv = new byte[16];
            byte[] array;

            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(AppSettings.EncryptionKey);
                aes.IV = iv;

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using var memoryStream = new MemoryStream();
                await using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    await using (var streamWriter = new StreamWriter(cryptoStream))
                    {
                        await streamWriter.WriteAsync(plainText);
                    }

                    array = memoryStream.ToArray();
                }
            }

            return new ProtectionResponseModel
            {
                ProtectedValue = WebEncoders.Base64UrlEncode(array),
                Success = true
            };
        }

        public async Task<ProtectionResponseModel> Decrypt(string cipherText)
        {
            var iv = new byte[16];
            var buffer = WebEncoders.Base64UrlDecode(cipherText);
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(AppSettings.EncryptionKey);
            aes.IV = iv;
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream(buffer);
            await using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);
            var response = await streamReader.ReadToEndAsync();
            return new ProtectionResponseModel
            {
                UnprotectedValue = response,
                Success = true
            };
        }

        //public async Task<EncryptedCookiesResponseModel> EncryptCookies([FromBody] CookiesRequestModel request)
        //{

        //    var idResponse = await Encrypt(request.User?.Id.ToString()!);
        //    var emailResponse = await Encrypt(request.User?.EmailAddress!);
        //    var adminResponse = await Encrypt(request.User?.Admin.ToString()!);

        //    return new EncryptedCookiesResponseModel
        //    {
        //        UserIdValue = idResponse.ProtectedValue,
        //        UserEmailAddressValue = emailResponse.ProtectedValue,
        //        AdminValue = adminResponse.ProtectedValue,
        //        Success = true
        //    };
        //}
    }
}


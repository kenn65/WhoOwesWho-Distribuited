using Microsoft.AspNetCore.WebUtilities;
using System.Security.Cryptography;
using System.Text;
using WhoOwesWho.EncryptionService.Models;
using WhoOwesWho.EncryptionService.Services.Base;
using WhoOwesWho.Shared.Models;

namespace WhoOwesWho.EncryptionService.Services
{

    public interface IEncryptionService
    {
        Task<ProtectionResponseModel> Encrypt(string plainText);
        Task<ProtectionResponseModel> Decrypt(string cipherText);
        Task<EncryptedCookiesResponseModel> EncryptCookies(CookiesRequestModel request);

        Task<DecryptedCookiesModel> DecryptCookies(string userId, string userEmailAddress, string admin);
    }

    public class EncryptionService(IConfiguration configuration) : ServiceBase(configuration), IEncryptionService
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
           
            return await Task.FromResult(new ProtectionResponseModel
            {
                ProtectedValue = WebEncoders.Base64UrlEncode(array)
            });
        }

        public async Task<ProtectionResponseModel> Decrypt(string cipherText)
        {
            var iv = new byte[16];
            var buffer = WebEncoders.Base64UrlDecode(cipherText);
            //var buffer = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(AppSettings.EncryptionKey);
            aes.IV = iv;
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream(buffer);
            await using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);
            var result = await streamReader.ReadToEndAsync();
            return await Task.FromResult(new ProtectionResponseModel
            {
                UnprotectedValue = result
            });
        }

        public async Task<EncryptedCookiesResponseModel> EncryptCookies(CookiesRequestModel request)
        {
            var idResponse = await Encrypt(request.User?.Id.ToString()!);
            var emailResponse =  await Encrypt(request.User?.EmailAddress!);
            var adminResponse = await Encrypt(request.User?.Admin.ToString()!);
            
            return new EncryptedCookiesResponseModel
            {
                UserIdValue = idResponse.ProtectedValue,
                UserEmailAddressValue = emailResponse.ProtectedValue,
                AdminValue = adminResponse.ProtectedValue
            };
        }

        public async Task<DecryptedCookiesModel> DecryptCookies(string userId, string userEmailAddress, string admin)
        {
            return await Task.FromResult(new DecryptedCookiesModel
            {
                UserIdValue = Guid.Parse((await Decrypt(userId)).UnprotectedValue!),
                UserEmailAddressValue = (await Decrypt(userEmailAddress)).UnprotectedValue,
                AdminValue = bool.Parse((await Decrypt(admin)).UnprotectedValue!)
            });
        }
    }
}


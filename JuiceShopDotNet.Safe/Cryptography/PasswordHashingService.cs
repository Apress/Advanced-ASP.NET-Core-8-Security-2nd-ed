using JuiceShopDotNet.Common.Cryptography;
using JuiceShopDotNet.Safe.Data;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics.CodeAnalysis;

namespace JuiceShopDotNet.Safe.Cryptography.Hashing;

public class PasswordHashingService : BaseCryptographyProvider, IPasswordHasher<JuiceShopUser>
{
    //Following this recommendation: https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html
    private const int DEFAULT_ITERATIONS = 210000;
    private const PasswordHashingAlgorithm DEFAULT_HASHING_ALGORITHM = PasswordHashingAlgorithm.PBKDF2_SHA512;
    private const int DEFAULT_SALT_LENGTH = 32;

    public enum PasswordHashingAlgorithm
    {
        PBKDF2_SHA512 = 1
    }

    public string HashPassword(JuiceShopUser user, string password)
    {
        var salt = Randomizer.CreateRandomString(DEFAULT_SALT_LENGTH);

        if (DEFAULT_HASHING_ALGORITHM == PasswordHashingAlgorithm.PBKDF2_SHA512)
            return $"{GetPrefixWithDefaults()}{salt}{PBKDF2_SHA512(password, salt, DEFAULT_ITERATIONS)}";
        else
            throw new NotImplementedException($"Cannot find implementation of algorithm: {DEFAULT_HASHING_ALGORITHM}");
    }

    public PasswordVerificationResult VerifyHashedPassword(JuiceShopUser user, string hashedPassword, string providedPassword)
    {
        if (hashedPassword.StartsWith('['))
        {
            try
            {
                var hashInfo = hashedPassword.Substring(1, hashedPassword.IndexOf(']') - 1).Split(",");

                if (hashInfo.Length != 3)
                    throw new InvalidOperationException("Invalid number of parameters in the hash info prefix");

                var algorithm = int.Parse(hashInfo[0]);
                var saltLength = int.Parse(hashInfo[1]) * 2; //We're storing the string in hex format, so we have two characters for each byte
                var iterations = int.Parse(hashInfo[2]);
                var salt = hashedPassword.Substring(hashedPassword.IndexOf(']') + 1, saltLength);
                var cipherText = hashedPassword.Substring(hashedPassword.IndexOf(']') + saltLength + 1);

                if (algorithm == (int)PasswordHashingAlgorithm.PBKDF2_SHA512)
                {
                    var providedAsHash = PBKDF2_SHA512(providedPassword, salt, iterations);

                    if (cipherText != providedAsHash)
                        return PasswordVerificationResult.Failed;
                    else if (iterations != DEFAULT_ITERATIONS || saltLength / 2 != DEFAULT_SALT_LENGTH)
                        return PasswordVerificationResult.SuccessRehashNeeded;
                    else
                        return PasswordVerificationResult.Success;
                }
                else
                {
                    throw new NotImplementedException($"Cannot find implementation for hashing algorithm value {algorithm}");
                }
            }
            catch (Exception ex)
            {
                //TODO: Log this
                return PasswordVerificationResult.Failed;
            }
        }
        else
        {
            //We may be dealing with a legacy system, so use the default hasher
            var defaultHasher = new PasswordHasher<JuiceShopUser>();
            var result = defaultHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);

            if (result == PasswordVerificationResult.Success)
                return PasswordVerificationResult.SuccessRehashNeeded;
            else
                return result;
        }
    }

    public string GetPrefixWithDefaults()
    {
        return $"[{(int)DEFAULT_HASHING_ALGORITHM},{DEFAULT_SALT_LENGTH},{DEFAULT_ITERATIONS}]";
    }

    internal static string PBKDF2_SHA512(string plainText, string salt, int iterations)
    {
        byte[] saltAsBytes = HexStringToByteArray(salt);
        byte[] hashed = KeyDerivation.Pbkdf2(plainText, saltAsBytes, KeyDerivationPrf.HMACSHA512, iterations, 512 / 8);
        return ByteArrayToString(hashed);
    }
}

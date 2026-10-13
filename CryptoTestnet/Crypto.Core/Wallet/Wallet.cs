using System.Security.Cryptography;
using Crypto.Core.Blockchain;

namespace Crypto.Core.Wallet;
public class Wallet
{
    private readonly ECDsa _key;
    public string PrivateKey { get; }
    public string PublicKey { get; }
    public string Address { get; }
    public Wallet()
    {
        _key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        PrivateKey = Convert.ToHexString(
            _key.ExportECPrivateKey()
        ).ToLowerInvariant();
        PublicKey = Convert.ToHexString(
            _key.ExportSubjectPublicKeyInfo()
        ).ToLowerInvariant();
        Address = GenerateAddress(PublicKey);
    }
    public static string GenerateAddress(string publicKey)
    {
        string hash = HashHelper.CalculateHash(publicKey);
        return "0x" + hash[..40];
    }
    public string Sign(string data)
    {
        byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(data);
        byte[] signature = _key.SignData(
            dataBytes,
            HashAlgorithmName.SHA256
        );
        return Convert.ToHexString(signature).ToLowerInvariant();
    }
    public static bool Verify(
    string data,
    string signature,
    string publicKey)
    {
        using ECDsa publicKeyObject = ECDsa.Create();
        publicKeyObject.ImportSubjectPublicKeyInfo(
            Convert.FromHexString(publicKey),
            out _
        );
        byte[] dataBytes =
            System.Text.Encoding.UTF8.GetBytes(data);
        byte[] signatureBytes =
            Convert.FromHexString(signature);
        return publicKeyObject.VerifyData(
            dataBytes,
            signatureBytes,
            HashAlgorithmName.SHA256
        );
    }
}
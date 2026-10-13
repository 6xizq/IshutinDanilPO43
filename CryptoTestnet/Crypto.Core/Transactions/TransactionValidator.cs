using Crypto.Core.Blockchain;
using CryptoWallet = Crypto.Core.Wallet.Wallet;

namespace Crypto.Core.Transactions;
public static class TransactionValidator
{
    public static bool IsValid(
        Transaction transaction)
    {
        if (string.IsNullOrWhiteSpace(transaction.Signature))
            return false;
        if (string.IsNullOrWhiteSpace(transaction.PublicKey))
            return false;
        // Проверяем, что PublicKey действительно принадлежит адресу From
        string expectedAddress =
            CryptoWallet.GenerateAddress(transaction.PublicKey);
        if (!string.Equals(
                expectedAddress,
                transaction.From,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        // Проверяем цифровую подпись
        return CryptoWallet.Verify(
            transaction.GetDataToSign(),
            transaction.Signature,
            transaction.PublicKey
        );
    }
}
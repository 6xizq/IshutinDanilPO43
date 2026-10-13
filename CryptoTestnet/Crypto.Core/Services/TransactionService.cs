using Crypto.Core.Transactions;
using CryptoWallet = Crypto.Core.Wallet.Wallet;

namespace Crypto.Core.Services;
public class TransactionService
{
    public Transaction CreateTransaction(
        CryptoWallet wallet,
        string to,
        decimal amount,
        decimal fee,
        long nonce)
    {
        var transaction = new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            From = wallet.Address,
            PublicKey = wallet.PublicKey,
            To = to,
            Amount = amount,
            Fee = fee,
            Nonce = nonce,
            Timestamp = DateTime.UtcNow
        };
        string dataToSign = transaction.GetDataToSign();
        transaction.Signature = wallet.Sign(dataToSign);
        return transaction;
    }
}
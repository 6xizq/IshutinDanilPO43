namespace Crypto.Core.Transactions;

public class Transaction
{
    public string Id { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Fee { get; set; }
    public long Nonce { get; set; }
    public string Signature { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string GetDataToSign()
    {
        return
            $"{Id}" +
            $"{From}" +
            $"{To}" +
            $"{Amount}" +
            $"{Fee}" +
            $"{Nonce}" +
            $"{Timestamp:O}";
    }
    public override string ToString()
    {
        return $"{From} -> {To}: {Amount} TEST";
    }
}
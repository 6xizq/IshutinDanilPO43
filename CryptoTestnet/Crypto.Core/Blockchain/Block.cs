using Crypto.Core.Transactions;

namespace Crypto.Core.Blockchain;
public class Block
{
    public int Index { get; set; }
    public DateTime Timestamp { get; set; }
    public string PreviousHash { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public long Nonce { get; set; }
    public List<Transaction> Transactions { get; set; } = new();
    public override string ToString()
    {
        return $"Block #{Index} | Hash: {Hash}";
    }
}
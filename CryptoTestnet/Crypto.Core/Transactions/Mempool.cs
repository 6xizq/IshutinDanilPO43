namespace Crypto.Core.Transactions;

public class Mempool
{
    private readonly List<Transaction> _transactions = new();
    public IReadOnlyList<Transaction> Transactions =>
        _transactions;
    public bool AddTransaction(Transaction transaction)
    {
        if (!TransactionValidator.IsValid(transaction))
            return false;

        _transactions.Add(transaction);

        return true;
    }
    public bool RemoveTransaction(Transaction transaction)
    {
        return _transactions.Remove(transaction);
    }
    public void Clear()
    {
        _transactions.Clear();
    }
}
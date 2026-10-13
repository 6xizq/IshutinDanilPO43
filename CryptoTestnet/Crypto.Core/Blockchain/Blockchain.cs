using Crypto.Core.Transactions;

namespace Crypto.Core.Blockchain;
public class Blockchain
{
    public List<Block> Chain { get; } = new();
    public int Difficulty { get; } = 5;
    public Blockchain()
    {
        Chain.Add(CreateGenesisBlock());
    }
    private Block CreateGenesisBlock()
    {
        var block = new Block
        {
            Index = 0,
            Timestamp = DateTime.UtcNow,
            PreviousHash = "0",
            Nonce = 0
        };
        block.Transactions.Add(new Transaction
        {
            Id = "genesis",
            From = "SYSTEM",
            To = "NETWORK",
            Amount = 0,
            Fee = 0,
            Nonce = 0,
            Timestamp = DateTime.UtcNow
        });
        block.Hash = CalculateBlockHash(block);
        return block;
    }
    public Block GetLatestBlock()
    {
        return Chain[^1];
    }
    public Block AddBlock(List<Transaction> transactions)
    {
        foreach (var transaction in transactions)
        {
            if (!TransactionValidator.IsValid(transaction))
            {
                throw new InvalidOperationException(
                    $"Invalid transaction: {transaction.Id}");
            }
        }
        Block previousBlock = GetLatestBlock();
        var block = new Block
        {
            Index = previousBlock.Index + 1,
            Timestamp = DateTime.UtcNow,
            PreviousHash = previousBlock.Hash,
            Transactions = transactions
        };
        block.Hash = CalculateBlockHash(block);
        Chain.Add(block);
        return block;
    }
    public void RecalculateBlockHash(Block block)
    {
        block.Hash = CalculateBlockHash(block);
    }
    private string CalculateBlockHash(Block block)
    {
        string transactionData = string.Join(
            "|",
            block.Transactions.Select(t =>
                $"{t.Id}" +
                $"{t.From}" +
                $"{t.PublicKey}" +
                $"{t.To}" +
                $"{t.Amount}" +
                $"{t.Fee}" +
                $"{t.Nonce}" +
                $"{t.Signature}" +
                $"{t.Timestamp:O}"
            ));
        string data =
            $"{block.Index}" +
            $"{block.Timestamp:O}" +
            $"{block.PreviousHash}" +
            $"{block.Nonce}" +
            transactionData;
        return HashHelper.CalculateHash(data);
    }
    public Block MineBlock(List<Transaction> transactions)
    {
        foreach (var transaction in transactions)
        {
            if (!TransactionValidator.IsValid(transaction))
            {
                throw new InvalidOperationException(
                    $"Invalid transaction: {transaction.Id}");
            }
        }
        Block previousBlock = GetLatestBlock();
        var block = new Block
        {
            Index = previousBlock.Index + 1,
            Timestamp = DateTime.UtcNow,
            PreviousHash = previousBlock.Hash,
            Transactions = transactions,
            Nonce = 0
        };
        string target = new string('0', Difficulty);
        while (true)
        {
            block.Hash = CalculateBlockHash(block);

            if (block.Hash.StartsWith(target))
                break;

            block.Nonce++;
        }
        Chain.Add(block);
        return block;
    }
    public bool IsValid()
    {
        for (int i = 1; i < Chain.Count; i++)
        {
            Block current = Chain[i];
            Block previous = Chain[i - 1];
            if (current.PreviousHash != previous.Hash)
                return false;
            string recalculatedHash = CalculateBlockHash(current);
            if (current.Hash != recalculatedHash)
                return false;
            if (!current.Hash.StartsWith(
                    new string('0', Difficulty)))
                return false;
        }
        return true;
    }
}
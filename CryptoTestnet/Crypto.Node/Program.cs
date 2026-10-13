using Crypto.Core.Blockchain;
using Crypto.Core.Services;
using Crypto.Core.Transactions;
using CryptoWallet = Crypto.Core.Wallet.Wallet;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var blockchain = new Blockchain();
var mempool = new Mempool();
var transactionService = new TransactionService();

// ================================
// ROOT
// ================================

app.MapGet("/", () =>
{
    return "Crypto Testnet Node is running!";
});

// ================================
// STATUS
// ================================

app.MapGet("/status", () =>
{
    return new
    {
        status = "online",
        node = "Crypto.Node",
        blocks = blockchain.Chain.Count,
        mempool = mempool.Transactions.Count
    };
});

// ================================
// BLOCKCHAIN
// ================================

app.MapGet("/blockchain", () =>
{
    return blockchain.Chain;
});

// ================================
// LATEST BLOCK
// ================================

app.MapGet("/blocks/latest", () =>
{
    return blockchain.GetLatestBlock();
});

// ================================
// MEMPOOL
// ================================

app.MapGet("/mempool", () =>
{
    return mempool.Transactions;
});

// ================================
// CREATE TEST WALLET
// ================================

app.MapGet("/wallet/create", () =>
{
    var wallet = new CryptoWallet();

    return new
    {
        address = wallet.Address,
        publicKey = wallet.PublicKey
    };
});

// ================================
// ADD TRANSACTION
// ================================

app.MapPost("/transactions", (Transaction transaction) =>
{
    bool added = mempool.AddTransaction(transaction);

    if (!added)
    {
        return Results.BadRequest(new
        {
            success = false,
            message = "Invalid transaction"
        });
    }

    return Results.Ok(new
    {
        success = true,
        message = "Transaction added to mempool",
        transaction
    });
});

// ================================
// MINING
// ================================

app.MapPost("/mine", () =>
{
    if (mempool.Transactions.Count == 0)
    {
        return Results.BadRequest(new
        {
            success = false,
            message = "Mempool is empty"
        });
    }

    var transactions =
        mempool.Transactions.ToList();

    var block = blockchain.MineBlock(transactions);

    foreach (var transaction in transactions)
    {
        mempool.RemoveTransaction(transaction);
    }

    return Results.Ok(new
    {
        success = true,
        message = "Block mined successfully",
        block
    });
});

// ================================
// BLOCKCHAIN VALIDATION
// ================================

app.MapGet("/blockchain/valid", () =>
{
    return new
    {
        valid = blockchain.IsValid()
    };
});

app.Run();
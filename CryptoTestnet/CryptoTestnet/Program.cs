using Crypto.Core.Blockchain;
using Crypto.Core.Services;
using Crypto.Core.Transactions;
using CryptoWallet = Crypto.Core.Wallet.Wallet;

Console.WriteLine("=================================");
Console.WriteLine("       CRYPTO TESTNET");
Console.WriteLine("=================================");

// ================================
// CREATE WALLETS
// ================================

var alice = new CryptoWallet();
var bob = new CryptoWallet();

var transactionService = new TransactionService();
var blockchain = new Blockchain();
var mempool = new Mempool();

// ================================
// ALICE
// ================================

Console.WriteLine();
Console.WriteLine("ALICE");
Console.WriteLine("---------------------------------");
Console.WriteLine($"Address: {alice.Address}");

// ================================
// BOB
// ================================

Console.WriteLine();
Console.WriteLine("BOB");
Console.WriteLine("---------------------------------");
Console.WriteLine($"Address: {bob.Address}");

// ================================
// CREATE TRANSACTION
// ================================

Console.WriteLine();
Console.WriteLine("CREATE TRANSACTION");
Console.WriteLine("---------------------------------");

var transaction = transactionService.CreateTransaction(
    alice,
    bob.Address,
    10,
    0.01m,
    1
);

Console.WriteLine($"From:   {transaction.From}");
Console.WriteLine($"To:     {transaction.To}");
Console.WriteLine($"Amount: {transaction.Amount} TEST");
Console.WriteLine($"Valid:  {TransactionValidator.IsValid(transaction)}");

// ================================
// ADD TO MEMPOOL
// ================================

Console.WriteLine();
Console.WriteLine("ADD TRANSACTION TO MEMPOOL");
Console.WriteLine("---------------------------------");

bool addedToMempool =
    mempool.AddTransaction(transaction);

Console.WriteLine($"Added: {addedToMempool}");

Console.WriteLine(
    $"Mempool transactions: {mempool.Transactions.Count}");

// ================================
// MINING
// ================================

Console.WriteLine();
Console.WriteLine("MINING BLOCK");
Console.WriteLine("---------------------------------");

var miningStart = DateTime.UtcNow;

var block = blockchain.MineBlock(
    mempool.Transactions.ToList()
);

var miningTime = DateTime.UtcNow - miningStart;

// ================================
// REMOVE MINED TRANSACTIONS
// FROM MEMPOOL
// ================================

foreach (var minedTransaction in block.Transactions)
{
    mempool.RemoveTransaction(minedTransaction);
}

Console.WriteLine(
    $"Mempool after mining: {mempool.Transactions.Count}");

// ================================
// MINING RESULT
// ================================

Console.WriteLine();
Console.WriteLine("MINING RESULT");
Console.WriteLine("---------------------------------");

Console.WriteLine($"Block mined: # {block.Index}");
Console.WriteLine($"Nonce:        {block.Nonce}");
Console.WriteLine($"Hash:         {block.Hash}");
Console.WriteLine($"Difficulty:   {blockchain.Difficulty}");
Console.WriteLine($"Mining time:  {miningTime.TotalMilliseconds:F0} ms");

Console.WriteLine(
    $"Blockchain valid: {blockchain.IsValid()}");

// ================================
// BLOCK INFO
// ================================

Console.WriteLine();
Console.WriteLine("BLOCK INFO");
Console.WriteLine("---------------------------------");

Console.WriteLine($"Block added: #{block.Index}");
Console.WriteLine($"Block hash:  {block.Hash}");

Console.WriteLine(
    $"Blockchain valid: {blockchain.IsValid()}");

// ================================
// TRANSACTION TAMPERING
// ================================

Console.WriteLine();
Console.WriteLine("TAMPERING TRANSACTION");
Console.WriteLine("---------------------------------");

transaction.Amount = 1000;

Console.WriteLine(
    $"New amount: {transaction.Amount} TEST");

Console.WriteLine(
    $"Valid after tampering: " +
    $"{TransactionValidator.IsValid(transaction)}");

// ================================
// TRY TO ADD TAMPERED TRANSACTION
// ================================

Console.WriteLine();
Console.WriteLine("TRY TO ADD TAMPERED TRANSACTION");
Console.WriteLine("---------------------------------");

try
{
    blockchain.AddBlock(
        new List<Transaction>
        {
            transaction
        }
    );

    Console.WriteLine("ERROR: Block was added!");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine("BLOCK REJECTED");
    Console.WriteLine($"Reason: {ex.Message}");
}

// ================================
// TEST FINISHED
// ================================

Console.WriteLine();
Console.WriteLine("=================================");
Console.WriteLine("       TEST FINISHED");
Console.WriteLine("=================================");

// ================================
// BLOCK TAMPERING TEST
// ================================

Console.WriteLine();
Console.WriteLine("TAMPER BLOCK DATA");
Console.WriteLine("---------------------------------");

// Сохраняем оригинальную подпись
string originalSignature =
    blockchain.Chain[1].Transactions[0].Signature;

// Ломаем подпись
blockchain.Chain[1].Transactions[0].Signature =
    "FAKE_SIGNATURE";

Console.WriteLine(
    $"Blockchain valid after tampering: " +
    $"{blockchain.IsValid()}");

// Восстанавливаем оригинальную подпись
blockchain.Chain[1].Transactions[0].Signature =
    originalSignature;

Console.WriteLine(
    "Original data restored");

// Перемайниваем блок,
// чтобы снова выполнить Proof-of-Work
var restoredBlock = blockchain.Chain[1];

restoredBlock.Nonce = 0;

while (true)
{
    blockchain.RecalculateBlockHash(restoredBlock);

    if (restoredBlock.Hash.StartsWith(
            new string('0', blockchain.Difficulty)))
    {
        break;
    }

    restoredBlock.Nonce++;
}

Console.WriteLine(
    $"Restored block hash: {restoredBlock.Hash}");

Console.WriteLine(
    $"Restored nonce: {restoredBlock.Nonce}");

Console.WriteLine(
    $"Blockchain valid after restoration: " +
    $"{blockchain.IsValid()}");
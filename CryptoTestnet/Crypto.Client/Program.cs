using Crypto.Core.Services;
using System.Net.Http.Json;
using CryptoWallet = Crypto.Core.Wallet.Wallet;

Console.WriteLine("=================================");
Console.WriteLine("       CRYPTO TESTNET CLIENT");
Console.WriteLine("=================================");

// ================================
// CREATE WALLETS
// ================================

var alice = new CryptoWallet();
var bob = new CryptoWallet();

Console.WriteLine();
Console.WriteLine("ALICE");
Console.WriteLine("---------------------------------");
Console.WriteLine($"Address: {alice.Address}");

Console.WriteLine();
Console.WriteLine("BOB");
Console.WriteLine("---------------------------------");
Console.WriteLine($"Address: {bob.Address}");

// ================================
// CREATE TRANSACTION
// ================================

var transactionService = new TransactionService();

var transaction = transactionService.CreateTransaction(
    alice,
    bob.Address,
    10,
    0.01m,
    1
);

using var httpClient = new HttpClient();

string nodeUrl = "http://localhost:5000";

Console.WriteLine();
Console.WriteLine("TRANSACTION");
Console.WriteLine("---------------------------------");
Console.WriteLine($"From:      {transaction.From}");
Console.WriteLine($"To:        {transaction.To}");
Console.WriteLine($"Amount:    {transaction.Amount} TEST");
Console.WriteLine($"Fee:       {transaction.Fee} TEST");
Console.WriteLine($"Nonce:     {transaction.Nonce}");
Console.WriteLine($"Signature: {transaction.Signature[..20]}...");

// ================================
// SEND TRANSACTION TO NODE
// ================================

Console.WriteLine();
Console.WriteLine("SEND TRANSACTION TO NODE");
Console.WriteLine("---------------------------------");

var response = await httpClient.PostAsJsonAsync(
    $"{nodeUrl}/transactions",
    transaction
);

string responseBody =
    await response.Content.ReadAsStringAsync();

Console.WriteLine($"HTTP Status: {(int)response.StatusCode}");
Console.WriteLine($"Response: {responseBody}");
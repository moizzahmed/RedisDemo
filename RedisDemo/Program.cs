using RedisDemo.Model;
using RedisDemo.Seeder;
using RedisDemo.Service;
using RedisDemo.Service.Implementation;

namespace RedisDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var repository = new MockTransactionRepository();
            var redisService = new RedisTransactionService(repository);

            // Seed the dummy transactions (calling the seeder)
            TransactionSeeder.SeedTransactions(redisService, 10);

            // Retrieve and display some transactions by status
            Console.WriteLine("\n--- Retrieving 'Completed' Transactions ---");
            var completedTransactions = redisService.SearchTransactionsByStatus(TransactionStatus.Failed);
            foreach (var transaction in completedTransactions)
            {
                Console.WriteLine($"Transaction ID: {transaction.TransactionId}, Amount: {transaction.Amount}, Status: {transaction.Status}, Timestamp: {transaction.Timestamp}");
            }

            // Retrieve a specific transaction by ID
            Console.WriteLine("\n--- Retrieving Specific Transaction ---");
            string specificTransactionId = "transaction_5";
            var specificTransaction = redisService.GetTransactionById(specificTransactionId);
            if (specificTransaction != null)
            {
                Console.WriteLine($"Transaction ID: {specificTransaction.TransactionId}, Amount: {specificTransaction.Amount}, Status: {specificTransaction.Status}, Timestamp: {specificTransaction.Timestamp}");
            }
            else
            {
                Console.WriteLine("Transaction not found.");
            }
        }
    }
}

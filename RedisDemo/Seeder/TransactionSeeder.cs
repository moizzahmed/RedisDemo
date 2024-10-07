using RedisDemo.Model;
using RedisDemo.Service;
using System;
using System.Collections.Generic;

namespace RedisDemo.Seeder
{
    public class TransactionSeeder
    {
        private static readonly List<TransactionStatus> statuses = new List<TransactionStatus>
        {
            TransactionStatus.Pending,
            TransactionStatus.Completed,
            TransactionStatus.Failed,
            TransactionStatus.Cancelled
        };

        // Method to generate and save multiple dummy transactions
        public static void SeedTransactions(RedisTransactionService redisService, int count = 10)
        {
            for (int i = 1; i <= count; i++)
            {
                var transaction = new Transaction
                {
                    TransactionId = $"transaction_{i}",
                    Amount = new Random().Next(100, 1000), // Random amount between 100 and 1000
                    Status = statuses[i % 4], // Cycle through the statuses
                    Timestamp = DateTime.Now.AddMinutes(-i * 10) // Varying timestamps
                };

                // Save transaction to Redis and mock DB
                redisService.SaveTransactionToRedis(transaction);

                Console.WriteLine($"Saved Transaction: ID = {transaction.TransactionId}, Status = {transaction.Status}");
            }
        }
    }
}

using RedisDemo.Model;
using RedisDemo.Service.Interface;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RedisDemo.Service
{
    public class RedisTransactionService
    {
        private readonly ITransactionRepository _repository;
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisTransactionService(ITransactionRepository repository)
        {
            _repository = repository;
            _redis = ConnectionMultiplexer.Connect("localhost");
            _db = _redis.GetDatabase();
        }

        // Save a transaction to Redis (both as a hash and in sorted sets by timestamp and status)
        public void SaveTransactionToRedis(Transaction transaction)
        {
            string transactionKey = $"transaction:{transaction.TransactionId}";

            // Store the transaction data as a hash in Redis
            _db.HashSet(transactionKey, new HashEntry[]
            {
                new HashEntry("TransactionId", transaction.TransactionId),
                new HashEntry("Amount", transaction.Amount),
                new HashEntry("Status", transaction.Status.ToString()),
                new HashEntry("Timestamp", transaction.Timestamp.ToString())
            });

            // Store transaction ID in sorted sets by timestamp and status for optimized queries
            _db.SortedSetAdd("transactions_by_time", transactionKey, transaction.Timestamp.ToOADate());
            _db.SetAdd($"transactions_by_status:{transaction.Status}", transactionKey);

            // Also save it to the mock database (fallback mechanism)
            _repository.SaveTransaction(transaction);
        }

        // Retrieve a transaction by its ID from Redis
        public Transaction GetTransactionById(string transactionId)
        {
            string transactionKey = $"transaction:{transactionId}";
            HashEntry[] hashEntries = _db.HashGetAll(transactionKey);

            // If the transaction exists in Redis, parse it from the hash
            if (hashEntries.Length > 0)
            {
                return new Transaction
                {
                    TransactionId = hashEntries.FirstOrDefault(h => h.Name == "TransactionId").Value,
                    Amount = (double)hashEntries.FirstOrDefault(h => h.Name == "Amount").Value,
                    Status = Enum.Parse<TransactionStatus>(hashEntries.FirstOrDefault(h => h.Name == "Status").Value),
                    Timestamp = DateTime.Parse(hashEntries.FirstOrDefault(h => h.Name == "Timestamp").Value)
                };
            }
            else
            {
                // If not found in Redis, fall back to the mock database
                return _repository.GetTransactionById(transactionId);
            }
        }

        // Delete a transaction from Redis
        public void DeleteTransaction(string transactionId)
        {
            string transactionKey = $"transaction:{transactionId}";

            // Get the transaction's status before deletion
            var status = Enum.Parse<TransactionStatus>(_db.HashGet(transactionKey, "Status"));

            // Delete the transaction from Redis
            _db.KeyDelete(transactionKey);

            // Remove the transaction from sorted sets and status sets
            _db.SortedSetRemove("transactions_by_time", transactionKey);
            _db.SetRemove($"transactions_by_status:{status}", transactionKey);
        }

        // Retrieve transactions by status using Redis Sets
        public IEnumerable<Transaction> SearchTransactionsByStatus(TransactionStatus status)
        {
            // Get all transaction keys that match the given status
            var transactionKeys = _db.SetMembers($"transactions_by_status:{status}");

            var transactions = new List<Transaction>();

            foreach (var redisKey in transactionKeys)
            {
                // Convert RedisValue to string and extract the actual transaction key
                string key = redisKey.ToString();

                // If the key is in the format "{transaction:transaction_9}", extract the actual key
                if (key.StartsWith("transaction:"))
                {
                    key = key.Trim().Split(':')[1]; // Extract transaction_9
                }

                // Retrieve the transaction by the actual key
                var transaction = GetTransactionById(key);
                if (transaction != null)
                {
                    transactions.Add(transaction);
                }
            }

            return transactions;
        }

        // Retrieve transactions by timestamp range using Redis Sorted Sets
        public IEnumerable<Transaction> SearchTransactionsByTimestampRange(DateTime start, DateTime end)
        {
            // Query sorted set by timestamp range
            var transactionKeys = _db.SortedSetRangeByScore("transactions_by_time", start.ToOADate(), end.ToOADate());

            var transactions = new List<Transaction>();

            foreach (var key in transactionKeys)
            {
                // Retrieve each transaction by ID
                var transaction = GetTransactionById(key);
                if (transaction != null)
                {
                    transactions.Add(transaction);
                }
            }

            return transactions;
        }
    }
}

using RedisDemo.Model;
using RedisDemo.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisDemo.Service.Implementation
{
    public class MockTransactionRepository : ITransactionRepository
    {
        private readonly Dictionary<string, Transaction> _mockDb = new();

        public Transaction GetTransactionById(string transactionId)
        {
            return _mockDb.ContainsKey(transactionId) ? _mockDb[transactionId] : null;
        }

        public void SaveTransaction(Transaction transaction)
        {
            _mockDb[transaction.TransactionId] = transaction;
        }
    }
}

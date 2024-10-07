using RedisDemo.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisDemo.Service.Interface
{
    public interface ITransactionRepository
    {
        Transaction GetTransactionById(string transactionId);
        void SaveTransaction(Transaction transaction);
    }
}

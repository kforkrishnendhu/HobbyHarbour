using System;
using HobbyHarbour.Models;

namespace HobbyHarbour.Service
{
    public interface IWalletService
    {
        Task<bool> CreditWalletAsync(string userId, decimal amount, Method method);
        Task<bool> DebitWalletAsync(string userId, decimal amount);
    }
}


using System;
using HobbyHarbour.Data;
using HobbyHarbour.Models;
using Microsoft.EntityFrameworkCore;

namespace HobbyHarbour.Service
{
    public class WalletService : IWalletService
    {
        private readonly ApplicationDbContext _db; // Assume ApplicationDbContext for data access

        public WalletService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> CreditWalletAsync(string userId, decimal amount, Method method)
        {
            try
            {
                var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserID == userId);

                if (wallet == null)
                {
                    // Wallet doesn't exist for the user, create a new one
                    wallet = new Wallet
                    {
                        UserID = userId,
                        Balance = 0 
                    };

                    _db.Wallets.Add(wallet);
                }

                wallet.Balance += amount;

                var wallethistory = new WalletHistory
                {
                    WalletID = wallet.WalletID,
                    Date = DateTime.Now, 
                    Amount = amount,
                    Method = method
                };

                // Add the new WalletHistory to the DbSet and save changes
                _db.WalletHistories.Add(wallethistory);

                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Handle exceptions (logging, etc.)
                return false;
            }
        }

        public async Task<bool> DebitWalletAsync(string userId, decimal amount)
        {
            try
            {
                var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserID == userId);

                if (wallet != null && wallet.Balance >= amount)
                {
                    wallet.Balance -= amount;
                    await _db.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                // Handle exceptions (logging, etc.)
                return false;
            }
        }
    }
}


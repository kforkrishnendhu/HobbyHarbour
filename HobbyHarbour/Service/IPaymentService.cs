using System;
using System.Threading.Tasks;
using HobbyHarbour.Models;
using Microsoft.AspNetCore.Http;
namespace HobbyHarbour.Service
{
    public interface IPaymentService
    {
        Task<MerchantOrder> ProcessMerchantOrder(Order payRequest);
        Task<string> CompleteOrderProcess(IHttpContextAccessor _httpContextAccessor);
    }
}


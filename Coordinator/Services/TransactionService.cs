using Coordinator.Contexts;
using Coordinator.Entities;
using Coordinator.Enums;
using Coordinator.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Coordinator.Services
{
    public class TransactionService(TwoPhaseCommitContext _context, IHttpClientFactory _httpClientFactory) : ITransactionService
    {

        HttpClient _orderHttpClient = _httpClientFactory.CreateClient("OrderAPI");
        HttpClient _paymentHttpClient = _httpClientFactory.CreateClient("PaymentAPI");
        HttpClient _stockHttpClient = _httpClientFactory.CreateClient("StockAPI");

        public async Task<Guid> CreateTransactionAsync()
        {
            Guid transactionId = Guid.NewGuid();

            var nodes = await _context.Nodes.ToListAsync();
            nodes.ForEach(node => node.NodeStates = new List<NodeState>()
            {
                new(transactionId)
                {
                    IsReady = ReadyType.Pending,
                    TransactionState = TransactionState.Pending
                }
            });

            await _context.SaveChangesAsync();
            return transactionId;
        }

        public async Task PrepareServicesAsync(Guid transactionId)
        {
            var transactionNodes = _context.NodeStates
                .Include(ns => ns.Node)
                .Where(ns => ns.TransactionId == transactionId)
                .ToList();

            foreach (var transactionNode in transactionNodes)
            {
                try
                {
                    var response = await (transactionNode.Node.Name switch
                    {
                        "Order.API" => _orderHttpClient.GetAsync("ready"),
                        "Payment.API" => _paymentHttpClient.GetAsync("ready"),
                        "Stock.API" => _stockHttpClient.GetAsync("ready")
                    });

                    bool result = bool.Parse(await response.Content.ReadAsStringAsync());
                    transactionNode.IsReady = result ? ReadyType.Ready : ReadyType.Unready;
                }
                catch
                {
                    transactionNode.IsReady = ReadyType.Unready;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> CheckReadyServicesAsync(Guid transactionId)
            => (await _context.NodeStates
                    .Where(ns => ns.TransactionId == transactionId)
                    .ToListAsync()).TrueForAll(ns => ns.IsReady == ReadyType.Ready);

        public async Task CommitAsync(Guid transactionId)
        {
            var transactionNodes = await _context.NodeStates
                .Include(ns => ns.Node)
                .Where(ns => ns.TransactionId == transactionId)
                .ToListAsync();

            foreach (var transactionNode in transactionNodes)
            {
                try
                {
                    var response = await (transactionNode.Node.Name switch
                    {
                        "Order.API" => _orderHttpClient.GetAsync("commit"),
                        "Payment.API" => _paymentHttpClient.GetAsync("commit"),
                        "Stock.API" => _stockHttpClient.GetAsync("commit")
                    });

                    bool result = bool.Parse(await response.Content.ReadAsStringAsync());
                    transactionNode.TransactionState = result ? TransactionState.Done : TransactionState.Abort;
                }
                catch
                {
                    transactionNode.TransactionState = TransactionState.Abort;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> CheckTransactionStateServicesAsync(Guid transactionId)
            => (await _context.NodeStates
                    .Where(ns => ns.TransactionId == transactionId)
                    .ToListAsync()).TrueForAll(ns => ns.TransactionState == TransactionState.Done);

        public async Task RollbackAsync(Guid transactionId)
        {
            var transactionNodes = await _context.NodeStates
                .Where(ns => ns.TransactionId == transactionId)
                .Include(ns => ns.Node)
                .ToListAsync();

            foreach (var transactionNode in transactionNodes)
            {
                try
                {
                    if(transactionNode.TransactionState == TransactionState.Done)
                        _ = await (transactionNode.Node.Name switch
                        {
                            "Order.API" => _orderHttpClient.GetAsync("rollback"),
                            "Payment.API" => _paymentHttpClient.GetAsync("rollback"),
                            "Stock.API" => _stockHttpClient.GetAsync("rollback")
                        });

                    transactionNode.TransactionState = TransactionState.Abort;
                }
                catch
                {
                    transactionNode.TransactionState = TransactionState.Abort;
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}

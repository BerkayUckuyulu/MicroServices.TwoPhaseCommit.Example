using _2PC.Coordinator.Contexts;
using _2PC.Coordinator.Entities;
using _2PC.Coordinator.Enums;
using Microsoft.EntityFrameworkCore;

namespace _2PC.Coordinator.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly CoordinatorDbContext _dbContext;
        private readonly Dictionary<string, HttpClient> _httpClients = new();

        public TransactionService(CoordinatorDbContext dbContext, IHttpClientFactory httpClientFactory)
        {
            _dbContext = dbContext;
            dbContext.Nodes.ToList().ForEach(x =>
            {
                var client = httpClientFactory.CreateClient(x.Name);
                client.BaseAddress = new(x.Url);
                _httpClients.Add(x.Name, client);
            });
        }

        public async Task<Guid> CreateTransactionAsync(List<int>? nodeList)
        {
            Guid transactionId = Guid.NewGuid();

            var nodes = nodeList is null ? (await _dbContext.Nodes.ToListAsync()) : (await _dbContext.Nodes.ToListAsync()).Where(x => nodeList.Contains(x.Id)).ToList();

            List<NodeState> nodeStates = new();

            foreach (var node in nodes)
            {
                nodeStates.Add(new()
                {
                    TransactionId = transactionId,
                    Node = node,
                    PreparationState = PreparationStateTypes.Pending
                });
            }

            await _dbContext.NodeStates.AddRangeAsync(nodeStates);
            await _dbContext.SaveChangesAsync();

            return transactionId;

        }

        public async Task SendControlRequestAsync(Guid transactionId)
        {
            var nodeStates = await _dbContext.NodeStates.Where(x => x.TransactionId == transactionId).Include(x => x.Node).ToListAsync();


            await Parallel.ForEachAsync(nodeStates, async (nodeState, cancellationToken) =>
            {
                try
                {
                    var response = await _httpClients[nodeState.Node.Name].GetAsync("control");
                    var result = bool.Parse(await response.Content.ReadAsStringAsync());
                    nodeState.PreparationState = result ? PreparationStateTypes.Ready : PreparationStateTypes.UnReady;
                }
                catch
                {
                    nodeState.PreparationState = PreparationStateTypes.UnReady;
                }
            });

            await _dbContext.SaveChangesAsync();

        }

        public async Task<bool> CheckServicesControlStatusAsync(Guid transactionId)
        {
            return await _dbContext.NodeStates.Where(x => x.TransactionId == transactionId).AllAsync(x => x.PreparationState == PreparationStateTypes.Ready);
        }

        public async Task CommitAsync(Guid transactionId)
        {
            var nodeStates = await _dbContext.NodeStates.Where(x => x.TransactionId == transactionId).Include(x => x.Node).ToListAsync();


            await Parallel.ForEachAsync(nodeStates, async (nodeState, cancellationToken) =>
            {
                try
                {
                    var response = await _httpClients[nodeState.Node.Name].GetAsync("commit");
                    var result = bool.Parse(await response.Content.ReadAsStringAsync());
                    nodeState.TransactionState = result ? TransactionStateTypes.Done : TransactionStateTypes.Failed;
                }
                catch
                {
                    nodeState.TransactionState = TransactionStateTypes.Failed;
                }
            });

            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> CheckServicesTransactionStatusAsync(Guid transactionId)
        {
            return await _dbContext.NodeStates.Where(x => x.TransactionId == transactionId).AllAsync(x => x.TransactionState == TransactionStateTypes.Done);
        }

        public async Task RollBackAsync(Guid transactionId)
        {
            var nodeStates = await _dbContext.NodeStates.Where(x => x.TransactionId == transactionId && x.TransactionState == TransactionStateTypes.Done).Include(x => x.Node).ToListAsync();

            await Parallel.ForEachAsync(nodeStates, async (nodeState, cancellationToken) =>
            {
                try
                {
                    var response = await _httpClients[nodeState.Node.Name].GetAsync("rollback");
                    var result = bool.Parse(await response.Content.ReadAsStringAsync());
                    nodeState.TransactionState = result ? TransactionStateTypes.Abort : TransactionStateTypes.Abort_Failed;
                }
                catch
                {
                    nodeState.TransactionState = TransactionStateTypes.Abort_Failed;
                }
            });

            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> CheckServicesRollBackStatusAsync(Guid transactionId)
        {
            return await _dbContext.NodeStates.Where(x => x.TransactionId == transactionId).AllAsync(x => x.TransactionState == TransactionStateTypes.Abort);
        }
    }
}


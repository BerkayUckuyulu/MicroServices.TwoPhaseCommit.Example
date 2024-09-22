namespace _2PC.Coordinator.Services
{
    public interface ITransactionService
    {
        Task<Guid> CreateTransactionAsync(List<int>? nodeList);

        //birinci aşama
        Task SendControlRequestAsync(Guid transactionId);
        Task<bool> CheckServicesControlStatusAsync(Guid transactionId);

        //ikinci aşama
        Task CommitAsync(Guid transactionId);
        Task<bool> CheckServicesTransactionStatusAsync(Guid transactionId);

        //commit başarısızsa
        Task RollBackAsync(Guid transactionId);
        Task<bool> CheckServicesRollBackStatusAsync(Guid transactionId);
    }
}


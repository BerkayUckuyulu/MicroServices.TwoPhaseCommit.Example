using _2PC.Coordinator.Enums;

namespace _2PC.Coordinator.Entities
{
    public class NodeState
    {
        public int Id { get; set; }
        public Guid TransactionId { get; set; }

        /// <summary>
        /// servisin, node'un kontrol aşaması neticesinde hazır olup olmadığının bilgisi.
        /// </summary>
        public PreparationStateTypes PreparationState { get; set; }

        /// <summary>
        /// işlemin serviste işlenme durumunun bilgisi.
        /// </summary>
        public TransactionStateTypes? TransactionState { get; set; }

        public Node Node { get; set; }
    }
}


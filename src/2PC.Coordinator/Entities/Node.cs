namespace _2PC.Coordinator.Entities
{
    public class Node
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public ICollection<NodeState> NodeStates { get; set; }
    }
}
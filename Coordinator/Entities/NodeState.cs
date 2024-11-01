using Coordinator.Enums;

namespace Coordinator.Entities
{
    public record NodeState(Guid TransactionId)
    {
        public Guid Id { get; set; }
        /// <summary>
        /// 1. aşamanın durumunu ifade eder.
        /// </summary>
        public ReadyType IsReady { get; set; }
        /// <summary>
        /// 2. aşama neticesinde işlemin tamamlanıp, tamamlanmadığını ifade eder.
        /// </summary>
        public TransactionState TransactionState { get; set; }
        public Node Node { get; set; }
    }
}

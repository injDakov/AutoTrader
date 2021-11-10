using System;

namespace AT.Domain
{
    /// <summary>PairHistory entity class.</summary>
    public class PairHistory
    {
        /// <summary>Initializes a new instance of the <see cref="PairHistory" /> class.</summary>
        /// <param name="orderAmount">The order amount.</param>
        /// <param name="orderLevel">The order level.</param>
        /// <param name="isActive">if set to <c>true</c> [is active].</param>
        public PairHistory(decimal orderAmount, int orderLevel, bool isActive)
        {
            StartDate = DateTime.UtcNow;
            OrderAmount = orderAmount;
            OrderLevel = orderLevel;
            IsActive = isActive;
        }

        /// <summary>Gets or sets the identifier for the pairHistory entity.</summary>
        /// <value>The identifier.</value>
        public long Id { get; set; }

        /// <summary>Gets or sets the start date.</summary>
        /// <value>The start date.</value>
        public DateTime StartDate { get; set; }

        /// <summary>Gets or sets the end date.</summary>
        /// <value>The end date.</value>
        public DateTime? EndDate { get; set; }

        /// <summary>Gets or sets the order amount.</summary>
        /// <value>The order amount.</value>
        public decimal OrderAmount { get; set; }

        /// <summary>Gets or sets the order level.</summary>
        /// <value>The order level.</value>
        public int OrderLevel { get; set; }

        /// <summary>Gets or sets the active hours.</summary>
        /// <value>The active hours.</value>
        public int ActiveHours { get; set; }

        /// <summary>Gets or sets the executed sell order count.</summary>
        /// <value>The executed sell order count.</value>
        public int ExecutedSellOrderCount { get; set; }

        /// <summary>Gets or sets the executed buy order count.</summary>
        /// <value>The executed buy order count.</value>
        public int ExecutedBuyOrderCount { get; set; }

        /// <summary>Gets or sets a value indicating whether this pair is active.</summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        public bool IsActive { get; set; }

        /// <summary>Gets or sets the pair identifier.</summary>
        /// <value>The pair identifier.</value>
        public long PairId { get; set; }

        /// <summary>Gets or sets the pair.</summary>
        /// <value>The pair.</value>
        public virtual Pair Pair { get; set; }
    }
}
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace AT.Domain
{
    /// <summary>Pair entity class.</summary>
    public class Pair
    {
        private ILazyLoader LazyLoader { get; set; }

        private ICollection<PairHistory> _pairHistory;

        /// <summary>Initializes a new instance of the <see cref="Pair" /> class.</summary>
        public Pair()
        {
            PairHistory = new List<PairHistory>();
        }

        /// <summary>Gets or sets the identifier for the pair entity.</summary>
        /// <value>The identifier.</value>
        public long Id { get; set; }

        /// <summary>Gets or sets the create date.</summary>
        /// <value>The create date.</value>
        public DateTime CreateDate { get; set; }

        /// <summary>Gets or sets the last update date.</summary>
        /// <value>The last update date.</value>
        public DateTime? LastUpdateDate { get; set; }

        /// <summary>Gets or sets the name.</summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>Gets or sets the order amount.</summary>
        /// <value>The order amount.</value>
        public decimal OrderAmount { get; set; }

        /// <summary>Gets or sets the maximum order level.</summary>
        /// <value>The maximum order level.</value>
        public int MaxOrderLevel { get; set; }

        /// <summary>Gets or sets a value indicating whether this instance is active.</summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        public bool IsActive { get; set; }

        /// <summary>Gets or sets the pair history.</summary>
        /// <value>The pair history.</value>
        public virtual ICollection<PairHistory> PairHistory
        {
            get => LazyLoader.Load(this, ref _pairHistory);
            set => _pairHistory = value;
        }
    }
}
using System;

namespace AT.Domain
{
    public class PairHistory
    {
        public PairHistory()
        {
        }

        public PairHistory(bool isActive)
        {
            CreateDate = DateTime.UtcNow;
            IsActive = isActive;
        }

        public long Id { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        public int ActiveHours { get; set; }

        public int ExecutedSellOrderCount { get; set; }

        public int ExecutedBuyOrderCount { get; set; }

        public bool IsActive { get; set; }

        public long PairId { get; set; }

        public virtual Pair Pair { get; set; }
    }
}
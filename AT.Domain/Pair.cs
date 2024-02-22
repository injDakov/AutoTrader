using System;
using AT.Domain.Enums;

namespace AT.Domain
{
    public class Pair
    {
        public Pair()
        {
            PairHistory = new PairHistory();
        }

        public long Id { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? LastUpdateDate { get; set; }

        public int ActiveHours { get; set; }

        public Exchange Exchange { get; set; }

        public string Name { get; set; }

        public decimal OrderAmount { get; set; }

        public int MaxOrderLevelCount { get; set; }

        public bool IsActive { get; set; }

        public virtual PairHistory PairHistory { get; set; }
    }
}
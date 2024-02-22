using AT.Domain;
using AT.Domain.Enums;

namespace AT.Tests
{
    public static class TestConstants
    {
        public static readonly Pair PredefinedPair = new ()
        {
            ActiveHours = 3,
            Exchange = Exchange.Bitfinex,
            Name = "BTCUSD",
            OrderAmount = 0.01M,
            MaxOrderLevelCount = 3,

            IsActive = true,

            // TODO:
            //PairHistory = new List<PairHistory>()
            //{
            //    new PairHistory()
            //    {
            //        CreateDate = DateTime.Parse("2021-12-02 17:45:26Z").ToUniversalTime(),
            //        ActiveHours = 0,
            //        IsActive = true,
            //    },
            //    new PairHistory()
            //    {
            //        CreateDate = DateTime.Parse("2021-12-02 17:45:26Z").ToUniversalTime(),
            //        ActiveHours = 0,
            //        IsActive = true,
            //    },
            //    new PairHistory()
            //    {
            //        CreateDate = DateTime.Parse("2021-12-02 17:45:26Z").ToUniversalTime(),
            //        LastUpdateDate = DateTime.Parse("2021-12-02 19:50:26Z").ToUniversalTime(),
            //        ActiveHours = 1,
            //        IsActive = true,
            //    },
            //    new PairHistory()
            //    {
            //        CreateDate = DateTime.Parse("2021-12-02 17:45:26Z").ToUniversalTime(),
            //        EndDate = DateTime.Parse("2021-12-02 20:45:26Z").ToUniversalTime(),
            //        LastUpdateDate = DateTime.Parse("2021-12-02 20:50:26Z").ToUniversalTime(),
            //        ActiveHours = 3,
            //        IsActive = false,
            //    },
            //},
        };
    }
}

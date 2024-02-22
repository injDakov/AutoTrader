namespace AT.Business.Models.Exchange
{
    public enum OrderStatusEnum
    {
        Active = 0,

        Executed = 1,

        // PartiallyFilled = 2, // TODO:

        Canceled = 3,

        Unknown = 4,
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using AT.Domain;

namespace AT.Business.Helpers
{
    public static class ConfigHelper
    {
        public static void CheckPairConfiguration(List<Models.AppSettings.Pair> configPairs)
        {
            // TODO: Create test !

            if (configPairs.GroupBy(p => p.Name).Any(x => x.Count() > 1))
            {
                var pair = configPairs.GroupBy(p => p.Name).FirstOrDefault(x => x.Count() > 1);

                throw new Exception($"There are '{pair.Count()}' duplicate pairs with name '{pair.Key}'.");
            }

            if (configPairs.Any(p => string.IsNullOrWhiteSpace(p.Name)))
            {
                throw new Exception("There are pairs with empty value for name.");
            }

            if (configPairs.Any(p => p.OrderAmount <= 0))
            {
                // TODO: Test why if "OrderAmount": null, the pair missing from the list

                throw new Exception($"The pair {configPairs.FirstOrDefault(p => p.OrderAmount == 0).Name} has empty value for OrderAmount.");
            }

            if (configPairs.Any(p => p.MaxOrderLevelCount < 0))
            {
                // TODO: Test why if "MaxOrderLevelCount": null, the pair missing from the list

                throw new Exception($"The pair {configPairs.FirstOrDefault(p => p.MaxOrderLevelCount < 0).Name} has value smaller than 0 for MaxOrderLevelCount.");
            }
        }

        public static void AddPairOrders(Pair dbPair)
        {
            dbPair.PairHistory.CreateDate = dbPair.CreateDate;
            dbPair.PairHistory.StartDate = dbPair.StartDate;
            dbPair.PairHistory.EndDate = null;
            dbPair.PairHistory.IsActive = true;

            dbPair.PairHistory.LastUpdateDate = dbPair.LastUpdateDate;
        }

        public static void ActivatePair(Pair dbPair)
        {
            var dateTimeUtcNow = DateTime.UtcNow;

            dbPair.StartDate = dateTimeUtcNow;
            dbPair.EndDate = null;
            dbPair.IsActive = true;

            dbPair.LastUpdateDate = dateTimeUtcNow;

            ActivatePairOrders(dbPair.PairHistory);
        }

        public static void DeactivatePair(Pair dbPair)
        {
            // Set an inactive flag for a pair that doesn't exist in the actual config file or is it inactive

            var dateTimeUtcNow = DateTime.UtcNow;

            dbPair.EndDate = dateTimeUtcNow;
            dbPair.ActiveHours += (int)(dateTimeUtcNow - (DateTime)dbPair.StartDate).TotalHours;
            dbPair.IsActive = false;

            dbPair.LastUpdateDate = dateTimeUtcNow;

            DeactivatePairOrders(dbPair.PairHistory);
        }

        public static void ActivatePairOrders(PairHistory dbPairHistory)
        {
            var dateTimeUtcNow = DateTime.UtcNow;

            dbPairHistory.StartDate = dateTimeUtcNow;
            dbPairHistory.EndDate = null;
            dbPairHistory.IsActive = true;

            dbPairHistory.LastUpdateDate = dateTimeUtcNow;
        }

        public static void DeactivatePairOrders(PairHistory dbPairHistory)
        {
            // Set an inactive flag.
            // Calculate the active hours.

            var dateTimeUtcNow = DateTime.UtcNow;

            dbPairHistory.ActiveHours += (int)(dateTimeUtcNow - (DateTime)dbPairHistory.StartDate).TotalHours;

            dbPairHistory.EndDate = dateTimeUtcNow;
            dbPairHistory.IsActive = false;

            dbPairHistory.LastUpdateDate = dateTimeUtcNow;
        }
    }
}
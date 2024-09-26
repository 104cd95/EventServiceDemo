namespace EventServiceDemo
{
    // We can use something like this class 
    // to make event tracking more convenient for users
    // however this can be implemented in another way
    public static class EventServiceUtility
    {
        private const string LEVEL_START = "levelStart";
        private const string LEVEL_START_PARAM = "level";
        
        private const string REWARD_CLAIM = "rewardClaim";
        private const string REWARD_CLAIM_PARAM = "rewardBundle";
        
        private const string COINS_SPENDING = "coinsSpending";
        private const string COINS_SPENDING_PARAM = "coinNumber";
        
        public static void TrackLevelStart(int level)
        {
            TrackEvent(LEVEL_START, LEVEL_START_PARAM, level.ToString());
        }
        
        public static void TrackRewardClaim(string rewardBundle)
        {
            TrackEvent(REWARD_CLAIM, REWARD_CLAIM_PARAM, rewardBundle);
        }
        
        public static void TrackCoinsSpending(int coinNumber)
        {
            TrackEvent(COINS_SPENDING, COINS_SPENDING_PARAM, coinNumber.ToString());
        }

        // The ticket said that the EventService's TrackEvent should have 2 parameters,
        // but the json example contains "level:3" data, so this solution is just to satisfy the requirements
        private static void TrackEvent(string eventType, string eventParamType, string eventParamValue)
        {
            EventService.Instance.TrackEvent(eventType, $"{eventParamType}:{eventParamValue}");
        }
    }
}
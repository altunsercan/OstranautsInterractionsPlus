namespace InteractionsPlus.Handlers
{
    public struct InteractionTriggerArgs
    {
        public readonly CondOwner Us;
        public readonly CondOwner Them;
        public readonly bool Stats;
        public readonly bool IgnoreItems;
        public readonly bool CheckPath;

        public InteractionTriggerArgs(CondOwner us, CondOwner them, bool stats, bool ignoreItems, bool checkPath)
        {
            Us = us;
            Them = them;
            Stats = stats;
            IgnoreItems = ignoreItems;
            CheckPath = checkPath;
        }
    }
}
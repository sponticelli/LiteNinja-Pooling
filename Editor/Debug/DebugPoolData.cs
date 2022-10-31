namespace LiteNinja.Pooling.Editor
{
    internal struct DebugPoolData
    {
        public string title;
        public int activeObjectCount;
        public int inactiveObjectCount;

        public DebugPoolData(string title, int activeObjectCount, int inactiveObjectCount)
        {
            this.title = title;
            this.activeObjectCount = activeObjectCount;
            this.inactiveObjectCount = inactiveObjectCount;
        }
    }
}
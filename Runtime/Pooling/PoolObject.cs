namespace MobX.Mediator.Pooling
{
    public readonly struct PoolObject<T> where T : class
    {
        public readonly bool AutoRelease;
        public readonly float ReleaseTimeStamp;
        public readonly T Object;

        public PoolObject(bool autoRelease, float releaseTimeStamp, T tracked)
        {
            AutoRelease = autoRelease;
            ReleaseTimeStamp = releaseTimeStamp;
            Object = tracked;
        }
    }
}
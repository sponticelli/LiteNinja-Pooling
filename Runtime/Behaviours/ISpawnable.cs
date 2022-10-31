namespace LiteNinja.Pooling
{
    /// <summary>
    /// If a component implements this interface, the OnSpawn methods will be called when the object is spawned
    /// instead of GameObject.SetActive.
    /// </summary>
    public interface ISpawnable
    {
        void OnSpawn(bool active);
    }
}
namespace UserEvents.Infra.Interfaces;

public interface IEventProducer<TKey, TValue> where TValue : class
{
    Task ProduceAsync(TKey key, TValue value);
}
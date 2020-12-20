namespace HomeController.Processors
{
    public interface INormalizer<TIn, out TOut>
    {
        TOut Process(in Context<TIn> input);
    }
}

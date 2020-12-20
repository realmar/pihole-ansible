namespace HomeController.Processors
{
    public readonly struct Context<TModel>
    {
        public string Topic { get; }
        public TModel Model { get; }

        private Context(string topic, TModel model)
        {
            Topic = topic;
            Model = model;
        }

        public static Context<TModel> Create(string topic, TModel model) => new(topic, model);
    }
}

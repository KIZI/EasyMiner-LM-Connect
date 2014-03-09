namespace LMConnect.Client
{
    public class Miner
    {
        public string Id { get; private set; }

        internal Miner(string id)
        {
            this.Id = id;
        }
    }
}
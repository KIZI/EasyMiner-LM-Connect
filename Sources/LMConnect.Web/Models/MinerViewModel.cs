namespace LMConnect.Web.Models
{
	public class MinerViewModel
	{
		public LISpMiner.LISpMiner Miner { get; private set; }

		public string Id
		{
			get
			{
				return this.Miner.Id;
			}
		}

		public bool SharedPool
		{
			get
			{
				return this.Miner.SharedPool;
			}
		}

		public string Owner { get; private set; }

		internal MinerViewModel(LISpMiner.LISpMiner miner, string owner)
		{
			this.Miner = miner;
			this.Owner = owner;
		}
	}
}
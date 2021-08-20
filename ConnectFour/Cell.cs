using System;
using BoardGameLearner;

namespace BoardGameLearner
{
	public record Cell
	{
		public PlayerID PlayerId
		{
			get => _playerId;
			set
			{
				if (IsEmpty)
					_playerId = value;
				else throw new Exception($"Could not overwrite a non-null Player ID");
			}
		}
		private PlayerID _playerId = null;
		public bool IsEmpty => _playerId == null;
		public Cell(PlayerID playerId)
			=> _playerId = playerId;

		public Cell()
			: this(playerId: null)
		{ }
	}
}

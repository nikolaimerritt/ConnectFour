using System;
using BoardGameLearner;

using System.Collections.Generic;

namespace BoardGameLearner
{
	public record Cell
	{
		public PlayerID PlayerId = null;
		public bool IsEmpty => PlayerId == null;
		public Cell(PlayerID playerId)
			=> PlayerId = playerId;

		public Cell()
			: this(playerId: null)
		{ }

		public List<double> OneHotEncode(PlayerID firstPlayer, PlayerID secondPlayer)
        {
			List<double> values = new() { 0, 0, 0 };
			int oneIdx = 0;
			if (PlayerId == firstPlayer)
				oneIdx = 1;
			else if (PlayerId == secondPlayer)
				oneIdx = 2;
			values[oneIdx] = 1;
			return values;
        }
	}
}

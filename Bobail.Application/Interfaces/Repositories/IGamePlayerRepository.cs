using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bobail.Application.Interfaces.Repositories
{
    public interface IGamePlayerRepository
    {
        Task AddPlayersForGame(Guid gameId, Guid userId, bool isVsBot);
    }
}

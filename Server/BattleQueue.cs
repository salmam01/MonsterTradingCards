using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Server
{
    public class BattleQueue
    {
        private readonly List<User> _userQueue = new();
        private readonly object _lock = new();

        public void AddUserToQueue(User user)
        {
            lock (_lock)
            {
                if (_userQueue.Count < 2)
                {
                    _userQueue.Add(user);
                }
            }
        }

        public List<User> GetUsersInQueue()
        {
            lock (_lock)
            {
                List<User> userQueueCopy = _userQueue;
                _userQueue.Clear();

                return userQueueCopy;
            }
        }
    }
}

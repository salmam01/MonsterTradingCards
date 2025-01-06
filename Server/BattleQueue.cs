using MonsterTradingCardsGame.Models;
using MonsterTradingCardsGame.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Server
{
    public class BattleQueue
    {
        private readonly Queue<User> _userQueue = new();
        private readonly object _lock = new();

        public void AddUserToQueue(User user)
        {
            lock (_lock)
            {
                _userQueue.Enqueue(user);
            }
        }

        public List<User> GetNextBattle()
        {
            lock (_lock)
            {
                if(_userQueue.Count >= 2)
                {
                    Console.WriteLine("Users in Queue: ", _userQueue.Count);
                    List<User> userList = new(2);
                    userList.Add(_userQueue.Dequeue());
                    userList.Add(_userQueue.Dequeue());
                    return userList;
                }

                return new List<User>();
            }
        }
    }
}

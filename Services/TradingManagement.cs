using MonsterTradingCardsGame.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardsGame.Services
{
    public class TradingManagement
    {
        private readonly DatabaseConnection _dbConnection;
        private readonly CardManagement _cardManagement;


        public TradingManagement(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
            _cardManagement = new();
        }
    }
}

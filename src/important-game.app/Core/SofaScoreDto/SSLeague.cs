using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace important_game.ui.Core.SofaScoreDto
{
    internal class SSLeague
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<SSTeam> Teams { get; set; }
    }
}

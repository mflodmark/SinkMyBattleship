using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinkMyBattleshipWPF.Utils
{
    public enum AnswerCodes
    {
        [Description("210 BATTLESHIP/1.0")]
        Battleship = 210,

        [Description("221 Client Starts")]
        ClientStarts = 221,

        [Description("222 Host Starts")]
        HostStarts = 222,

        [Description("230 Miss")]
        Miss = 230,

        [Description("241 You hit my Carrier")]
        YouHitMyCarrier = 241,

        [Description("242 You hit my Battleship")]
        YouHitMyBattleship = 242,

        [Description("243 You hit my Destroyer")]
        YouHitMyDestroyer = 243,

        [Description("244 You hit my Submarine")]
        YouHitMySubmarine = 244,

        [Description("245 You hit my Patrol Boat")]
        YouHitMyPatrolBpat = 245,

        [Description("251 You sunk my Carrier")]
        YouSunkMyCarrier = 251,

        [Description("252 You sunk my Battleship")]
        YouSunkMyBattleship = 252,

        [Description("253 You sunk my Destroyer")]
        YouSunkMyDestroyer = 253,

        [Description("254 You sunk my Submarine")]
        YouSunkMySubmarine = 254,

        [Description("255 You sunk my Patrol Boat")]
        YouSunkMyPatrolBoat = 255,

        [Description("260 You win")]
        YouWin = 260,

        [Description("270 Connection closed")]
        ConnectionClosed = 270,

        [Description("500 Syntax Error")]
        Syntax_Error = 500,

        [Description("501 Sequence Error")]
        Sequence_Error = 501
    }
}

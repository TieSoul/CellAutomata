using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CellAutomata
{
    class ElementaryAutomaton : Automaton
    {
        byte code;
        public ElementaryAutomaton(byte code)
        {
            this.code = code;
            dimensions = 1;
            colors = 2;
            neighbourhood = new int[][]
            {
                new int[]{-1},
                new int[] {0},
                new int[] {1}
            };
        }
        protected override int rule(int state, int[] neighbourhood)
        {
            if (dimensions != 1 || colors != 2) throw new Exception("Elementary automata are strictly 1-dimensional with 2 colors.");
            int num = (neighbourhood[0] << 2) + (neighbourhood[1] << 1) + neighbourhood[2];
            return (code >> num) & 1;
        }
    }
}

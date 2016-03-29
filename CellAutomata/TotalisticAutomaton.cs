using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CellAutomata
{
    class TotalisticAutomaton : Automaton
    {
        ulong code;
        public TotalisticAutomaton(int dimensions, ulong rule, int colors = 2, int range = 1)
        {
            this.dimensions = dimensions;
            code = rule;
            this.colors = colors;
            int[] x = new int[dimensions];
            for (var i = 0; i < x.Length; i++)
            {
                x[i] = -range;
            }
            var done = false;
            var neighbourhood = new List<int[]>();
            while (!done)
            {
                var y = new int[dimensions];
                int i;
                for (i = 0; i < dimensions; i++)
                {
                    y[i] = x[dimensions - i - 1];
                }
                neighbourhood.Add(y);
                x[0]++;
                i = 0;
                while (x[i] > range)
                {
                    x[i] = -range;
                    i++;
                    if (i >= x.Length)
                    {
                        done = true;
                        break;
                    }
                    x[i]++;
                }
            }
            this.neighbourhood = neighbourhood.ToArray();
        }

        protected override int rule(int state, int[] neighbourhood)
        {
            int total = 0;
            for (var i = 0; i < neighbourhood.Length; i++)
            {
                total += neighbourhood[i];
            }
            return (int)((code / Math.Pow(colors, total)) % colors);
        }
    }
}

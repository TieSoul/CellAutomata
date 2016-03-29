using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CellAutomata
{
    public class ArrayEqualityComparer : IEqualityComparer<int[]>
    {
        public bool Equals(int[] x, int[] y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                {
                    return false;
                }
            }
            return true;
        }

        public int GetHashCode(int[] obj)
        {
            int result = 17;
            for (int i = 0; i < obj.Length; i++)
            {
                unchecked
                {
                    result = result * 23 + obj[i];
                }
            }
            return result;
        }
    }
    public abstract class Automaton
    {
        public static ArrayEqualityComparer COMPARER = new ArrayEqualityComparer();
        public int currentStep = 0;
        public int dimensions;
        public int colors;
        public int backgroundColor = 0;
        public int[][] neighbourhood;
        public List<int> backgroundHistory = new List<int>();
        public List<Dictionary<int[], int>> cellHistory = new List<Dictionary<int[], int>>();
        public Dictionary<int[], int> cells = new Dictionary<int[], int>(COMPARER);
        protected abstract int rule(int state, int[] neighbourhood);

        public int getFromHistory(int step, params int[] index)
        {
            if (index.Length != dimensions)
            {
                throw new ArgumentException("Length of index array " + index.Length + " is wrong, should be " + dimensions + ".");
            }
            if (step == currentStep) return this[index];
            var cs = cellHistory.ElementAtOrDefault(step);
            var bg = backgroundHistory.ElementAtOrDefault(step);
            if (cs == null)
            {
                return bg;
            }
            if (cs.ContainsKey(index)) return cs[index];
            else return bg;
        }

        public int this[params int[] index]
        {
            get
            {
                if (index.Length == dimensions)
                {
                    if (cells.ContainsKey(index))
                    {
                        return cells[index];
                    }
                    else return backgroundColor;
                }
                else throw new ArgumentException("Length of index array " + index.Length + " is wrong, should be " + dimensions + ".");
            }
            set
            {
                if (value == backgroundColor)
                {
                    if (cells.ContainsKey(index))
                    {
                        cells.Remove(index);
                    }
                }
                else
                {
                    cells[index] = value;
                }
            }
        }

        public void step()
        {
            if (dimensions == 1)
            {
                backgroundHistory.Add(backgroundColor);
                cellHistory.Add(cells);
            }
            var newBackground = rule(backgroundColor, Enumerable.Repeat(backgroundColor, neighbourhood.Length).ToArray());
            var newCells = new Dictionary<int[], int>(COMPARER);
            var coordsToCheck = new List<int[]>();
            foreach (var coordinates in cells.Keys)
            {
                foreach (var neighbourCoords in neighbourhood)
                {
                    var newCoords = coordinates.Zip(neighbourCoords, (x, y) => x - y).ToArray();
                    if (!coordsToCheck.Contains(newCoords, COMPARER)) coordsToCheck.Add(newCoords);
                }
                if (!coordsToCheck.Contains(coordinates, COMPARER)) coordsToCheck.Add(coordinates);
            }
            foreach (var coords in coordsToCheck)
            {
                var newValue = rule(this[coords], getNeighbourhood(coords));
                if (newValue != newBackground) newCells[coords] = newValue;
            }
            cells = newCells;
            backgroundColor = newBackground;
            currentStep++;
        }

        public int[] getNeighbourhood(params int[] coords)
        {
            var neighbours = new List<int>();
            foreach (var neighbourCoords in neighbourhood)
            {
                var newCoords = coords.Zip(neighbourCoords, (x, y) => x + y).ToArray();
                neighbours.Add(this[newCoords]);
            }
            return neighbours.ToArray();
        }
    }
}

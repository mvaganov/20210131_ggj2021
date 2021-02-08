using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NumberSequence {
    public static void GenerateAdvancementOrder(int[] howManyOfEach, out int[] color, out int[] shape) {
        int totalCount = 0;
        int[] counters = (int[])howManyOfEach.Clone();
        counters.ForEach(i => totalCount += i);
        color = new int[totalCount];
        shape = new int[totalCount];
        int j = 0; int limit = 1;
        for (int i = 0; i < totalCount; ++i) {
            color[i] = j;
            shape[i] = howManyOfEach[j] - counters[j];
            --counters[j];
            ++j;
            if (j >= limit) {
                j = 0;
                if (limit < counters.Length) { ++limit; }
            }
            while (counters[j] <= 0) { ++j; if (j>= counters.Length) { j = 0; break; } }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NumberSequence {
    public static void GenerateAdvancementOrder(int[] howManyOfEach, out int[] color, out int[] shape) {
        int totalCount = 0;
        howManyOfEach.ForEach(i => totalCount += i);
        color = new int[totalCount];
        shape = new int[totalCount];
        int j = 0; int limit = 1;
        for (int i = 0; i < totalCount; ++i) {
            --howManyOfEach[j];
            color[i] = j;
            shape[i] = howManyOfEach[j];
            ++j;
            if (j >= limit) {
                j = 0;
                if (limit < howManyOfEach.Length) { ++limit; }
            }
            while (howManyOfEach[j] <= 0) { ++j; if (j>=howManyOfEach.Length) { j = 0; break; } }
        }
    }
}

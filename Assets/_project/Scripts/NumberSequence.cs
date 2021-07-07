using NonStandard;
public class NumberSequence {
    public static void GenerateAdvancementOrder2(int[] howManyOfEach, out int[] color, out int[] shape) {
        int totalCount = 0;
        int[] counters = (int[])howManyOfEach.Clone();
        bool reversing = false;
        counters.ForEach(i => totalCount += i);
        color = new int[totalCount];
        shape = new int[totalCount];
        int v = 0; int limit = 1;
        for (int i = 0; i < totalCount; ++i) {
            color[i] = v;
            shape[i] = howManyOfEach[v] - counters[v];
            --counters[v];
            if (!reversing) ++v; else --v;
            if(v >= 0 && v < counters.Length && counters[v] <= 0) {
                v = -1;
            }
            if(v >= limit) {
                v -= 2; reversing = true;
			}
            if(v < 0) {
                v = 0;
                if (limit < counters.Length) { ++limit; }
                reversing = false;
            }
            while (v < counters.Length && counters[v] <= 0) { ++v; }
        }
    }
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

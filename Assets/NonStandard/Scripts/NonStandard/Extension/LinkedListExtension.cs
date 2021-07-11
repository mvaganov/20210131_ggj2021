using System;
using System.Collections.Generic;

namespace NonStandard.Extension {
public static class ExtensionLinkedList {
    
    public static void EnqueueRange<T>(this LinkedList<T> self, IList<T> multipleValues) {
        foreach (T value in multipleValues) { self.AddLast(value); }
    }
    public static void EnqueueRange<T>(this LinkedList<T> self, LinkedList<T> multipleValues) {
        foreach (T value in multipleValues) { self.AddLast(value); }
    }
    /// <summary>
    /// these get to go to the front of the line, in the order given
    /// </summary>
    public static void EnqueueRangeFirst<T>(this LinkedList<T> self, IList<T> multipleCommands) {
        for (int i = multipleCommands.Count - 1; i >= 0; --i) {
            self.AddFirst(multipleCommands[i]);
        }
    }
    
    /// <summary>
    /// As long as the linked list is only added to with AddLast, this should prevent strange threading shenanigans.
    /// WARNING! This may cause memory leaks in threads because of how C# keeps references alive due references from external scope.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="action"></param>
    /// <param name="removeAfterProcessing"></param>
    /// <typeparam name="T"></typeparam>
    public static void ForEachThreadSafeIsh<T>(this LinkedList<T> list, Action<LinkedListNode<T>> action, bool removeAfterProcessing = false) {
        if (list == null) return;
        LinkedListNode<T> cursor = list.First, last = list.Last;
        while (cursor != null) {
            LinkedListNode<T> next = cursor.Next;
            bool shouldStopAfterThis = cursor == last;
            if (cursor.Value != null) {
                action(cursor);
            }
            if (removeAfterProcessing) { list.Remove(cursor); }
            if (shouldStopAfterThis) { break; }
            cursor = next;
        }
    }
}
}
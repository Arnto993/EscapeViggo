using System;
using System.Collections.Generic;

namespace EscapeViggo.Tasks
{
    public static class CircuitSequence
    {
        public static bool IsValid(IReadOnlyList<int> sequence)
        {
            if (sequence == null || sequence.Count != 3) return false;
            foreach (var element in sequence) if (element < 1 || element > 3) return false;
            return true;
        }

        public static bool TryAdvance(WorkTaskState current, IReadOnlyList<int> sequence, ulong sender,
            int element, uint expectedRevision, out WorkTaskState next)
        {
            next = current;
            if (!IsValid(sequence)) throw new ArgumentException("Circuit sequence must contain three elements from 1 to 3.");
            if (current.Phase != WorkTaskPhase.InProgress || current.WorkerId != sender ||
                current.Revision != expectedRevision || element < 1 || element > 3) return false;
            next.WrongInput = element != sequence[current.Progress];
            next.Progress = next.WrongInput ? 0 : current.Progress + 1;
            next.Revision++;
            if (next.Progress == sequence.Count) next.Phase = WorkTaskPhase.Completed;
            return true;
        }
    }
}

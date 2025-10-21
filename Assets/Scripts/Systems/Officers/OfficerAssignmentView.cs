using System.Collections.Generic;

namespace IFC.Systems.Officers
{
    /// <summary>
    /// Lightweight view of officer assignments to logical slots/facilities.
    /// </summary>
    public class OfficerAssignmentView
    {
        private readonly HashSet<string> _assigned = new HashSet<string>();

        public void SetAssigned(string slotId, bool assigned)
        {
            if (string.IsNullOrEmpty(slotId))
            {
                return;
            }

            if (assigned)
            {
                _assigned.Add(slotId);
            }
            else
            {
                _assigned.Remove(slotId);
            }
        }

        public bool IsOfficerAssignedTo(string slotId)
        {
            if (string.IsNullOrEmpty(slotId))
            {
                return false;
            }

            return _assigned.Contains(slotId);
        }

        public IEnumerable<string> GetAssignedSlots()
        {
            return _assigned;
        }

        public void Clear()
        {
            _assigned.Clear();
        }
    }
}

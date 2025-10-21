using System.Collections.Generic;

namespace IFC.Systems.Officers
{
    /// <summary>
    /// Applies officer-driven stat adjustments to runtime registries.
    /// </summary>
    public static class OfficerBuffPass
    {
        public const string TrainingTimeMultiplierKey = "TrainingTimeMultiplier";
        private const float AssignedTrainingMultiplier = 0.9f;

        public static void Apply(global::IFC.Systems.CityState city, OfficerAssignmentView assignments, global::IFC.Systems.StatModifierRegistry registry)
        {
            if (city == null || registry == null)
            {
                return;
            }

            var facilityIds = new HashSet<string>();
            for (int i = 0; i < city.trainingQueues.Count; i++)
            {
                var queue = city.trainingQueues[i];
                if (queue == null || string.IsNullOrEmpty(queue.facilityId))
                {
                    continue;
                }

                facilityIds.Add(queue.facilityId);
            }

            if (assignments == null)
            {
                return;
            }

            foreach (var facilityId in assignments.GetAssignedSlots())
            {
                if (string.IsNullOrEmpty(facilityId))
                {
                    continue;
                }

                if (facilityIds.Count > 0 && !facilityIds.Contains(facilityId))
                {
                    continue;
                }

                float current = registry.GetMultiplierOrDefault(facilityId, TrainingTimeMultiplierKey, 1f);
                registry.SetMultiplier(facilityId, TrainingTimeMultiplierKey, current * AssignedTrainingMultiplier);
            }
        }
    }
}

using System;
using UnityEngine;

namespace Mossmark.Day
{
    [CreateAssetMenu(fileName = "DayCycleAmbientTextData", menuName = "Mossmark/Day/Ambient Text Data")]
    public class DayCycleAmbientTextData : ScriptableObject
    {
        [Serializable]
        public class PhaseEntry
        {
            public DayPhase Phase;
            [TextArea] public string[] Lines;
        }

        [SerializeField] private PhaseEntry[] entries;

        public string GetTextForPhase(DayPhase phase)
        {
            foreach (var entry in entries)
            {
                if (entry.Phase != phase || entry.Lines == null || entry.Lines.Length == 0) continue;

                return entry.Lines[UnityEngine.Random.Range(0, entry.Lines.Length)];
            }

            return null;
        }
    }
}

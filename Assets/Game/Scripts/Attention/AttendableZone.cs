using UnityEngine;

namespace Mossmark.Attention
{
    [RequireComponent(typeof(Collider2D))]
    public class AttendableZone : MonoBehaviour
    {
        public IAttendable Attendable { get; private set; }

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            Attendable = GetComponent<IAttendable>();

            if (Attendable == null)
            {
                Debug.LogError($"AttendableZone on '{name}' has no sibling component implementing IAttendable.", this);
            }
        }
    }
}

using UnityEngine;

namespace Sakura.Trial
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class TrialGate : MonoBehaviour
    {
        [SerializeField] Sprite[] openingFrames;
        [SerializeField, Min(0.02f)] float secondsPerFrame = 0.11f;
        SpriteRenderer door;
        int frame;
        float openingTime;
        bool opening;

        public void Configure(Sprite[] frames)
        {
            openingFrames = frames;
            GetComponent<SpriteRenderer>().sprite = frames[0];
        }

        public void Open()
        {
            if (opening || openingFrames == null || openingFrames.Length == 0) return;
            door = GetComponent<SpriteRenderer>();
            opening = true;
        }

        void Update()
        {
            if (!opening || frame >= openingFrames.Length - 1) return;
            openingTime += Time.deltaTime;
            int next = Mathf.Min(openingFrames.Length - 1, Mathf.FloorToInt(openingTime / secondsPerFrame));
            if (frame == next) return;
            frame = next;
            door.sprite = openingFrames[frame];
        }
    }
}

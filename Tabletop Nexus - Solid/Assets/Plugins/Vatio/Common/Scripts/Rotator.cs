using UnityEngine;

namespace Vatio.Examples
{
    /// <summary>
    /// This script applies nice, sinusoidal, random rotation continuously over time to the GameObject it's attached to.
    /// </summary>
    public class Rotator : MonoBehaviour
    {
        public float rotationRange = 20.0f;
        public Vector2 timeRange = new Vector2(1.0f, 5.0f);

        private Quaternion referenceRotation;
        private Quaternion targetRotation;
        private float rotationBeginTime;
        private float targetDuration;

        private void BeginNewRotation()
        {
            referenceRotation = transform.localRotation;

            float rotationX = Random.Range(-rotationRange / 2.0f, rotationRange / 2.0f);
            float rotationY = Random.Range(-rotationRange / 2.0f, rotationRange / 2.0f);
            float rotationZ = Random.Range(-rotationRange / 2.0f, rotationRange / 2.0f);

            targetRotation = Quaternion.Euler(rotationX, rotationY, rotationZ);

            rotationBeginTime = Time.time;
            targetDuration = Random.Range(timeRange.x, timeRange.y);
        }

        private void Start()
        {
            BeginNewRotation();
        }

        private void Update()
        {
            float elapsedTime = Time.time - rotationBeginTime;

            // If the rotation time elapsed begin new rotation
            if (elapsedTime >= targetDuration)
            {
                transform.localRotation = targetRotation;
                BeginNewRotation();

                elapsedTime = Time.time - rotationBeginTime;        // Parameters have changed, recalculate the elapsed time
            }

            // Calculate current rotation
            transform.localRotation = Quaternion.Lerp(referenceRotation, targetRotation, MapSinTime(elapsedTime, targetDuration));
        }

        // This function returns value in range (0,1)
        private float MapSinTime(float elapsedTime, float duration)
        {
            float relativeElapsedTime = elapsedTime / duration;                             // Get elapsed time in range (0, 1)
            float mappedSinInput = (relativeElapsedTime - 0.5f) * Mathf.PI;                 // To have the right output shape we need the sinus result to be in range (-1, 1) so the parameter should be in range (-Pi/2, Pi/2)

            float sinOutput = Mathf.Sin(mappedSinInput);
            float mappedSinOutput = (sinOutput + 1.0f) / 2.0f;                              // Map sinus result to range (0, 1)

            return mappedSinOutput;
        }
    }
}

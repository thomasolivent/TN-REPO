using UnityEngine;

namespace Vatio.Examples
{
    /// <summary>
    /// This script rotates and scales the cube according to the slider values and parameters.
    /// </summary>
    public class CubeTransformer : MonoBehaviour
    {
        public float rotationRange = 180.0f;
        public float scaleMin = 1.0f;
        public float scaleMax = 3.0f;

        private float xRotationValue = 0.0f;
        private float yRotationValue = 0.0f;

        public void RotateX(float value)
        {
            xRotationValue = value;
            transform.localRotation = Quaternion.Euler(xRotationValue * rotationRange, -yRotationValue * rotationRange, 0.0f);
        }

        public void RotateY(float value)
        {
            yRotationValue = value;
            transform.localRotation = Quaternion.Euler(xRotationValue * rotationRange, -yRotationValue * rotationRange, 0.0f);
        }

        public void ScaleX(float value)
        {
            transform.localScale = new Vector3(scaleMin + value * (scaleMax - scaleMin), transform.localScale.y, transform.localScale.z);
        }

        public void ScaleY(float value)
        {
            transform.localScale = new Vector3(transform.localScale.x, scaleMin + value * (scaleMax - scaleMin), transform.localScale.z);
        }
    }
}

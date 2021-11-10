using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AirThrasher.Assets.Scripts.UserInterface
{
    public class CircleSpeedIndicator : MonoBehaviour
    {
        [SerializeField] private Image _bar;
        public RectTransform button;
        private float maxSpeed = 150f;

        public void SpeedChange(float speedValue)
        {
            float amount = speedValue / maxSpeed * 180.0f / 360;
            _bar.fillAmount = amount;
            float buttonAngle = amount * 360;
            button.localEulerAngles = new Vector3(0, 0, -buttonAngle);
        }
    }
}
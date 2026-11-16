using System;
using UnityEngine;

namespace Tools
{
    public class ShowFPS:MonoBehaviour
    {
        private float deltaTime = 0f;
        private int currentFPS;
        private void Update()
        {
            deltaTime += (Time.unscaledDeltaTime-deltaTime)*0.1f;
            currentFPS = (int)(1f / deltaTime);
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 100, 20), "FPS: " + Mathf.Ceil(currentFPS).ToString());
        }
    }
}
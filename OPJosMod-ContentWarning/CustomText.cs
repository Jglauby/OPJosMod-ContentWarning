using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OPJosMod_ContentWarning
{
    public class CustomText
    {
        private Camera mainCamera;
        private GameObject textDisplayObject;
        private TextMesh textMesh;
        private float startDisplay;
        private float DisplayTime = -1;
        private bool hasStarted = false;

        public void Start()
        {
            mainCamera = Camera.main;
            startDisplay = Time.time;
            textDisplayObject = GameObject.Find("TextDisplay");
            if (textDisplayObject == null)
            {
                textDisplayObject = new GameObject("TextDisplay");
            }
            textMesh = textDisplayObject.GetComponent<TextMesh>();
            if (textMesh == null)
            {
                textMesh = textDisplayObject.AddComponent<TextMesh>();
            }

            hasStarted = true;
        }

        public void Update()
        {
            if (!hasStarted)
                return;

            if (mainCamera != null && Time.time - startDisplay < DisplayTime)
            {
                textDisplayObject.gameObject.SetActive(true);

                Vector3 newPosition = mainCamera.transform.position + mainCamera.transform.forward * 2f;
                newPosition.y -= 0.5f; 
                textDisplayObject.transform.position = newPosition;

                Quaternion targetRotation = Quaternion.LookRotation(textDisplayObject.transform.position - mainCamera.transform.position);
                textDisplayObject.transform.rotation = targetRotation;
            }
            else 
            {
                textDisplayObject.gameObject.SetActive(false);
            }
        }

        public void DisplayText(string text, float displayTime)
        {
            startDisplay = Time.time;
            DisplayTime = displayTime;

            textMesh.text = text;
            textMesh.fontSize = 24;
            textMesh.characterSize = 0.1f;
            textMesh.color = Color.white;
            textDisplayObject.transform.position = new Vector3(0, 1, 0);

            if (mainCamera != null)
            {
                Vector3 position = mainCamera.transform.position + mainCamera.transform.forward * 2f;
                textDisplayObject.transform.position = position;
                Quaternion rotation = Quaternion.LookRotation(textDisplayObject.transform.position - mainCamera.transform.position);
                textDisplayObject.transform.rotation = rotation;
            }
        }
    }
}

﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.VR.WSA.Input;

namespace HoloToolkit.Unity
{
    /// <summary>
    /// GestureManager creates a gesture recognizer and signs up for a tap gesture.
    /// When a tap gesture is detected, GestureManager uses GazeManager to find the game object.
    /// GestureManager then sends a message to that game object.
    /// </summary>
    [RequireComponent(typeof(GazeManager))]
    public partial class GestureManager : Singleton<GestureManager>
    {
        //public GameObject focusedobject;
        public TextMesh text;
        public GameObject InCanvas;

        /// <summary>
        /// To select even when a hologram is not being gazed at,
        /// set the override focused object.
        /// If its null, then the gazed at object will be selected.
        /// </summary>
        public GameObject OverrideFocusedObject
        {
            get; set;
        }

        /// <summary>
        /// Gets the currently focused object, or null if none.
        /// </summary>
        public GameObject FocusedObject
        {
            get { return focusedObject; }
        }

        private GestureRecognizer gestureRecognizer;
        private GameObject focusedObject;

        void Start()
        {
            // Create a new GestureRecognizer. Sign up for tapped events.
            gestureRecognizer = new GestureRecognizer();
            gestureRecognizer.SetRecognizableGestures(GestureSettings.Tap | GestureSettings.Hold);

            gestureRecognizer.TappedEvent += GestureRecognizer_TappedEvent;
            gestureRecognizer.HoldStartedEvent += GestureRecognizer_HoldStartedEvent1;
            gestureRecognizer.HoldCanceledEvent += GestureRecognizer_HoldCanceledEvent;
            gestureRecognizer.HoldCompletedEvent += GestureRecognizer_HoldCompletedEvent;

            // Start looking for gestures.
            gestureRecognizer.StartCapturingGestures();
        }

        private void GestureRecognizer_HoldCompletedEvent(InteractionSourceKind source, Ray headRay)
        {
            //throw new System.NotImplementedException();
            InCanvas.SendMessage("OnHoldCompleted", focusedObject);
            text.text = "HoldCompleted";
        }

        private void GestureRecognizer_HoldCanceledEvent(InteractionSourceKind source, Ray headRay)
        {
            //throw new System.NotImplementedException();
            InCanvas.SendMessage("OnHoldCanceled", focusedObject);
            text.text = "HoldCanceled";
        }

        private void GestureRecognizer_HoldStartedEvent1(InteractionSourceKind source, Ray headRay)
        {
            //throw new System.NotImplementedException();
            InCanvas.SendMessage("OnHoldStarted", focusedObject);
            text.text = "HoldStarted";
        }


        private void GestureRecognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay)
        {
            if (focusedObject == null)
            {
                text.text = null;
            }
            else
            {
                text.text = focusedObject.name;
            }

            InCanvas.SendMessage("OnSelect", focusedObject);
            
            
            //text.text = "Select";
        }

        void LateUpdate()
        {
            GameObject oldFocusedObject = focusedObject;

            if (GazeManager.Instance.Hit &&
                OverrideFocusedObject == null &&
                GazeManager.Instance.HitInfo.collider != null)
            {
                // If gaze hits a hologram, set the focused object to that game object.
                // Also if the caller has not decided to override the focused object.
                focusedObject = GazeManager.Instance.HitInfo.collider.gameObject;
            }
            else
            {
                // If our gaze doesn't hit a hologram, set the focused object to null or override focused object.
                focusedObject = OverrideFocusedObject;
            }

            if (focusedObject != oldFocusedObject)
            {
                // If the currently focused object doesn't match the old focused object, cancel the current gesture.
                // Start looking for new gestures.  This is to prevent applying gestures from one hologram to another.
                gestureRecognizer.CancelGestures();
                gestureRecognizer.StartCapturingGestures();
            }
        }

        void OnDestroy()
        {
            gestureRecognizer.StopCapturingGestures();
            gestureRecognizer.TappedEvent -= GestureRecognizer_TappedEvent;
        }
    }
}
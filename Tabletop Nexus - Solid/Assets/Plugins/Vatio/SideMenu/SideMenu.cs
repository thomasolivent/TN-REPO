using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Vatio.UI
{
    /// <summary>
    /// This class shows getures- or buttons-controlled side menu.
    /// </summary>
    /// <remarks>
    /// To use just attach the script to any GameObject in the scene.
    /// For clean project it's recommended to attach it to the parent of the side menu in the UI section.
    /// Assign 'Menu' and 'Full Screen Canvas' properties and You are ready to go.
    /// At the moment there can be only one side menu on each side of the screen (4 total).
    /// </remarks>
    public class SideMenu : MonoBehaviour
    {
        #region ENUMS

        /// <summary>
        /// Enumerates possible positions of the menu in the app.
        /// </summary>
        public enum Side { left, top, right, bottom };

        /// <summary>
        /// Enumerates possible states of the menu.
        /// </summary>
        public enum MenuState { Out, SlidingIn, In, SlidingOut, TouchIn, TouchOut };

        #endregion ENUMS

        #region EXTERNAL_REFERENCES

        [Tooltip("The RectTransform component of the side menu GameObject (topmost GameObject of side menu).")]
        public RectTransform menu;

        [Tooltip("The main UI canvas which the side menu is placed under in the scene hierarchy.")]
        public Canvas fullScreenCanvas;

        [Tooltip("Toggle button of the menu, if there is any. The button will be parented to the menu so it moves with it and won't be included in the snapshot when the menu is open. The button itself still needs a separate script to work.")]
        public RectTransform toggleButton;

        [Tooltip("A list of GameObjects that shouldn't be included in the snapshot taken and moved when the menu is being opened.")]
        public List<GameObject> dontIncludeInSnapshot;

        #endregion EXTERNAL_REFERENCES

        #region CONFIGURATION_VARIABLES

        public Side side;

        public bool initiallyOpen = false;
        public Vector2 sizeRelativeToScreen = new Vector2(1.0f, 1.0f);
        public float animationDuration = 1.0f;

        [Tooltip("If enabled, will take a snapshot of the entire screen that is visible and move it with the menu, so it looks like while the menu is slided from side, the main screen slides out.")]
        public bool moveRestWhenOpen = false;

        public bool UseTouches = true;
        public bool UseMouse = true;

        [Tooltip("How far the input needs to go before the menu starts sliding in or out. The value is relative to the screen and to the menu size in appropriate dimension. Example: if the menu is on the left side and set to be half the screen and this value is set to 0.05, than the user needs to move to input: 0.05 * 0.5 = 0.025 part of the screen before the menu slide begins (or 2.5%). This stops the menu from catching all user input.")]
        public float touchThreshold = 0.05f;

        #endregion CONFIGURATION_VARIABLES

        #region PROTECTED_VARIABLES

        protected MenuState menuState = MenuState.In;

        protected int touchID;
        protected float touchBeganTime;
        protected Vector2 touchBegan;
        protected Vector2 lastTouch;
        protected Vector2 positionIn;
        protected Vector2 positionOut;

        protected float animationBegan;
        protected Vector2 anchorMinBegan;
        protected Vector2 anchorChangeRequired;
        protected float desiredDuration;

        protected RawImage snapshotImage = null;

        protected static bool isAMenuOpen = false;
        protected static object isAMenuOpenLock = new object();
        protected bool hasMenuOpen = false;

        protected Dictionary<GameObject, bool> objectsActiveBeforeTakingSnapshot;
        protected bool toggleButtonActiveBeforeTakingSnapshot;

        #endregion PROTECTED_VARIABLES

        #region MENU_POSITIONS

        /// <summary>
        /// This function determines the desired position of the menu when it's shown.
        /// </summary>
        /// <returns>Position for the menu when it's visible.</returns>
        private Vector2 GetMenuPositionIn()
        {
            switch (side)
            {
                case Side.left:
                    return new Vector2(0.0f, (1.0f - sizeRelativeToScreen.y) / 2.0f);

                case Side.top:
                    return new Vector2((1.0f - sizeRelativeToScreen.x) / 2.0f, 1.0f - sizeRelativeToScreen.y);

                case Side.right:
                    return new Vector2(1.0f - sizeRelativeToScreen.x, (1.0f - sizeRelativeToScreen.y) / 2.0f);

                case Side.bottom:
                    return new Vector2((1.0f - sizeRelativeToScreen.x) / 2.0f, 0.0f);
            }

            return new Vector2(0.0f, (1.0f - sizeRelativeToScreen.y) / 2.0f);
        }

        /// <summary>
        /// This function determines the desired position of the menu when it's hidden.
        /// </summary>
        /// <returns>Position for the menu when it's out of the screen.</returns>
        private Vector2 GetMenuPositionOut()
        {
            switch (side)
            {
                case Side.left:
                    return new Vector2(-sizeRelativeToScreen.x, (1.0f - sizeRelativeToScreen.y) / 2.0f);

                case Side.top:
                    return new Vector2((1.0f - sizeRelativeToScreen.x) / 2.0f, 1.0f);

                case Side.right:
                    return new Vector2(1.0f, (1.0f - sizeRelativeToScreen.y) / 2.0f);

                case Side.bottom:
                    return new Vector2((1.0f - sizeRelativeToScreen.x) / 2.0f, -sizeRelativeToScreen.y);
            }

            return new Vector2(-sizeRelativeToScreen.x, (1.0f - sizeRelativeToScreen.y) / 2.0f);
        }

        #endregion MENU_POSITIONS

        #region MENU_STATES

        /// <summary>
        /// This is the setter for the menu state.
        /// It sets the new state and sends a message to every child GameObject that the menu changed state.
        /// </summary>
        /// <param name="state">The new menu state.</param>
        private void SetMenuState(MenuState state)
        {
            menuState = state;
            BroadcastMessage("SideMenuStateChanged", state, SendMessageOptions.DontRequireReceiver);
        }

        #endregion MENU_STATES

        #region USER_INPUT

        /// <summary>
        /// This is a class holding unified input from mouse and touch to be used in the main class.
        /// </summary>
        private class MouseTouchInput : IEquatable<MouseTouchInput>
        {
            public TouchPhase phase;
            public Vector2 position;
            public int id;

            public bool Equals(MouseTouchInput other)
            {
                return ((phase == other.phase) && ((position - other.position).SqrMagnitude() < 0.1f));
            }
        }

        /// <summary>
        /// This function checks for mouse inputs.
        /// </summary>
        /// <returns>Array of inputs from mouse.</returns>
        private MouseTouchInput[] GetMouseAsTouch()
        {
            List<MouseTouchInput> mouseButtonsClicked = new List<MouseTouchInput>();

            for (int i = 0; i < 3; i++)
            {
                if (Input.GetMouseButtonDown(i))
                {
                    MouseTouchInput t = new MouseTouchInput();
                    t.phase = TouchPhase.Began;
                    t.position = Input.mousePosition;
                    t.id = -i;
                    mouseButtonsClicked.Add(t);
                    continue;
                }
                if (Input.GetMouseButtonUp(i))
                {
                    MouseTouchInput t = new MouseTouchInput();
                    t.phase = TouchPhase.Ended;
                    t.position = Input.mousePosition;
                    t.id = -i;
                    mouseButtonsClicked.Add(t);
                    continue;
                }
                if (Input.GetMouseButton(i))
                {
                    MouseTouchInput t = new MouseTouchInput();
                    t.phase = TouchPhase.Moved;
                    t.position = Input.mousePosition;
                    t.id = -i;
                    mouseButtonsClicked.Add(t);
                    continue;
                }
            }

            return mouseButtonsClicked.ToArray();
        }

        /// <summary>
        /// This function checks for touch inputs.
        /// </summary>
        /// <returns>Array of touch inputs</returns>
        private MouseTouchInput[] GetTouches()
        {
            MouseTouchInput[] touches = new MouseTouchInput[Input.touchCount];

            for (int i = 0; i < Input.touchCount; i++)
            {
                MouseTouchInput t = new MouseTouchInput();
                t.phase = Input.touches[i].phase;
                t.position = Input.touches[i].position;
                t.id = Input.touches[i].fingerId;
                touches[i] = t;
            }

            return touches;
        }

        /// <summary>
        /// This function returns exactly one or none input according to the set input methods.
        /// When there are more inputs they are ignored.
        /// The behaviour is different for different "Input.simulateMouseWithTouches" values
        /// </summary>
        /// <returns>One input or NULL if none</returns>
        private MouseTouchInput GetOneTouch()
        {
            List<MouseTouchInput> inputs = new List<MouseTouchInput>();

            if (UseMouse)
            {
                inputs.AddRange(GetMouseAsTouch());

                if (Input.simulateMouseWithTouches)
                {
                    if (!UseTouches)
                    {
                        MouseTouchInput[] inputsToRemove = GetTouches();
                        foreach (MouseTouchInput input in inputsToRemove)
                            inputs.Remove(input);
                    }
                }
                else
                {
                    if (UseTouches)
                    {
                        inputs.AddRange(GetTouches());
                    }
                }
            }
            else
            {
                if (UseTouches)
                {
                    inputs.AddRange(GetTouches());
                }
            }

            if (inputs.Count == 1)
            {
                MouseTouchInput theInput = inputs[0];

                switch (theInput.phase)
                {
                    case TouchPhase.Began:
                        touchID = theInput.id;
                        touchBegan = theInput.position;
                        touchBeganTime = Time.timeSinceLevelLoad;
                        return theInput;

                    case TouchPhase.Moved:
                        if (theInput.id != touchID)
                        {
                            touchID = int.MinValue;
                            return null;
                        }
                        return theInput;

                    case TouchPhase.Stationary:
                        if (theInput.id != touchID)
                        {
                            touchID = int.MinValue;
                            return null;
                        }
                        return theInput;

                    case TouchPhase.Ended:
                        if (theInput.id != touchID)
                        {
                            touchID = int.MinValue;
                            return null;
                        }
                        return theInput;

                    case TouchPhase.Canceled:
                        {
                            touchID = int.MinValue;
                            return null;
                        }

                    default:
                        {
                            touchID = int.MinValue;
                            return null;
                        }
                }
            }

            touchID = int.MinValue;
            return null;
        }

        #endregion USER_INPUT

        #region SNAPSHOT

        /// <summary>
        /// This method initiates snapshot taking when menu is being opened.
        /// It creates a GameObject for the snapshot and sets the SideMenu object into snapshot taking mode.
        /// The snapshot needs to be RawImage due to a Unity bug with Sprites on iOS.
        /// All objects that should not be displayed in the snapshot are hidden here
        /// </summary>
        private void TakeAndDisplaySnapshot()
        {
            if (snapshotImage == null)              // Create the snapshot RawImage if it does not exist yet
            {
                // Create image child for snapshot
                GameObject go = new GameObject();
                go.name = "MenuSlider Snapshot";

                RectTransform rt = go.AddComponent<RectTransform>();
                rt.SetParent(fullScreenCanvas.transform, false);

                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;

                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                snapshotImage = go.AddComponent<RawImage>();

                rt.SetParent(menu.transform, true);

                go.SetActive(false);
                if (toggleButton != null)
                    toggleButton.SetAsLastSibling();
            }

            if (!snapshotImage.gameObject.activeInHierarchy)        // If the snapshot is not already displayed, hide all objects that should not be visible on the snapshot and than take and display it
            {
                if (toggleButton != null)
                {
                    toggleButtonActiveBeforeTakingSnapshot = toggleButton.gameObject.activeSelf;
                    toggleButton.gameObject.SetActive(false);
                }

                if (dontIncludeInSnapshot != null)
                {
                    objectsActiveBeforeTakingSnapshot = new Dictionary<GameObject, bool>();

                    for (int i = 0; i < dontIncludeInSnapshot.Count; i++)
                    {
                        GameObject go = dontIncludeInSnapshot[i];

                        if (go != null)
                        {
                            objectsActiveBeforeTakingSnapshot.Add(go, go.activeSelf);
                            go.SetActive(false);
                        }
                    }
                }

                StartCoroutine(TakeSnapshot());
            }
        }

        /// <summary>
        /// This method takes the snapshot at the end of the frame.
        /// It needs to be called as a Coroutine.
        /// </summary>
        private IEnumerator TakeSnapshot()
        {
            yield return new WaitForEndOfFrame();

            if (!snapshotImage.gameObject.activeInHierarchy)
            {
                Texture2D snapshot = new Texture2D(Screen.width, Screen.height);
                snapshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                snapshot.Apply();

                snapshotImage.texture = snapshot;
            }

            PostSnapshotCleanup();

            if (!snapshotImage.gameObject.activeInHierarchy)
            {
                yield return null;
                snapshotImage.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// This method displays all objects that were hidden for snapshot taking
        /// It's important that the order of objects and toggleButton be the opposite of what it is in the PrepareForTakingSnapshot method
        /// That way if the user adds the toggle button to the dontIncludeInSnapshot variable the script will still correctly remember and assign it's state
        /// </summary>
        private void PostSnapshotCleanup()
        {
            if (objectsActiveBeforeTakingSnapshot != null)
            {
                foreach (KeyValuePair<GameObject, bool> goData in objectsActiveBeforeTakingSnapshot)
                {
                    goData.Key.SetActive(goData.Value);
                }

                objectsActiveBeforeTakingSnapshot = null;
            }

            if (toggleButton != null)
                toggleButton.gameObject.SetActive(toggleButtonActiveBeforeTakingSnapshot);
        }

        /// <summary>
        /// This method hides the snapshot when menu is closed.
        /// </summary>
        private void HideSnapshot()
        {
            if (snapshotImage != null)
                snapshotImage.gameObject.SetActive(false);
        }

        #endregion SNAPSHOT

        #region MATH_UTILS

        /// <summary>
        /// Method calculating quadratic ease in.
        /// </summary>
        /// <param name="time">Time elapsed since the beginning of the animation</param>
        /// <param name="initialValue">Initial value</param>
        /// <param name="changeInValue">Desired change in value</param>
        /// <param name="duration">Total desired time of the animation</param>
        /// <returns>Transition value at the supplied time in the range [0, 1]</returns>
        private Vector2 EaseIn(float time, Vector2 initialValue, Vector2 changeInValue, float duration)
        {
            float x = time / duration;
            float y = x * x;

            return initialValue + changeInValue * y;
        }

        /// <summary>
        /// Method calculating quadratic ease out.
        /// </summary>
        /// <param name="time">Time elapsed since the beginning of the animation</param>
        /// <param name="initialValue">Initial value</param>
        /// <param name="changeInValue">Desired change in value</param>
        /// <param name="duration">Total desired time of the animation</param>
        /// <returns>Transition value at the supplied time in the range [0, 1]</returns>
        private Vector2 EaseOut(float time, Vector2 initialValue, Vector2 changeInValue, float duration)
        {
            float x = time / duration;
            float y = 1 - (x - 1.0f) * (x - 1.0f);

            return initialValue + changeInValue * y;
        }

        #endregion MATH_UTILS

        #region SIDEMENU_UTILS

        /// <summary>
        /// Sets the position of the menu.
        /// </summary>
        /// <param name="newAnchorMin">Minimum anchor to be set.</param>
        private void SetPosition(Vector2 newAnchorMin)
        {
            menu.anchorMin = newAnchorMin;
            menu.anchorMax = newAnchorMin + sizeRelativeToScreen;
        }

        #endregion SIDEMENU_UTILS

        #region STATES_CHANGES

        /// <summary>
        /// Begins gesture sliding the SideMenu onto the screen
        /// </summary>
        private void BeginTouchIn()
        {
            if (moveRestWhenOpen)
                TakeAndDisplaySnapshot();

            positionIn = GetMenuPositionIn();
            positionOut = GetMenuPositionOut();
            SetMenuState(MenuState.TouchIn);
        }

        /// <summary>
        /// Begins gesture sliding the SideMenu out of the screen
        /// </summary>
        private void BeginTouchOut()
        {
            positionIn = GetMenuPositionIn();
            positionOut = GetMenuPositionOut();
            SetMenuState(MenuState.TouchOut);
        }

        #endregion STATES_CHANGES

        #region SLIDE_UTILS

        /// <summary>
        /// Checks if the input has reached the slide in threshold to begin sliding the menu in.
        /// </summary>
        /// <param name="deltaTouch">Touch movement since it's beginning</param>
        /// <returns>True if the input has reached the threshold and false otherwise.</returns>
        private bool ReachedSlideInThreshold(Vector2 deltaTouch)
        {
            switch (side)
            {
                case Side.left:
                    return deltaTouch.x > touchThreshold;

                case Side.top:
                    return deltaTouch.y < -touchThreshold;

                case Side.right:
                    return deltaTouch.x < -touchThreshold;

                case Side.bottom:
                    return deltaTouch.y > touchThreshold;

                default:
                    return deltaTouch.x > touchThreshold;
            }
        }

        /// <summary>
        /// Checks if the input has reached the slide out threshold to begin sliding the menu out.
        /// </summary>
        /// <param name="deltaTouch">Touch movement since it's beginning</param>
        /// <returns>True if the input has reached the threshold and false otherwise.</returns>
        private bool ReachedSlideOutThreshold(Vector2 deltaTouch)
        {
            switch (side)
            {
                case Side.left:
                    return deltaTouch.x < -touchThreshold;

                case Side.top:
                    return deltaTouch.y > touchThreshold;

                case Side.right:
                    return deltaTouch.x > touchThreshold;

                case Side.bottom:
                    return deltaTouch.y < -touchThreshold;

                default:
                    return deltaTouch.x < -touchThreshold;
            }
        }

        #endregion SLIDE_UTILS

        #region STATES_PROCESSING

        /// <summary>
        /// Method called each frame to process the current state
        /// It calls the appropriate method for processing the current state
        /// </summary>
        private void ProcessCurrentState()
        {
            switch (menuState)
            {
                case MenuState.Out:
                    ProcessMenuOut();
                    break;

                case MenuState.SlidingIn:
                    ProcessMenuSlidingIn();
                    break;

                case MenuState.In:
                    ProcessMenuIn();
                    break;

                case MenuState.SlidingOut:
                    ProcessMenuSlidingOut();
                    break;

                case MenuState.TouchIn:
                    ProcessMenuTouchIn();
                    break;

                case MenuState.TouchOut:
                    ProcessMenuTouchOut();
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Method called each frame when the menu is out.
        /// It checks for possible slide in triggers, but only if no other menu is shown.
        /// </summary>
        private void ProcessMenuOut()
        {
            MouseTouchInput touch = GetOneTouch();
            if (touch != null)
            {
                Vector2 deltaTouch = new Vector2((touch.position.x - touchBegan.x) / Screen.width, (touch.position.y - touchBegan.y) / Screen.height);
                if (ReachedSlideInThreshold(deltaTouch))
                {
                    lock (isAMenuOpenLock)
                    {
                        if (!isAMenuOpen)
                        {
                            isAMenuOpen = true;
                            hasMenuOpen = true;
                            BeginTouchIn();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Method called each frame when the menu is sliding in.
        /// It checks for possible slide out triggers.
        /// </summary>
        private void ProcessMenuSlidingIn()
        {
            MouseTouchInput theInput = GetOneTouch();

            if (theInput != null)
            {
                MouseTouchInput touch = GetOneTouch();
                if (touch != null)
                {
                    Vector2 deltaTouch = new Vector2((touch.position.x - touchBegan.x) / Screen.width, (touch.position.y - touchBegan.y) / Screen.height);
                    if (ReachedSlideOutThreshold(deltaTouch))
                        BeginTouchOut();
                }
            }

            float time = Time.timeSinceLevelLoad - animationBegan;
            if (time > desiredDuration)
            {
                SetMenuState(MenuState.In);

                SetPosition(anchorMinBegan + anchorChangeRequired);
            }
            else
            {
                SetPosition(EaseOut(time, anchorMinBegan, anchorChangeRequired, desiredDuration));
            }
        }

        /// <summary>
        /// Method called each frame when the menu is in.
        /// It checks for possible slide out triggers.
        /// </summary>
        private void ProcessMenuIn()
        {
            MouseTouchInput touch = GetOneTouch();
            if (touch != null)
            {
                Vector2 deltaTouch = new Vector2((touch.position.x - touchBegan.x) / Screen.width, (touch.position.y - touchBegan.y) / Screen.height);
                if (ReachedSlideOutThreshold(deltaTouch))
                    BeginTouchOut();
            }
        }

        /// <summary>
        /// Method called each frame when the menu is sliding out.
        /// It checks for possible slide in triggers, but only if no other menu is shown.
        /// </summary>
        private void ProcessMenuSlidingOut()
        {
            MouseTouchInput theInput = GetOneTouch();

            if (theInput != null)
            {
                MouseTouchInput touch = GetOneTouch();
                if (touch != null)
                {
                    Vector2 deltaTouch = new Vector2((touch.position.x - touchBegan.x) / Screen.width, (touch.position.y - touchBegan.y) / Screen.height);
                    if (ReachedSlideInThreshold(deltaTouch))
                        lock (isAMenuOpenLock)
                        {
                            if (hasMenuOpen || !isAMenuOpen)
                            {
                                isAMenuOpen = true;
                                hasMenuOpen = true;
                                BeginTouchIn();
                            }
                        }
                }
            }

            float time = Time.timeSinceLevelLoad - animationBegan;
            if (time > desiredDuration)
            {
                SetMenuState(MenuState.Out);

                if (hasMenuOpen)
                {
                    lock (isAMenuOpenLock)
                    {
                        hasMenuOpen = false;
                        isAMenuOpen = false;
                    }
                }

                SetPosition(anchorMinBegan + anchorChangeRequired);

                if (moveRestWhenOpen)
                    HideSnapshot();
            }
            else
            {
                SetPosition(EaseIn(time, anchorMinBegan, anchorChangeRequired, desiredDuration));
            }
        }

        /// <summary>
        /// Method called each frame when the menu is being gestured in.
        /// It checks for gesture end to begin either slide in or slide out transition.
        /// </summary>
        private void ProcessMenuTouchIn()
        {
            MouseTouchInput theInput = GetOneTouch();

            Vector2 deltaTouch;
            float directionDeltaTouch = 0.0f;

            bool end = false;

            if (theInput == null)
            {
                end = true;
            }
            else
            {
                if ((theInput.phase == TouchPhase.Ended) || (theInput.phase == TouchPhase.Canceled))
                    end = true;
            }

            if (end)
            {
                deltaTouch = new Vector2((lastTouch.x - touchBegan.x) / Screen.width / sizeRelativeToScreen.x, (lastTouch.y - touchBegan.y) / Screen.height / sizeRelativeToScreen.y);
                float deltaTime = Time.timeSinceLevelLoad - touchBeganTime;

                switch (side)
                {
                    case Side.left:
                        directionDeltaTouch = deltaTouch.x;
                        break;

                    case Side.top:
                        directionDeltaTouch = -deltaTouch.y;
                        break;

                    case Side.right:
                        directionDeltaTouch = -deltaTouch.x;
                        break;

                    case Side.bottom:
                        directionDeltaTouch = deltaTouch.y;
                        break;
                }

                if ((directionDeltaTouch * 3.0f > deltaTime) || (directionDeltaTouch > 0.6f))
                {
                    SlideIn();
                }
                else
                {
                    SlideOut();
                }

                return;
            }

            deltaTouch = new Vector2((theInput.position.x - touchBegan.x) / Screen.width, (theInput.position.y - touchBegan.y) / Screen.height);

            switch (side)
            {
                case Side.left:
                    directionDeltaTouch = deltaTouch.x / sizeRelativeToScreen.x;
                    break;

                case Side.top:
                    directionDeltaTouch = -deltaTouch.y / sizeRelativeToScreen.y;
                    break;

                case Side.right:
                    directionDeltaTouch = -deltaTouch.x / sizeRelativeToScreen.x;
                    break;

                case Side.bottom:
                    directionDeltaTouch = deltaTouch.y / sizeRelativeToScreen.y;
                    break;
            }

            if (directionDeltaTouch > 1.0f)
                directionDeltaTouch = 1.0f;

            if (directionDeltaTouch < 0)
            {
                touchBegan = theInput.position;
            }
            else
            {
                SetPosition(positionIn * directionDeltaTouch + positionOut * (1.0f - directionDeltaTouch));
            }

            lastTouch = theInput.position;
        }

        /// <summary>
        /// Method called each frame when the menu is being gestured out.
        /// It checks for gesture end to begin either slide out or slide in transition.
        /// </summary>
        private void ProcessMenuTouchOut()
        {
            MouseTouchInput theInput = GetOneTouch();

            Vector2 deltaTouch;
            float directionDeltaTouch = 0.0f;

            bool end = false;

            if (theInput == null)
            {
                end = true;
            }
            else
            {
                if ((theInput.phase == TouchPhase.Ended) || (theInput.phase == TouchPhase.Canceled))
                    end = true;
            }

            if (end)
            {
                deltaTouch = new Vector2((lastTouch.x - touchBegan.x) / Screen.width / sizeRelativeToScreen.x, (lastTouch.y - touchBegan.y) / Screen.height / sizeRelativeToScreen.y);
                float deltaTime = Time.timeSinceLevelLoad - touchBeganTime;

                switch (side)
                {
                    case Side.left:
                        directionDeltaTouch = -deltaTouch.x;
                        break;

                    case Side.top:
                        directionDeltaTouch = deltaTouch.y;
                        break;

                    case Side.right:
                        directionDeltaTouch = deltaTouch.x;
                        break;

                    case Side.bottom:
                        directionDeltaTouch = -deltaTouch.y;
                        break;
                }

                if ((directionDeltaTouch * 3.0f > deltaTime) || (directionDeltaTouch > 0.6f))
                {
                    SlideOut();
                }
                else
                {
                    SlideIn();
                }

                return;
            }

            deltaTouch = new Vector2((theInput.position.x - touchBegan.x) / Screen.width, (theInput.position.y - touchBegan.y) / Screen.height);

            switch (side)
            {
                case Side.left:
                    directionDeltaTouch = -deltaTouch.x / sizeRelativeToScreen.x;
                    break;

                case Side.top:
                    directionDeltaTouch = deltaTouch.y / sizeRelativeToScreen.y;
                    break;

                case Side.right:
                    directionDeltaTouch = deltaTouch.x / sizeRelativeToScreen.x;
                    break;

                case Side.bottom:
                    directionDeltaTouch = -deltaTouch.y / sizeRelativeToScreen.y;
                    break;
            }

            if (directionDeltaTouch > 1.0f)
                directionDeltaTouch = 1.0f;

            if (directionDeltaTouch < 0)
            {
                touchBegan = theInput.position;
            }
            else
            {
                SetPosition(positionOut * directionDeltaTouch + positionIn * (1.0f - directionDeltaTouch));
            }

            lastTouch = theInput.position;
        }

        #endregion STATES_PROCESSING

        #region MONOBEHAVIOUR_METHODS

        /// <summary>
        /// Method called at the start of the scene. It set's initial menu position and state and parents the toggle button if there is any.
        /// </summary>
        public void Start()
        {
            if (initiallyOpen && !isAMenuOpen)
            {
                SetPosition(GetMenuPositionIn());
                SetMenuState(MenuState.In);
                lock (isAMenuOpenLock)
                {
                    isAMenuOpen = true;
                    hasMenuOpen = true;
                }
            }
            else
            {
                SetPosition(GetMenuPositionOut());
                SetMenuState(MenuState.Out);
            }

            if (toggleButton != null)
            {
                toggleButton.SetParent(menu, true);
            }
        }

        /// <summary>
        /// Method called every frame. It initiates taking the snapshot if it was called and calls appropriate state processing method depending on the current state.
        /// </summary>
        public void Update()
        {
            ProcessCurrentState();
        }

        /// <summary>
        /// Method called when the script is destroyed. It releases the `isAMenuOpen` lock if the menu has it.
        /// </summary>
        public void OnDestroy()
        {
            if (hasMenuOpen)
            {
                lock (isAMenuOpenLock)
                {
                    hasMenuOpen = false;
                    isAMenuOpen = false;
                }
            }

            if (snapshotImage != null)
            {
                DestroyImmediate(snapshotImage.gameObject);
            }
        }

        #endregion MONOBEHAVIOUR_METHODS

        #region PUBLIC_METHODS

        /// <summary>
        /// Shows the menu.
        /// </summary>
        public void SlideIn()
        {
            if (menuState == MenuState.Out || menuState == MenuState.TouchIn || menuState == MenuState.TouchOut)
            {

                lock (isAMenuOpenLock)
                {
                    if (!isAMenuOpen || hasMenuOpen)
                    {
                        hasMenuOpen = true;
                        isAMenuOpen = true;
                    }
                    else
                    {
                        Debug.LogError("Can not open a side menu while another one is open");
                        return;
                    }
                }

                if (moveRestWhenOpen)
                    TakeAndDisplaySnapshot();

                animationBegan = Time.timeSinceLevelLoad;
                SetMenuState(MenuState.SlidingIn);

                anchorMinBegan = menu.anchorMin;

                anchorChangeRequired = GetMenuPositionIn() - menu.anchorMin;
                desiredDuration = animationDuration * anchorChangeRequired.magnitude;
            }
        }

        /// <summary>
        /// Hides the menu.
        /// </summary>
        public void SlideOut()
        {
            if (menuState == MenuState.In || menuState == MenuState.TouchIn || menuState == MenuState.TouchOut)
            {
                animationBegan = Time.timeSinceLevelLoad;
                SetMenuState(MenuState.SlidingOut);

                anchorMinBegan = menu.anchorMin;
                anchorChangeRequired = GetMenuPositionOut() - menu.anchorMin;
                desiredDuration = animationDuration * anchorChangeRequired.magnitude;
            }
        }

        /// <summary>
        /// Shows or hides the menu depending on the current state.
        /// </summary>
        public void Toggle()
        {
            if (menuState == MenuState.In || menuState == MenuState.TouchOut || menuState == MenuState.SlidingIn)
            {
                SlideOut();
            }
            else
            {
                SlideIn();
            }
        }

        #endregion PUBLIC_METHODS
    }
}
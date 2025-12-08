using System.Collections;
using System.Collections.Generic;
using Lineage.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using Lineage.Debug;

namespace Lineage.Managers
{
    public class SelectionManager : MonoBehaviour
    {
        // Singleton instance
        public static SelectionManager Instance { get; private set; }

        [Header("Selection Settings")]
        public LayerMask popLayerMask;
        public LayerMask groundLayerMask;
        public float dragThreshold = 10f;

        [Header("References")]
        public RectTransform selectionBox;
        public InputActionAsset inputActions;

        // Private variables
        private InputAction clickAction;
        private InputAction pointAction;
        private InputAction rightClickAction;

        private Vector2 startDragPos;
        private bool isDragging = false;
        private List<GameObject> selectedPops = new List<GameObject>();
        private Camera mainCamera;

        private void Awake()
        {
            // Ensure singleton instance
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            mainCamera = Camera.main;

            // Load the input actions if not set
            if (inputActions == null)
            {
                inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
                Log.Warning("InputActions reference not set - attempting to load from Resources", Log.LogCategory.Systems);
            }

            // Hide the selection box initially
            if (selectionBox != null)
            {
                selectionBox.gameObject.SetActive(false);
            }
            else
            {
                Log.Error("Selection box reference is missing!", Log.LogCategory.Systems);
            }
        }

        private void OnEnable()
        {
            if (inputActions != null)
            {
                // Enable input actions
                clickAction = inputActions.FindAction("UI/Click");
                pointAction = inputActions.FindAction("UI/Point");
                rightClickAction = inputActions.FindAction("UI/RightClick");

                if (clickAction != null)
                {
                    clickAction.started += OnClickStarted;
                    clickAction.canceled += OnClickCanceled;
                    clickAction.Enable();
                }

                if (pointAction != null) pointAction.Enable();
                if (rightClickAction != null)
                {
                    rightClickAction.performed += OnRightClick;
                    rightClickAction.Enable();
                }
            }
        }

        private void OnDisable()
        {
            if (clickAction != null)
            {
                clickAction.started -= OnClickStarted;
                clickAction.canceled -= OnClickCanceled;
                clickAction.Disable();
            }

            if (pointAction != null) pointAction.Disable();

            if (rightClickAction != null)
            {
                rightClickAction.performed -= OnRightClick;
                rightClickAction.Disable();
            }
        }

        private void OnClickStarted(InputAction.CallbackContext context)
        {
            // Store the drag start position
            if (pointAction != null)
            {
                startDragPos = pointAction.ReadValue<Vector2>();
                isDragging = false;
            }
        }

        private void OnClickCanceled(InputAction.CallbackContext context)
        {
            if (isDragging)
            {
                // Finalize the selection
                CalculateDragSelection();
                isDragging = false;

                // Hide selection box
                if (selectionBox != null)
                {
                    selectionBox.gameObject.SetActive(false);
                }
            }
            else
            {
                // This was a click, not a drag
                HandleSingleClick();
            }
        }

        private void OnRightClick(InputAction.CallbackContext context)
        {
            if (selectedPops.Count > 0 && mainCamera != null)
            {
                // Get the destination from mouse position
                Vector3 mousePos = pointAction != null ?
                    pointAction.ReadValue<Vector2>() :
                    Input.mousePosition;

                Ray ray = mainCamera.ScreenPointToRay(mousePos);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayerMask))
                {
                    // Tell selected pops to move to this position
                    foreach (GameObject pop in selectedPops)
                    {
                        if (pop == null) continue;
                        
                        PopController controller = pop.GetComponent<PopController>();
                        if (controller != null)
                        {
                            controller.MoveTo(hit.point);
                        }
                    }
                }
            }
        }

        private void Update()
        {
            // Update the selection box if dragging
            if (isDragging)
            {
                UpdateSelectionBox();
            }
            else if (pointAction != null)
            {
                // Check for drag start
                Vector2 currentMousePos = pointAction.ReadValue<Vector2>();
                float dragDistance = Vector2.Distance(startDragPos, currentMousePos);

                if (dragDistance > dragThreshold && clickAction.phase == InputActionPhase.Started)
                {
                    isDragging = true;
                    ClearSelection();

                    if (selectionBox != null)
                    {
                        selectionBox.gameObject.SetActive(true);
                    }
                }
            }
        }

        private void UpdateSelectionBox()
        {
            if (selectionBox == null || pointAction == null) return;

            Vector2 currentMousePos = pointAction.ReadValue<Vector2>();
            Vector2 lowerLeft = new Vector2(
                Mathf.Min(startDragPos.x, currentMousePos.x),
                Mathf.Min(startDragPos.y, currentMousePos.y)
            );
            Vector2 upperRight = new Vector2(
                Mathf.Max(startDragPos.x, currentMousePos.x),
                Mathf.Max(startDragPos.y, currentMousePos.y)
            );

            // Update selection box position and size
            selectionBox.position = lowerLeft;
            selectionBox.sizeDelta = upperRight - lowerLeft;
        }

        private void CalculateDragSelection()
        {
            if (pointAction == null) return;

            Vector2 currentMousePos = pointAction.ReadValue<Vector2>();

            Rect selectionRect = new Rect(
                Mathf.Min(startDragPos.x, currentMousePos.x),
                Mathf.Min(startDragPos.y, currentMousePos.y),
                Mathf.Abs(currentMousePos.x - startDragPos.x),
                Mathf.Abs(currentMousePos.y - startDragPos.y)
            );

            // Use PopulationManager's cached list instead of expensive FindGameObjectsWithTag
            if (PopulationManager.Instance != null)
            {
                var allPops = PopulationManager.Instance.GetLivingPops();
                foreach (var pop in allPops)
                {
                    if (pop == null) continue;
                    Vector2 screenPos = mainCamera.WorldToScreenPoint(pop.transform.position);
                    if (selectionRect.Contains(screenPos))
                    {
                        AddToSelectionWithLimit(pop.gameObject);
                    }
                }
            }
        }

        private void HandleSingleClick()
        {
            if (pointAction == null || mainCamera == null) return;

            Vector2 clickPos = pointAction.ReadValue<Vector2>();
            Ray ray = mainCamera.ScreenPointToRay(clickPos);

            bool shiftPressed = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, popLayerMask))
            {
                // If not holding shift, clear previous selection
                if (!shiftPressed)
                {
                    ClearSelection();
                }

                AddToSelectionWithLimit(hit.collider.gameObject);
            }
            else if (!shiftPressed)
            {
                // Clicked on nothing while not holding shift - clear selection
                ClearSelection();
            }
        }

        private void AddToSelectionWithLimit(GameObject obj)
        {
            const int MAX_SELECTION = 50; // Prevent performance degradation from selecting 100+ pops
            
            if (selectedPops.Contains(obj)) return; // Already selected, skip
            
            if (selectedPops.Count >= MAX_SELECTION)
            {
                // Remove oldest selection (FIFO) - it's the first in the list
                GameObject oldestPop = selectedPops[0];
                selectedPops.RemoveAt(0);
                PopController oldController = oldestPop.GetComponent<PopController>();
                if (oldController != null)
                {
                    oldController.OnSelected(false);
                }
                Log.Warning($"SelectionManager: Selection limit ({MAX_SELECTION}) reached. Removed earliest selection.", Log.LogCategory.Systems);
            }

            // Add new selection
            selectedPops.Add(obj);
            PopController controller = obj.GetComponent<PopController>();
            if (controller != null)
            {
                controller.OnSelected(true);
            }
        }

        public void ClearSelection()
        {
            // Notify each pop it's deselected
            foreach (GameObject pop in selectedPops)
            {
                PopController controller = pop.GetComponent<PopController>();
                if (controller != null)
                {
                    controller.OnSelected(false);
                }
            }

            selectedPops.Clear();
        }

        public List<GameObject> GetSelectedPops()
        {
            return selectedPops;
        }

        /// <summary>
        /// Returns the number of currently selected pops.
        /// </summary>
        public int GetSelectedCount() => selectedPops.Count;

        /// <summary>
        /// Checks if a specific pop is currently selected.
        /// </summary>
        public bool IsSelected(GameObject pop) => selectedPops.Contains(pop);

        /// <summary>
        /// Removes a specific pop from selection (useful for death/despawn events).
        /// </summary>
        public void RemoveFromSelection(GameObject pop)
        {
            if (selectedPops.Remove(pop))
            {
                PopController controller = pop.GetComponent<PopController>();
                if (controller != null)
                {
                    controller.OnSelected(false);
                }
            }
        }
    }
}

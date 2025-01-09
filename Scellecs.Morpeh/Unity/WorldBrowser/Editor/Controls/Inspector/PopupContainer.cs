#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class PopupContainer : VisualElement {
        internal static class Reflection {
            internal static readonly Func<VisualElement, VisualElement> GetRootVisualContainer;

            static Reflection() {
                var methodInfo = typeof(VisualElement).GetMethod("GetRootVisualContainer",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                if (methodInfo != null) {
                    GetRootVisualContainer = (Func<VisualElement, VisualElement>)
                        Delegate.CreateDelegate(typeof(Func<VisualElement, VisualElement>), methodInfo);
                }
                else {
                    Debug.LogError("Failed to create delegate for rootVisualContainer");
                    GetRootVisualContainer = (element) => null;
                }
            }
        }

        private VisualElement container;
        private VisualElement targetElement;
        private VisualElement rootContainer;
        private Vector2 mousePosition;
        private bool closeOnClickOutside = true;
        private bool closeOnParentResize = true;

        private float desiredX;
        private float desiredY;

        public VisualElement ContentContainer => this.container;

        public PopupContainer() {
            this.container = new VisualElement();
            this.container.pickingMode = PickingMode.Position;
            this.hierarchy.Add(this.container);

            this.RegisterCallback<AttachToPanelEvent>(this.OnAttachToPanel);
            this.RegisterCallback<DetachFromPanelEvent>(this.OnDetachFromPanel);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt) {
            if (evt.destinationPanel == null) {
                return;
            }

            this.RegisterCallback<PointerDownEvent>(this.OnPointerDown);
            this.RegisterCallback<PointerMoveEvent>(this.OnPointerMove);
            evt.destinationPanel.visualTree.RegisterCallback<GeometryChangedEvent>(this.OnParentResized);
            this.container.RegisterCallback<FocusOutEvent>(this.OnFocusOut);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt) {
            if (evt.originPanel == null) {
                return;
            }

            this.UnregisterCallback<PointerDownEvent>(this.OnPointerDown);
            this.UnregisterCallback<PointerMoveEvent>(this.OnPointerMove);
            evt.originPanel.visualTree.UnregisterCallback<GeometryChangedEvent>(this.OnParentResized);
            this.container.UnregisterCallback<FocusOutEvent>(this.OnFocusOut);
        }

        private void OnPointerDown(PointerDownEvent evt) {
            this.mousePosition = this.container.WorldToLocal(evt.position);

            if (this.closeOnClickOutside && !this.container.ContainsPoint(this.mousePosition)) {
                this.Hide();
                evt.StopPropagation();
            }
        }

        private void OnPointerMove(PointerMoveEvent evt) {
            this.mousePosition = this.container.WorldToLocal(evt.position);
        }

        private void OnFocusOut(FocusOutEvent evt) {
            if (this.closeOnClickOutside && !this.container.ContainsPoint(this.mousePosition)) {
                this.Hide();
            }
        }

        private void OnParentResized(GeometryChangedEvent evt) {
            if (this.closeOnParentResize) {
                this.Hide();
            }
            else {
                this.EnsureVisibilityInParent();
            }
        }

        public void Show(VisualElement target, Rect position) {
            if (target == null) {
                Debug.LogError("PopupContainer needs a target element to determine root container");
                return;
            }

            if (this.parent == null) {
                this.rootContainer = Reflection.GetRootVisualContainer(target);
                if (this.rootContainer == null) {
                    Debug.LogError("Could not find root container");
                    return;
                }
                this.rootContainer.Add(this);

                this.style.position = Position.Absolute;
                this.style.left = rootContainer.layout.x;
                this.style.top = rootContainer.layout.y;
                this.style.width = rootContainer.layout.width;
                this.style.height = rootContainer.layout.height;
            }

            this.targetElement = target;
            this.targetElement.RegisterCallback<DetachFromPanelEvent>(this.OnTargetDetached);

            this.style.display = DisplayStyle.Flex;

            var rootSpaceRect = this.rootContainer.WorldToLocal(position);
            this.desiredX = rootSpaceRect.x - this.rootContainer.layout.x;
            this.desiredY = rootSpaceRect.y + position.height - this.rootContainer.layout.y;

            this.container.style.left = this.desiredX;
            this.container.style.top = this.desiredY;
            this.container.Focus();
            this.EnsureVisibilityInParent();
        }

        public void Hide() {
            if (this.targetElement != null) {
                this.targetElement.UnregisterCallback<DetachFromPanelEvent>(this.OnTargetDetached);
                this.targetElement = null;
            }

            this.style.display = DisplayStyle.None;
        }

        private void OnTargetDetached(DetachFromPanelEvent evt) {
            this.Hide();
        }

        private void EnsureVisibilityInParent() {
            if (this.rootContainer == null) {
                return;
            }

            void OnGeometryChanged(GeometryChangedEvent evt) {
                this.container.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);

                float maxX = this.rootContainer.layout.width - this.container.layout.width;
                float x = Mathf.Clamp(this.desiredX, 0, maxX);
                this.container.style.left = x;

                float maxY = this.rootContainer.layout.height - this.container.layout.height;
                float y = Mathf.Clamp(this.desiredY, 0, maxY);
                this.container.style.top = y;
            }

            this.container.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        public void SetCloseOnClickOutside(bool value) {
            this.closeOnClickOutside = value;
        }
    }
}
#endif
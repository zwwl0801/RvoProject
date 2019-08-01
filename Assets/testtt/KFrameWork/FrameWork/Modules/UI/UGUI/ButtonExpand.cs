﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using KUtils;

namespace KFrameWork
{
    public class ButtonExpand : Button
    {
        private RectTransform rect;

        public RectTransform rectTransform
        {
            get
            {
                if (rect == null)
                    rect = GetComponent<RectTransform>();
                return rect;
            }
        }

        private CanvasRenderer _canvasrender;

        public CanvasRenderer canvasrender
        {
            get
            {
                if (_canvasrender == null)
                    _canvasrender = GetComponent<CanvasRenderer>();
                return _canvasrender;
            }
        }

        public TextExpand btnlabel;

        public override bool IsActive()
        {
            return base.IsActive();
        }

        protected override void Awake()
        {
            base.Awake();

            if (btnlabel == null)
            {
                Transform tr = this.transform.Find("Label");
                if (tr != null)
                    btnlabel = tr.GetComponent<TextExpand>();
            }

        }

        //protected override void OnRectTransformDimensionsChange()
        //{
        //    base.OnRectTransformDimensionsChange();
        //}

        //protected override void OnBeforeTransformParentChanged()
        //{
        //    base.OnBeforeTransformParentChanged();
        //}

        //protected override void OnTransformParentChanged()
        //{
        //    base.OnTransformParentChanged();
        //}

        //protected override void OnDidApplyAnimationProperties()
        //{
        //    base.OnDidApplyAnimationProperties();
        //}

        //protected override void OnCanvasGroupChanged()
        //{
        //    base.OnCanvasGroupChanged();
        //}

        //protected override void OnCanvasHierarchyChanged()
        //{
        //    base.OnCanvasHierarchyChanged();
        //}

        //public override bool IsInteractable()
        //{
        //    return base.IsInteractable();
        //}

        //public override Selectable FindSelectableOnLeft()
        //{
        //    return base.FindSelectableOnLeft();
        //}

        //public override Selectable FindSelectableOnRight()
        //{
        //    return base.FindSelectableOnRight();
        //}

        //public override Selectable FindSelectableOnUp()
        //{
        //    return base.FindSelectableOnUp();
        //}

        //public override Selectable FindSelectableOnDown()
        //{
        //    return base.FindSelectableOnDown();
        //}

        //public override void Select()
        //{
        //    base.Select();
        //}

    }
}


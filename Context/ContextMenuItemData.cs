using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Context
{
    [Serializable]
    public class ContextMenuItemData
    {
        public string id;
        public string text;

        public UnityEvent OnClick;
        public ContextMenuItemData(string id, string text, UnityEvent onClick)
        {
            this.id = id;
            this.text = text;
            OnClick = onClick;
        }
    }
}
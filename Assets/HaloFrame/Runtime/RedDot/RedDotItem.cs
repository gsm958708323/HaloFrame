using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HaloFrame
{
    public enum RedType
    {
        Normal,
        Num,
    }

    public class RedDotItem : MonoBehaviour
    {
        public string Key;
        public RedType RedType = RedType.Normal;

        Text numTxt;
        Image redImg;

        public RedDotNode Bind(string key, RedType redType = RedType.Normal)
        {
            if (string.IsNullOrEmpty(key))
                return null;
            
            Key = key;
            RedType = redType;
            InitUI();
            var node = GameManager.RedDot.AddListener(Key, OnRedCallback);
            return node;
        }

        private void Awake()
        {
            Bind(Key, RedType);
        }

        private void InitUI()
        {
            redImg = transform.Find("Red").GetComponent<Image>();
            redImg.gameObject.SetActiveEx(false);
            numTxt = transform.Find("Num").GetComponent<Text>();
            numTxt.gameObject.SetActiveEx(RedType == RedType.Num);
        }

        private void OnRedCallback(int value)
        {
            numTxt.text = value.ToString();
            redImg.gameObject.SetActiveEx(value > 0);
            numTxt.gameObject.SetActiveEx(value > 0 && RedType == RedType.Num);
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HaloFrame;

    public class GameManager : GameManagerBase
    {
        public static DriverManager DriverManager;
        public static RedDotManager RedDotManager;

        protected override void InitManager()
        {
            base.InitManager();

            DriverManager = GetManager<DriverManager>();
            RedDotManager = GetManager<RedDotManager>();
        }
    }

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FrameWork
{
    public delegate void AsynTaskDelegate(AsynTask asynTask);
    public abstract class AsynTask
    {
        #region Variables
        private AsynTaskDelegate m_callbackHandler = null;
		public AsynTaskDelegate CallBackHandler { set {m_callbackHandler = value;}}
        #endregion
        public virtual void Execute() { OnTaskDone(); }
        protected void OnTaskDone()
        {
            if (null != m_callbackHandler)
                m_callbackHandler(this);
        }
    }
	public abstract class UnityCbTask : AsynTask
	{
		public abstract void UpdateExec();
	}
}

#region

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using AN_Client.interfaces;
using AN_Client.srcNonFramed;

#endregion

namespace AN_Client.srcFramed
{
    class Dialog : IDialog, IDialogCallbacks
    {
        static Stack<Dialog> dialogHistory = new Stack<Dialog>();

        protected DialogParm args = DialogParm.Empty;
        protected readonly IDialogForm dialogForm;

        Action back;
        Action cancel;
        readonly IDialog cancelTarget;
        Action<DialogParm> ok;

        public Dialog(IDialogForm dialogForm)
        {
            this.dialogForm = dialogForm;
        }

        public Dialog(IDialogForm dialogForm, IDialog cancelTarget)
            : this(dialogForm)
        {
            this.cancelTarget = cancelTarget;
        }

        public Form Form
        {
            get { return (Form) dialogForm; }
        }

        #region IDialog Members

        void IDialog.Activate()
        {
            ((IDialog)this).Activate(args);
        }

        void IDialog.Activate(DialogParm args)
        {
            PreviousFormOnLeave(); 
            ActivateCore(args);
            HidePreviousForm();
        }

        static void PreviousFormOnLeave()
        {
            if (dialogHistory.Count > 0)
            {
                Dialog previousDialog = dialogHistory.Peek();
                previousDialog.OnLeave();
            }
        }

        void HidePreviousForm()
        {
            if (dialogHistory.Count > 0)
            {
                Form previousForm = dialogHistory.Peek().Form;
                if (Form != previousForm)
                {
                    previousForm.Visible = false;
                }
            }
        }

        void IDialog.End()
        {
            OnLeave();
            Form.Visible = false;
            dialogHistory.Clear();
        }

        Action<DialogParm> IDialog.Ok
        {
            set { ok = value; }
        }

        Action IDialog.Back
        {
            set { back = value; }
        }

        Action IDialog.Cancel
        {
            set { cancel = value; }
        }

        #endregion

        #region IDialogCallbacks Members

        void IDialogCallbacks.OnOk(DialogParm result)
        {
            dialogHistory.Push(this);
            if (ok != null)
            {
                try
                {
                    ok.Invoke(result);
                }
                catch (Exception ex)
                {
                    ToolboxMethods.ShowException("Dialog OnOk", ex);
                    ToolboxMethods.LogException(ex);
                }
            }
        }

        void IDialogCallbacks.OnBack()
        {
            NavigateBack();
            if (back != null)
            {
                try
                {
                    back.Invoke();
                }
                catch (Exception ex)
                {
                    ToolboxMethods.ShowException("Dialog OnBack", ex);
                    ToolboxMethods.LogException(ex);
                }
            }
        }

        void IDialogCallbacks.OnCancel()
        {
            Cancel();
            if (cancel != null)
            {
                try
                {
                    cancel.Invoke();
                }
                catch (Exception ex)
                {
                    ToolboxMethods.ShowException("Dialog OnCancel", ex);
                    ToolboxMethods.LogException(ex);
                }
            }
        }

        #endregion

        public static void ClearNavigationHistory()
        {
            dialogHistory = new Stack<Dialog>();
        }

        public static void OnBack(IDialogCallbacks callbacks)
        {
            if (callbacks != null)
            {
                callbacks.OnBack();
            }
        }

        public static void OnCancel(IDialogCallbacks callbacks)
        {
            if (callbacks != null)
            {
                callbacks.OnCancel();
            }
        }

        public static void OnOk(IDialogCallbacks callbacks, DialogParm result)
        {
            if (callbacks != null)
            {
                callbacks.OnOk(result);
            }
        }

        void NavigateBack()
        {
            if (dialogHistory.Count > 0)
            {
                OnLeave();
                dialogHistory.Pop().ActivateAndHideThis(Form);
            }
            else
            {
                OnLeave();
                Form.Visible = false;
            }
        }

        void Cancel()
        {
            OnLeave();
            Form.Visible = false;
            dialogHistory.Clear();
            if (cancelTarget != null)
            {
                cancelTarget.Activate();
            }
        }

        void ActivateAndHideThis(Control sourceForm)
        {
            ActivateCore(args);
            if (Form != sourceForm)
            {
                sourceForm.Visible = false;
            }
        }

        void ActivateCore(DialogParm args)
        {
            this.args = args;
            dialogForm.Activate(this, args);
            Form.TopMost = true;
            Form.Show();
            OnEnter();
        }

        protected virtual void OnEnter()
        {}

        protected virtual void OnLeave()
        {}
    }
}
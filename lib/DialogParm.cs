namespace AN_Client.srcFramed
{
    class DialogParm
    {
        static DialogParm()
        {
            Empty = new DialogParm();
        }

        public static DialogParm Empty { get; set; }
    }

    class DialogParm<T> : DialogParm
    {
        public DialogParm(T parm)
        {
            Value = parm;
        }

        public T Value { get; set; }
    }
}
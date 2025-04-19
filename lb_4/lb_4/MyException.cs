namespace lb_4
{
    class ValueLessThanZero : Exception
    {
        public ValueLessThanZero(string name, string addition = "") : base(string.Format("{0} must be greater than zero {1}", name, addition)) { }

    }
}

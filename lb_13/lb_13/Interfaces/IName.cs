namespace lb_13.Interfaces
{
    public interface IName : IComparable
    {
        string Name { get; set; }
        decimal Price { get; set; }

        int CompareTo(object obj);
    }
    public interface IName<T>
    {
        string Name { get; set; }
    }
}

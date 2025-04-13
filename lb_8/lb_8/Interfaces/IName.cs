namespace lb_8.Interfaces
{
    public interface IName : IComparable
    {
        string Name { get; set; }

    }
    public interface IName<T>
    {
        string Name { get; set; }
    
    }
}

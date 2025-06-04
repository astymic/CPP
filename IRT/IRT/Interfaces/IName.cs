namespace IRT.Interfaces
{
    public interface IName : IComparable
    {
        string Name { get; set; }
    }
    public interface IName<T> : IComparable<T>
    {
        string Name { get; set; }
    }
}

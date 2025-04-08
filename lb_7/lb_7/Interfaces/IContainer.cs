namespace lb_7.Interfaces
{
    public interface IContainer
    {
        object[] items();
        int[] insertOrder();
        int count();
        int size();
        int nextInsertionId();
    }
}

namespace BusinessObjects
{
    public interface IExpert
    {
        string AccountName();
        string Symbol();
        string Comment();

        double Volume();

        long Magic();
    }
}
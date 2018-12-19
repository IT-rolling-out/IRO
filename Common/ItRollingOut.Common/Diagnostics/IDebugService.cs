namespace ItRollingOut.Common.Diagnostics
{
    public interface IDebugService
    {
        void WriteLine(object obj);

        void Write(object obj);
    }
}

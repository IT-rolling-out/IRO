using System;

namespace ItRollingOut.Xamarin
{ 
    public interface IGuiContext
    {
        void Invoke(Action act);
    }
}
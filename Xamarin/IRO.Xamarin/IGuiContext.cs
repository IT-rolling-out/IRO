using System;

namespace IRO.Xamarin
{ 
    public interface IGuiContext
    {
        void Invoke(Action act);
    }
}
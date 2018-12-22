using System;

namespace ItRollingOut.Xamarin.Droid
{
    class GuiContext : IGuiContext
    {
        /// <summary>
        /// Исполняет делегат в главном потоке.
        /// </summary>
        public void Invoke(Action act)
        {
            Android.App.Application.SynchronizationContext.Post(
                (obj) => { act(); },
                null
                );
        }
    }
}
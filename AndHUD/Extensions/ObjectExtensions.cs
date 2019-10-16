using System;
using Android.App;
using Android.OS;

namespace AndroidHUD.Extensions
{
    /// <summary>
    /// Extensions for Java.Lang.Object.
    /// </summary>
    internal static class ObjectExtensions
    {
        /// <summary>
        /// Checks whether a Java.Lang.Object is null, handle is Zero or if the type
        /// is an Android Activity, then check whether it is Finishing or Destroyed.
        /// </summary>
        /// <param name="thing">A <see cref="Java.Lang.Object"/> to check for liveliness.</param>
        /// <returns>
        /// Returns false if <paramref name="thing"/> is <see cref="null"/>.
        /// Returns false if <paramref name="thing.Handle"/> is <see cref="IntPtr.Zero"/>.
        /// Returns false if <paramref name="thing"/> is an <see cref="Activity"/> and <see cref="Activity.IsFinishing"/> is true.
        /// Returns false if <paramref name="thing"/> is an <see cref="Activity"/> and <see cref="Activity.IsDestroyed"/> is true.
        /// </returns>
        internal static bool IsAlive(this Java.Lang.Object thing)
        {
            if (thing == null)
            {
                return false;
            }

            if (thing.Handle == IntPtr.Zero)
            {
                return false;
            }

            if (thing is Activity activity)
            {
                if (activity.IsFinishing)
                {
                    return false;
                }

                if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1
                    && activity.IsDestroyed)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

using Squirrel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Phantom
{
    /// <summary>
    /// Check for updates and send messages to our main view
    /// </summary>
    public static class Updater
    {
        /// <summary>
        /// Set our update interval to 6 hours.
        /// </summary>
        const int HOUR = 60 * 60 * 1000;
        const int UPDATE_INTERVAL = HOUR * 6;

        /// <summary>
        /// The actual timer we'll use to schedule updates
        /// </summary>
        static System.Timers.Timer updateTimer;

        /// <summary>
        /// Set a timer that will check for updates every several hours
        /// </summary>
        public static void BeginUpdateTimer()
        {
            // set our timer to the update interval
            updateTimer = new System.Timers.Timer(UPDATE_INTERVAL);
            updateTimer.Elapsed += UpdateTimer_Elapsed;
            updateTimer.Start();

            // initially check for updates on boot
            CheckForUpdate();
        }

        /// <summary>
        /// Check for updates
        /// </summary>
        static private void UpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            CheckForUpdate();
        }

        async static private void CheckForUpdate()
        {
            // try to perform an update
            try
            {
                using (var mgr = new UpdateManager("https://vaporsoft.net/public/software/Phantom/"))
                {
                    var updateResult = await mgr.CheckForUpdate();
                    if (updateResult.ReleasesToApply.Count > 0)
                    {
                        await mgr.UpdateApp();
                        MessageBox.Show($"Phantom has been updated to a new version.  Please restart the application for the changes to take effect.");
                    }
                }
            }
            catch (Exception ex)
            {
                if (MessageBox.Show($"Exception {ex.Message} - We ran into a problem updating. Would you like to manually install the update?", "Update Failed", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start("https://vaporsoft.net/public/software/Phantom/Setup.exe");
                }
                else
                {
                    return;
                }
            }
        }
    }
}

using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;


namespace Newspaper
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetLoader
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // event += method to call
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        }

        /// <summary>The method called after a new day starts.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            string forecast = this.Helper.Reflection
                .GetMethod(new StardewValley.Objects.TV(), "getFortuneForecast")
                .Invoke<string>(Game1.player);
            this.Monitor.Log(forecast, LogLevel.Debug);

            string weatherReport = this.Helper.Reflection
                .GetMethod(new StardewValley.Objects.TV(), "getWeatherForecast")
                .Invoke<string>();
            this.Monitor.Log(weatherReport, LogLevel.Debug);

            string[] recipe = this.Helper.Reflection
                .GetMethod(new StardewValley.Objects.TV(), "getWeeklyRecipe")
                .Invoke<string[]>();
            this.Monitor.Log(string.Join(" ", recipe), LogLevel.Debug);
           
            // using this as test
            // this.Monitor.Log(CanLoad(IAssetInfo).ToString(), LogLevel.Debug);
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("PelicanCronicals");
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            return (T)(object)new Dictionary<string, string> // (T)(object) converts a known type to the generic 'T' placeholder
            {
                ["Introduction"] = "Hi there! My name is Jonathan."
            };
        }

    }
}
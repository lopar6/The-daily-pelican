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
    public class ModEntry : Mod
    {
        /*********** Public *********/
        // 9876 to so it is unique
        public override void Entry(IModHelper helper)
        {
            // event += method to call
            
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        }

        /*********** Private *********/
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

            Point paperSpot = this.Helper.Reflection
                .GetMethod(new StardewValley.Locations.FarmHouse(), "getPorchStandingSpot")
                .Invoke<Point>();

            Vector2 porchLocation = new Vector2(paperSpot.X, paperSpot.Y);



            
            // This matches content patch newspaper
            int newspaperID = 9876;

            //public virtual bool dropObject(StardewValley.Object obj, Vector2 dropLocation, xTile.Dimensions.Rectangle viewport, bool initialPlacement, Farmer who = null);
            StardewValley.Object newspaper = new StardewValley.Object(newspaperID, 1);
            newspaper.destroyOvernight = true;

            Game1.getLocationFromName("farm").dropObject(newspaper, porchLocation * 64f, Game1.viewport, true, (Farmer)null);

        }


    }
}
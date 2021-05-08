using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using System.Text;
using System.Xml.Serialization;
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
        

        internal static IMonitor monitor;
        public override void Entry(IModHelper helper)
        {
   
            foreach (IContentPack contentPack in this.Helper.ContentPacks.GetOwned())
            {
                this.Monitor.Log("Reading content pack: {NewspaperContent.Manifest.Name} {NewspaperContent.Manifest.Version} from {Newspaper}", LogLevel.Debug);
                
                if (!contentPack.HasFile("content.json"))
                {
                    this.Monitor.Log("Required content pack 'NewspaperContent' Not found!", LogLevel.Debug);
                }
            }

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.DayEnding += this.OnDayEnding;
        }


        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            //ignore input if the player isnt free to move aka world not loaded,
            //they're in an event, a menu is up, etc
            if (!Context.CanPlayerMove)
                return;

            //action button works for right click on mouse and action button for controllers
            if (!e.Button.IsActionButton() && !e.Button.IsUseToolButton())
                return;
            // TODO add hover tags
            //check if the selected tile is the newspaper
            Vector2 tile = Helper.Input.GetCursorPosition().GrabTile;
            Game1.currentLocation.Objects.TryGetValue(tile, out StardewValley.Object obj);
            if (obj != null)
            {
                if (obj.Name == "Newspaper")
                {
                    if (e.Button.IsActionButton() || e.Button.IsUseToolButton())
                    {
                        Game1.drawLetterMessage(obj.getDescription());
                        Game1.getLocationFromName("farm").removeObject(getPaperSpot(), false);
                    }
                }

            }
            
        }

        public class NewspaperObject : StardewValley.Object
        {
            public new bool isRecipe;

            private string _description;
            private IModHelper Helper;

            public NewspaperObject() { }
            public NewspaperObject(IModHelper helper)
            {
                this.displayName = "The Daily Pelican";
                this.ParentSheetIndex = 931;
                this.CanBeSetDown = false;
                this.CanBeGrabbed = true;
                this.IsSpawnedObject = true;
                this.name = "Newspaper";
                this.Price = 1;
                this.isRecipe = true;
                this.Helper = helper;
            }
            public override int Stack
            {
                get { return 1; }
                set { }
            }

            public override bool isPlaceable()
            {
                return true;
            }

            public override string DisplayName
            {
                get { return DisplayName; }
                set { this.DisplayName = value; }
            }

            public override string getDescription()
            {
                // TODO: add check for +1, 0, recepie name for description

                // New line chars do not work!
                // this is the aproxamate size of a line on the page window "                                                   "
                // 50 chars ish. It is based on actual char sized, not fixed char size. 
                // this solution is very hacky and needs redone 
                             //"                                                   "
                _description = "                 The Daily Pelican                  ";
                _description += "                                                   ";
                _description += getLuckValue();
                _description += "                                                        ";
                _description += getWeatherValue();
                _description += "                                                        ";
                _description += getExtraValue();

                return _description;
            }

            private string getExtraValue()
            {
                string dayOfWeek = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
                string extraValue = "";


                if (dayOfWeek.Equals("Mon") || dayOfWeek.Equals("Thu"))
                {
                    extraValue += fixLength("Livin' Off The Land");
                    extraValue += getLivinLand();
                }
                if (dayOfWeek.Equals("Sun"))
                {
                    extraValue += fixLength("The Queen of Sauce");
                    extraValue += getRecipe();
                }
                if (dayOfWeek.Equals("Wed") && Game1.stats.DaysPlayed > 7U)
                {
                    extraValue += fixLength("The Queen of Sauce Reprint");
                    extraValue += getRecipe();

                }
                return extraValue;
            }

            private string getWeatherValue()
            {
                String weatherString = getWeather();
                if (weatherString.Length < 50)
                {
                    return fixLength(weatherString);
                }
                else
                {
                    if(String.Equals(weatherString, "Partially cloudy with a light breeze. Expect lots of pollen!"))
                    {
                        return "Partially cloudy with a light breeze.              " +
                               "    Expect lots of pollen!                         ";
                    }
                    if(String.Equals(weatherString, "It's going to be cloudy, with gusts of wind throughout the day."))
                    { 
                        return "It's going to be cloudy,                           " +
                               "    with gusts of wind throughout the day.         ";
                    }
                    if (String.Equals(weatherString, "Looks like a storm is approaching. Thunder and lightning is expected."))
                    {
                        return "Looks like a storm is approaching.                 " +
                               "    Thunder and lightning is expected.             ";
                    }
                    else
                    {
                        return "It's going to be clear and sunny tomorrow...       " +
                               "    perfect weather for the Festival!              ";
                    }

                }
            }

            private string getLuckValue()
            {
                String fortuneString = getFortune();
                if (String.Equals(fortuneString, "The spirits are very happy today! They will do their best to shower everyone with good fortune."))
                {
                    return "The spirits are very happy today!                  " +
                           "    Greater than .07 luck!                         ";
                } 
                if(String.Equals(fortuneString, "The spirits are in good humor today. I think you'll have a little extra luck."))
                {
                    return "The spirits are in good humor today.               " +
                           "    Between .02 and .07 luck.                      ";
                }
                if(String.Equals(fortuneString, "The spirits feel neutral today. The day is in your hands."))
                {
                    return "The spirits feel neutral today.                    " +
                           "    Between .02 and -.02 luck.                     ";
                }
                if(String.Equals(fortuneString, "This is rare. The spirits feel absolutely neutral today"))
                {
                    return "The spirits feel absolutely neutral today.         " +
                           "    This is rare.                                  ";
                }
                if (String.Equals(fortuneString, "The spirits are somewhat annoyed today. Luck will not be on your side."))
                {
                    return "The spirits are somewhat annoyed today.            " +
                           "    Between -.02 and -.07 luck.                    ";
                }
                if(String.Equals(fortuneString, "The spirits are somewhat mildly perturbed today. Luck will not be on your side."))
                {
                    return "The spirits are somewhat mildly perturbed today.   " +
                           "    Between -.02 and -.07 luck.                    ";
                }
                if(String.Equals(fortuneString, "The spirits are very displeased today. They will do their best to make your life difficult."))
                {
                    return "The spirits are very displeased today.             " +
                           "    Less than -.07 Luck.                           ";
                }
                else
                {
                    return "No luck Value found. Gosh Dang.                    ";
                }
            }
            private string getFortune()
            {
                string forecast = this.Helper.Reflection
                    .GetMethod(new StardewValley.Objects.TV(), "getFortuneForecast")
                    .Invoke<string>(Game1.player);
                return forecast;
            }

            private string getWeather()
            {
                string weatherReport = this.Helper.Reflection
                    .GetMethod(new StardewValley.Objects.TV(), "getWeatherForecast")
                    .Invoke<string>();
                return weatherReport;
            }

            private string getRecipe()
            {
                string[] recipeStringList = this.Helper.Reflection
                    .GetMethod(new StardewValley.Objects.TV(), "getWeeklyRecipe")
                    .Invoke<string[]>();
                string recipe = string.Join("", recipeStringList);
                return recipe;
            }

            private string getLivinLand()
            {
                string livinLand = this.Helper.Reflection
                    .GetMethod(new StardewValley.Objects.TV(), "getTodaysTip")
                    .Invoke<string>();
                return livinLand;
            }
            private string fixLength(string str)
            {
                String _ = "";
                for (int i = 0; i < 50 - str.Length; i++)
                {
                    _ += " ";
                }
                return str + _;
            }
        }

        public class ServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                throw new NotImplementedException();
            }
        }

        private Vector2 getPaperSpot()
        {
            Point paperSpot = this.Helper.Reflection
                .GetMethod(new StardewValley.Locations.FarmHouse(), "getPorchStandingSpot")
                .Invoke<Point>();

            Vector2 porchLocation = new Vector2(paperSpot.X - 1, paperSpot.Y);
            return porchLocation;
        }
        
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            NewspaperObject dailyPaper = new NewspaperObject(Helper);
            StardewValley.Object newspaper = new StardewValley.Object(getPaperSpot(), 931, "The Daily Pelican", false, true, false, true);
            Game1.getLocationFromName("farm").dropObject(dailyPaper, getPaperSpot() * 64f, Game1.viewport, true, (Farmer)null);
        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            Game1.getLocationFromName("farm").removeObject(getPaperSpot(), false);
        }
    }
}
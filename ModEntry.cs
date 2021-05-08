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
            //helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }
        public string getFortune()
        {
            string forecast = this.Helper.Reflection
                .GetMethod(new StardewValley.Objects.TV(), "getFortuneForecast")
                .Invoke<string>(Game1.player);
            this.Monitor.Log(forecast, LogLevel.Debug);
            return forecast;
        }
        /*********** Private *********/

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            //ignore input if the player isnt free to move aka world not loaded,
            //they're in an event, a menu is up, etc
            if (!Context.CanPlayerMove)
                return;

            //action button works for right click on mouse and action button for controllers
            if (!e.Button.IsActionButton() && !e.Button.IsUseToolButton())
                return;
            //check if the clicked tile contains a Farm Renderer
            Vector2 tile = Helper.Input.GetCursorPosition().GrabTile;
            Game1.currentLocation.Objects.TryGetValue(tile, out StardewValley.Object obj);
            if(obj != null)
            {
                if (obj.HasBeenPickedUpByFarmer)
                {
                    if (Game1.player.ActiveObject != null && Game1.player.ActiveObject.Name == "Newspaper")
                    {
                        if (e.Button.IsActionButton() || e.Button.IsUseToolButton())
                        {
                            Game1.drawObjectDialogue(obj.getDescription());
                            this.Monitor.Log(obj.getDescription(), LogLevel.Debug);
                        }
                    }
                }
                else
                {
                    if (obj.Name == "Newspaper")
                    {
                        if (e.Button.IsActionButton() || e.Button.IsUseToolButton())
                        {    
                           Game1.player.addItemToInventory(obj);
                           obj.HasBeenPickedUpByFarmer = true;
                        
                        }    
                    }
                }
            }
        }
    /***
        private void SendMoney()
        {
            string moneyBook = this.Helper.Reflection
                .GetMethod(new StardewValley.d(), "getFortuneForecast")
                .Invoke<string>(Game1.player);
            this.Monitor.Log(forecast, LogLevel.Debug);
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady ||  e.Button.ToString().Equals("p"))
                return;

            SendMoney();
        }
***/
        // object class inherits item

        //string item_constuctor = this.Helper.Reflection
        //    .GetMethod(new StardewValley.Item, "Item")
        //    .Invoke<string>();
        public class NewspaperObject : StardewValley.Object
        {
            private string _fortune;
            private string _weather;
            private string _recipe;
            private string _description;
            public NewspaperObject(){}
            public NewspaperObject(string fortune, string weather, string recipe)
            {
                this.displayName = "The Daily Pelican";
                this._fortune = fortune;
                this._weather = weather;
                this._recipe = recipe;
                this.ParentSheetIndex = 931;
                this.CanBeSetDown = false;
                this.CanBeGrabbed = true;
                this.IsSpawnedObject = true;
                this.name = "Newspaper";
                this.Price = 1;
            }

            public string fortune
            {
                get { return _fortune; }
                set { _fortune = value; }
            }

            public string weather
            {
                get { return _weather; }
                set { this._weather = value; }
            }

            public string recipe
            {
                get { return _recipe; }
                set { this._recipe = value; }
            }
            public override string DisplayName
            {
                get { return DisplayName; }
                set { this.DisplayName = value; }
            }

            public override string getDescription()
            {
                // TODO: add check for +1, 0, recepie name for description
                _description = _description + _fortune;
                _description = _description + _weather;
                _description = _description + _recipe;
                return _description;
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
        }

        public class ServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                throw new NotImplementedException();
            }
        }


        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {

            string fortune = this.Helper.Reflection
                        .GetMethod(new StardewValley.Objects.TV(), "getFortuneForecast")
                        .Invoke<string>(Game1.player);


            string weatherReport = this.Helper.Reflection
                .GetMethod(new StardewValley.Objects.TV(), "getWeatherForecast")
                .Invoke<string>();


            string[] recipeStringList = this.Helper.Reflection
                .GetMethod(new StardewValley.Objects.TV(), "getWeeklyRecipe")
                .Invoke<string[]>();
            string recipe = string.Join("", recipeStringList);

            NewspaperObject dailyPaper = new NewspaperObject(fortune, weatherReport, recipe);
            //dailyPaper.actionWhenBeingHeld((Farmer)null);
            //dailyPaper.canBeTrashed();
            //dailyPaper.ShouldDrawIcon();
            //dailyPaper.ShouldDrawIcon();

                        
            Point paperSpot = this.Helper.Reflection
                .GetMethod(new StardewValley.Locations.FarmHouse(), "getPorchStandingSpot")
                .Invoke<Point>();

            Vector2 porchLocation = new Vector2(paperSpot.X-1, paperSpot.Y);

            StardewValley.Object newspaper = new StardewValley.Object(porchLocation, 931, "The Daily Pelican", false, true, false, true);

            Game1.getLocationFromName("farm").dropObject(dailyPaper, porchLocation * 64f, Game1.viewport, true, (Farmer)null);

        }
    }
}
#region File Description
//-----------------------------------------------------------------------------
// GameScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using EGMGame.Library;
using EGMGame.Processors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace EGMGame
{
    /// <summary>
    /// Enum describes the screen transition state.
    /// </summary>
    public enum ScreenState
    {
        TransitionOn,
        Active,
        TransitionOff,
        Hidden,
    }
    /// <summary>
    /// A screen is a single layer that has update and draw logic, and which
    /// can be combined with other layers to build up a complex menu system.
    /// For instance the main menu, the options menu, the "are you sure you
    /// want to quit" message box, and the main game itself are all implemented
    /// as screens.
    /// </summary>
    public abstract class GameScreen
    {
        #region Properties


        /// <summary>
        /// Normally when one screen is brought up over the top of another,
        /// the first screen will transition off to make room for the new
        /// one. This property indicates whether the screen is only a small
        /// popup, in which case screens underneath it do not need to bother
        /// transitioning off.
        /// </summary>
        public bool IsPopup
        {
            get { return isPopup; }
            protected set { isPopup = value; }
        }

        bool isPopup = false;


        /// <summary>
        /// There are two possible reasons why a screen might be transitioning
        /// off. It could be temporarily going away to make room for another
        /// screen that is on top of it, or it could be going away for good.
        /// This property indicates whether the screen is exiting for real:
        /// if set, the screen will automatically remove itself as soon as the
        /// transition finishes.
        /// </summary>
        public bool IsExiting
        {
            get { return isExiting; }
            protected internal set { isExiting = value; }
        }

        bool isExiting = false;
        /// <summary>
        /// Checks whether this screen is active and can respond to user input.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return !otherScreenHasFocus &&
                       (Global.Instance.ScreenState == ScreenState.TransitionOn ||
                        Global.Instance.ScreenState == ScreenState.Active);
            }
        }

        bool otherScreenHasFocus;
        /// <summary>
        /// Gets the manager that this screen belongs to.
        /// </summary>
        public ScreenManager ScreenManager
        {
            get { return screenManager; }
            internal set { screenManager = value; }
        }

        ScreenManager screenManager;
        /// <summary>
        /// Gets the index of the player who is currently controlling this screen,
        /// or null if it is accepting input from any player. This is used to lock
        /// the game to a specific player profile. The main menu responds to input
        /// from any connected gamepad, but whichever player makes a selection from
        /// this menu is given control over all subsequent screens, so other gamepads
        /// are inactive until the controlling player returns to the main menu.
        /// </summary>
        public PlayerIndex? ControllingPlayer
        {
            get { return controllingPlayer; }
            internal set { controllingPlayer = value; }
        }

        PlayerIndex? controllingPlayer;
        #endregion

        #region Initialization


        /// <summary>
        /// Load graphics content for the screen.
        /// </summary>
        public virtual void LoadContent() { }


        /// <summary>
        /// Unload content for the screen.
        /// </summary>
        public virtual void UnloadContent() { }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the screen to run logic, such as updating the transition position.
        /// Unlike HandleInput, this method is called regardless of whether the screen
        /// is active, hidden, or in the middle of a transition.
        /// </summary>
        public virtual void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                      bool coveredByOtherScreen)
        {
            this.otherScreenHasFocus = otherScreenHasFocus;
            // Update Transition
            UpdateScreen(gameTime);
        }
        /// <summary>
        /// Helper for updating the screen transition position.
        /// </summary>
        void UpdateScreen(GameTime gameTime)
        {

        }
        /// <summary>
        /// Allows the screen to handle user input. Unlike Update, this method
        /// is only called when the screen is active, and not when some other
        /// screen has taken the focus.
        /// </summary>
        public virtual void HandleInput(InputState input) { }
        /// <summary>
        /// This is called when the screen should draw itself.
        /// </summary>
        public virtual void Draw(GameTime gameTime) { }
        #endregion

        #region Public Methods
        /// <summary>
        /// Fade out
        /// </summary>
        public static void FadeOut()
        {
            Global.Instance.TransitionOffTime = 60;
            Global.Instance.FadeColor = Color.Black;
            // Adjust fade color according to tint
            for (int i = 0; i < 3; i++)
            {
                if (Global.Instance.TintScreen[i].ScreenType == ScreenType.Global ||
                    (Global.Instance.TintScreen[i].ScreenType == ScreenType.Gameplay && GameplayScreen.IsCurrent) ||
                   (Global.Instance.TintScreen[i].ScreenType == ScreenType.Menu && MenuScreen.IsCurrent))
                {
                    Global.Instance.TintScreen[i].Adjust(ref  Global.Instance.FadeColor);
                }
            }
            Global.Instance.ScreenState = ScreenState.TransitionOff;

            Global.Instance.FadingOut = true;
        }
        /// <summary>
        /// Fade in
        /// </summary>
        public static void FadeIn()
        {
            Global.Instance.TransitionOnTime = 60;
            Global.Instance.FadeColor = new Color(0, 0, 0, 0);
            // Adjust fade color according to tint
            for (int i = 0; i < 3; i++)
            {
                if (Global.Instance.TintScreen[i].ScreenType == ScreenType.Global ||
                    (Global.Instance.TintScreen[i].ScreenType == ScreenType.Gameplay && GameplayScreen.IsCurrent) ||
                   (Global.Instance.TintScreen[i].ScreenType == ScreenType.Menu && MenuScreen.IsCurrent))
                {
                    Global.Instance.TintScreen[i].Adjust(ref  Global.Instance.FadeColor);
                }
            }
            Global.Instance.ScreenState = ScreenState.TransitionOn;
            if (Global.Instance.FadeColor == Color.White)
                Global.Instance.FadeColor = new Color(0, 0, 0, 0);
            Global.Instance.FadeToColor = Global.Instance.FadeColor;
            Global.Instance.FadeColor = Color.Black;
            Global.Instance.FadingOut = false;
        }
        /// <summary>
        /// Reset Fade
        /// </summary>
        public static void ResetFade(Color fadeColor)
        {
            if (!Global.Instance.FadingOut)
            {
                Global.Instance.FadeToColor = fadeColor;
            }
        }
        /// <summary>
        /// Tells the screen to go away. Unlike ScreenManager.RemoveScreen, which
        /// instantly kills the screen, this method respects the transition timings
        /// and will give the screen a chance to gradually transition off.
        /// </summary>
        public void ExitScreen()
        {
            if (Global.Instance.TransitionOffTime == 0)
            {
                // If the screen has a zero transition time, remove it immediately.
                ScreenManager.RemoveScreen(this);
            }
            else
            {
                // Otherwise flag that it should transition off and then exit.
                isExiting = true;
            }
        }
        #endregion
    }
}

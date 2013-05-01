using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.IO;

namespace WhatTheWord.Controls
{
    public static class SoundEffects
    {
        private static bool initialized = false;
        public static bool isSoundEnabled = true;

        private enum SFX { Bounce, Buy, Click, PictureZoom, TapLetter, Win, Wrong };
        private static Dictionary<SFX, SoundEffect> soundEffects;

        public static void Initialize()
        {
            // TODO: (1) need a isSoundEnabled config in App.Config, and (2) expose the setting via Settings popup

            if (SoundEffects.initialized)
                return;

            soundEffects = new Dictionary<SFX, SoundEffect>();

            soundEffects.Add(SFX.Bounce, SoundEffect.FromStream(TitleContainer.OpenStream("Assets/Sounds/Bounce05.wav")));
            soundEffects.Add(SFX.Buy, SoundEffect.FromStream(TitleContainer.OpenStream("Assets/Sounds/Buy05.wav")));
            soundEffects.Add(SFX.Click, SoundEffect.FromStream(TitleContainer.OpenStream("Assets/Sounds/Click02.wav")));
            soundEffects.Add(SFX.PictureZoom, SoundEffect.FromStream(TitleContainer.OpenStream("Assets/Sounds/PictureZoom02.wav")));
            soundEffects.Add(SFX.TapLetter, SoundEffect.FromStream(TitleContainer.OpenStream("Assets/Sounds/TapLetter04.wav")));
            soundEffects.Add(SFX.Win, SoundEffect.FromStream(TitleContainer.OpenStream("Assets/Sounds/Win-4.wav")));
            soundEffects.Add(SFX.Wrong, SoundEffect.FromStream(TitleContainer.OpenStream("Assets/Sounds/Wrong02.wav")));

            FrameworkDispatcher.Update();
            initialized = true;
        }

        public static void PlayBounce()
        {
            if (SoundEffects.initialized && isSoundEnabled)
            {
                FrameworkDispatcher.Update();
                soundEffects[SFX.Bounce].Play();
            }
        }

        public static void PlayBuy()
        {
            if (SoundEffects.initialized && isSoundEnabled)
            {
                FrameworkDispatcher.Update();
                soundEffects[SFX.Buy].Play();
            }
        }

        public static void PlayClick()
        {
            if (SoundEffects.initialized && isSoundEnabled)
            {
                FrameworkDispatcher.Update();
                soundEffects[SFX.Click].Play();
            }
        }

        public static void PlayPictureZoom()
        {
            if (SoundEffects.initialized && isSoundEnabled)
            {
                FrameworkDispatcher.Update();
                soundEffects[SFX.PictureZoom].Play();
            }
        }

        public static void PlayTapLetter()
        {
            if (SoundEffects.initialized && isSoundEnabled)
            {
                FrameworkDispatcher.Update();
                soundEffects[SFX.TapLetter].Play();
            }
        }

        public static void PlayWin()
        {
            if (SoundEffects.initialized && isSoundEnabled)
            {
                FrameworkDispatcher.Update();
                soundEffects[SFX.Win].Play();
            }
        }

        public static void PlayWrong()
        {
            if (SoundEffects.initialized && isSoundEnabled)
            {
                FrameworkDispatcher.Update();
                soundEffects[SFX.Wrong].Play();
            }
        }
    }
}

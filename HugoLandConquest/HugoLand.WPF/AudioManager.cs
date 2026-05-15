using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HugoLand.WPF
{
    public class AudioManager
    {
        public static readonly MediaPlayer MusiqueFond = new MediaPlayer();
        public static readonly MediaPlayer MenuSound = new MediaPlayer();
        public static readonly MediaPlayer Movements = new MediaPlayer();

        static AudioManager() 
        {
            MusiqueFond.Open(new Uri(@"sound/GameBeginning.wav", UriKind.Relative));
            MusiqueFond.MediaEnded += Media_Loop;

            MenuSound.Open(new Uri(@"sound/menubutton.mp3", UriKind.Relative));
            Movements.Open(new Uri(@"sound/Movements.mp3", UriKind.Relative));

            MusiqueFond.Volume= 0.5;

        }

        private static void Media_Loop(object sender, EventArgs e)
        {
            MusiqueFond.Position = TimeSpan.Zero;
            PlaySound();
        }

        private static void PlaySound()
        {
            if (MusiqueFond.Position == TimeSpan.Zero)
            {
                MusiqueFond.Play();
            }
        }

        public static void Stop()
        {
            MusiqueFond.Stop();
            MusiqueFond.MediaEnded -= Media_Loop;
        }

    }

}

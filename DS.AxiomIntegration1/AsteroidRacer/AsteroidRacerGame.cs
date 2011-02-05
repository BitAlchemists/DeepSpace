using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DS.AxiomIntegration1.Core;
using DS.AxiomIntegration1.Graphics;

namespace DS.AxiomIntegration1.AsteroidRacer
{
    public class AsteroidRacerGame : GameBase
    {
        protected AsteroidRacerGame game;

        public AsteroidRacerGame() : base()
        {

        }


        public override void RunGame()
        {
            AsteroidRacerScene scene = new AsteroidRacerScene(this);
            PlayScene(scene);
        }
    }
}
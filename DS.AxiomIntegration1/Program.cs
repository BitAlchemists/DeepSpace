using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Axiom.Core;
using System.IO;
using Axiom.Demos.Configuration;
using Axiom.Graphics;
using DS.AxiomIntegration1.Core;
using DS.AxiomIntegration1.AsteroidRacer;

namespace DS.AxiomIntegration1
{
    class Program
    {
        static void Main(string[] args)
        {
            GameBase app = new AsteroidRacerGame();
            app.Start();
        }


    }
}

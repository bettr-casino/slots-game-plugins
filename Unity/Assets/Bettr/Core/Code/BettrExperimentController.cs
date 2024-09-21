using System.Collections;
using System.Collections.Generic;
using CrayonScript.Code;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public class BettrExperimentController
    {
        public BettrServer bettrServer;

        public ConfigData configData;
        
        public List<BettrUserExperiment> BettrUserExperimentsList { get; private set; }
        
        public static BettrExperimentController Instance { get; private set; }
        
        public BettrExperimentController()
        {
            TileController.RegisterType<BettrExperimentController>("BettrExperimentController");
            TileController.AddToGlobals("BettrExperimentController", this);
            
            Instance = this;
        }
        
        public IEnumerator GetUserExperiments()
        {
            yield return bettrServer.GetUserExperiments(userExperimentsCallback: (_, payload, success, error) =>
            {
                BettrUserExperimentsList = payload.UserExperiments;
            });
        }
    }
}
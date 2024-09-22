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
        
        public string GetMachineExperimentVariant(string machineName, string defaultExperiment)
        {
            var experiment = BettrUserExperimentsList.Find(e => e.ExperimentName?.ToLower() == machineName?.ToLower());
            var treatment = experiment?.Treatment ?? defaultExperiment;
            // treatment has to be one of "control", "variant1", if not default to "control"
            return treatment is "control" or "variant1" ? treatment : "control";
        }
        
        public string GetLobbyExperimentVariant(string lobbyName, string defaultExperiment)
        {
            var experiment = BettrUserExperimentsList.Find(e => e.ExperimentName?.ToLower() == lobbyName?.ToLower());
            var treatment = experiment?.Treatment ?? defaultExperiment;
            // treatment has to be one of "control", "variant1", if not default to "control"
            return treatment is "control" or "variant1" ? treatment : "control";
        }
        
        public bool UseGeneratedOutcomes()
        {
            var experiment = BettrUserExperimentsList.Find(e => e.ExperimentName?.ToLower() == "outcomes");
            var treatment = experiment?.Treatment ?? "test";
            // treatment has to be one of "control", "variant1", if not default to "control"
            treatment = treatment is "test" or "generated" ? treatment : "test";
            return treatment == "generated";
        }
    }
}
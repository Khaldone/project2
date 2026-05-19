// Assets/Scripts/Presentation/Boot/SimRunnerEntryPoint.cs
using VContainer.Unity;
using UnityEngine;
using System;

public class SimRunnerEntryPoint : IStartable
{
    private readonly HeadlessSimOrchestrator _simOrchestrator;

    public SimRunnerEntryPoint(HeadlessSimOrchestrator simOrchestrator)
    {
        _simOrchestrator = simOrchestrator;
    }

    public void Start()
    {
        // Read the command line arguments passed by Jenkins
        string[] args = Environment.GetCommandLineArgs();
        bool isHeadless = Array.Exists(args, arg => arg == "-runSimulations");


        if (isHeadless)
        {
            Debug.Log("AAA Pipeline: Headless Simulation Mode Engaged.");
            RunStressTest();
        }
        else
        {
            // Normal game boot: Load Main Menu
            ServiceLocator.Get<ISceneLoader>().LoadMainMenu();
        }
    }

    private void RunStressTest()
    {
        int matchesToRun = 10000;
        int successfulMatches = 0;


        try
        {
            for (int i = 0; i < matchesToRun; i++)
            {
                // This executes purely in CPU memory. No rendering. No waiting.
                _simOrchestrator.RunSimulatedMatch();
                successfulMatches++;
            }


            Debug.Log($"AAA Pipeline: SUCCESS. {successfulMatches} matches simulated with 0 physics anomalies.");
            Application.Quit(0); // Exit code 0 tells Jenkins the build passed
        }
        catch (Exception ex)
        {
            // A physics bug was found!
            Debug.LogError($"AAA Pipeline: CRITICAL ANOMALY DETECTED at match {successfulMatches + 1}.");
            Debug.LogError(ex.Message);
            Debug.LogError(ex.StackTrace);

            Application.Quit(1); // Exit code 1 tells Jenkins to fail the build immediately
        }
    }
}
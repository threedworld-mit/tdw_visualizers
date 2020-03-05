using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor.Build.Reporting;


/// <summary>
/// Editor script to build standalone versions of utility applications 
/// on Windows, OS X, and Linux.
/// </summary>
public static class UtilityApplicationsBuilder
{

    #region ENUMS

    /// <summary>
    /// The application type. Used for directory and application names.
    /// </summary>
    private enum ApplicationType
    {
        ModelVisualizer,
        MaterialVisualizer
    }
    /// <summary>
    /// The scenes of the utility applications.
    /// </summary>
    private enum TDWScene
    {
        ModelVisualizer,
        MaterialVisualizer
    }

    #endregion

    #region STRUCTS

    /// <summary>
    /// Data for each release platform.
    /// </summary>
    private struct PlatformData
    {
        /// <summary>
        /// The application extension type.
        /// </summary>
        public string extension;
        /// <summary>
        /// The build target.
        /// </summary>
        public BuildTarget target;


        public PlatformData(string extension, BuildTarget target)
        {
            this.extension = extension;
            this.target = target;
        }
    }


    /// <summary>
    /// Data for building an application.
    /// </summary>
    private struct ApplicationData
    {
        /// <summary>
        /// All scenes that should be enabled for the build.
        /// </summary>
        public string[] scenePaths;
        /// <summary>
        /// Screen width.
        /// </summary>
        public int screenWidth;
        /// <summary>
        /// Screen height.
        /// </summary>
        public int screenHeight;
        /// <summary>
        /// Resolution dialog setting.
        /// </summary>
        public ResolutionDialogSetting resolutionDialogSetting;


        public ApplicationData(TDWScene[] scenes, int screenWidth, int screenHeight,
            ResolutionDialogSetting resolutionDialogSetting)
        {
            scenePaths = scenes.Select(s => ScenePaths[s]).ToArray();
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.resolutionDialogSetting = resolutionDialogSetting;
        }
    }

    #endregion

    #region FIELDS

    /// <summary>
    /// Paths to each scene.
    /// </summary>
    private readonly static Dictionary<TDWScene, string> ScenePaths = new Dictionary<TDWScene, string>
        {
            { TDWScene.ModelVisualizer, "Assets/Scenes/model.unity" },
            { TDWScene.MaterialVisualizer, "Assets/Scenes/material.unity" },
        };

    /// <summary>
    /// Data for each platform.
    /// </summary>
    private readonly static Dictionary<string, PlatformData> Platforms =
        new Dictionary<string, PlatformData>
    {
                { "Linux", new PlatformData(".x86_64", BuildTarget.StandaloneLinux64) },
                { "Darwin", new PlatformData(".app", BuildTarget.StandaloneOSX) },
                { "Windows", new PlatformData(".exe", BuildTarget.StandaloneWindows64) }
    };
    /// <summary>
    /// Build instructions for each application.
    /// </summary>
    private readonly static Dictionary<ApplicationType, ApplicationData> Applications =
        new Dictionary<ApplicationType, ApplicationData>
    {
            {
                ApplicationType.ModelVisualizer,
                    new ApplicationData(
                        new TDWScene[]{ TDWScene.ModelVisualizer },
                        1280, 720, ResolutionDialogSetting.Enabled)
            },
            {
                ApplicationType.MaterialVisualizer,
                    new ApplicationData(
                        new TDWScene[]{ TDWScene.MaterialVisualizer },
                        1280, 720, ResolutionDialogSetting.Enabled)
            }
    };

    #endregion

    #region METHDOS

    /// <summary>
    /// Build a utility application. Parameters are handled via environment args.
    /// </summary>
    public static void BuildUtilityApplication()
    {
        // Parse the args.
        string[] args = Environment.GetCommandLineArgs();
        string version = args[7];

        // e.g. Assets/bin
        string rootDirectory = Path.Combine(Application.dataPath, "bin", version);
        if (!Directory.Exists(rootDirectory))
        {
            Directory.CreateDirectory(rootDirectory);
        }
        // e.g. Assets/bin/Windows
        foreach (string platform in Platforms.Keys)
        {
            string platformDirectory = Path.Combine(rootDirectory, platform);
            if (!Directory.Exists(platformDirectory))
            {
                Directory.CreateDirectory(platformDirectory);
            }

            // Build each application.
            // e.g. Assets/bin/Windows/ModelVisualizer
            foreach (ApplicationType application in Applications.Keys)
            {
                string applicationDirectory = Path.Combine(platformDirectory, 
                    application.ToString());
                if (!Directory.Exists(applicationDirectory))
                {
                    Directory.CreateDirectory(applicationDirectory);
                }

                // Handle player settings.
                PlayerSettings.displayResolutionDialog = 
                    Applications[application].resolutionDialogSetting;
                PlayerSettings.defaultScreenHeight = 
                    Applications[application].screenHeight;
                PlayerSettings.defaultScreenWidth =
                    Applications[application].screenWidth;

                // Create the build.
                BuildReport result = BuildPipeline.BuildPlayer(
                    Applications[application].scenePaths,
                    Path.Combine(applicationDirectory, 
                    application.ToString() + Platforms[platform].extension),
                    Platforms[platform].target, BuildOptions.None);
                Debug.Log(result.steps);
                Debug.Log(result.summary);
            }
        }
    }

    #endregion

}
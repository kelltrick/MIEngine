﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MICore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Globalization;

namespace AndroidDebugLauncher
{
    internal class AndroidLaunchOptions
    {
        public AndroidLaunchOptions(MICore.Xml.LaunchOptions.AndroidLaunchOptions xmlOptions)
        {
            if (xmlOptions == null)
            {
                throw new ArgumentNullException("xmlOptions");
            }

            this.Package = LaunchOptions.RequireAttribute(xmlOptions.Package, "Package");
            this.IsAttach = xmlOptions.Attach;
            if (!IsAttach)
            {
                // LaunchActivity is only required when we're launching
                this.LaunchActivity = LaunchOptions.RequireAttribute(xmlOptions.LaunchActivity, "LaunchActivity");
            }
            this.SDKRoot = GetOptionalDirectoryAttribute(xmlOptions.SDKRoot, "SDKRoot");
            this.NDKRoot = GetOptionalDirectoryAttribute(xmlOptions.NDKRoot, "NDKRoot");
            this.TargetArchitecture = LaunchOptions.ConvertTargetArchitectureAttribute(xmlOptions.TargetArchitecture);
            this.IntermediateDirectory = RequireValidDirectoryAttribute(xmlOptions.IntermediateDirectory, "IntermediateDirectory");
            this.AdditionalSOLibSearchPath = xmlOptions.AdditionalSOLibSearchPath;
            this.DeviceId = LaunchOptions.RequireAttribute(xmlOptions.DeviceId, "DeviceId");
            this.LogcatServiceId = GetLogcatServiceIdAttribute(xmlOptions.LogcatServiceId);

            CheckTargetArchitectureSupported();
        }

        private string GetOptionalDirectoryAttribute(string value, string attributeName)
        {
            if (value == null)
                return null;

            EnsureValidDirectory(value, attributeName);
            return value;
        }

        private string RequireValidDirectoryAttribute(string value, string attributeName)
        {
            LaunchOptions.RequireAttribute(value, attributeName);

            EnsureValidDirectory(value, attributeName);

            return value;
        }

        private void EnsureValidDirectory(string value, string attributeName)
        {
            if (value.IndexOfAny(Path.GetInvalidPathChars()) >= 0 ||
                !Path.IsPathRooted(value) ||
                !Directory.Exists(value))
            {
                throw new LauncherException(Telemetry.LaunchFailureCode.NoReport, string.Format(CultureInfo.CurrentCulture, LauncherResources.Error_InvalidDirectoryAttribute, attributeName, value));
            }
        }

        private Guid GetLogcatServiceIdAttribute(string attributeValue)
        {
            const string attributeName = "LogcatServiceId";

            if (!string.IsNullOrEmpty(attributeValue))
            {
                Guid value;
                if (!Guid.TryParse(attributeValue, out value))
                {
                    throw new LauncherException(Telemetry.LaunchFailureCode.NoReport, string.Format(CultureInfo.CurrentCulture, LauncherResources.Error_InvalidAttribute, attributeName));
                }

                return value;
            }
            else
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// [Required] Package name to spawn
        /// </summary>
        public string Package { get; private set; }

        /// <summary>
        /// [Otprional] Activity name to spawn
        /// 
        /// This is required for a launch
        /// This is not required for an attach
        /// </summary>
        public string LaunchActivity { get; private set; }

        /// <summary>
        /// [Optional] Root of the Android SDK
        /// </summary>
        public string SDKRoot { get; private set; }

        /// <summary>
        /// [Optional] Root of the Android NDK
        /// </summary>
        public string NDKRoot { get; private set; }

        /// <summary>
        /// [Required] Target architecture of the application
        /// </summary>
        public TargetArchitecture TargetArchitecture { get; private set; }

        private void CheckTargetArchitectureSupported()
        {
            switch (this.TargetArchitecture)
            {
                case MICore.TargetArchitecture.X86:
                case MICore.TargetArchitecture.ARM:
                    return;

                default:
                    throw new LauncherException(Telemetry.LaunchFailureCode.NoReport, string.Format(CultureInfo.CurrentCulture, LauncherResources.UnsupportedTargetArchitecture, this.TargetArchitecture));
            }
        }

        /// <summary>
        /// [Required] Directory where files from the device/emulator will be downloaded to.
        /// </summary>
        public string IntermediateDirectory { get; private set; }

        /// <summary>
        /// [Optional] Additional directories to add to the search path
        /// </summary>
        public string AdditionalSOLibSearchPath { get; private set; }

        /// <summary>
        /// [Required] ADB device ID of the device/emulator to target
        /// </summary>
        public string DeviceId { get; private set; }

        /// <summary>
        /// [Optional] The VS Service id of the logcat service used by the launching project system
        /// </summary>
        public Guid LogcatServiceId { get; private set; }

        /// <summary>
        /// [Optional] Set to true if we are performing an attach instead of a launch. Default is false.
        /// </summary>
        public bool IsAttach { get; private set; }
    }
}

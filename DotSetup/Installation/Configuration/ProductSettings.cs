// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.Collections.Generic;
using DotSetup.UILayouts.ControlLayout;

namespace DotSetup.Installation.Configuration
{
    public struct ProductSettings
    {
        public string Name, Filename, RunPath, ExtractPath, RunParams, LayoutName, Behavior, Class, DownloadMethod, SecondaryDownloadMethod;

        public struct DownloadURL
        {
            public string Arch, URL;
        }
        public List<DownloadURL> DownloadURLs;

        public struct ProductEvent
        {
            public string Name, Trigger;
        }
        public List<ProductEvent> ProductEvents;

        public struct RequirementKey
        {
            public string Type;
            public string Value;
        }

        public struct ProductRequirement
        {
            public List<RequirementKey> Keys;
            public string Type, Value, LogicalOperator, ValueOperator, Delta;
        }
        public struct ProductRequirements
        {
            public List<ProductRequirement> RequirementList;
            public List<ProductRequirements> RequirementsList;
            public string LogicalOperator, UnfulfilledRequirementType, UnfulfilledRequirementDelta;
        }
        public ProductRequirements PreInstall;
        public ProductRequirements PostInstall;
        public ControlsLayout ControlsLayouts;
        public bool IsOptional, IsExtractable, RunWithBits, RunAndWait, Exclusive;
        public int MsiTimeoutMS;
        public object Other;

        public string Guid;
    }
}

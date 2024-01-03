// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

namespace MagicLeap.ZI
{
    internal struct PropertiesViewData
    {
        public static readonly PropertiesViewData emptyPropertiesData = new()
        {
            DisplayedType = new PropertyData<string>("Empty", false),
            Name = new PropertyData<string>("Empty", false),

            Position = new PropertyData<CombinedVector3>(CombinedVector3.zero, false),
            Orientation = new PropertyData<CombinedVector3>(CombinedVector3.zero, false),
            Scale = new PropertyData<CombinedVector3>(CombinedVector3.zero, false),

            Light = new PropertyData<LightPropertiesData>(LightPropertiesData.empty, false)
        };

        public static readonly PropertiesViewData incompatiblePropertiesData = new()
        {
            DisplayedType = new PropertyData<string>("<multi-selection>", false),
            Name = new PropertyData<string>("<multi-selection>", false),

            Position = new PropertyData<CombinedVector3>(CombinedVector3.zero, false),
            Orientation = new PropertyData<CombinedVector3>(CombinedVector3.zero, false),
            Scale = new PropertyData<CombinedVector3>(CombinedVector3.zero, false),

            Light = new PropertyData<LightPropertiesData>(LightPropertiesData.empty, false)
        };

        public PropertyData<string> DisplayedType;

        public PropertyData<LightPropertiesData> Light;
        public PropertyData<string> Name;
        public PropertyData<CombinedVector3> Orientation;

        public PropertyData<CombinedVector3> Position;
        public PropertyData<CombinedVector3> Scale;
    }
}

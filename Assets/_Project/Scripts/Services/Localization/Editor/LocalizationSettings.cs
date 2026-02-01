using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Services.Localization.Editor
{
    [Serializable]
    public class LocalizationSettings : ScriptableObject
    {
        public const string FileName = "LocalizationSettings.asset";

        public string TableId;
        public List<string> SheetIds = new();
        public bool IsSplitInternalAndExternal;
        public string InternalFolder;
        public string ExternalFolder;

        public override string ToString() =>
            $"[{GetType().Name}] " +
            $"{nameof(TableId)}: {TableId}, " +
            $"SheetIds: [{string.Join(", ", SheetIds)}], " +
            $"{nameof(IsSplitInternalAndExternal)}: {IsSplitInternalAndExternal}, " +
            $"{nameof(InternalFolder)}: {InternalFolder}, " +
            $"{nameof(ExternalFolder)}: {ExternalFolder}";
    }
}
using System;
using UnityEngine;

namespace Scripts.Services.Localization.Editor
{
    [Serializable]
    public class LocalizationSettings : ScriptableObject
    {
        public const string FileName = "LocalizationSettings.asset";

        public string TableId;
        public string SheetId;
        public bool IsSplitInternalAndExternal;
        public string InternalFolder;
        public string ExternalFolder;

        public override string ToString() =>
            $"[{GetType().Name}] " +
            $"{nameof(TableId)}: {TableId}, " +
            $"{nameof(SheetId)}: {SheetId}, " +
            $"{nameof(IsSplitInternalAndExternal)}: {IsSplitInternalAndExternal}, " +
            $"{nameof(InternalFolder)}: {InternalFolder}, " +
            $"{nameof(ExternalFolder)}: {ExternalFolder}";
    }
}
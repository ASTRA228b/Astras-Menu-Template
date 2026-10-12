using UnityEngine;
using HarmonyLib;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Internal;

// This is unused, i dont trust it that much tbh. Load this if you wan't, they're all exerpimental but might save you!

namespace Astras_Menu_Template.Core.Patches.Internal;

public class EveryPatch
{
    [HarmonyPatch(typeof(MonkeAgent), "SendReport")]
    public class SendReportPatch
    {
        // If you want to log, or send notifications for anti cheat reports you can do it here
        private static bool Prefix(string susReason, string susId, string susNick) => false;
    }

    [HarmonyPatch(typeof(MonkeAgent), "CloseInvalidRoom")]
    public class NoCloseInvalidRoom
    {
        private static bool Prefix() =>
                false;
    }

    [HarmonyPatch(typeof(MonkeAgent), "CheckReports")]
    public class NoCheckReports
    {
        private static bool Prefix() =>
                false;
    }

    [HarmonyPatch(typeof(MonkeAgent), "DispatchReport")]
    public class NoDispatchReport
    {
        private static bool Prefix() =>
                false;
    }

    [HarmonyPatch(typeof(MonkeAgent), "GetRPCCallTracker")]
    internal class NoGetRPCCallTracker
    {
        private static bool Prefix() =>
                false;
    }

    [HarmonyPatch(typeof(MonkeAgent), "LogErrorCount")]
    public class NoLogErrorCount
    {
        private static bool Prefix(string logString, string stackTrace, LogType type) =>
                false;
    }

    [HarmonyPatch(typeof(MonkeAgent), "QuitDelay", MethodType.Enumerator)]
    public class NoQuitDelay
    {
        private static bool Prefix() =>
                false;
    }

    [HarmonyPatch(typeof(GorillaGameManager), "ForceStopGame_DisconnectAndDestroy")]
    public class NoQuitOnBan
    {
        private static bool Prefix() =>
                false;
    }

    [HarmonyPatch(typeof(MonkeAgent), "ShouldDisconnectFromRoom")]
    public class NoShouldDisconnectFromRoom
    {
        private static bool Prefix() =>
                false;
    }

    [HarmonyPatch(typeof(GorillaNetworkPublicTestsJoin), "GracePeriod")]
    public class GracePeriodPatch1
    {
        private static bool Prefix() =>
                false;
    }

    [HarmonyPatch(typeof(GorillaNetworkPublicTestJoin2), "GracePeriod")]
    public class GracePeriodPatch2
    {
        private static bool Prefix() =>
                false;
    }

    [HarmonyPatch(typeof(VRRig), "IncrementRPC", typeof(PhotonMessageInfoWrapped), typeof(string))]
    public class NoIncrementRPC
    {
        private static bool Prefix(PhotonMessageInfoWrapped info, string sourceCall) =>
            false;
    }

    [HarmonyPatch(typeof(MonkeAgent), "IncrementRPCCall", typeof(PhotonMessageInfo), typeof(string))]
    public class NoIncrementRPCCall
    {
        private static bool Prefix(PhotonMessageInfo info, string callingMethod = "") =>
            false;
    }

    [HarmonyPatch(typeof(MonkeAgent), "IncrementRPCCallLocal")]
    public class NoIncrementRPCCallLocal
    {
        private static bool Prefix(PhotonMessageInfoWrapped infoWrapped, string rpcFunction) =>
            false;
    }

    [HarmonyPatch(typeof(PlayFabDeviceUtil), "SendDeviceInfoToPlayFab")]
    public class PlayfabUtil01
    {
        private static bool Prefix() =>
            false;
    }

    [HarmonyPatch(typeof(PlayFabClientInstanceAPI), "ReportDeviceInfo")]
    public class PlayfabUtil02
    {
        private static bool Prefix() =>
            false;
    }

    [HarmonyPatch(typeof(PlayFabClientAPI), "ReportDeviceInfo")]
    public class PlayfabUtil03
    {
        private static bool Prefix() =>
            false;
    }

    [HarmonyPatch(typeof(PlayFabDeviceUtil), "GetAdvertIdFromUnity")]
    public class PlayfabUtil04
    {
        private static bool Prefix() =>
            false;
    }

    [HarmonyPatch(typeof(PlayFabClientAPI), "AttributeInstall")]
    public class PlayfabUtil05
    {
        private static bool Prefix() =>
            false;
    }

    [HarmonyPatch(typeof(PlayFabHttp), "InitializeScreenTimeTracker")]
    public class PlayfabUtil06
    {
        private static bool Prefix() =>
            false;
    }

    [HarmonyPatch(typeof(PlayFabClientAPI), "UpdateUserTitleDisplayName")] // Credits to Shiny for letting me use this
    public class DisplayNamePatch
    {
        private static string RandomString(int length = 4)
        {
            string random = "";
            for (int i = 0; i < length; i++)
            {
                int rand = UnityEngine.Random.Range(0, 36);
                char c = rand < 26
                    ? (char)('A' + rand)
                    : (char)('0' + (rand - 26));
                random += c;
            }

            return random;
        }

        public static void Prefix(ref UpdateUserTitleDisplayNameRequest request, Action<UpdateUserTitleDisplayNameResult> resultCallback, Action<PlayFabError> errorCallback, object customData, Dictionary<string, string> extraHeaders) =>
            request.DisplayName = RandomString(UnityEngine.Random.Range(3, 12));
    }

    [HarmonyPatch(typeof(VRRig), nameof(VRRig.OnDisable))]
    public class RigDisablePatch
    {
        public static bool Prefix(VRRig __instance) =>
            __instance != VRRig.LocalRig;
    }

    [HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
    public class RigPostTickPatch
    {
        public static bool Prefix(VRRig __instance) =>
            !__instance.isLocal || __instance.enabled;
    }


}

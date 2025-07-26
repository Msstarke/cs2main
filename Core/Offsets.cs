namespace cs2aim.Core
{
    public static class Offsets
    {
        public static nint dwViewAngles = 0x1A78650;
        public static nint dwLocalPlayerPawn = 0x18590D0;
        public static nint dwEntityList = 0x1A05670;
        public static nint m_hPlayerPawn = 0x824;
        public static nint m_iHealth = 0x344;
        public static nint m_vOldOrigin = 0x1324;
        public static nint m_iTeamNum = 0x3E3;
        public static nint m_vecViewOffset = 0xCB0;
        public static nint m_lifeState = 0x348;
        public static nint m_modelState = 0x170;
        public static nint m_pGameSceneNode = 0x328;
        public const nint M_EntitySpottedState = 0x23D0;
        public const nint M_bIsSpotted = M_EntitySpottedState + 0x8;
    }
}

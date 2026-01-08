namespace ProjectS.Networking
{
    /// <summary>
    /// Centralized Photon custom property keys to avoid magic strings.
    /// </summary>
    public static class NetworkPropertyKeys
    {
        public static class Player
        {
            public const string Ready = "ready";
            public const string ClassId = "classId";
        }

        public static class Room
        {
            public const string Password = "password";
            public const string IsPrivate = "isPrivate";
        }
    }
}

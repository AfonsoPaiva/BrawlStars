using UnityEngine;

namespace Assets.Scripts.Commands
{
    public static class BrawlerCommandHelper
    {
        private static int _nextBrawlerID = 1;

        public static int GetNextBrawlerID()
        {
            return _nextBrawlerID++;
        }
        public static void ResetBrawlerIDCounter()
        {
            _nextBrawlerID = 1;
            Debug.Log("BrawlerCommandHelper: Brawler ID counter reset to 1");
        }
    }
}
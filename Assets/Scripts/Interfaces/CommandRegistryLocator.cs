using System;
using Assets.Scripts.Interfaces;


namespace Assets.Scripts.Interfaces
{
    public static class CommandRegistryLocator
    {
        public static ICommandRegistry Registry { get; set; }
    }
}
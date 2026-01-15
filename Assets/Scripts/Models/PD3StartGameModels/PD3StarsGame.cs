using System;
using System.Collections.Generic;

namespace Assets.Scripts.Models
{
    public class PD3StarsGame : UnityModelBaseClass
    {
        private readonly List<Brawler> _brawlers = new List<Brawler>();
        private Brawler _localBrawler;

        public IReadOnlyList<Brawler> Brawlers => _brawlers;
        
        public Brawler LocalBrawler
        {
            get => _localBrawler;
            private set
            {
                if (_localBrawler != value)
                {
                    _localBrawler = value;
                    OnPropertyChanged(nameof(LocalBrawler));
                    LocalBrawlerChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler<BrawlerAddedEventArgs> BrawlerAdded;
        public event EventHandler<BrawlerRemovedEventArgs> BrawlerRemoved;
        public event EventHandler LocalBrawlerChanged;

        public void AddBrawler(Brawler brawler, bool isLocalPlayer = false)
        {
            if (brawler == null) return;

            _brawlers.Add(brawler);
            brawler.Died += OnBrawlerDied;

            if (isLocalPlayer)
            {
                LocalBrawler = brawler;
            }

            BrawlerAdded?.Invoke(this, new BrawlerAddedEventArgs(brawler, isLocalPlayer));
        }

        public void RemoveBrawler(Brawler brawler)
        {
            if (brawler == null) return;

            brawler.Died -= OnBrawlerDied;
            _brawlers.Remove(brawler);

            if (LocalBrawler == brawler)
            {
                LocalBrawler = null;
            }

            BrawlerRemoved?.Invoke(this, new BrawlerRemovedEventArgs(brawler));
        }

        private void OnBrawlerDied(object sender, EventArgs e)
        {
            if (sender is Brawler brawler)
            {
                RemoveBrawler(brawler);
            }
        }
    }

    #region Event Args
    public class BrawlerAddedEventArgs : EventArgs
    {
        public Brawler Brawler { get; }
        public bool IsLocalPlayer { get; }

        public BrawlerAddedEventArgs(Brawler brawler, bool isLocalPlayer)
        {
            Brawler = brawler;
            IsLocalPlayer = isLocalPlayer;
        }
    }

    public class BrawlerRemovedEventArgs : EventArgs
    {
        public Brawler Brawler { get; }

        public BrawlerRemovedEventArgs(Brawler brawler)
        {
            Brawler = brawler;
        }
    }
    #endregion
}

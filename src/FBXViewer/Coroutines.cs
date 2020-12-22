using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Threading;

namespace FBXViewer
{
    public class Coroutines
    {
        private readonly DispatcherTimer _timer;
        private readonly ISet<Coroutine> _coroutines = new HashSet<Coroutine>();
        
        private readonly List<Coroutine> _toBeRemoved = new List<Coroutine>();

        public double DeltaTime;
        private DateTime _currentTime;

        public Coroutines()
        {
            _timer = new DispatcherTimer(DispatcherPriority.Normal);
            _timer.Interval = TimeSpan.FromMilliseconds(30);
            _timer.Tick += TimerOnTick;
            _currentTime = DateTime.Now;
            _timer.IsEnabled = true;
        }


        private void TimerOnTick(object? sender, EventArgs e)
        {
            DeltaTime = (DateTime.Now - _currentTime).TotalSeconds;
            _currentTime = DateTime.Now;
            
            foreach (var coroutine in _coroutines)
            {
                if (!coroutine.Enumerator.MoveNext())
                {
                    _toBeRemoved.Add(coroutine);
                }
            }

            foreach (var coroutine in _toBeRemoved)
            {
                Debug.WriteLine("Removing coroutine");
                _coroutines.Remove(coroutine);
            }

            _toBeRemoved.Clear();
            
            // Debug.WriteLine($"Any: {_coroutines.Any()}");
            // _timer.IsEnabled = _coroutines.Any();
        }

        public void StartCoroutine(IEnumerator enumerator)
        {
            _coroutines.Add(new Coroutine(enumerator));
        }


        private class Coroutine
        {
            public IEnumerator Enumerator { get; }

            public Coroutine(IEnumerator enumerator)
            {
                Enumerator = enumerator;
            }
        }
        
    }
}
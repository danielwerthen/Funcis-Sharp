using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FuncisSharp
{
	public class Funcis
	{
		private NodeMap<LocalNode> locals;
		private NodeMap<RemoteNode> remotes;
		private List<ProxyPipe> _proxies = new List<ProxyPipe>();
		
		private Dictionary<string, Signal> _signals = new Dictionary<string, Signal>();
		private readonly object _sigLock = new object();
		public string sigPath { get; set; }

		public Funcis(string sigPath = @"\Signals")
		{
			locals = new NodeMap<LocalNode>();
			remotes = new NodeMap<RemoteNode>();
			this.sigPath = sigPath;
		}

		public async Task Listen()
		{
			foreach (var proxy in _proxies)
			{
				var data = await proxy.ReceiveAsync();
				foreach (var sig in _signals.Values)
					await sig.HandleCall(data);
			}
		}

		ConcurrentQueue<JObject> _routedDatas = new ConcurrentQueue<JObject>();

		CancellationTokenSource listenCancel;
		Task[] listeners;
		Task dataHandler;
		public void BeginListen()
		{
			listenCancel = new CancellationTokenSource();
			var token = listenCancel.Token;
			listeners = _proxies.Select(proxy =>
			{
				return Task.Factory.StartNew(() =>
				{
					while (!token.IsCancellationRequested)
					{
						var t = proxy.ReceiveAsync();
						t.Wait();
						_routedDatas.Enqueue(t.Result);
					}
				}, token);
			}).ToArray();
			dataHandler = Task.Factory.StartNew(() =>
			{
				while (!token.IsCancellationRequested)
				{
					JObject data;
					if (_routedDatas.TryDequeue(out data))
					{
						Task[] ts = null;
						lock (_sigLock)
						{
							ts = _signals.Values.Select(row => row.HandleCall(data)).ToArray();
						}
						Task.WaitAll(ts);
					}
				}
			});
		}

		public void EndListen()
		{
			listenCancel.Cancel();
			Task.WaitAll(listeners, 1000);
		}

		private FileSystemWatcher watcher;
		public string GetWatchedPath()
		{
			if (watcher == null)
				return null;
			return watcher.Path;
		}
		private void startFSWatch()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + sigPath;
			watcher = new FileSystemWatcher();
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			watcher.Path = path;
			watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
			watcher.Filter = "*.is";
			watcher.Changed += watcher_Changed;
			watcher.Created += watcher_Changed;
			watcher.Deleted += watcher_Changed;
			watcher.Renamed += watcher_Renamed;
			watcher.EnableRaisingEvents = true;

			foreach (var file in Directory.GetFiles(watcher.Path, "*.is"))
			{
				var sig = File.ReadAllText(file);
				AddSignal(Path.GetFileName(file), sig);
			}

			loader = new Timer((s) =>
				{
					lock (loadSync)
					{
						foreach (var f in _toLoad.Distinct())
						{
							var sig = File.ReadAllText(f);
							AddSignal(Path.GetFileName(f), sig, true);
						}
						_toLoad.Clear();
					}
				}, null, Timeout.Infinite, Timeout.Infinite);
		}
		Timer loader;
		List<string> _toLoad = new List<string>();
		object loadSync = new object();
		private void Load(string path)
		{
			lock (loadSync)
			{
				_toLoad.Add(path);
			}
			loader.Change(500, Timeout.Infinite);
		}

		void watcher_Renamed(object sender, RenamedEventArgs e)
		{
			lock (_sigLock)
			{
				if (_signals.ContainsKey(e.OldName))
				{
					_signals[e.Name] = _signals[e.OldName];
					_signals[e.OldName] = null;
				}
			}
		}

		void watcher_Changed(object sender, FileSystemEventArgs e)
		{
			if (e.ChangeType == WatcherChangeTypes.Deleted)
			{
				RemoveSignal(e.Name);
			}
			else
			{
				Load(e.FullPath);
			}
		}

		public Signal AddSignal(string name, string sig, bool start = false)
		{
            lock (_sigLock)
            {
				RemoveSignal(name);

    			var signal = new Signal(locals, remotes);

    			signal.Load(sig);

    			if (start)
    			{
    				var t = signal.Start();
    				t.Wait();
    			}
				_signals[name] = signal;
    			return signal;
			}
		}

		public void RemoveSignal(string name)
		{
			lock (_sigLock)
			{
				if (_signals.ContainsKey(name))
				{
					_signals[name].Stop();
					_signals[name] = null;
				}
			}
		}

		public void AddRemoteNode(string uri, string name, string[] classes)
		{
			var pipe = new Pipe(uri);
			var rn = new RemoteNode(pipe, name, classes);
			remotes.AddNode(rn);
		}

		public void AddProxy(string uri)
		{
			var proxy = new ProxyPipe(locals, uri);
			_proxies.Add(proxy);
		}

		public LocalNode CreateNode(string name, string[] classes)
		{
			var loc = new LocalNode(name, classes);
			locals.AddNode(loc);
			return loc;
		}

		public async Task Start()
		{
			startFSWatch();
			List<Signal> sigs = null;
			lock (_sigLock)
			{
				sigs = _signals.Values.ToList();
			}
			foreach (var sig in sigs)
			{
				await sig.Start();
			}
		}
	}
}

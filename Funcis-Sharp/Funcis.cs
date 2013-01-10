using System;
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
		private string sigPath { get; set; }

		public Funcis(string sigPath = @"\Signals")
		{
			locals = new NodeMap<LocalNode>();
			remotes = new NodeMap<RemoteNode>();
			this.sigPath = sigPath;
			startFSWatch();
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

		private void startFSWatch()
		{
			FileSystemWatcher watcher = new FileSystemWatcher();
			watcher.Path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + sigPath;
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
							Console.WriteLine(Path.GetFileName(f));
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
			if (_signals.ContainsKey(e.OldName))
			{
				_signals[e.Name] = _signals[e.OldName];
				_signals[e.OldName] = null;
			}
		}

		void watcher_Changed(object sender, FileSystemEventArgs e)
		{
			if (e.ChangeType == WatcherChangeTypes.Deleted)
			{
				Console.WriteLine("Removed " + e.Name);
				RemoveSignal(e.Name);
			}
			else
			{
				Load(e.FullPath);
			}
		}

		public Signal AddSignal(string name, string sig, bool start = false)
		{
			var signal = new Signal(locals, remotes);
			signal.Load(sig);
			if (start)
			{
				var t = signal.Start();
				t.Wait();
			}
			RemoveSignal(name);
			_signals[name] = signal;
			return signal;
		}

		public void RemoveSignal(string name)
		{
			if (_signals.ContainsKey(name))
			{
				_signals[name].Stop();
				_signals[name] = null;
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

		public void Start()
		{
			foreach (var sig in _signals.Values)
			{
				sig.Start();
			}
		}
	}
}

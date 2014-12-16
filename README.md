TileMap2D
=====

2014/12/16:
Improvements, speed ups and lots of bugs squashed.  Added Ellipse room shape.  Code needs a lot of cleanup and organization.

2014/11/17:
Working at a beta level.  Generation now reads a struct Settings for dungeon varition.

2014/05/23:
Operating at a basic level now.  Buggy, does square rooms, 1 unit width tunnels, and dungeon type only, and many planned features moving forward.  Either something is wrong in tunnel building or the Clipper Library doesn't like something about the tunnel paths when offset.


Usage Example
===

	using System;
	using ioSoftSmiths.ioLog;
	using ioSoftSmiths.TileMap;

	class MainTest
	{

		static void Main(string[] args)
		{

			Console.SetBufferSize(500, 2000);
			Console.SetWindowSize(155, 50);

			Msg.DebugVerbosity = LogVerbosity.LOW;
			Msg.DebugAction = (_group, _msg) => Console.WriteLine(_group + " : " + _msg);

			Generate.SetDefaultMaterials(1, 2);

			//Generate.Settings.RndSeed = -1708127739;

			Generate.Settings.AspectRatio = 0f;

			Generate.Settings.RoomBufferMin = 1;
			Generate.Settings.RoomBufferMax = 3;
			Generate.Settings.TunnelBuffer = 3;
			Generate.Settings.TunnelPathMag = 3;
			Generate.Settings.TunnelRoutingStrength = Generate.Settings.MagStrength.Minimum;
			Generate.Settings.ClosestConnect = false;
			Generate.Settings.TunnelTurningCost = Generate.Settings.TTCost.Normal;

			Generate.Settings.RoomCountMin = 20;
			Generate.Settings.RoomCountMax = 26;
			Generate.Settings.RoomSizeMin = 6;
			Generate.Settings.RoomSizeMax = 20;

			Generate.Settings.RoomSpreadFactor = 0;

			Console.WriteLine(Generate.DebugSettingsToString());

			//System.Console.ReadLine();
			Generate.Dungeon();
				
			while(!Generate.GenerateDone)
			{
					
			}

			TileMap2D map = Generate.GetGeneratedMap();
			Console.WriteLine(map.DebugToString());
			//System.Console.ReadLine();
			Console.WriteLine("Closing...");
			Console.ReadLine();


		}
	}
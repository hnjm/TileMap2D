﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ioSoftSmiths.ioVector;
using ioSoftSmiths.ioSupport;

namespace ioSoftSmiths.TileMap
{


    public class TunnelNetwork
    {
        private static string TAG_DEBUG = "ioSoftSmiths.TileMap.TunnelNetwork";

        readonly List<IPath> m_Paths;
        public List<IPath> Paths { get { return m_Paths; } }

        public List<ISegment> Segments
        {
            get
            {
                var allPathSegs = new List<ISegment>();
                foreach (var path in m_Paths)
                    allPathSegs.AddRange(path.Segments);
                return allPathSegs;
            }
        }

        public HashSet<Room> ConnectedRooms
        {
            get
            {
                var rooms = new HashSet<Room>();
                foreach (var path in m_Paths)
                    rooms.UnionWith(path.Rooms);
                return rooms;
            }
        }


        public TunnelNetwork(IPath _path)
        {
            if (_path.Rooms.Length != 2)
                throw new ArgumentException("Can't create tunnel network with path (does not contain 2 rooms)");
            m_Paths = new List<IPath>() { _path };

        }

        public TunnelNetwork(IEnumerable<TunnelNetwork> _tNets, IPath _path)
        {
            m_Paths = new List<IPath>();
            foreach (var tNet in _tNets)
                m_Paths.AddRange(tNet.Paths);

            if (!Intersects(_path))
                throw new ArgumentException("Path does not intersect combined networks.");

            Add(_path);

        }

        public bool Intersects(TunnelNetwork _tunnel)
        {
            foreach (ISegment segment in Segments)
                foreach (ISegment newSegment in _tunnel.Segments)
                    if (segment.Intersects(newSegment)) return true;
            return false;
        }

        public bool Intersects(IPath _path)
        {
            foreach(IPath path in m_Paths)
                if (path.Intersects(_path))
                    return true;
            return false;


        }

        public bool ConnectsTo(Room _room)
        {
            if (_room == null)
                throw new ArgumentNullException(TAG_DEBUG + "ConnectsTo _room parameter cannot be null.");
            return m_Paths.Any(_path => _path.Rooms.Contains(_room));
        }

        public void Add(IPath _path)
        {
            //Get all coords in tunnel network
            var tCoords = GetAllPathCoords(true); //TODO include doorways?

            int start = 0;
            int end = _path.Points.Count - 1;
            for (int pathIdx = 1; pathIdx <= end; ++pathIdx)
            {
                //If coord lands on existing path or is at the end
                if (tCoords.Contains(_path.Points[pathIdx]) || pathIdx == end)
                {
                    if (pathIdx == start + 1)
                    {
                        start++;
                        continue;
                    }
                    var newPathCoords = new List<IVector2>();
                    Room fromRoom = null;
                    Room toRoom = null;
                    if (start == 0) fromRoom = _path.Rooms[0];
                    for (int newPathIdx = start; newPathIdx <= pathIdx; ++newPathIdx)
                        newPathCoords.Add(_path.Points[newPathIdx]);
                    if (newPathCoords.Contains(_path.Points[end]))
                        toRoom = _path.Rooms[1];
                    m_Paths.Add(new IPath(newPathCoords, fromRoom, toRoom));
                    start = pathIdx;

                }
            }
        }



        public class IPath
        {
            private List<IVector2> m_Path;
            private readonly Room m_FromRoom;
            private readonly Room m_ToRoom;

            public List<IVector2> Points { get { return m_Path; } internal set { m_Path = value; } }

            public List<IVector2> PointsNoDoors
            {
                get
                {
                    var noDoorsPath = new List<IVector2>(m_Path);

                    if (m_FromRoom != null)
                        foreach (var doorway in m_FromRoom.Doorways)
                            noDoorsPath.Remove(doorway);

                    if (m_ToRoom != null)
                        foreach (var doorway in m_ToRoom.Doorways)
                            noDoorsPath.Remove(doorway);
                    return noDoorsPath;
                }


            }

            public List<IVector2> PointsReverse
            {
                get
                {
                    List<IVector2> reversePath = new List<IVector2>();
                    for (int i = m_Path.Count - 1; i >= 0; --i) reversePath.Add(m_Path[i]);
                    return reversePath;
                }
            }
            
            public Room[] Rooms { get { return new Room[2] { m_FromRoom, m_ToRoom }; } }

            public List<int> CornerIndicies
            {
                get
                {
                    var cornerIndicies = new List<int>();
                    for (int idx = 1; idx < Points.Count - 1; ++idx)
                    {
                        var prevCoord = Points[idx - 1];
                        var nextCoord = Points[idx + 1];
                        if (prevCoord.x != nextCoord.x && prevCoord.y != nextCoord.y)
                            cornerIndicies.Add(idx);
                    }
                    return cornerIndicies;

                }
            }

            public List<ISegment> Segments
            {
                get
                {
                    var pathSegs = new List<ISegment>();
                    foreach (var turnIdx in CornerIndiciesWithEnds)
                        if (turnIdx != m_Path.Count - 1)
                            pathSegs.Add(new ISegment(m_Path[turnIdx], m_Path[turnIdx + 1]));
                    return pathSegs;
                }
            }

            public List<int> CornerIndiciesWithEnds
            {
                get
                {
                    var cornerIndicies = new List<int>();
                    cornerIndicies.Add(0);
                    for (int idx = 1; idx < Points.Count - 1; ++idx)
                    {
                        var prevCoord = Points[idx - 1];
                        var nextCoord = Points[idx + 1];
                        if (prevCoord.x != nextCoord.x && prevCoord.y != nextCoord.y)
                            cornerIndicies.Add(idx);
                    }
                    cornerIndicies.Add(m_Path.Count - 1);
                    return cornerIndicies;

                }
            }

            public bool IsStraightaway(int _index)
            {
                if (_index <= 0 || _index >= Points.Count - 1)
                    return false;

                var prevCoord = Points[_index - 1];
                var nextCoord = Points[_index + 1];
                if (prevCoord.x == nextCoord.x || prevCoord.y == nextCoord.y)
                    return true;
                return false;
            }


            public IPath(List<IVector2> _path, Room _from, Room _to)
            {
                m_Path = new List<IVector2>(_path);
                m_FromRoom = _from;
                m_ToRoom = _to;
            }


            public bool Intersects(IPath _path)
            {
                foreach(var coord in Points)
                    if (_path.Points.Contains(coord))
                        return true;
                return false;

            }

            public List<IVector2> GetIntersections(IPath _path)
            {
                var intscts = new List<IVector2>();
                foreach (ISegment segment in Segments)
                    foreach (ISegment thatSegment in _path.Segments)
                    {
                        var possibleInt = segment.GetIntersection(thatSegment);
                        if (possibleInt != null) intscts.Add(possibleInt.Value);
                    }
                return intscts;
            }

            /// <summary>
            /// If _from or _to is in the path, the returned path is this path cut such that it starts at _from and/or ends at _to
            /// if _cutRooms is true it removes the associated _from or _to room if _from/_to is not null
            /// </summary>
            /// <param name="_from"></param>
            /// <param name="_to"></param>
            /// <param name="_cutRooms"></param>
            /// <returns></returns>
            public IPath Cut(IVector2? _from, IVector2? _to, bool _cutRooms = true)
            {
                IVector2 from;
                Room fromRoom = null;
                IVector2 to;
                Room toRoom = null;

                if (_from == null)
                {
                    from = Points[0];
                    fromRoom = m_FromRoom;
                }
                else
                {
                    from = _from.Value;
                    if (!_cutRooms) fromRoom = m_FromRoom;
                }

                if (_to == null)
                {
                    to = Points.Last();
                    toRoom = m_ToRoom;
                }
                else
                {
                    to = _to.Value;
                    if (!_cutRooms) toRoom = m_ToRoom;
                }


                var newPath = new List<IVector2>();
                bool copy = false;

                for (int i = 0; i < m_Path.Count - 1; ++i)
                {
                    var seg = new ISegment(m_Path[i], m_Path[i + 1]);
                    if (seg.Contains(from))
                    {
                        copy = true;
                        newPath.Add(from);
                        i++;
                    }
                    if (copy)
                    {
                        newPath.Add(m_Path[i]);
                        if (i >= m_Path.Count - 1) break;
                        var lastSeg = new ISegment(newPath.Last(), m_Path[i + 1]);
                        if (lastSeg.Contains(to))
                        {
                            newPath.Add(to);
                            break;
                        }
                    }
                }
                return new IPath(newPath, fromRoom, toRoom);


            }

            public bool ConnectsTo(Room _room)
            {
                if (Rooms.Contains(_room))
                    return true;
                else
                    return false;

            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                foreach (var coord in m_Path)
                {
                    sb.AppendLine(coord.ToString());
                }
                return sb.ToString();
            }


        }

        public HashSet<IVector2> GetAllPathCoords(bool _includeDoorways)
        {
            var coords = new HashSet<IVector2>();
            if (_includeDoorways)
            {
                foreach (var path in Paths)
                    coords.UnionWith(path.Points);
            }
            else
            {
                foreach (var path in Paths)
                    coords.UnionWith(path.PointsNoDoors);
            }

            return coords;

        }

        /*public static TunnelNetwork Combine(HashSet<TunnelNetwork> _toCombine)
        {
            var allPaths = new HashSet<IPath>();
            foreach(var tNet in _toCombine)
                foreach (var tPath in tNet.Paths)
                    allPaths.Add(tPath);


            var newNet = new TunnelNetwork(allPaths.First());
            allPaths.Remove(allPaths.First());

            while (allPaths.Count != 0)
            {
                IPath curPath = null;
                foreach (var tPath in allPaths.Where(tPath => newNet.Intersects(tPath)))
                    curPath = tPath;
                newNet.Add(curPath);
                allPaths.Remove(curPath);
            }
            return newNet;

        }*/
    }


}

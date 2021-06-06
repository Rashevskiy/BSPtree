using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public static class RoomExtensions
{

    private const int timeout = 1000;
    private const float stepSize = 1;






    public static Room[] BSP(this Room[] rooms)
    {
        List<Room> newRooms = new List<Room>();
        for (int i = 0; i < rooms.Length; i++)
        {
            newRooms.AddRange(rooms[i].BSP());
        }
        return newRooms.ToArray();
    }



    public static Room[] Resize(this Room[] rooms, int addWidth, int addHeight, int addLength)
    {
        for (int i = 0; i < rooms.Length; i++)
        {
            rooms[i].Resize(addWidth, addHeight, addLength);
        }
        return rooms;
    }

    public static Room[] ShiftSize(this Room[] rooms, int shiftWidth, int shiftHeight, int shiftLength)
    {
        for (int i = 0; i < rooms.Length; i++)
        {
            rooms[i].ShiftSize(shiftWidth, shiftHeight, shiftLength);
        }
        return rooms;
    }

    public static Room[] ShiftSize(this Room[] rooms, Func<int> shiftWidth, Func<int> shiftHeight, Func<int> shiftLength)
    {
        for (int i = 0; i < rooms.Length; i++)
        {
            rooms[i].ShiftSize(shiftWidth(), shiftHeight(), shiftLength());
        }
        return rooms;
    }

    public static Vector3Int Min(this Room[] rooms)
    {
        Vector3Int min = Vector3Int.one * int.MaxValue;
        for (int i = 0; i < rooms.Length; i++)
        {
            min.x = rooms[i].x < min.x ? rooms[i].x : min.x;
            min.y = rooms[i].y < min.y ? rooms[i].y : min.y;
            min.z = rooms[i].z < min.z ? rooms[i].z : min.z;
        }
        return min;
    }

    public static Vector3Int Max(this Room[] rooms)
    {
        Vector3Int max = Vector3Int.one * int.MinValue;
        for (int i = 0; i < rooms.Length; i++)
        {
            max.x = rooms[i].x + rooms[i].width > max.x ? rooms[i].x + rooms[i].width : max.x;
            max.y = rooms[i].y + rooms[i].height > max.y ? rooms[i].y + rooms[i].height : max.y;
            max.z = rooms[i].z + rooms[i].length > max.z ? rooms[i].z + rooms[i].length : max.z;
        }
        return max;
    }

    private static List<Room> Connect(this Room left, Room right)
    {
        var halls = new List<Room>();
        if (left.width > left.length)
        {
            var tmp = left;
            left = right;
            right = tmp;
        }
        else if (right.length > right.width)
        {
            var tmp = left;
            left = right;
            right = tmp;
        }
        Vector3Int point1 = left.Center;
        Vector3Int point2 = right.Center;
        Vector3Int delta = point2 - point1;
        if (delta.x > 0)
        {
            halls.Add(new Room(new Vector3Int(point1.x, point1.y, point1.z), new Vector3Int(delta.x + 1, 1, 1)));
        }
        else
        {
            halls.Add(new Room(new Vector3Int(point1.x + delta.x, point1.y, point1.z), new Vector3Int(-delta.x + 1, 1, 1)));
        }
        if (delta.z > 0)
        {
            halls.Add(new Room(new Vector3Int(point2.x, point2.y, point2.z - delta.z), new Vector3Int(1, 1, delta.z + 1)));
        }
        else
        {
            halls.Add(new Room(new Vector3Int(point2.x, point2.y, point2.z), new Vector3Int(1, 1, -delta.z + 1)));
        }
        return halls;
    }


    private static RectRoom[] ToRects(Room[] rooms, int sizeAdd = 0)
    {
        RectRoom[] rects = new RectRoom[rooms.Length];
        for (int i = 0; i < rooms.Length; i++)
        {
            Vector2 center = new Vector2(rooms[i].x, rooms[i].z);
            Vector2 size = new Vector2(rooms[i].width + 1 + sizeAdd, rooms[i].length + 1 + sizeAdd);
            rects[i] = new RectRoom(center, size, rooms[i]);
        }
        return rects;
    }

  


    public static void Resize(this Room room, int addWidth, int addHeight, int addLength)
    {
        room.width += addWidth;
        room.height += addHeight;
        room.length += addLength;
        room.x -= addWidth / 2;
        room.y -= addHeight / 2;
        room.z -= addLength / 2;
        Mathf.Clamp(room.width, 0, float.MaxValue);
        Mathf.Clamp(room.height, 0, float.MaxValue);
        Mathf.Clamp(room.length, 0, float.MaxValue);
    }

    public static void ShiftSize(this Room room, int shiftWidth, int shiftHeight, int shiftLength)
    {
        room.width -= Math.Abs(shiftWidth);
        room.height -= Math.Abs(shiftHeight);
        room.length -= Math.Abs(shiftLength);
        if (shiftWidth > 0)
            room.x += shiftWidth;
        if (shiftHeight > 0)
            room.y += shiftHeight;
        if (shiftLength > 0)
            room.z += shiftLength;
    }

    private static void AddDelta(this RectRoom[] rects)
    {
        for (int i = 0; i < rects.Length; i++)
        {
            rects[i].AddDelta();
        }
    }

    private class RectRoom
    {
        Rect rect;
        public Room room;

        public Vector2 center
        {
            get => rect.center;
            set
            {
                rect.center = value;
                room.x = Mathf.RoundToInt(center.x - size.x / 2);
                room.z = Mathf.RoundToInt(center.y - size.y / 2);
            }
        }

        public Vector2 size
        {
            get => rect.size;
            set
            {
                rect.size = value;
            }
        }

        public bool Overlaps(RectRoom rectRoom)
        {
            return rect.Overlaps(rectRoom.rect);
        }

        public RectRoom(Vector2 center, Vector2 size, Room room)
        {
            this.room = room;
            rect = new Rect(Vector2.zero, size);
            this.size = size;
            this.center = center;
        }

        public void AddDelta()
        {
            center += UnityEngine.Random.insideUnitCircle.normalized;
        }
    }
}
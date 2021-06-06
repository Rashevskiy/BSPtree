using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace BSP
{
    public class Leaf
    {
        public Leaf left, right;

        public int x;
        public int y;
        public int z;
        public int width;
        public int height;
        public int length;

        public int debugId;

        public static System.Random rnd = new System.Random();

        public static int minSize = 4;
        public static int maxSize = 4;
        public static bool threading = false;
        public static bool sizeDependent = true;
        public static int maxIter = 5;

        private static float maxDeltaVolume = 0.5f;
        public static float MaxDeltaVolume
        {
            get => maxDeltaVolume;
            set => maxDeltaVolume = Mathf.Clamp(value, 0, 1);
        }

        private static int debugCounter = 0;

        public Leaf(int x, int y, int z, int width, int height, int length)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.width = width;
            this.height = height;
            this.length = length;
            debugId = debugCounter;
            debugCounter++;
        }

        public Leaf(Room room)
        {
            this.x = room.x;
            this.y = room.y;
            this.z = room.z;
            this.width = room.width;
            this.height = room.height;
            this.length = room.length;
            debugId = debugCounter;
            debugCounter++;
        }

        public bool IAmLeaf => left == null && right == null;

        public bool SplitRandom()
        {
            if (Mathf.Min((float)length, (float)width) / 2 < minSize)
            {
                return false;
            }

            bool splitH;
            if ((float)width / (float)length >= 1.1)
            {
                splitH = false;
            }
            else if ((float)length / (float)width >= 1.1)
            {
                splitH = true;
            }
            else
            {
                splitH = rnd.NextDouble() > 0.5;
            }

            if (splitH)
            {
                int split = rnd.Next(minSize, (length - minSize));
                left = new Leaf(x, y, z, width, height, split);
                right = new Leaf(x, y, z + split - 1, width, height, length - split + 1);
            }
            else
            {
                int split = rnd.Next(minSize, (width - minSize));
                left = new Leaf(x, y, z, split, height, length);
                right = new Leaf(x + split - 1, y, z, width - split + 1, height, length);
            }
            return true;
        }

        public bool SplitSizeDependent()
        {
            var splitRes = SplitRandom();
            bool good = false;
            for (int i = 0; (i < maxIter || !good) && splitRes; i++)
            {
                var leftVolume = left.width * left.height * left.length;
                var rightVolume = right.width * right.height * right.length;
                var deltaVolume = leftVolume > rightVolume ? (float)rightVolume / leftVolume : (float)leftVolume / rightVolume;
                if (deltaVolume > maxDeltaVolume)
                    good = true;
                else
                    splitRes = SplitRandom();
            }
            return splitRes;
        }

        private List<Vector3Int> Roads()
        {
            var halls = new List<Vector3Int>();
            Vector3Int point1 = new Vector3Int(left.x + left.width / 2, left.y, left.z + left.length / 2);
            Vector3Int point2 = new Vector3Int(right.x + right.width / 2, right.y, right.z + right.length / 2);
            Vector3Int delta = point2 - point1;
            for (int i = 0; i < Mathf.Abs(delta.x); i++)
            {
                halls.Add(new Vector3Int(point1.x + i * System.Math.Sign(delta.x), point1.y, point1.z));
            }
            for (int i = 0; i < Mathf.Abs(delta.z); i++)
            {
                halls.Add(new Vector3Int(point2.x, point2.y, point2.z - i * System.Math.Sign(delta.z)));
            }
            return halls;
        }

        private List<Vector3Int> Roads(Leaf leaf)
        {
            var halls = new List<Vector3Int>();
            if (leaf.IAmLeaf)
                return halls;
            halls.AddRange(leaf.Roads());
            halls.AddRange(Roads(leaf.left));
            halls.AddRange(Roads(leaf.right));
            return halls;
        }

        private Room[] GetHalls()
        {
            var halls = Roads(this);
            var hallsSet = new HashSet<Room>();
            for (int i = 0; i < halls.Count; i++)
            {
                hallsSet.Add(new Room(halls[i], Vector3Int.one));
            }
            return hallsSet.ToArray();
        }

        [System.Obsolete("TODO: think about comments")]
        public Room[] GetRooms()
        {
            List<Room> rooms = new List<Room>();
            if (sizeDependent)
                CreateBSPSizeDependent(this);
            else
                CreateBSP(this);
            GetRooms(this, rooms);
            //var halls = GetHalls();
            //rooms.AddRange(halls);
            return rooms.ToArray();
        }

        private void GetRooms(Leaf current, List<Room> rooms)
        {
            if (current.IAmLeaf)
            {
                Room r = new Room(current.width, current.length);
                r.x = current.x;
                r.y = current.y;
                r.z = current.z;
                rooms.Add(r);
            }
            if (current.left != null)
                GetRooms(current.left, rooms);
            if (current.right != null)
                GetRooms(current.right, rooms);
        }

        private void CreateBSP(Leaf leaf)
        {
            if (leaf.width > maxSize || leaf.length > maxSize || rnd.NextDouble() > 0.25)
            {
                if (leaf.SplitRandom())
                {
                    if (threading)
                    {
                        var t1 = Task.Run(() => CreateBSP(leaf.left));
                        var t2 = Task.Run(() => CreateBSP(leaf.right));
                        Task.WaitAll(t1, t2);
                    }
                    else
                    {
                        CreateBSP(leaf.left);
                        CreateBSP(leaf.right);
                    }
                }
            }
        }

        private void CreateBSPSizeDependent(Leaf leaf)
        {
            if (leaf.width > maxSize || leaf.length > maxSize || rnd.NextDouble() > 0.25)
            {
                if (leaf.SplitSizeDependent())
                {
                    if (threading)
                    {
                        var t1 = Task.Run(() => CreateBSPSizeDependent(leaf.left));
                        var t2 = Task.Run(() => CreateBSPSizeDependent(leaf.right));
                        Task.WaitAll(t1, t2);
                    }
                    else
                    {
                        CreateBSPSizeDependent(leaf.left);
                        CreateBSPSizeDependent(leaf.right);
                    }
                }
            }

        }
    }

}
using MazeHell.Level;
using NUnit.Framework;
using UnityEngine;

namespace MazeHell.Tests
{
    public class LevelMapTests
    {
        [Test]
        public void ExplorationRevealsOnlyEnteredRoomsAndTraversedPassages()
        {
            LevelMap map = new LevelMap();
            int entrance = map.AddRoom(new Bounds(Vector3.zero, new Vector3(10f, 4f, 10f)));
            int middle = map.AddRoom(new Bounds(Vector3.right * 20f, new Vector3(10f, 4f, 10f)));
            int exit = map.AddRoom(new Bounds(Vector3.right * 40f, new Vector3(10f, 4f, 10f)));
            map.Connect(entrance, middle);
            map.Connect(middle, exit);
            map.Connect(entrance, exit);
            map.SetEndpoints(entrance, exit);

            Assert.IsTrue(map.VisitPosition(Vector3.zero));
            Assert.IsTrue(map.IsRoomDiscovered(entrance));
            Assert.IsFalse(map.IsRoomDiscovered(middle));
            Assert.IsFalse(map.IsRoomDiscovered(exit));
            Assert.IsFalse(map.Passages[0].IsDiscovered);
            Assert.IsFalse(map.VisitPosition(Vector3.right * 10f));
            Assert.AreEqual(entrance, map.CurrentRoomIndex);

            Assert.IsTrue(map.VisitPosition(Vector3.right * 20f));
            Assert.AreEqual(middle, map.CurrentRoomIndex);
            Assert.IsTrue(map.Passages[0].IsDiscovered);
            Assert.IsFalse(map.Passages[1].IsDiscovered);
            Assert.IsTrue(map.VisitPosition(Vector3.right * 40f));
            Assert.IsTrue(map.IsRoomDiscovered(exit));
            Assert.IsTrue(map.Passages[1].IsDiscovered);
            Assert.IsFalse(map.Passages[2].IsDiscovered);
            Assert.IsTrue(map.VisitPosition(Vector3.zero));
            Assert.IsTrue(map.Passages[2].IsDiscovered);
            Assert.IsTrue(map.IsRoomDiscovered(middle));
            Assert.IsFalse(map.VisitPosition(Vector3.up * 100f));
        }

        [Test]
        public void NewLevelStartsUnexplored()
        {
            LevelMap map = new LevelMap();
            int room = map.AddRoom(new Bounds(Vector3.zero, Vector3.one * 10f));
            Assert.AreEqual(-1, map.CurrentRoomIndex);
            Assert.IsFalse(map.IsRoomDiscovered(room));
            Assert.IsFalse(map.VisitPosition(Vector3.one * 100f));
        }
    }
}

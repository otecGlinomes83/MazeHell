using System;
using MazeHell.Level;
using UnityEngine;
using UnityEngine.UI;

namespace MazeHell.UI
{
    [DisallowMultipleComponent]
    public class MinimapGraphic : MaskableGraphic
    {
        private const float Padding = 16f;
        private const float RoomSize = 18f;
        private const float RoomBorder = 1.5f;
        private const float PassageWidth = 2f;
        private const float MarkerSize = 5f;
        private const float MinimumWorldSpan = 40f;

        [SerializeField] private Color _backgroundColor = new Color(0.015f, 0.012f, 0.012f, 1f);
        [SerializeField] private Color _roomColor = new Color(0.65f, 0.59f, 0.48f, 1f);
        [SerializeField] private Color _entranceColor = new Color(0.3f, 0.8f, 0.48f, 1f);
        [SerializeField] private Color _exitColor = new Color(0.95f, 0.24f, 0.17f, 1f);
        [SerializeField] private Color _playerColor = new Color(1f, 0.92f, 0.65f, 1f);

        private LevelMap _map;
        private Vector2 _worldCenter;
        private float _mapScale;

        public LevelMap Map => _map;

        public void Setup(LevelMap map)
        {
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            _map = map;
            raycastTarget = false;
            SetVerticesDirty();
        }

        public void Tick(Vector3 playerPosition)
        {
            if (_map != null && _map.VisitPosition(playerPosition))
            {
                SetVerticesDirty();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vertices)
        {
            vertices.Clear();
            Rect panel = GetPixelAdjustedRect();
            AddRectangle(vertices, panel, _roomColor);
            AddRectangle(vertices, new Rect(panel.x + RoomBorder, panel.y + RoomBorder,
                Mathf.Max(0f, panel.width - RoomBorder * 2f), Mathf.Max(0f, panel.height - RoomBorder * 2f)),
                _backgroundColor);

            if (_map == null || _map.CurrentRoomIndex < 0 || panel.width <= Padding * 2f || panel.height <= Padding * 2f)
            {
                return;
            }

            FitDiscoveredRooms(panel);
            DrawPassages(vertices, panel.center);
            DrawRooms(vertices, panel.center);
            Vector2 player = Project(_map.Rooms[_map.CurrentRoomIndex].center, panel.center);
            AddRectangle(vertices, new Rect(player - Vector2.one * MarkerSize * 0.5f,
                Vector2.one * MarkerSize), _playerColor);
        }

        private void FitDiscoveredRooms(Rect panel)
        {
            Bounds current = _map.Rooms[_map.CurrentRoomIndex];
            Vector2 minimum = new Vector2(current.center.x, current.center.z);
            Vector2 maximum = minimum;

            for (int roomIndex = 0; roomIndex < _map.Rooms.Count; roomIndex++)
            {
                if (_map.IsRoomDiscovered(roomIndex) == false)
                {
                    continue;
                }

                Vector3 center = _map.Rooms[roomIndex].center;
                Vector2 point = new Vector2(center.x, center.z);
                minimum = Vector2.Min(minimum, point);
                maximum = Vector2.Max(maximum, point);
            }

            foreach (LevelMapPassage passage in _map.Passages)
            {
                if (passage.IsDiscovered == false)
                {
                    continue;
                }

                foreach (Vector2 point in passage.Points)
                {
                    minimum = Vector2.Min(minimum, point);
                    maximum = Vector2.Max(maximum, point);
                }
            }

            _worldCenter = (minimum + maximum) * 0.5f;
            Vector2 span = maximum - minimum;
            _mapScale = Mathf.Min((panel.width - Padding * 2f) / Mathf.Max(MinimumWorldSpan, span.x),
                (panel.height - Padding * 2f) / Mathf.Max(MinimumWorldSpan, span.y));
        }

        private void DrawPassages(VertexHelper vertices, Vector2 panelCenter)
        {
            foreach (LevelMapPassage passage in _map.Passages)
            {
                if (passage.IsDiscovered == false)
                {
                    continue;
                }

                for (int pointIndex = 1; pointIndex < passage.Points.Length; pointIndex++)
                {
                    Vector2 start = panelCenter + (passage.Points[pointIndex - 1] - _worldCenter) * _mapScale;
                    Vector2 end = panelCenter + (passage.Points[pointIndex] - _worldCenter) * _mapScale;
                    Vector2 direction = end - start;
                    Vector2 normal = new Vector2(-direction.y, direction.x).normalized * PassageWidth * 0.5f;
                    AddQuad(vertices, start - normal, start + normal, end + normal, end - normal, _roomColor);
                }
            }
        }

        private void DrawRooms(VertexHelper vertices, Vector2 panelCenter)
        {
            for (int roomIndex = 0; roomIndex < _map.Rooms.Count; roomIndex++)
            {
                if (_map.IsRoomDiscovered(roomIndex) == false)
                {
                    continue;
                }

                Color border = _roomColor;

                if (roomIndex == _map.EntranceRoomIndex)
                {
                    border = _entranceColor;
                }
                else if (roomIndex == _map.ExitRoomIndex)
                {
                    border = _exitColor;
                }

                Vector2 center = Project(_map.Rooms[roomIndex].center, panelCenter);
                AddRectangle(vertices, new Rect(center - Vector2.one * RoomSize * 0.5f, Vector2.one * RoomSize), border);
                float insideSize = RoomSize - RoomBorder * 2f;
                AddRectangle(vertices, new Rect(center - Vector2.one * insideSize * 0.5f,
                    Vector2.one * insideSize), Color.black);
            }
        }

        private Vector2 Project(Vector3 worldPosition, Vector2 panelCenter)
        {
            return panelCenter + (new Vector2(worldPosition.x, worldPosition.z) - _worldCenter) * _mapScale;
        }

        private void AddRectangle(VertexHelper vertices, Rect rectangle, Color tint)
        {
            AddQuad(vertices, new Vector2(rectangle.xMin, rectangle.yMin), new Vector2(rectangle.xMin, rectangle.yMax),
                new Vector2(rectangle.xMax, rectangle.yMax), new Vector2(rectangle.xMax, rectangle.yMin), tint);
        }

        private void AddQuad(VertexHelper vertices, Vector2 first, Vector2 second, Vector2 third, Vector2 fourth, Color tint)
        {
            int startIndex = vertices.currentVertCount;
            Color finalColor = tint * color;
            vertices.AddVert(first, finalColor, Vector2.zero);
            vertices.AddVert(second, finalColor, Vector2.zero);
            vertices.AddVert(third, finalColor, Vector2.zero);
            vertices.AddVert(fourth, finalColor, Vector2.zero);
            vertices.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertices.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
        }
    }
}

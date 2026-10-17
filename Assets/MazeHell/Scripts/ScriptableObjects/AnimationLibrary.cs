using System;
using UnityEngine;

namespace MazeHell.Configuration
{
    [CreateAssetMenu(fileName = "AnimationLibrary", menuName = "MazeHell/Configurations/Animation Library")]
    public class AnimationLibrary : ScriptableObject
    {
        [Header("Enemy")]
        [SerializeField] private EnemySpriteSetData _enemySet = new EnemySpriteSetData();

        [Header("Weapon")]
        [SerializeField] private WeaponSpriteSetData _weaponSet = new WeaponSpriteSetData();

        [Header("Face")]
        [SerializeField] private FaceSpriteSetData _faceSet = new FaceSpriteSetData();

        public EnemySpriteSetData EnemySet => _enemySet;

        public WeaponSpriteSetData WeaponSet => _weaponSet;

        public FaceSpriteSetData FaceSet => _faceSet;
    }

    [Serializable]
    public class EnemySpriteSetData
    {
        [Header("Walk")]
        [SerializeField] private Sprite[] _walkFrontSprites = new Sprite[0];

        [SerializeField] private Sprite[] _walkSideSprites = new Sprite[0];

        [SerializeField] private Sprite[] _walkBackSprites = new Sprite[0];

        [Header("Combat")]
        [SerializeField] private Sprite[] _windupSprites = new Sprite[0];

        [SerializeField] private Sprite[] _lungeSprites = new Sprite[0];

        [SerializeField] private Sprite[] _painSprites = new Sprite[0];

        [SerializeField] private Sprite _corpseSprite;

        [Header("Playback")]
        [Min(0.01f)]
        [SerializeField] private float _walkFrameInterval = 0.15f;

        [Min(0.01f)]
        [SerializeField] private float _attackFrameInterval = 0.1f;

        [Min(0.1f)]
        [SerializeField] private float _worldHeight = 1.8f;

        [Min(0f)]
        [SerializeField] private float _corpseGroundOffset = 0.04f;

        public float CorpseGroundOffset => _corpseGroundOffset;

        public Sprite[] WalkFrontSprites => _walkFrontSprites;

        public Sprite[] WalkSideSprites => _walkSideSprites;

        public Sprite[] WalkBackSprites => _walkBackSprites;

        public Sprite[] WindupSprites => _windupSprites;

        public Sprite[] LungeSprites => _lungeSprites;

        public Sprite[] PainSprites => _painSprites;

        public Sprite CorpseSprite => _corpseSprite;

        public float WalkFrameInterval => _walkFrameInterval;

        public float AttackFrameInterval => _attackFrameInterval;

        public float WorldHeight => _worldHeight;
    }

    [Serializable]
    public class WeaponSpriteSetData
    {
        [SerializeField] private Sprite _idleSprite;

        [SerializeField] private Sprite[] _shotFrames = new Sprite[0];

        [SerializeField] private Sprite[] _volleyFrames = new Sprite[0];

        [SerializeField] private Sprite[] _reloadFrames = new Sprite[0];

        [SerializeField] private Sprite _emptySprite;

        [SerializeField] private Sprite _blockSprite;

        [Min(0.01f)]
        [SerializeField] private float _frameInterval = 0.08f;

        public Sprite IdleSprite => _idleSprite;

        public Sprite[] ShotFrames => _shotFrames;

        public Sprite[] VolleyFrames => _volleyFrames;

        public Sprite[] ReloadFrames => _reloadFrames;

        public Sprite EmptySprite => _emptySprite;

        public Sprite BlockSprite => _blockSprite;

        public float FrameInterval => _frameInterval;
    }

    [Serializable]
    public class FaceSpriteSetData
    {
        [SerializeField] private FaceTierData _healthy = new FaceTierData();

        [SerializeField] private FaceTierData _scratched = new FaceTierData();

        [SerializeField] private FaceTierData _bleeding = new FaceTierData();

        [SerializeField] private FaceTierData _critical = new FaceTierData();

        [SerializeField] private FaceTierData _nearDeath = new FaceTierData();

        [SerializeField] private FaceTierData _hit = new FaceTierData();

        [Min(0.01f)]
        [SerializeField] private float _loopFrameInterval = 0.25f;

        public FaceTierData Healthy => _healthy;

        public FaceTierData Scratched => _scratched;

        public FaceTierData Bleeding => _bleeding;

        public FaceTierData Critical => _critical;

        public FaceTierData NearDeath => _nearDeath;

        public FaceTierData Hit => _hit;

        public float LoopFrameInterval => _loopFrameInterval;
    }

    [Serializable]
    public class FaceTierData
    {
        [SerializeField] private Sprite[] _frames = new Sprite[0];

        [SerializeField] private float[] _frameDurations = new float[0];

        public FaceTierData()
        {
        }

        public FaceTierData(Sprite[] frames)
        {
            _frames = frames;
        }

        public Sprite[] Frames => _frames;

        public float[] FrameDurations => _frameDurations;
    }
}

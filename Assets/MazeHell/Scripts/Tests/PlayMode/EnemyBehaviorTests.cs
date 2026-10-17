using System.Collections;
using MazeHell.Combat;
using MazeHell.Configuration;
using MazeHell.Enemies;
using MazeHell.Player;
using NUnit.Framework;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace MazeHell.Tests
{
    public class EnemyBehaviorTests
    {
        private const float FixedDelta = 0.02f;
        private const float GeometryOriginX = 1000f;
        private const float GeometryOriginZ = 1000f;
        private const float HeroMaxHealth = 100f;
        private const float HomeTolerance = 0.5f;
        private const int InvestigationFrameLimit = 500;

        private GameObject _geometryRoot;
        private GameObject _hero;
        private GameObject _enemy;
        private GameObject _spriteChild;
        private GameObject _cameraObject;
        private GameObject _wall;
        private EnemyConfig _enemyConfig;
        private PlayerMovementConfig _movementConfig;
        private PlayerCombatConfig _combatConfig;
        private AnimationLibrary _library;
        private ShotNoise _noise;
        private EnemyBrain _brain;
        private Health _heroHealth;
        private PlayerDefense _heroDefense;
        private int _heroChangedCount;
        private float _previousCaptureDelta;
        private bool _hasNavMeshData;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _previousCaptureDelta = Time.captureDeltaTime;
            Time.captureDeltaTime = FixedDelta;
            _heroChangedCount = 0;
            _hasNavMeshData = false;

            _geometryRoot = new GameObject("EnemyBehaviorGeometry");
            _geometryRoot.transform.position = new Vector3(GeometryOriginX, 0f, GeometryOriginZ);

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(_geometryRoot.transform);
            floor.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(40f, 1f, 40f);

            NavMeshSurface surface = _geometryRoot.AddComponent<NavMeshSurface>();
            Physics.SyncTransforms();
            surface.BuildNavMesh();
            _hasNavMeshData = true;

            _noise = new ShotNoise();
            _enemyConfig = ScriptableObject.CreateInstance<EnemyConfig>();
            _movementConfig = ScriptableObject.CreateInstance<PlayerMovementConfig>();
            _combatConfig = ScriptableObject.CreateInstance<PlayerCombatConfig>();
            _library = ScriptableObject.CreateInstance<AnimationLibrary>();

            CreateHero(new Vector3(GeometryOriginX, 0.05f, GeometryOriginZ + 2f));
            CreateEnemy(new Vector3(GeometryOriginX, 0f, GeometryOriginZ));

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.captureDeltaTime = _previousCaptureDelta;

            if (_heroHealth != null)
            {
                _heroHealth.Changed -= OnHeroHealthChanged;
            }

            if (_geometryRoot != null)
            {
                Object.Destroy(_geometryRoot);
            }

            if (_hero != null)
            {
                Object.Destroy(_hero);
            }

            if (_enemy != null)
            {
                Object.Destroy(_enemy);
            }

            if (_cameraObject != null)
            {
                Object.Destroy(_cameraObject);
            }

            if (_wall != null)
            {
                Object.Destroy(_wall);
            }

            if (_enemyConfig != null)
            {
                Object.Destroy(_enemyConfig);
            }

            if (_movementConfig != null)
            {
                Object.Destroy(_movementConfig);
            }

            if (_combatConfig != null)
            {
                Object.Destroy(_combatConfig);
            }

            if (_library != null)
            {
                Object.Destroy(_library);
            }

            if (_hasNavMeshData)
            {
                NavMesh.RemoveAllNavMeshData();
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator NoiseAtReachablePointRunsInvestigateSearchReturnHome()
        {
            _hero.transform.position = new Vector3(GeometryOriginX, 0.05f, GeometryOriginZ + 20f);
            Physics.SyncTransforms();

            bool sawInvestigating = false;
            bool sawSearching = false;
            bool sawReturning = false;

            _noise.Emit(new Vector3(GeometryOriginX, 0f, GeometryOriginZ + 3f), 25f);

            for (int frameIndex = 0; frameIndex < InvestigationFrameLimit; frameIndex += 1)
            {
                _brain.Tick(Time.deltaTime);

                if (_brain.State == EnemyState.Investigating)
                {
                    sawInvestigating = true;
                }

                if (_brain.State == EnemyState.Searching)
                {
                    sawSearching = true;
                }

                if (_brain.State == EnemyState.Returning)
                {
                    sawReturning = true;
                }

                if (sawReturning && _brain.State == EnemyState.Idle)
                {
                    break;
                }

                yield return null;
            }

            Assert.IsTrue(sawInvestigating);
            Assert.IsTrue(sawSearching);
            Assert.IsTrue(sawReturning);
            Assert.AreEqual(EnemyState.Idle, _brain.State);

            Vector3 homeOffset = _enemy.transform.position - new Vector3(GeometryOriginX, 0f, GeometryOriginZ);
            homeOffset.y = 0f;

            Assert.Less(homeOffset.magnitude, HomeTolerance);
        }

        [UnityTest]
        public IEnumerator WallBetweenEnemyAndHeroPreventsLungeHit()
        {
            yield return TickFrames(1);

            Assert.AreEqual(EnemyState.Windup, _brain.State);

            _wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _wall.name = "Wall";
            _wall.transform.position = new Vector3(GeometryOriginX, 1f, GeometryOriginZ + 1f);
            _wall.transform.localScale = new Vector3(3f, 2f, 0.4f);
            Physics.SyncTransforms();

            yield return TickFrames(60);

            Assert.AreEqual(HeroMaxHealth, _heroHealth.CurrentHealth);
            Assert.AreEqual(0, _heroChangedCount);
            Assert.AreNotEqual(EnemyState.Windup, _brain.State);
            Assert.AreNotEqual(EnemyState.Lunging, _brain.State);
        }

        [UnityTest]
        public IEnumerator UnblockedLungeDeliversSingleHitWithinConfiguredDamage()
        {
            bool heroWasHit = false;

            for (int frameIndex = 0; frameIndex < 80 && heroWasHit == false; frameIndex += 1)
            {
                _brain.Tick(Time.deltaTime);

                if (_heroHealth.CurrentHealth < HeroMaxHealth)
                {
                    heroWasHit = true;
                }

                yield return null;
            }

            Assert.IsTrue(heroWasHit);

            yield return TickFrames(40);

            float damageTaken = HeroMaxHealth - _heroHealth.CurrentHealth;

            Assert.GreaterOrEqual(damageTaken, _enemyConfig.Attack.DamageMin);
            Assert.LessOrEqual(damageTaken, _enemyConfig.Attack.DamageMax);
            Assert.AreEqual(1, _heroChangedCount);
        }

        [UnityTest]
        public IEnumerator VisionFollowsRootForwardNotSpriteBillboard()
        {
            _hero.transform.position = new Vector3(GeometryOriginX, 0.05f, GeometryOriginZ + 3f);
            _enemy.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            Physics.SyncTransforms();

            yield return TickFrames(30);

            Assert.AreEqual(EnemyState.Idle, _brain.State);

            _enemy.transform.rotation = Quaternion.identity;
            Physics.SyncTransforms();

            yield return TickFrames(30);

            Assert.IsTrue(_brain.State == EnemyState.Chasing || _brain.State == EnemyState.Windup);
        }

        private IEnumerator TickFrames(int frameCount)
        {
            for (int frameIndex = 0; frameIndex < frameCount; frameIndex += 1)
            {
                _brain.Tick(Time.deltaTime);

                yield return null;
            }
        }

        private void CreateHero(Vector3 position)
        {
            _hero = new GameObject("Hero");
            _hero.layer = 8;
            _hero.transform.position = position;

            CharacterController controller = _hero.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.radius = 0.35f;
            controller.skinWidth = 0.04f;

            PlayerMotor motor = _hero.AddComponent<PlayerMotor>();
            motor.Setup(controller, _movementConfig);

            _heroHealth = _hero.AddComponent<Health>();
            _heroHealth.Setup(HeroMaxHealth);
            _heroHealth.Changed += OnHeroHealthChanged;

            _heroDefense = _hero.AddComponent<PlayerDefense>();
            _heroDefense.Setup(_combatConfig, _heroHealth, motor);
        }

        private void CreateEnemy(Vector3 position)
        {
            _enemy = new GameObject("Enemy");
            _enemy.layer = 9;
            _enemy.transform.position = position;

            CapsuleCollider hitCollider = _enemy.AddComponent<CapsuleCollider>();
            hitCollider.height = 1.8f;
            hitCollider.radius = 0.45f;
            hitCollider.center = new Vector3(0f, 0.9f, 0f);

            Health health = _enemy.AddComponent<Health>();
            health.Setup(30f);

            DamageTarget damageTarget = _enemy.AddComponent<DamageTarget>();
            damageTarget.Setup(health);

            EnemyPerception perception = _enemy.AddComponent<EnemyPerception>();
            perception.Setup(_hero.transform, _noise, _enemyConfig);

            NavMeshAgent agent = _enemy.AddComponent<NavMeshAgent>();
            agent.Warp(position);

            EnemyLocomotion locomotion = _enemy.AddComponent<EnemyLocomotion>();
            locomotion.Setup(agent, _enemyConfig, 1f);

            _spriteChild = new GameObject("Sprite");
            _spriteChild.layer = 9;
            _spriteChild.transform.SetParent(_enemy.transform);
            _spriteChild.transform.localPosition = Vector3.zero;
            _spriteChild.AddComponent<SpriteRenderer>();

            _cameraObject = new GameObject("ViewCamera");
            _cameraObject.AddComponent<Camera>();

            EnemySpriteAnimator animator = _enemy.AddComponent<EnemySpriteAnimator>();
            animator.Setup(_spriteChild.GetComponent<SpriteRenderer>(), _enemy.transform, _cameraObject.GetComponent<Camera>(), _library, _enemyConfig);

            _brain = _enemy.AddComponent<EnemyBrain>();
            _brain.Setup(_enemyConfig, perception, locomotion, animator, health, damageTarget, _heroDefense, _hero.transform, hitCollider);
        }

        private void OnHeroHealthChanged(Health health)
        {
            _heroChangedCount += 1;
        }
    }
}

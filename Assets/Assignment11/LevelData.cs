using UnityEngine;

[CreateAssetMenu(menuName="Sakura/Level Data")]
public class LevelData : ScriptableObject
{
 public int levelIndex;
 public string sceneName;
 public int enemyScore;
 public int bossScore;
 [System.Serializable] public class PlayerMovementValues {
  public float moveSpeed;
  public float jumpForce;
  public float shootCooldown;
  public float bulletSpawnDelay;
 }
 public PlayerMovementValues playerMovement = new PlayerMovementValues();
 [System.Serializable] public class PlayerHealthValues {
  public int maxHealth;
  public float invulnerabilityTime;
  public float deathDelay;
 }
 public PlayerHealthValues playerHealth = new PlayerHealthValues();
 [System.Serializable] public class EnemyValues {
  public int maxHealth;
  public float moveSpeed;
  public float moveDistance;
  public float idleTime;
  public float deathDelay;
  public int contactDamage;
 }
 public EnemyValues enemy = new EnemyValues();
 [System.Serializable] public class BulletValues {
  public float speed;
  public float lifetime;
  public int damage;
  public float hitAnimationDuration;
 }
 public BulletValues bullet = new BulletValues();
 [System.Serializable] public class BossControllerValues {
  public int maxHealth;
  public int contactDamage;
  public float attackInterval;
  public float attackAnimationDelay;
  public float phaseTwoHealthRatio;
  public float phaseTwoAttackInterval;
  public float phaseTwoProjectileSpeedMultiplier;
  public float deathAnimationDuration;
 }
 public BossControllerValues bossController = new BossControllerValues();
 [System.Serializable] public class BossProjectileValues {
  public float speed;
  public float lifetime;
  public int damage;
 }
 public BossProjectileValues bossProjectile = new BossProjectileValues();
 [System.Serializable] public class HazardValues {
  public int damage;
  public float lifetime;
 }
 public HazardValues hazard = new HazardValues();
 [System.Serializable] public class HazardSpawnerValues {
  public float spawnInterval;
  public float spawnHeight;
  public float minX;
  public float maxX;
  public float initialDelay;
 }
 public HazardSpawnerValues hazardSpawner = new HazardSpawnerValues();
 [System.Serializable] public class BombValues {
  public float explosionRadius;
  public float fuseDuration;
  public float explosionDuration;
 }
 public BombValues bomb = new BombValues();
 [System.Serializable] public class RockValues {
  public float breakAnimationDuration;
 }
 public RockValues rock = new RockValues();
 [System.Serializable] public class HeartPickupValues {
  public int healAmount;
 }
 public HeartPickupValues heartPickup = new HeartPickupValues();
}

public static class LevelBalance
{
 public static void Apply(PlayerMovement target) { var data = LevelContext.Current; if(data == null) return; var settings=data.playerMovement;
 target.moveSpeed = settings.moveSpeed;
 target.jumpForce = settings.jumpForce;
 target.shootCooldown = settings.shootCooldown;
 target.bulletSpawnDelay = settings.bulletSpawnDelay;
 }
 public static void Apply(PlayerHealth target) { var data = LevelContext.Current; if(data == null) return; var settings=data.playerHealth;
 target.maxHealth = settings.maxHealth;
 target.invulnerabilityTime = settings.invulnerabilityTime;
 target.deathDelay = settings.deathDelay;
 }
 public static void Apply(Enemy target) { var data = LevelContext.Current; if(data == null) return; var settings=data.enemy;
 target.maxHealth = settings.maxHealth;
 target.moveSpeed = settings.moveSpeed;
 target.moveDistance = settings.moveDistance;
 target.idleTime = settings.idleTime;
 target.deathDelay = settings.deathDelay;
 target.contactDamage = settings.contactDamage;
 }
 public static void Apply(Bullet target) { var data = LevelContext.Current; if(data == null) return; var settings=data.bullet;
 target.speed = settings.speed;
 target.lifetime = settings.lifetime;
 target.damage = settings.damage;
 target.hitAnimationDuration = settings.hitAnimationDuration;
 }
 public static void Apply(BossController target) { var data = LevelContext.Current; if(data == null) return; var settings=data.bossController;
 target.maxHealth = settings.maxHealth;
 target.contactDamage = settings.contactDamage;
 target.attackInterval = settings.attackInterval;
 target.attackAnimationDelay = settings.attackAnimationDelay;
 target.phaseTwoHealthRatio = settings.phaseTwoHealthRatio;
 target.phaseTwoAttackInterval = settings.phaseTwoAttackInterval;
 target.phaseTwoProjectileSpeedMultiplier = settings.phaseTwoProjectileSpeedMultiplier;
 target.deathAnimationDuration = settings.deathAnimationDuration;
 }
 public static void Apply(BossProjectile target) { var data = LevelContext.Current; if(data == null) return; var settings=data.bossProjectile;
 target.speed = settings.speed;
 target.lifetime = settings.lifetime;
 target.damage = settings.damage;
 }
 public static void Apply(Hazard target) { var data = LevelContext.Current; if(data == null) return; var settings=data.hazard;
 target.damage = settings.damage;
 target.lifetime = settings.lifetime;
 }
 public static void Apply(HazardSpawner target) { var data = LevelContext.Current; if(data == null) return; var settings=data.hazardSpawner;
 target.spawnInterval = settings.spawnInterval;
 target.spawnHeight = settings.spawnHeight;
 target.minX = settings.minX;
 target.maxX = settings.maxX;
 target.initialDelay = settings.initialDelay;
 }
 public static void Apply(Bomb target) { var data = LevelContext.Current; if(data == null) return; var settings=data.bomb;
 target.explosionRadius = settings.explosionRadius;
 target.fuseDuration = settings.fuseDuration;
 target.explosionDuration = settings.explosionDuration;
 }
 public static void Apply(Rock target) { var data = LevelContext.Current; if(data == null) return; var settings=data.rock;
 target.breakAnimationDuration = settings.breakAnimationDuration;
 }
 public static void Apply(HeartPickup target) { var data = LevelContext.Current; if(data == null) return; var settings=data.heartPickup;
 target.healAmount = settings.healAmount;
 }
}

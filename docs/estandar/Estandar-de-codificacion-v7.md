# Estandar de codificacion v7

> Convertido desde el .docx original, que sigue siendo el documento normativo del equipo.

Estándar de codificación

Tecnologías para la construcción

**Integrantes**

Jesus Bautista Hernandez

Valentin Benavides Martínez

20/08/2026

## Contenido

## 1. Introducción

Este documento establece las reglas de codificación obligatorias para todos los integrantes del equipo en la construcción, revisión y mantenimiento del código del proyecto. El proyecto es un videojuego desarrollado en C#, y todos los ejemplos de este documento pertenecen a ese contexto. Su propósito es mantener un código consistente, legible y verificable durante la revisión.

El idioma del código es el inglés. Se escriben en inglés los identificadores de todo tipo, los comentarios, la documentación XML, los mensajes de las excepciones y los mensajes de registro. Este documento y las tareas del equipo se redactan en español.

El estándar cubre las capas del proyecto, el nombrado, los sufijos y prefijos de identificador, los espacios, las líneas en blanco y la indentación, el estilo de código, los comentarios, el manejo de errores y excepciones, el registro de eventos y las pruebas.

Cada regla es independiente y se aplica por sí sola: enuncia una sola obligación, explica su propósito y, cuando aporta claridad, se acompaña de un ejemplo Con estándar y de un ejemplo Sin estándar. Las referencias a otras reglas sirven para localizar dónde se desarrolla un punto relacionado; no añaden requisitos a la regla que las menciona.

Los ejemplos Con estándar cumplen todas las reglas del documento, no solo la que ilustran. Los ejemplos Sin estándar muestran el incumplimiento de la regla que acompañan y no deben tomarse como modelo de ninguna otra.

Cuando una regla no proviene de una fuente externa, sino de un acuerdo del equipo, termina con la frase Regla propia del estándar.

## 2. Capas del proyecto

### 2.1 Capas y responsabilidades

El código del proyecto se organiza en cuatro capas, y cada tipo pertenece a una sola de ellas. La capa a la que pertenece un tipo determina qué responsabilidad puede asumir, en qué espacio de nombres se declara y qué sufijo lleva su nombre, conforme a las reglas de la sección 4.

- Cliente (Game.Client): dibuja la pantalla, lee la entrada del jugador y envía sus intenciones. No decide el resultado de ninguna acción del juego.
- Servicios (Game.Services): expone las operaciones que el servidor ofrece al cliente. Recibe la intención, valida los argumentos, coordina los pasos de la operación y devuelve el resultado. No implementa las reglas del juego, las invoca.
- Dominio (Game.Domain): contiene las entidades del juego y las reglas que las gobiernan, como el cálculo de daño, la progresión de oleadas o el estado de una partida. Es la capa donde vive la lógica del juego.
- Persistencia (Game.Persistence): lee y escribe los datos que sobreviven al cierre de la partida, como las partidas guardadas y las puntuaciones. No contiene reglas del juego.
**Con estándar**

```csharp
namespace Game.Domain
{
    public sealed class DamageCalculator
    {
    }
}

namespace Game.Services
{
    public sealed class MatchService : IMatchService
    {
    }
}

namespace Game.Persistence
{
    public sealed class SaveGameRepository : ISaveGameRepository
    {
    }
}
```

**Sin estándar**

```csharp
namespace Game.Client
{
    public sealed class MatchScreen
    {
        public int ApplyDamage(Enemy target, int damageAmount)
        {
            int effectiveDamage = damageAmount - target.Armor;
            _database.Save(target);

            return effectiveDamage;
        }
    }
}
```

### 2.2 Dirección de las dependencias

El cliente depende de la capa de servicios, la capa de servicios depende del dominio y de la persistencia, y el dominio no depende de ninguna otra capa. Una capa no conoce a la que la usa: la capa de servicios no referencia al cliente y el dominio no referencia a la persistencia. El propósito es que las reglas del juego puedan probarse sin levantar el servidor ni abrir la ventana del juego.

Cuando el dominio necesita un dato que solo la persistencia sabe obtener, la capa de servicios lo lee y se lo entrega como argumento, en lugar de que el dominio llame a un repositorio.

**Con estándar**

```csharp
public sealed class MatchService : IMatchService
{
    private readonly ISaveGameRepository _saveGameRepository;
    private readonly DamageCalculator _damageCalculator;

    public int ApplyDamage(string slotName, int enemyId, int damageAmount)
    {
        SaveGame saveGame = _saveGameRepository.Load(slotName);
        Enemy target = saveGame.Match.FindEnemy(enemyId);

        return _damageCalculator.Calculate(target, damageAmount);
    }
}
```

**Sin estándar**

```csharp
public sealed class DamageCalculator
{
    private readonly ISaveGameRepository _saveGameRepository;

    public int Calculate(string slotName, int enemyId, int damageAmount)
    {
        SaveGame saveGame = _saveGameRepository.Load(slotName);
        Enemy target = saveGame.Match.FindEnemy(enemyId);

        return damageAmount - target.Armor;
    }
}
```

## 3. Nombrado

### 3.1 Variables locales y parámetros

Las variables locales y los parámetros se nombran en camelCase, sin prefijos ni guiones bajos, con un nombre que describe el significado del dato y no su tipo. El guion bajo inicial queda reservado a los campos, conforme a la regla 3.5.

Los nombres de una sola letra se admiten en dos casos, y solo en esos dos: la variable de control de un bucle for, conforme a la regla 6.17, y el parámetro e de un manejador de eventos, conforme a la regla 3.3.

**Con estándar**

```csharp
public void ApplyDamage(Enemy target, int damageAmount)
{
    int effectiveDamage = damageAmount - target.Armor;

    if (effectiveDamage > 0)
    {
        target.TakeDamage(effectiveDamage);
    }
}
```

**Sin estándar**

```csharp
public void ApplyDamage(Enemy Target, int intDamage)
{
    int i_effectiveDamage = intDamage - Target.Armor;

    if (i_effectiveDamage > 0)
    {
        Target.TakeDamage(i_effectiveDamage);
    }
}
```

### 3.2 Variable de una cláusula catch

La variable de una cláusula catch se llama exception. No se admiten los nombres e ni ex, porque e está reservado al parámetro de un manejador de eventos, conforme a la regla 3.3, y ex es una abreviatura que no aporta información. Cuando un catch anidado deja dos excepciones a la vista en el mismo método, la interior recibe un nombre descriptivo en camelCase que indique de qué operación proviene.

El nombre se escribe siempre, aunque el bloque no vaya a usar la variable; si el bloque realmente no la necesita, se omite la variable entera y se escribe solo el tipo. Regla propia del estándar.

**Con estándar**

```csharp
try
{
    _saveGameRepository.Read(saveStream);
}
catch (IOException exception)
{
    throw new CorruptedSaveFileException("The save file could not be read.", exception);
}
```

**Sin estándar**

```csharp
try
{
    _saveGameRepository.Read(saveStream);
}
catch (IOException e)
{
    throw new CorruptedSaveFileException("The save file could not be read.", e);
}
```

### 3.3 Parámetros de un manejador de eventos

Un manejador de eventos recibe dos parámetros y estos se llaman sender y e, sin excepción. Es la convención de .NET y la única situación del proyecto en la que un parámetro de una sola letra es correcto. El nombre e no se usa en ningún otro lugar del código.

El método que atiende un evento se nombra con el tipo que publica el evento, la palabra On y el nombre del evento, con la forma PublicadorOnEvento. Esa forma permite distinguir varios manejadores del mismo evento suscritos a publicadores distintos, y no se confunde con el método protegido que lanza el evento, que lleva solo On y el nombre del evento conforme a la regla 6.32.

**Con estándar**

```csharp
private void CombatSystemOnEnemyDefeated(object sender, EnemyDefeatedEventArgs e)
{
    _scoreBoard.Add(e.ScoreReward);
}
```

**Sin estándar**

```csharp
private void Handler(object o, EnemyDefeatedEventArgs eventArguments)
{
    _scoreBoard.Add(eventArguments.ScoreReward);
}
```

### 3.4 Parámetro de una expresión lambda

El parámetro de una expresión lambda lleva un nombre descriptivo en camelCase, en singular, igual que cualquier otro parámetro. No se admiten los nombres de una sola letra, ni x, ni e, ni el nombre del tipo en minúsculas. El motivo es que la lambda se lee dentro de una línea ya densa y el nombre del parámetro es la única pista sobre qué se está recorriendo.

Cuando la lambda recorre una colección, el parámetro toma el singular del nombre de esa colección. Las condiciones de uso de las lambdas se definen en la regla 6.30. Regla propia del estándar.

**Con estándar**

```csharp
_activeEnemies.RemoveAll(enemy => !enemy.IsAlive);

IReadOnlyList<Item> equippedItems = _inventory.Where(item => item.IsEquipped).ToList();
```

**Sin estándar**

```csharp
_activeEnemies.RemoveAll(e => !e.IsAlive);

IReadOnlyList<Item> equippedItems = _inventory.Where(x => x.IsEquipped).ToList();
```

### 3.5 Campos privados

Los campos privados e internos se nombran en camelCase con un guion bajo inicial, tanto los de instancia como los estáticos. Ese guion bajo distingue el campo de una variable local o de un parámetro, por lo que no se usa this. para desambiguar.

Un campo es una variable declarada dentro de la clase, que vive mientras vive el objeto. Una variable local se declara dentro de un método y desaparece al terminar. El guion bajo se aplica solo a los campos; las variables locales y los parámetros nunca lo llevan, conforme a la regla 3.1. Un dato que se expone fuera de la clase no se declara como campo, sino como propiedad, conforme a la regla 3.6.

**Con estándar**

```csharp
public sealed class WaveDirector
{
    private static readonly TimeSpan _defaultWaveDelay = TimeSpan.FromSeconds(3);
    private readonly List<Enemy> _activeEnemies = new List<Enemy>();
    private readonly EnemySpawner _enemySpawner;
    private int _completedWaves;
}
```

**Sin estándar**

```csharp
public sealed class WaveDirector
{
    private static readonly TimeSpan defaultWaveDelay = TimeSpan.FromSeconds(3);
    private readonly EnemySpawner m_enemySpawner;
    public List<Enemy> activeEnemies = new List<Enemy>();
    private int completedWaves;
}
```

### 3.6 Propiedades

Las propiedades se nombran en PascalCase con un sustantivo, una frase sustantiva o un adjetivo, nunca con un verbo. Una propiedad expone un dato, no ejecuta una acción; si el miembro ejecuta una acción, es un método y se nombra conforme a la regla 3.11.

Una propiedad no se nombra anteponiendo Get o Set al nombre del dato: esa forma corresponde a un método y, junto a la propiedad, produce dos miembros que dicen lo mismo. Las formas de escritura admitidas para una propiedad se definen en las reglas 6.4 a 6.8.

**Con estándar**

```csharp
public int Health { get; set; }

public int MaxHealth { get; }

public bool IsAlive => Health > 0;
```

**Sin estándar**

```csharp
public int GetHealth { get; set; }

public int getMaxHealth { get; }

public bool CheckAlive => Health > 0;
```

### 3.7 Colecciones

Las variables, campos, propiedades y parámetros que contienen varios elementos se nombran en plural, describiendo los elementos que contienen y no la estructura de datos que los almacena. No se usan los sufijos List, Array, Collection ni Dictionary en el nombre del identificador.

Cuando la colección es un diccionario, el nombre describe la clave que lo indexa, con la forma elementosPorClave. Esta regla se refiere a los nombres de los identificadores; el uso de los tipos Collection y Dictionary no está restringido, y la obligación de que ciertos tipos de la jerarquía lleven esos sufijos en su nombre se define en la regla 4.1.

**Con estándar**

```csharp
private readonly List<Enemy> _activeEnemies;
private readonly Dictionary<int, Player> _playersById;

public IReadOnlyList<Item> Inventory { get; }
```

**Sin estándar**

```csharp
private readonly List<Enemy> _enemyCollection;
private readonly Dictionary<int, Player> _playerDictionary;

public IReadOnlyList<Item> InventoryList { get; }
```

### 3.8 Identificadores booleanos

Los identificadores booleanos se escriben como una frase afirmativa. Se admiten los prefijos Is, Can y Has cuando aportan claridad. No se permiten nombres en forma negada, porque obligan a leer una doble negación dentro de las condiciones. Tampoco se usa el sufijo Flag, que no indica qué representa el valor.

**Con estándar**

```csharp
public bool IsAlive { get; private set; }
public bool CanAttack => _attackCooldown <= TimeSpan.Zero;
public bool HasShield { get; private set; }
```

**Sin estándar**

```csharp
public bool alive { get; private set; }
public bool NotReadyToAttack => _attackCooldown > TimeSpan.Zero;
public bool shieldFlag { get; private set; }
```

### 3.9 Constantes

Las constantes se nombran en PascalCase, tanto los campos const como las constantes locales, sin importar su nivel de acceso. No se usa UPPER_SNAKE_CASE, porque el guion bajo no se admite dentro de un identificador y la convención de .NET no distingue la capitalización por nivel de acceso. El nombre expresa el significado del valor, no el valor en sí.

**Con estándar**

```csharp
private const int MaxActiveEnemies = 12;
private const float CriticalHitMultiplier = 1.5f;
public const int StartingLives = 3;
```

**Sin estándar**

```csharp
private const int MAX_ACTIVE_ENEMIES = 12;
private const float critical_hit_multiplier = 1.5f;
public const int THREE = 3;
```

### 3.10 Enumeraciones

El tipo enumerado se nombra en singular, salvo que represente campos de bits marcados con FlagsAttribute, en cuyo caso se nombra en plural. Los miembros se escriben en PascalCase y no repiten el nombre del tipo como prefijo, porque el tipo ya aparece al usarlos. Los sufijos Enum, Flag y Flags están prohibidos conforme a la regla 4.6.

**Con estándar**

```csharp
public enum WeaponKind
{
    Sword,
    Axe,
    Bow
}
```

**Sin estándar**

```csharp
public enum WeaponKindEnum
{
    WK_SWORD,
    WK_AXE,
    WK_BOW
}
```

### 3.11 Métodos

Los métodos se nombran en PascalCase con un verbo o una frase verbal que describe la acción que ejecutan. El nombre indica qué hace el método, no cómo lo hace ni qué tipo devuelve.

**Con estándar**

```csharp
public void ApplyDamage(Enemy target, int damageAmount)
{
}

public int CalculateScoreReward(EnemyKind kind)
{
    return GetScoreReward(kind);
}
```

**Sin estándar**

```csharp
public void Damage2(Enemy target, int damageAmount)
{
}

public int IntScoreReward(EnemyKind kind)
{
    return GetScoreReward(kind);
}
```

### 3.12 Sufijo Async

Todo método que devuelve Task, Task<TResult> o ValueTask<TResult> termina con el sufijo Async, y ningún método síncrono lleva ese sufijo. El sufijo advierte al llamador de que debe esperar el resultado, y su ausencia en un método asíncrono es la causa habitual de que una llamada quede sin await.

**Con estándar**

```csharp
public Task<SaveGame> LoadSaveGameAsync(string slotName)
{
    return _saveGameRepository.LoadAsync(slotName);
}
```

**Sin estándar**

```csharp
public Task<SaveGame> LoadSaveGame(string slotName)
{
    return _saveGameRepository.LoadAsync(slotName);
}
```

### 3.13 Prefijo Try

El prefijo Try se reserva al patrón Try, y un método solo puede llamarse así si cumple las tres condiciones: devuelve bool, entrega el resultado por un parámetro out y existe además el miembro equivalente que lanza excepción cuando la operación no puede completarse. Un método que devuelve el valor directamente, o null cuando no lo encuentra, no lleva el prefijo Try. Los casos en los que procede ofrecer este par de miembros se definen en la regla 8.6.

**Con estándar**

```csharp
public bool TryGetEquippedWeapon(out Weapon weapon)
{
    weapon = _equippedWeapon;

    return weapon is not null;
}
```

**Sin estándar**

```csharp
public Weapon TryGetEquippedWeapon()
{
    return _equippedWeapon;
}
```

### 3.14 Eventos

Los eventos se nombran con un verbo, en presente si se lanzan antes de que ocurra la acción y en pasado si se lanzan después. No se usan los prefijos ni sufijos Before y After, porque el tiempo verbal ya expresa ese momento, ni el prefijo On, que se reserva al método protegido que lanza el evento conforme a la regla 6.32.

**Con estándar**

```csharp
public event EventHandler<EnemyDefeatedEventArgs> EnemyDefeated;

public event EventHandler EnemySpawning;
```

**Sin estándar**

```csharp
public event EventHandler<EnemyDefeatedEventArgs> OnEnemyDefeated;

public event EventHandler BeforeEnemySpawn;
```

### 3.15 Clases

El nombre de una clase es un sustantivo o una frase sustantiva en PascalCase y en singular, que describe la entidad o la responsabilidad que representa. No se anteponen prefijos de tipo ni de proyecto, no se usan verbos ni abreviaturas no reconocidas, y el nombre no repite el espacio de nombres que ya lo contiene.

Las clases derivadas conservan como núcleo el sustantivo del tipo base y anteponen el calificador que las especializa, de modo que la relación de herencia se lea en el nombre. Los acrónimos de dos letras se escriben en mayúsculas y los de tres o más en PascalCase.

**Con estándar**

```csharp
public abstract class Enemy
{
}

public sealed class FlyingEnemy : Enemy
{
}

public sealed class DamageCalculator
{
}
```

**Sin estándar**

```csharp
public abstract class EnemyBaseClass
{
}

public sealed class Enemy2 : EnemyBaseClass
{
}

public sealed class CDmgCalc
{
}
```

### 3.16 Interfaces

El nombre de una interfaz lleva el prefijo I seguido de PascalCase, y se construye con un sustantivo o con una frase adjetiva que describe lo que el implementador es capaz de hacer. Cuando una clase es la implementación estándar de una interfaz, ambos nombres difieren únicamente en la I. Este prefijo es la única excepción admitida a la prohibición de prefijos de la regla 4.7.

**Con estándar**

```csharp
public interface IDamageable
{
    void TakeDamage(int amount);
}

public sealed class SaveGameRepository : ISaveGameRepository
{
}
```

**Sin estándar**

```csharp
public interface Damageable
{
    void TakeDamage(int amount);
}

public sealed class SaveGameRepositoryImpl : SaveGameRepositoryInterface
{
}
```

## 4. Sufijos y prefijos de identificadores

### 4.1 Sufijos obligatorios por herencia o implementación

Cuando un tipo deriva de una clase base o implementa una interfaz de la siguiente lista, su nombre termina con el sufijo asociado: Attribute, Collection, Dictionary, EventArgs, EventHandler, Exception y Stream. La obligación funciona en los dos sentidos: un tipo que no deriva del tipo asociado ni lo implementa no puede usar ese sufijo.

Esta regla se aplica únicamente a los nombres de tipo. Los nombres de variables, campos, propiedades y parámetros no llevan estos sufijos y se rigen por la regla 3.7.

**Con estándar**

```csharp
public sealed class CorruptedSaveFileException : GameException
{
}

public sealed class LootTableCollection : ICollection<LootTable>
{
}
```

**Sin estándar**

```csharp
public sealed class CorruptedSaveFile : GameException
{
}

public sealed class LootTableCollection
{
}
```

### 4.2 Sufijo Service

El sufijo Service se reserva a los tipos de la capa de servicios descrita en la regla 2.1, es decir, a los que exponen al cliente una operación completa del servidor. Su responsabilidad es recibir la intención del cliente, validar los argumentos, coordinar los pasos de la operación y devolver el resultado.

Una clase que implementa reglas del juego pertenece al dominio y no lleva este sufijo: se nombra por la responsabilidad que cumple, como DamageCalculator o WaveDirector. Un tipo Service no contiene el cálculo, lo delega. Toda clase con este sufijo implementa una interfaz con el mismo nombre precedido de I, conforme a la regla 3.16, para que el cliente dependa del contrato y no de la implementación. Regla propia del estándar.

**Con estándar**

```csharp
namespace Game.Services
{
    public interface IMatchService
    {
        int ApplyDamage(int enemyId, int damageAmount);
    }

    public sealed class MatchService : IMatchService
    {
        private readonly DamageCalculator _damageCalculator;
        private readonly List<Enemy> _activeEnemies = new List<Enemy>();

        public int ApplyDamage(int enemyId, int damageAmount)
        {
            Enemy target = _activeEnemies[enemyId];

            return _damageCalculator.Calculate(target, damageAmount);
        }
    }
}
```

**Sin estándar**

```csharp
namespace Game.Domain
{
    public sealed class DamageService
    {
        public int Calculate(Enemy target, int damageAmount)
        {
            return damageAmount - target.Armor;
        }
    }
}
```

### 4.3 Sufijo Repository

El sufijo Repository se reserva a los tipos de la capa de persistencia descrita en la regla 2.1, y designa al tipo que encapsula el acceso a los datos persistidos de una entidad. Sus miembros son operaciones de lectura y escritura de esa entidad, y ninguno contiene reglas del juego.

Cada repositorio cubre una sola entidad y su nombre la menciona en singular. Un tipo que guarda datos de varias entidades no lleva este sufijo, sino que se divide en un repositorio por entidad.

**Con estándar**

```csharp
namespace Game.Persistence
{
    public sealed class SaveGameRepository : ISaveGameRepository
    {
        public SaveGame Load(string slotName)
        {
            using FileStream saveStream = File.OpenRead(GetPath(slotName));

            return Read(saveStream);
        }
    }
}
```

**Sin estándar**

```csharp
namespace Game.Persistence
{
    public sealed class GameDataRepository
    {
        public SaveGame Load(string slotName)
        {
            SaveGame saveGame = Read(slotName);
            saveGame.Player.Health = saveGame.Player.MaxHealth;

            return saveGame;
        }
    }
}
```

### 4.4 Sufijo Factory

El sufijo Factory se reserva al tipo cuya única responsabilidad es construir instancias de otro tipo y decidir qué variante construir. Sus miembros devuelven la instancia creada y no la almacenan ni la modifican después.

No se usa este sufijo en un tipo que además guarda, actualiza o coordina lo que construye; en ese caso el tipo se nombra por su responsabilidad principal y la construcción se extrae a una fábrica aparte.

**Con estándar**

```csharp
public sealed class EnemyFactory
{
    public Enemy Create(EnemyKind kind)
    {
        return kind switch
        {
            EnemyKind.Grunt => new Grunt(),
            EnemyKind.Archer => new Archer(),
            EnemyKind.Boss => new Boss(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
    }
}
```

**Sin estándar**

```csharp
public sealed class EnemyFactory
{
    private readonly List<Enemy> _activeEnemies = new List<Enemy>();

    public Enemy Create(EnemyKind kind)
    {
        Enemy enemy = new Grunt();
        _activeEnemies.Add(enemy);

        return enemy;
    }
}
```

### 4.5 Sufijo Tests

La clase que agrupa las pruebas de otra clase se nombra con el nombre de la clase probada más el sufijo Tests, en plural. Ese sufijo es la única marca que identifica al tipo como clase de pruebas, y no se antepone la palabra Test al nombre. El nombrado de los métodos de prueba se define en la regla 10.1.

**Con estándar**

```csharp
public sealed class EnemySpawnerTests
{
}
```

**Sin estándar**

```csharp
public sealed class TestEnemySpawner
{
}
```

### 4.6 Sufijos prohibidos

Quedan prohibidos los sufijos Delegate, Enum, Flag, Flags, Impl, Interface, Base, New y Ex, porque repiten en el nombre lo que la declaración del tipo ya expresa, y también los sufijos Manager, Helper, Utils, Data, Info y Stuff, porque no describen ninguna responsabilidad y admiten cualquier contenido.

Cuando un tipo solo se deja nombrar con uno de estos sufijos, la causa suele ser que reúne responsabilidades distintas: se divide en tipos que sí puedan nombrarse por lo que hacen. Regla propia del estándar.

**Con estándar**

```csharp
public sealed class SaveGameRepository : ISaveGameRepository
{
}

public sealed class EnemyFactory
{
}
```

**Sin estándar**

```csharp
public sealed class SaveGameManager
{
}

public sealed class EnemyUtils
{
}
```

### 4.7 Prefijos prohibidos

Ningún identificador lleva prefijos de tipo, de ámbito ni de proyecto: no se usan las formas i, str, obj, m_, s_, g_ ni las siglas del proyecto delante del nombre. El tipo y el ámbito ya los declara el código, y el prefijo se desactualiza en cuanto uno de los dos cambia.

Se admiten dos únicas excepciones, ambas definidas en otras reglas: el prefijo I de las interfaces, conforme a la regla 3.16, y el guion bajo inicial de los campos privados, conforme a la regla 3.5.

**Con estándar**

```csharp
private readonly EnemySpawner _enemySpawner;

public interface IDamageable
{
}
```

**Sin estándar**

```csharp
private readonly EnemySpawner m_enemySpawner;

public interface GDamageable
{
}
```

## 5. Espacios, líneas en blanco e indentación

### 5.1 Indentación

Cada nivel se indenta con cuatro espacios y no se usan tabuladores. Se indenta un nivel el contenido de un espacio de nombres respecto de su declaración, el de una clase respecto de la clase, el de un método o una propiedad respecto de su firma, y el de una estructura de control respecto de la palabra reservada que la abre. La llave de cierre queda en la columna de la línea que abrió el bloque.

**Con estándar**

```csharp
namespace Game.Services
{
    public sealed class MatchService : IMatchService
    {
        public bool IsRunning { get; private set; }

        public void AddEnemy(Enemy enemy)
        {
            if (IsRunning)
            {
                _activeEnemies.Add(enemy);
            }
        }
    }
}
```

**Sin estándar**

```csharp
namespace Game.Services
{
public sealed class MatchService : IMatchService
{
      public bool IsRunning { get; private set; }
        public void AddEnemy(Enemy enemy)
        {
        if (IsRunning)
            {
                _activeEnemies.Add(enemy);
        }
        }
}
}
```

### 5.2 Indentación de las líneas de continuación

Cuando una sentencia se parte en varias líneas, cada línea de continuación se indenta un nivel respecto de la línea que continúa, y ese nivel no se acumula aunque la sentencia ocupe más de dos líneas. El corte se hace antes del operador binario, de modo que el operador abra la línea siguiente y se vea de un vistazo cómo se encadenan las partes.

**Con estándar**

```csharp
bool canBeAttacked = target.IsAlive
    && (distanceToTarget <= AttackRange)
    && !target.HasShield;
```

**Sin estándar**

```csharp
bool canBeAttacked = target.IsAlive &&
        (distanceToTarget <= AttackRange) &&
            !target.HasShield;
```

### 5.3 Espacios

Se escribe un espacio entre la palabra reservada y el paréntesis de apertura de una estructura de control, y ninguno entre el nombre de un método y su paréntesis: esa diferencia permite distinguir de un vistazo una llamada de una estructura de control. No se escribe espacio después del paréntesis de apertura ni antes del de cierre.

Todo operador binario lleva un espacio antes y otro después. La coma lleva espacio después y ninguno antes, y lo mismo el punto y coma dentro de la cabecera de un for. El operador de conversión explícita no lleva espacio entre el paréntesis de cierre y el valor convertido.

**Con estándar**

```csharp
for (var i = 0; i < _spawnPoints.Count; i++)
{
    Spawn(_spawnPoints[i], (int)_difficulty);
}
```

**Sin estándar**

```csharp
for(var i=0 ;i<_spawnPoints.Count ;i++)
{
    Spawn ( _spawnPoints[i] , (int) _difficulty );
}
```

### 5.4 Líneas en blanco

Se deja una línea en blanco entre métodos y constructores, entre las guardas y el cuerpo de un método, y antes del return final cuando el método tiene más de una sentencia. El propósito es que todos los métodos del proyecto se lean con la misma silueta: primero lo que se valida, después lo que se hace y al final lo que se devuelve. Regla propia del estándar.

Los miembros que ocupan una sola línea, como los campos y las propiedades automáticas o con cuerpo de expresión, pueden escribirse en líneas consecutivas. No se dejan líneas en blanco inmediatamente después de la llave de apertura ni antes de la de cierre, ni dos líneas en blanco consecutivas.

**Con estándar**

```csharp
public int CalculateScoreReward(Enemy enemy)
{
    ArgumentNullException.ThrowIfNull(enemy);

    int reward = GetScoreReward(enemy.Kind);

    return reward * _difficultyMultiplier;
}
```

**Sin estándar**

```csharp
public int CalculateScoreReward(Enemy enemy)
{

    ArgumentNullException.ThrowIfNull(enemy);
    int reward = GetScoreReward(enemy.Kind);
    return reward * _difficultyMultiplier;

}
```

### 5.5 Longitud de línea

Una línea no supera los 500 caracteres. Cuando una sentencia no cabe, se divide en varias líneas conforme a la regla 5.2.

El límite de 500 caracteres es una regla propia del estándar y se amplía frente a los 100 o 120 caracteres habituales en otras guías de C#: el equipo prioriza evitar cortes de línea intrusivos en construcciones con nombres largos y confía en el ajuste visual del editor para la lectura, no en el corte físico de la línea. El límite acota la línea, no autoriza a escribir sentencias largas; las condiciones y los métodos tienen sus propios límites en las reglas 6.15, 6.28 y 6.29.

**Con estándar**

```csharp
bool isEliteEnemy = enemy.IsBoss
    || (enemy.Health > EliteHealthThreshold)
    || enemy.HasShield;

_matchLog.RecordDefeat(enemy.Id, enemy.Kind, _currentWave, _elapsedTime);
```

**Sin estándar**

```csharp
bool isEliteEnemy = enemy.IsBoss || (enemy.Health > EliteHealthThreshold) || enemy.HasShield || (enemy.ScoreReward > EliteScoreThreshold) || (enemy.Armor > EliteArmorThreshold) || enemy.IsEnraged || (enemy.Kind == EnemyKind.Boss) || (enemy.Kind == EnemyKind.Archer) || (enemy.AbilityCount > EliteAbilityThreshold) || (enemy.SpawnWave > EliteWaveThreshold) || (enemy.DamagePerHit > EliteDamageThreshold) || (enemy.MovementSpeed > EliteSpeedThreshold) || (enemy.CriticalChance > EliteCriticalThreshold) || enemy.IsInvulnerable;
```

### 5.6 Una sentencia y una declaración por línea

Se escribe una sola sentencia por línea y se declara una sola variable por línea, aunque el lenguaje permita separar varias con comas. Dos declaraciones en la misma línea comparten el tipo y esconden cuál de las dos se está inicializando; dos sentencias en la misma línea impiden ver en el depurador cuál de ellas falló.

**Con estándar**

```csharp
int effectiveDamage = damageAmount - target.Armor;
int remainingHealth = target.Health - effectiveDamage;

target.TakeDamage(effectiveDamage);
```

**Sin estándar**

```csharp
int effectiveDamage = damageAmount - target.Armor, remainingHealth = 0;
remainingHealth = target.Health - effectiveDamage; target.TakeDamage(effectiveDamage);
```

## 6. Estilo de código

### 6.1 Llaves

La llave de apertura ocupa su propia línea, alineada con la primera columna de la declaración, y la llave de cierre ocupa también su propia línea en esa misma columna. Aplica a espacios de nombres, tipos, métodos, propiedades y estructuras de control.

Las palabras else, else if, catch y finally inician su propia línea después de la llave de cierre del bloque anterior. La única excepción es la cláusula while del do while, que se escribe en la misma línea que la llave de cierre conforme a la regla 6.20. Los miembros con cuerpo de expresión no llevan llaves, conforme a la regla 6.3.

**Con estándar**

```csharp
public sealed class EnemySpawner
{
    public void Spawn(SpawnPoint spawnPoint)
    {
        if (HasCapacity)
        {
            _activeEnemies.Add(_enemyFactory.Create(spawnPoint));
        }
    }
}
```

**Sin estándar**

```csharp
public sealed class EnemySpawner {
    public void Spawn(SpawnPoint spawnPoint) {
        if (HasCapacity) {
            _activeEnemies.Add(_enemyFactory.Create(spawnPoint));
        }
    }
}
```

### 6.2 Llaves obligatorias en las estructuras de control

Toda estructura de control lleva llaves, incluso cuando su cuerpo es una sola sentencia y aunque el lenguaje permita omitirlas. El motivo es que, al añadir después una segunda sentencia, es fácil olvidar las llaves y dejar esa sentencia fuera del bloque sin que el compilador avise.

**Con estándar**

```csharp
if (!enemy.IsAlive)
{
    _activeEnemies.Remove(enemy);
}
```

**Sin estándar**

```csharp
if (!enemy.IsAlive)
    _activeEnemies.Remove(enemy);
```

### 6.3 Cuerpo de expresión

Un miembro cuyo cuerpo es una sola expresión puede escribirse con cuerpo de expresión, usando la flecha, y en ese caso no lleva llaves, por lo que la regla 6.1 no le aplica. Se usa en propiedades de solo lectura y en métodos que únicamente devuelven el resultado de una expresión, siempre que quepan en una sola línea.

Si el cuerpo necesita más de una sentencia o no cabe en una línea, se escribe con bloque y llaves. La única excepción es el método cuyo cuerpo completo es una expresión switch, conforme a la regla 6.22, que sí puede ocupar varias líneas.

**Con estándar**

```csharp
public bool CanAttack => _attackCooldown <= TimeSpan.Zero;

public int TotalScore => _baseScore + _bonusScore;
```

**Sin estándar**

```csharp
public bool CanAttack
{
    get { return _attackCooldown <= TimeSpan.Zero; }
}

public int TotalScore =>
    _baseScore + _bonusScore
    + _comboScore + _difficultyBonus;
```

### 6.4 Formas admitidas de propiedad

Una propiedad se escribe en una de las cuatro formas que definen las reglas 6.5 a 6.8, y en ninguna otra. La forma se elige respondiendo a tres preguntas: si el valor se guarda, si cambia después de construido el objeto y si leerlo o escribirlo exige lógica adicional.

En particular, no se escribe un bloque get o set con llaves que solo devuelve o asigna el campo de respaldo sin hacer nada más: esa forma ocupa una decena de líneas para decir lo que la propiedad autoimplementada de la regla 6.5 dice en una, y el lector tiene que leerla entera para comprobar que no hace nada.

**Con estándar**

```csharp
public sealed class Player
{
    private const int MaxStamina = 100;

    private int _stamina;

    public int Health { get; set; }
    public int MaxHealth { get; }
    public bool IsAlive => Health > 0;

    public int Stamina
    {
        get => _stamina;
        set => _stamina = Math.Clamp(value, 0, MaxStamina);
    }
}
```

**Sin estándar**

```csharp
public sealed class Player
{
    private int _health;

    public int Health
    {
        get
        {
            return _health;
        }
        set
        {
            _health = value;
        }
    }
}
```

### 6.5 Propiedad autoimplementada de lectura y escritura

Se usa cuando el valor se guarda, se expone sin lógica adicional y puede cambiar durante la vida del objeto. Se declara con get y set y sin cuerpo; el compilador genera el campo de respaldo, que no se declara ni se nombra.

El set se declara privado cuando el valor solo cambia desde dentro de la clase. Si escribir el valor exige validar o transformar algo, esta forma no aplica y se usa la de la regla 6.7: sacar esa validación a un método aparte deja abierta la vía de asignar la propiedad directamente y saltársela.

**Con estándar**

```csharp
public int Health { get; set; }

public bool IsAlive { get; private set; }
```

**Sin estándar**

```csharp
public int Stamina { get; set; }

public void ApplyStamina(int value)
{
    Stamina = Math.Clamp(value, 0, MaxStamina);
}
```

### 6.6 Propiedad autoimplementada de solo lectura

Se usa cuando el valor se fija al construir el objeto y no vuelve a cambiar. Se declara con get únicamente, sin set, y solo puede asignarse en su declaración o dentro del constructor; cualquier otra asignación la rechaza el compilador.

Esta forma expresa en un miembro expuesto la misma garantía que readonly expresa en un campo, conforme a la regla 6.9. Cuando el valor no cambia no se declara un set privado en su lugar, porque un set privado sigue permitiendo la reasignación desde cualquier método de la clase y la garantía deja de existir.

**Con estándar**

```csharp
public sealed class Weapon
{
    public Weapon(string name, int baseDamage)
    {
        Name = name;
        BaseDamage = baseDamage;
    }

    public string Name { get; }
    public int BaseDamage { get; }
}
```

**Sin estándar**

```csharp
public sealed class Weapon
{
    public Weapon(string name, int baseDamage)
    {
        Name = name;
        BaseDamage = baseDamage;
    }

    public string Name { get; set; }
    public int BaseDamage { get; private set; }
}
```

### 6.7 Propiedad con campo de respaldo

Se usa cuando leer o escribir el valor exige validación, transformación o cualquier otra lógica. El valor se guarda en un campo privado nombrado conforme a la regla 3.5, y la propiedad declara los bloques get y set que lo leen y lo escriben.

Cada bloque se escribe con cuerpo de expresión cuando cabe en una expresión, conforme a la regla 6.3, y con llaves solo cuando necesita más de una sentencia. El campo de respaldo es privado y no se expone: quien está fuera de la clase ve únicamente la propiedad, de modo que la lógica no se puede rodear.

**Con estándar**

```csharp
private int _stamina;

public int Stamina
{
    get => _stamina;
    set => _stamina = Math.Clamp(value, 0, MaxStamina);
}
```

**Sin estándar**

```csharp
public int _stamina;

public int Stamina
{
    get
    {
        return _stamina;
    }
    set
    {
        if (value < 0)
        {
            _stamina = 0;
        }
        else
        {
            _stamina = value;
        }
    }
}
```

### 6.8 Propiedad con cuerpo de expresión

Se usa cuando la propiedad es de solo lectura y su valor se calcula en una sola expresión a partir de otros miembros, sin guardar estado propio. No lleva campo de respaldo y su valor se recalcula en cada lectura, así que no se emplea para un cálculo costoso que se consulte muchas veces por fotograma.

La escritura de esta forma se rige por la regla 6.3. Cuando el cálculo necesita más de una expresión no se parte en varias líneas: se convierte en un método con nombre, porque un cálculo largo detrás de lo que parece un dato sorprende a quien lo lee.

**Con estándar**

```csharp
public bool IsAlive => Health > 0;

public int TotalScore => _baseScore + _bonusScore;
```

**Sin estándar**

```csharp
public int TotalScore
{
    get
    {
        int total = _baseScore + _bonusScore;
        total += _comboScore * _difficultyMultiplier;

        return total;
    }
}
```

### 6.9 Campos readonly

Todo campo que no cambia después de terminar el constructor se declara readonly. La palabra readonly documenta que el valor no se reasigna y hace que el compilador rechace cualquier intento de hacerlo, de modo que la garantía no depende de la disciplina de quien edite la clase después.

En un campo readonly de tipo colección lo que no cambia es la referencia, no el contenido: la lista sigue admitiendo elementos nuevos. Cuando además debe impedirse la modificación del contenido, se expone como IReadOnlyList y no como List.

**Con estándar**

```csharp
public sealed class WaveDirector
{
    private readonly EnemySpawner _enemySpawner;
    private readonly List<Enemy> _activeEnemies = new List<Enemy>();
    private int _completedWaves;

    public WaveDirector(EnemySpawner enemySpawner)
    {
        ArgumentNullException.ThrowIfNull(enemySpawner);

        _enemySpawner = enemySpawner;
    }
}
```

**Sin estándar**

```csharp
public sealed class WaveDirector
{
    private EnemySpawner _enemySpawner;
    private List<Enemy> _activeEnemies = new List<Enemy>();
    private int _completedWaves;

    public WaveDirector(EnemySpawner enemySpawner)
    {
        _enemySpawner = enemySpawner;
    }
}
```

### 6.10 Inicialización de campos

Un campo se inicializa en la declaración cuando su valor es constante, es el valor por omisión o no depende de nada externo, como una colección vacía o un valor compartido de tipo static readonly. Se inicializa en el constructor cuando su valor depende de parámetros de entrada, de dependencias recibidas, de una validación o de una llamada a otro miembro.

Un campo no se inicializa en los dos sitios a la vez, porque la asignación de la declaración se ejecuta antes que el constructor y quedaría sobrescrita sin efecto. Un campo static readonly se inicializa en su declaración o en el constructor estático, nunca en un constructor de instancia, porque el lenguaje no lo permite.

**Con estándar**

```csharp
public sealed class WaveDirector
{
    private static readonly TimeSpan _defaultWaveDelay = TimeSpan.FromSeconds(3);
    private readonly List<Enemy> _activeEnemies = new List<Enemy>();
    private readonly EnemySpawner _enemySpawner;

    public WaveDirector(EnemySpawner enemySpawner)
    {
        ArgumentNullException.ThrowIfNull(enemySpawner);

        _enemySpawner = enemySpawner;
    }
}
```

**Sin estándar**

```csharp
public sealed class WaveDirector
{
    private readonly List<Enemy> _activeEnemies = new List<Enemy>();
    private readonly EnemySpawner _enemySpawner = new EnemySpawner();

    public WaveDirector(EnemySpawner enemySpawner)
    {
        _enemySpawner = enemySpawner;
        _activeEnemies = new List<Enemy>();
    }
}
```

### 6.11 Clases selladas

Toda clase se declara sealed salvo que esté pensada para ser heredada, y en ese caso se declara abstract o se documenta en su comentario XML qué miembros se esperan redefinir. El propósito es que la herencia sea una decisión explícita: una clase sellada puede cambiar por dentro sin romper a nadie, mientras que una clase abierta obliga a considerar a los posibles derivados en cada cambio.

Este estándar aplica esa decisión de forma sistemática en sus ejemplos: las clases concretas aparecen como sealed y solo las bases de una jerarquía, como Enemy, aparecen como abstract. Regla propia del estándar.

**Con estándar**

```csharp
public abstract class Enemy
{
    public abstract void Update(GameTime gameTime);
}

public sealed class Grunt : Enemy
{
    public override void Update(GameTime gameTime)
    {
    }
}
```

**Sin estándar**

```csharp
public class Enemy
{
    public virtual void Update(GameTime gameTime)
    {
    }
}

public class Grunt : Enemy
{
}
```

### 6.12 Directivas using

Las directivas using se escriben al inicio del archivo y fuera de la declaración del espacio de nombres, porque dentro del espacio de nombres la resolución de nombres depende del contexto y puede cambiar al agregarse un espacio de nombres nuevo. Se ordenan alfabéticamente y las del espacio System van primero, separadas del resto por una línea en blanco.

No se conservan directivas que el archivo no utiliza ni se repiten directivas ya cubiertas por otra. El proyecto activa la regla de análisis que detecta las directivas innecesarias para que aparezcan durante la compilación. Esta regla trata la directiva using del encabezado del archivo; la declaración y la sentencia using que liberan un recurso son otra construcción distinta y se rigen por la regla 8.11.

**Con estándar**

```csharp
using System;
using System.Collections.Generic;

using Game.Domain;
using Game.Persistence;

namespace Game.Services
{
}
```

**Sin estándar**

```csharp
using Game.Persistence;
using System.Collections.Generic;
using System;
using System.IO;

namespace Game.Services
{
    using Game.Domain;
}
```

### 6.13 Uso de var

Se usa var únicamente en dos situaciones: cuando el tipo aparece de forma literal a la derecha de la asignación, es decir con new o con una conversión explícita, y en la variable de control de un bucle for, donde el tipo se deduce sin ambigüedad del literal inicial y la variable no sale del bucle.

En cualquier otro caso se escribe el tipo, incluso cuando parezca evidente, y en particular cuando el inicializador es una llamada a un método, una propiedad, un elemento indexado o un literal: en esos casos el lector tendría que abrir otra declaración para saber con qué está trabajando. En un foreach el tipo se declara siempre de forma explícita, conforme a la regla 6.18.

**Con estándar**

```csharp
var spawner = new EnemySpawner(_enemyFactory);
int activeCount = _activeEnemies.Count;
Enemy firstEnemy = _activeEnemies[0];

for (var i = 0; i < _spawnPoints.Count; i++)
{
    spawner.Spawn(_spawnPoints[i]);
}
```

**Sin estándar**

```csharp
EnemySpawner spawner = new EnemySpawner(_enemyFactory);
var activeCount = _activeEnemies.Count;
var firstEnemy = _activeEnemies[0];
```

### 6.14 Expresión new sin tipo

No se usa la forma de new que omite el tipo y lo deduce del lado izquierdo. El tipo se escribe siempre a la derecha del new, tanto en una variable local como en un campo, donde var no es aplicable y la tentación de abreviar es mayor.

El motivo es que la regla 6.13 reparte el trabajo entre las dos mitades de la línea: cuando el tipo está a la derecha se escribe var a la izquierda, y cuando no está se escribe el tipo. Omitirlo en las dos mitades deja una declaración en la que el tipo no aparece por ningún lado. Regla propia del estándar.

**Con estándar**

```csharp
public sealed class WaveDirector
{
    private readonly List<Enemy> _activeEnemies = new List<Enemy>();
    private readonly Dictionary<int, Player> _playersById = new Dictionary<int, Player>();
}
```

**Sin estándar**

```csharp
public sealed class WaveDirector
{
    private readonly List<Enemy> _activeEnemies = new();
    private readonly Dictionary<int, Player> _playersById = new();
}
```

### 6.15 Condiciones

En las condiciones se emplean únicamente los operadores de cortocircuito, y no sus versiones sin cortocircuito, porque estas evalúan siempre ambos operandos y anulan el efecto protector de una comprobación previa. Cuando la condición combina más de un operador lógico, cada cláusula que contenga un operador de comparación se encierra entre paréntesis, de modo que la precedencia quede escrita y no haya que recordarla.

No se compara un valor booleano contra true o contra false, porque el valor ya es la condición. Una condición no contiene más de tres operandos; si se requieren más, se extrae a una variable booleana con nombre o a un método privado que devuelva bool. El máximo de tres operandos y la prohibición de comparar contra true o false son reglas propias del estándar.

**Con estándar**

```csharp
bool canBeAttacked = target.IsAlive
    && (distanceToTarget <= AttackRange)
    && !target.HasShield;

if (canBeAttacked)
{
    enemy.Attack(target);
}
```

**Sin estándar**

```csharp
if (target.IsAlive & distanceToTarget <= AttackRange && target.HasShield == false && enemy.CanAttack)
{
    enemy.Attack(target);
}
```

### 6.16 if, else if y else

La palabra reservada if, un espacio y la condición entre paréntesis van en una línea, y la llave de apertura en la siguiente. Las palabras else y else if inician su propia línea después de la llave de cierre de la rama anterior. La forma else if se escribe como dos palabras seguidas de su condición, y no se sustituye por un if anidado dentro del bloque else, que añade un nivel de anidamiento sin añadir información.

No se escribe un else vacío ni un else que solo contenga un comentario. Cuando la cadena compara un mismo valor contra tres o más alternativas, se sustituye por switch, conforme a la regla 6.21. Ambos puntos son reglas propias del estándar.

**Con estándar**

```csharp
if (enemy.Health <= 0)
{
    enemy.Die();
}
else if (enemy.Health <= LowHealthThreshold)
{
    enemy.Flee();
}
else
{
    enemy.Attack(player);
}
```

**Sin estándar**

```csharp
if (enemy.Health <= 0) {
    enemy.Die();
} else {
    if (enemy.Health <= LowHealthThreshold)
        enemy.Flee();
    else
    {
        enemy.Attack(player);
    }
}
```

### 6.17 for

Las tres secciones del for se escriben en una sola línea, separadas por punto y coma con un espacio después de cada uno. La variable de control se declara con var dentro del inicializador, conforme a la regla 6.13; no se declara fuera del bucle y no se modifica dentro del cuerpo.

La cabecera declara una sola variable de control y contiene una sola expresión en el iterador, aunque el lenguaje admita listas separadas por comas, y no se admite la forma sin condición. El for se usa cuando se necesita el índice o cuando se recorre en orden inverso; para recorrer una colección de principio a fin sin usar el índice se usa foreach, conforme a la regla 6.18. Estas restricciones son reglas propias del estándar.

**Con estándar**

```csharp
for (var i = _activeEnemies.Count - 1; i >= 0; i--)
{
    if (!_activeEnemies[i].IsAlive)
    {
        _activeEnemies.RemoveAt(i);
    }
}
```

**Sin estándar**

```csharp
int i;
for(i = 0, j = 0 ; i < _activeEnemies.Count ; i++, j--)
{
    if (!_activeEnemies[i].IsAlive)
        _activeEnemies.RemoveAt(i);
}
```

### 6.18 foreach

El tipo de la variable de iteración se declara de forma explícita, nunca con var, y se nombra en singular. El cuerpo no la reasigna, porque la reasignación se pierde en la iteración siguiente y no modifica la colección.

No se modifica la colección que se está recorriendo, porque al hacerlo el enumerador queda invalidado y la siguiente llamada a MoveNext lanza InvalidOperationException. Para eliminar elementos se usa RemoveAll o un recorrido por índice en sentido descendente, conforme a la regla 6.17. Tampoco se recorre una colección que pueda ser null, ya que aplicar foreach a null lanza NullReferenceException.

**Con estándar**

```csharp
foreach (Enemy enemy in _activeEnemies)
{
    enemy.Update(deltaTime);
}

_activeEnemies.RemoveAll(enemy => !enemy.IsAlive);
```

**Sin estándar**

```csharp
foreach (var e in _activeEnemies)
{
    e.Update(deltaTime);

    if (!e.IsAlive)
    {
        _activeEnemies.Remove(e);
    }
}
```

### 6.19 while

Se escribe while, un espacio y la condición entre paréntesis, y la llave de apertura en la línea siguiente. No se escribe punto y coma después del paréntesis de cierre, porque ese punto y coma convierte el cuerpo en un bucle vacío que no avanza. El while se usa cuando el cuerpo puede no ejecutarse ninguna vez.

Algo dentro del cuerpo debe modificar el valor que evalúa la condición, o el cuerpo debe contener un break o un return. La forma while con condición siempre verdadera se admite solo cuando la salida es un break o un return explícito y el cuerpo no supera las diez líneas. Regla propia del estándar.

**Con estándar**

```csharp
while (_pendingSpawns.Count > 0)
{
    SpawnPoint spawnPoint = _pendingSpawns.Dequeue();
    _enemySpawner.Spawn(spawnPoint);
}
```

**Sin estándar**

```csharp
while (_pendingSpawns.Count > 0);
{
    SpawnPoint spawnPoint = _pendingSpawns.Peek();
    _enemySpawner.Spawn(spawnPoint);
}
```

### 6.20 do while

La palabra reservada do ocupa su propia línea y la llave de apertura va en la siguiente. La cláusula de cierre se escribe en la misma línea que la llave de cierre, con la palabra while y la condición entre paréntesis, y termina siempre con punto y coma. Es la única excepción a la regla 6.1.

El do while se usa únicamente cuando el cuerpo debe ejecutarse al menos una vez. Su cuerpo no supera las diez líneas, ya que la condición aparece al final y quien lee tiene que sostener el cuerpo entero en la cabeza hasta llegar a ella; si las supera, se extrae a un método privado. Regla propia del estándar.

**Con estándar**

```csharp
do
{
    currentWave++;
    _waveDirector.StartWave(currentWave);
} while (currentWave < TotalWaves);
```

**Sin estándar**

```csharp
do {
    currentWave++;
    _waveDirector.StartWave(currentWave);
}
while (currentWave < TotalWaves)
```

### 6.21 switch, case y default

Las etiquetas case y default se indentan un nivel respecto de la palabra reservada switch, y el contenido de cada sección se indenta un nivel respecto de su etiqueta. Cada sección termina en break, return o throw, ya que en C# el control no puede caer de una sección a la siguiente. Cuando varios valores comparten la misma lógica, sus etiquetas se apilan sobre una única sección en lugar de duplicar el cuerpo.

Todo switch sobre un tipo enumerado cubre todos los miembros declarados e incluye siempre la etiqueta default, de modo que un valor agregado más tarde falle de inmediato en lugar de pasar inadvertido. Como reglas propias del estándar, default se coloca al final, se deja una línea en blanco entre secciones y una sección cuyo cuerpo supera las cinco líneas se extrae a un método privado.

**Con estándar**

```csharp
switch (weapon.Kind)
{
    case WeaponKind.Sword:
    case WeaponKind.Axe:
        ApplyMeleeDamage(target, weapon);
        break;

    case WeaponKind.Bow:
        ApplyRangedDamage(target, weapon);
        break;

    default:
        throw new ArgumentOutOfRangeException(nameof(weapon));
}
```

**Sin estándar**

```csharp
switch (weapon.Kind)
{
case WeaponKind.Sword:
ApplyMeleeDamage(target, weapon);
break;
default:
    break;
case WeaponKind.Bow:
    ApplyRangedDamage(target, weapon);
    break;
}
```

### 6.22 Expresión switch

Cuando todas las ramas producen un valor que se asigna o se devuelve, se usa la expresión switch en lugar de la sentencia. Cada brazo se escribe en su propia línea, indentado un nivel, con el patrón, la flecha, el valor y una coma al final; la llave de cierre lleva el punto y coma en la misma línea. Si algún brazo necesita más de una operación, se usa la sentencia switch de la regla 6.21.

El brazo de descarte va siempre en último lugar y lanza ArgumentOutOfRangeException cuando el valor no está previsto, para que un caso no contemplado no devuelva de forma silenciosa un valor por omisión. Regla propia del estándar.

**Con estándar**

```csharp
private static int GetScoreReward(EnemyKind kind) => kind switch
{
    EnemyKind.Grunt => GruntScoreReward,
    EnemyKind.Archer => ArcherScoreReward,
    EnemyKind.Boss => BossScoreReward,
    _ => throw new ArgumentOutOfRangeException(nameof(kind))
};
```

**Sin estándar**

```csharp
private static int GetScoreReward(EnemyKind kind)
{
    switch (kind)
    {
        case EnemyKind.Grunt: return GruntScoreReward;
        default: return 0;
    }
}
```

### 6.23 Operador condicional

El operador condicional se usa únicamente para asignar un valor o para devolverlo, y se prefiere sobre un if else cuyas dos ramas solo asignan a la misma variable o solo devuelven un valor. Sus dos ramas producen un valor del mismo tipo y ninguna invoca un método que devuelva void.

La condición contiene un solo operador y no se anidan operadores condicionales, porque un condicional dentro de otro obliga a leer dos decisiones en una sola línea; si el caso no cabe en esa forma, se escribe con if y else conforme a la regla 6.16. Regla propia del estándar.

**Con estándar**

```csharp
string statusText = player.IsAlive ? "In combat" : "Defeated";

return weapon.IsCritical ? weapon.CriticalDamage : weapon.BaseDamage;
```

**Sin estándar**

```csharp
string statusText = player.IsAlive
    ? (player.Health < LowHealthThreshold ? "Wounded" : "In combat")
    : "Defeated";
```

### 6.24 break, continue y return

Las sentencias break, continue y return se escriben solas en su línea, dentro de un bloque con llaves, nunca en la misma línea que la condición que las provoca. El break dentro de un switch anidado en un bucle termina solo el switch; cuando se necesita terminar también el bucle, se usa return o se extrae el bucle a un método propio.

Como regla propia del estándar, continue se usa solo al inicio del cuerpo del bucle, como filtro de los elementos que no se procesan; un continue en medio del cuerpo obliga a revisar todo lo anterior para saber qué quedó hecho.

**Con estándar**

```csharp
foreach (Enemy enemy in _activeEnemies)
{
    if (!enemy.IsAlive)
    {
        continue;
    }

    enemy.Update(deltaTime);
}
```

**Sin estándar**

```csharp
foreach (Enemy enemy in _activeEnemies)
{
    if (!enemy.IsAlive) continue;

    enemy.Update(deltaTime);

    if (enemy.IsBoss) break;
}
```

### 6.25 goto

La sentencia goto está prohibida. La única forma admitida es goto case o goto default dentro de un switch, cuando el encadenamiento entre secciones es deliberado. Para salir de bucles anidados no se usan banderas booleanas ni goto: el bucle interno se extrae a un método privado y se sale de él con return.

**Con estándar**

```csharp
private bool ContainsBoss(IReadOnlyList<Wave> waves)
{
    foreach (Wave wave in waves)
    {
        if (WaveContainsBoss(wave))
        {
            return true;
        }
    }

    return false;
}

private bool WaveContainsBoss(Wave wave)
{
    foreach (Enemy enemy in wave.Enemies)
    {
        if (enemy.IsBoss)
        {
            return true;
        }
    }

    return false;
}
```

**Sin estándar**

```csharp
foreach (Wave wave in waves)
{
    foreach (Enemy enemy in wave.Enemies)
    {
        if (enemy.IsBoss)
        {
            goto Found;
        }
    }
}

Found:
_matchLog.RecordBossEncounter();
```

### 6.26 Cláusulas de guarda y return temprano

Las condiciones bajo las cuales el método no debe hacer nada se resuelven con un return temprano al inicio y no envolviendo el cuerpo completo en un if. Un if cuyo cuerpo abarca todo el resto del método se invierte y se convierte en guarda, con lo que el cuerpo principal queda un nivel menos anidado.

Un método admite como máximo un return temprano. Cuando una segunda condición también obligaría a salir antes de tiempo, ambas condiciones se combinan en una sola guarda con el operador lógico que corresponda, o el método se divide en métodos más pequeños de una sola responsabilidad. Esta guarda va seguida de una línea en blanco, conforme a la regla 5.4. Regla propia del estándar.

**Con estándar**

```csharp
public void UpdateEnemies(GameTime gameTime)
{
    ArgumentNullException.ThrowIfNull(gameTime);

    if (_state != GameState.Playing)
    {
        return;
    }

    foreach (Enemy enemy in _activeEnemies)
    {
        ProcessEnemyUpdate(enemy, gameTime);
    }
}
```

**Sin estándar**

```csharp
public void UpdateEnemies(GameTime gameTime)
{
    if (gameTime != null)
    {
        if (_state == GameState.Playing)
        {
            foreach (Enemy enemy in _activeEnemies)
            {
                enemy.Update(gameTime);
            }
        }
    }
}
```

### 6.27 Comentario del return temprano

Un return temprano lleva, en la línea inmediatamente anterior, un comentario que explica por qué el método no continúa, siempre que la condición por sí sola no lo deje claro. Se escribe el comentario cuando la condición se apoya en un acuerdo del equipo, en una restricción externa o en un estado cuyo significado no se lee en el nombre de la variable.

No se escribe el comentario cuando la condición ya es explícita, porque en ese caso el comentario repetiría el código y eso lo prohíbe la regla 7.7. Regla propia del estándar.

**Con estándar**

```csharp
public void ApplyRegeneration(Player player)
{
    // Regeneration is disabled during a boss wave by design decision of the team.
    if (_waveDirector.IsBossWave)
    {
        return;
    }

    player.Health = Math.Min(player.Health + RegenerationAmount, player.MaxHealth);
}
```

**Sin estándar**

```csharp
public void ApplyRegeneration(Player player)
{
    if (_waveDirector.IsBossWave)
    {
        return;
    }

    player.Health = Math.Min(player.Health + RegenerationAmount, player.MaxHealth);
}
```

### 6.28 Niveles de anidamiento

Un método no supera tres niveles de anidamiento, contando el cuerpo del método como nivel cero. Cada if, cada bucle, cada switch y cada bloque try suma un nivel. Un método muy anidado obliga a recordar todas las condiciones activas para entender la línea que se está leyendo.

Cuando dos bucles quedan anidados y el interno tiene sentido por sí mismo, el interno se extrae a un método privado con nombre propio. Cuando el anidamiento proviene de comprobaciones previas, se convierten en guarda conforme a la regla 6.26. Regla propia del estándar.

**Con estándar**

```csharp
public void UpdateWaves(GameTime gameTime)
{
    foreach (Wave wave in _activeWaves)
    {
        UpdateWaveEnemies(wave, gameTime);
    }
}

private void UpdateWaveEnemies(Wave wave, GameTime gameTime)
{
    foreach (Enemy enemy in wave.Enemies)
    {
        if (enemy.IsAlive)
        {
            enemy.Update(gameTime);
        }
    }
}
```

**Sin estándar**

```csharp
public void UpdateWaves(GameTime gameTime)
{
    foreach (Wave wave in _activeWaves)
    {
        foreach (Enemy enemy in wave.Enemies)
        {
            if (enemy.IsAlive)
            {
                if (enemy.IsStunned)
                {
                    enemy.RecoverFromStun(gameTime);
                }
            }
        }
    }
}
```

### 6.29 Complejidad ciclomática

La complejidad ciclomática mide el número de caminos independientes de un método y se calcula sumando uno por cada rama: cada if, cada bucle, cada case de un switch y cada operador lógico dentro de una condición. Ningún método del proyecto supera una complejidad ciclomática de 10.

Cuando un método supera ese valor, se refactoriza extrayendo la lógica a métodos privados con nombre propio o sustituyendo la cadena de condiciones por un switch. El límite de 10 es una regla propia del estándar: la regla de análisis que mide la complejidad trae 25 como valor por omisión y el proyecto lo ajusta a 10 en su archivo de configuración de métricas.

**Con estándar**

```csharp
public int CalculateDamage(Weapon weapon, Enemy target)
{
    int baseDamage = GetBaseDamage(weapon);
    int modifiedDamage = ApplyWeaponModifiers(baseDamage, weapon);

    return ApplyArmorReduction(modifiedDamage, target);
}
```

**Sin estándar**

```csharp
public int CalculateDamage(Weapon weapon, Enemy target)
{
    int damage = weapon.BaseDamage;

    if (weapon.IsCritical)
    {
        if (weapon.Kind == WeaponKind.Bow)
        {
            damage = (int)(damage * CriticalHitMultiplier * RangedBonus);
        }
        else if (weapon.IsEnchanted)
        {
            damage = (int)(damage * CriticalHitMultiplier * EnchantBonus);
        }
        else
        {
            damage = (int)(damage * CriticalHitMultiplier);
        }
    }
    else if (weapon.IsEnchanted && !target.HasShield)
    {
        damage = (int)(damage * EnchantBonus);
    }

    return damage - target.Armor;
}
```

### 6.30 Expresiones lambda

Las expresiones lambda se admiten y su uso queda limitado a estos casos: los argumentos de los métodos de consulta y de modificación de colecciones, como Where, Select, Any, All, FirstOrDefault y RemoveAll; la suscripción a un evento cuando el manejador cabe en una sola expresión y no necesita darse de baja; y la configuración que un método recibe como delegado; y el argumento de una aserción que verifica una excepción, conforme a la regla 10.5. En cualquier otro caso se escribe un método con nombre.

El cuerpo de una lambda es una sola expresión y cabe en una línea. No se escriben lambdas con bloque y llaves, ni lambdas anidadas dentro de otra lambda, ni lambdas asignadas a un campo o a una variable para invocarlas después: en esos tres casos se declara un método privado, que puede nombrarse, documentarse y probarse. El nombre del parámetro se rige por la regla 3.4. Regla propia del estándar.

Una lambda que se suscribe a un evento no puede darse de baja después, porque no existe una referencia con la que llamar a la baja; cuando la suscripción debe cancelarse, se usa un método con nombre, conforme a la regla 6.33.

**Con estándar**

```csharp
_activeEnemies.RemoveAll(enemy => !enemy.IsAlive);

bool hasBoss = _activeEnemies.Any(enemy => enemy.IsBoss);

IReadOnlyList<Item> equippedItems = _inventory.Where(item => item.IsEquipped).ToList();
```

**Sin estándar**

```csharp
Func<Enemy, bool> isDefeated = enemy =>
{
    return !enemy.IsAlive && enemy.Health <= 0;
};

_activeEnemies.RemoveAll(enemy => isDefeated(enemy));

_combatSystem.EnemyDefeated += (sender, e) => _scoreBoard.Add(e.ScoreReward);
```

### 6.31 Declaración de eventos

Los eventos se declaran con EventHandler cuando no llevan datos y con EventHandler genérico cuando sí los llevan, en lugar de declarar un delegado propio. Los datos viajan en una clase derivada de EventArgs, cuyo nombre lleva ese sufijo conforme a la regla 4.1, con propiedades de solo lectura asignadas en el constructor.

El manejador que se suscribe devuelve void y recibe dos parámetros, sender de tipo object y e del tipo de los argumentos, conforme a la regla 3.3. No se declaran delegados propios para eventos: EventHandler ya cubre las dos formas posibles y un delegado propio obliga a leer su declaración para saber qué parámetros llegan.

**Con estándar**

```csharp
public sealed class EnemyDefeatedEventArgs : EventArgs
{
    public EnemyDefeatedEventArgs(int scoreReward)
    {
        ScoreReward = scoreReward;
    }

    public int ScoreReward { get; }
}

public sealed class CombatSystem
{
    public event EventHandler<EnemyDefeatedEventArgs> EnemyDefeated;
    public event EventHandler EnemySpawning;
}
```

**Sin estándar**

```csharp
public sealed class EnemyDefeatedArgs : EventArgs
{
    public int ScoreReward { get; set; }
}

public delegate void EnemyDefeatedDelegate(object o, EnemyDefeatedArgs args);

public sealed class CombatSystem
{
    public event EnemyDefeatedDelegate OnEnemyDefeated;
}
```

### 6.32 Lanzamiento de eventos

El evento se lanza desde un método protegido y virtual llamado On seguido del nombre del evento, que recibe un único parámetro e y no hace nada más que lanzarlo. Ese método es el único punto desde el que se invoca el evento, de modo que una clase derivada pueda intervenir redefiniéndolo.

Al lanzarlo se comprueba que existan suscriptores con el operador condicional nulo, porque un evento sin suscriptores vale null y invocarlo directamente lanza NullReferenceException. No se pasa null como sender de un evento de instancia ni como datos del evento; cuando el evento no lleva datos se pasa EventArgs.Empty.

**Con estándar**

```csharp
public sealed class CombatSystem
{
    public event EventHandler<EnemyDefeatedEventArgs> EnemyDefeated;

    protected virtual void OnEnemyDefeated(EnemyDefeatedEventArgs e)
    {
        EnemyDefeated?.Invoke(this, e);
    }
}
```

**Sin estándar**

```csharp
public sealed class CombatSystem
{
    public event EventHandler<EnemyDefeatedEventArgs> EnemyDefeated;

    public void Defeat(Enemy enemy)
    {
        EnemyDefeated(null, new EnemyDefeatedEventArgs(enemy.ScoreReward));
    }
}
```

### 6.33 Suscripción y baja de eventos

Quien se suscribe a un evento se da de baja cuando deja de necesitarlo, en el mismo lugar donde termina la vida del suscriptor. Mientras la suscripción sigue viva, el publicador mantiene una referencia al suscriptor y este no se libera, aunque ya no se use en ninguna otra parte.

Por ese motivo la suscripción que debe cancelarse se hace siempre con un método con nombre y no con una lambda, conforme a la regla 6.30: la baja necesita la misma referencia al delegado que se usó al suscribirse, y una lambda no deja ninguna.

**Con estándar**

```csharp
public void Attach(CombatSystem combatSystem)
{
    _combatSystem = combatSystem;
    _combatSystem.EnemyDefeated += CombatSystemOnEnemyDefeated;
}

public void Detach()
{
    _combatSystem.EnemyDefeated -= CombatSystemOnEnemyDefeated;
    _combatSystem = null;
}
```

**Sin estándar**

```csharp
public void Attach(CombatSystem combatSystem)
{
    combatSystem.EnemyDefeated += (sender, e) => _scoreBoard.Add(e.ScoreReward);
}
```

## 7. Comentarios

### 7.1 Cuándo se comenta

El comentario explica por qué el código hace algo, no qué hace. Se comenta una decisión que no se deduce leyendo el código: una restricción externa, un valor acordado por el equipo o la solución a un caso particular del juego. Si el código necesita un comentario para entenderse, primero se intenta renombrar un identificador o extraer un método, y solo si eso no basta se escribe el comentario.

**Con estándar**

```csharp
// The pool keeps twelve enemies because the arena never shows more at once.
private const int MaxActiveEnemies = 12;
```

**Sin estándar**

```csharp
// Assigns twelve to the maximum number of enemies.
private const int MaxActiveEnemies = 12;
```

### 7.2 Formato del comentario de línea

Los comentarios de una línea se escriben con dos barras, un espacio y la primera letra en mayúscula, y terminan en punto. Se colocan en la línea anterior al código que explican, con la misma indentación que esa línea, y nunca al final de ella: un comentario al final de la línea desplaza el código, se pierde de vista al ajustar la ventana y se corta al comparar versiones.

**Con estándar**

```csharp
// The delay comes from the wave configuration agreed with the design team.
TimeSpan delay = _waveConfiguration.SpawnDelay;
```

**Sin estándar**

```csharp
TimeSpan delay = _waveConfiguration.SpawnDelay; //delay
```

### 7.3 Alcance de la documentación XML

Se documentan con comentarios XML, escritos con tres barras, los tipos públicos y los miembros públicos de la capa de servicios descrita en la regla 2.1. Es la capa que el cliente consume sin ver su código, de modo que su documentación es la única descripción disponible de lo que hace cada operación.

Cuando una operación se declara en una interfaz y se implementa en una clase, la documentación se escribe en la interfaz, porque es el contrato que el llamador consulta. La clase que la implementa no repite el texto: hereda la documentación con la etiqueta inheritdoc.

En las capas de cliente, dominio y persistencia la documentación XML no es obligatoria, y lo que se explique allí se escribe como comentario conforme a la regla 7.1. Los miembros privados no se documentan con XML en ninguna capa.

**Con estándar**

```csharp
public interface IMatchService
{
    /// <summary>
    /// Applies damage to an enemy and returns its remaining health.
    /// </summary>
    int ApplyDamage(int enemyId, int damageAmount);
}

public sealed class MatchService : IMatchService
{
    /// <inheritdoc/>
    public int ApplyDamage(int enemyId, int damageAmount)
    {
        return 0;
    }
}
```

**Sin estándar**

```csharp
public sealed class MatchService : IMatchService
{
    public int ApplyDamage(int enemyId, int damageAmount)
    {
        return 0;
    }

    /// <summary>
    /// Private helper.
    /// </summary>
    private Enemy FindEnemy(int enemyId)
    {
        return _activeEnemies[enemyId];
    }
}
```

### 7.4 Etiquetas de la documentación XML

Todo miembro documentado lleva como mínimo la etiqueta summary. Además lleva una etiqueta param por cada parámetro, sin omitir ninguno; una etiqueta returns cuando el miembro devuelve un valor; y una etiqueta exception por cada excepción que el llamador debe contemplar y tratar. Un tipo documentado lleva únicamente summary.

No se documenta una excepción que el llamador no puede evitar ni manejar, como las que provienen de un fallo del entorno; esas se registran conforme a la sección 9. El texto se escribe en inglés, igual que el resto del código.

**Con estándar**

```csharp
/// <summary>
/// Applies damage to an enemy and returns its remaining health.
/// </summary>
/// <param name="enemyId">Identifier of the enemy that receives the damage.</param>
/// <param name="damageAmount">Damage points before armor reduction.</param>
/// <returns>Remaining health of the enemy after the damage is applied.</returns>
/// <exception cref="ArgumentOutOfRangeException">damageAmount is negative.</exception>
int ApplyDamage(int enemyId, int damageAmount);
```

**Sin estándar**

```csharp
/// <summary>
/// Applies damage.
/// </summary>
/// <param name="enemyId">The enemy id.</param>
int ApplyDamage(int enemyId, int damageAmount);
```

### 7.5 Redacción de la documentación XML

El texto de summary describe la responsabilidad del miembro y su comportamiento observable, en una frase que empieza con un verbo en tercera persona y termina en punto. No repite el nombre del miembro descompuesto en palabras, porque en ese caso no añade nada a lo que ya se lee en la firma.

La documentación tampoco describe la implementación: no menciona a qué repositorio llama ni en qué orden ejecuta los pasos, porque eso cambia sin que cambie el contrato y deja la documentación desactualizada. Lo que se documenta es qué puede esperar el llamador.

**Con estándar**

```csharp
/// <summary>
/// Stores the current match so it can be resumed later from the given slot.
/// </summary>
/// <param name="slotName">Name of the slot where the match is stored.</param>
void SaveMatch(string slotName);
```

**Sin estándar**

```csharp
/// <summary>
/// SaveMatch method. Calls the repository, opens the file and writes the match.
/// </summary>
/// <param name="slotName">The slot name.</param>
void SaveMatch(string slotName);
```

### 7.6 TODO y FIXME

Se admiten dos marcas dentro de un comentario: TODO para el trabajo pendiente y FIXME para un defecto conocido que todavía no se corrige. Ambas se escriben en mayúsculas, van seguidas del responsable entre paréntesis y de una frase que describe qué falta hacer, y se resuelven antes de cerrar la entrega en la que aparecen. Regla propia del estándar.

**Con estándar**

```csharp
// TODO(valentin): Replace the fixed delay with the wave configuration value.
```

**Sin estándar**

```csharp
//todo: arreglar esto luego
```

### 7.7 Comentarios prohibidos

No se deja código comentado en el repositorio, porque el historial de versiones ya conserva lo que se eliminó y el código comentado no se compila, no se prueba y nadie sabe si sigue siendo válido. Tampoco se escriben comentarios que repiten lo que el código ya dice, bloques decorativos con líneas de asteriscos o guiones, ni firmas ni fechas dentro del código, que el control de versiones registra por sí solo.

**Sin estándar**

```csharp
//*******************************
// Update method
//*******************************
// Modified by Jesus, 20/08/2026
public void Update(GameTime gameTime)
{
    // UpdateLegacyEnemies(gameTime);
    UpdateEnemies(gameTime);
}
```

## 8. Manejo de errores y excepciones

### 8.1 Validación de argumentos

Todo método público valida sus argumentos al inicio, antes de cualquier otra lógica, usando los métodos auxiliares del framework: ArgumentNullException.ThrowIfNull para las referencias que el método no admite como nulas y los métodos ThrowIf de ArgumentOutOfRangeException para los rangos. Cuando el argumento es inválido por otra razón, se lanza ArgumentException indicando el nombre del parámetro con nameof.

Un argumento inválido nunca se ignora en silencio ni se sustituye por un valor por omisión: el método falla y el llamador se entera. Un parámetro declarado como nullable admite null por contrato y no se valida contra null, sino que se trata conforme a la regla 8.3. En un método que devuelve Task, la validación se hace antes de la parte asíncrona, para que el error llegue al llamar y no al esperar.

**Con estándar**

```csharp
public void Spawn(SpawnPoint spawnPoint, int enemyCount)
{
    ArgumentNullException.ThrowIfNull(spawnPoint);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(enemyCount);

    _activeEnemies.Add(_enemyFactory.Create(spawnPoint));
}
```

**Sin estándar**

```csharp
public void Spawn(SpawnPoint spawnPoint, int enemyCount)
{
    if (spawnPoint == null)
    {
        return;
    }

    _activeEnemies.Add(_enemyFactory.Create(spawnPoint));
}
```

### 8.2 Contexto nullable y declaración de referencias

El proyecto compila con el contexto nullable habilitado, de modo que el tipo declara si una referencia admite null: se escribe con signo de interrogación cuando puede serlo y sin él cuando no. Esa declaración es la fuente de verdad, y por eso no se comprueba contra null un valor cuyo tipo lo declara como no nulo: esa comprobación sugiere una duda que el tipo ya resolvió.

**Con estándar**

```csharp
public Enemy FindEnemy(int enemyId)
{
    return _activeEnemies[enemyId];
}

public Enemy? FindBoss()
{
    return _activeEnemies.FirstOrDefault(enemy => enemy.IsBoss);
}
```

**Sin estándar**

```csharp
public Enemy FindEnemy(int enemyId)
{
    Enemy enemy = _activeEnemies[enemyId];

    if (enemy == null)
    {
        return null;
    }

    return enemy;
}
```

### 8.3 Comparación contra null

Para comparar contra null se usan los patrones is null e is not null, y no los operadores de igualdad y desigualdad. El motivo es que un tipo puede redefinir el operador de igualdad y hacer que la comparación ejecute código propio, mientras que el patrón siempre comprueba la referencia.

**Con estándar**

```csharp
if (equippedWeapon is null)
{
    return _defaultWeapon;
}
```

**Sin estándar**

```csharp
if (equippedWeapon == null)
{
    return _defaultWeapon;
}
```

### 8.4 Acceso y sustitución de valores nulos

Para acceder a un miembro de una referencia que puede ser null se usa el operador condicional nulo, que devuelve null en lugar de lanzar la excepción. Para sustituir un valor ausente por otro se usa el operador de fusión nulo, que expresa la alternativa en la misma línea en la que se lee el valor.

Las dos formas se combinan cuando se lee un miembro de una referencia opcional y se necesita un valor por omisión. Lo que no se hace es encadenar comprobaciones con if para lo mismo, porque ocupan varias líneas y ocultan cuál es el valor final.

**Con estándar**

```csharp
public string GetDisplayName(Player? player)
{
    return player?.Nickname ?? UnknownPlayerName;
}
```

**Sin estándar**

```csharp
public string GetDisplayName(Player? player)
{
    if (player != null && player.Nickname != null)
    {
        return player.Nickname;
    }

    return UnknownPlayerName;
}
```

### 8.5 Operador de tolerancia a nulos

No se usa el operador de tolerancia a nulos, que es el signo de admiración tras una expresión, para silenciar una advertencia del compilador. Ese operador no comprueba nada: solo indica al compilador que confíe, y si la referencia resulta ser null el fallo aparece más adelante y en otro lugar.

Se admite únicamente cuando el compilador no puede deducir una garantía que el código sí tiene, y en ese caso va precedido de un comentario que explica de dónde proviene esa garantía. Regla propia del estándar.

**Con estándar**

```csharp
Enemy? boss = _activeEnemies.FirstOrDefault(enemy => enemy.IsBoss);

if (boss is not null)
{
    boss.Enrage();
}
```

**Sin estándar**

```csharp
Enemy? boss = _activeEnemies.FirstOrDefault(enemy => enemy.IsBoss);
boss!.Enrage();
```

### 8.6 Fallos esperables

Las excepciones se reservan para lo excepcional. Cuando el fallo forma parte del flujo normal y ocurre con frecuencia, como que una partida guardada no exista todavía, se comprueba la condición antes de la operación o se ofrece un método Try que devuelve bool y entrega el resultado por un parámetro out, conforme a la regla 3.13.

No se usan excepciones para controlar el flujo normal del programa: lanzar y capturar una excepción cuesta mucho más que comprobar una condición, y en un bucle de juego que se ejecuta muchas veces por segundo ese coste se nota.

**Con estándar**

```csharp
if (_saveGameRepository.TryLoad(slotName, out SaveGame saveGame))
{
    _session.Restore(saveGame);
}
```

**Sin estándar**

```csharp
try
{
    SaveGame saveGame = _saveGameRepository.Load(slotName);
    _session.Restore(saveGame);
}
catch (FileNotFoundException)
{
}
```

### 8.7 Tipos de excepción del framework

Se usan los tipos predefinidos del framework siempre que apliquen: ArgumentException y sus derivadas para argumentos inválidos, e InvalidOperationException cuando el objeto está en un estado que no admite la operación. Elegir el tipo que ya existe permite que el llamador lo capture sin conocer tipos propios del proyecto.

No se lanzan Exception ni SystemException de forma directa, porque obligan a capturar todo para tratar un caso concreto. Tampoco se lanzan los tipos reservados al motor de ejecución, como NullReferenceException, IndexOutOfRangeException o StackOverflowException, porque su presencia indica un defecto del código y lanzarlos a propósito hace imposible distinguir ambos casos.

**Con estándar**

```csharp
public void StartWave(int waveNumber)
{
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(waveNumber);

    if (_state != GameState.Playing)
    {
        throw new InvalidOperationException("A wave cannot start while the match is paused.");
    }

    _waveDirector.Start(waveNumber);
}
```

**Sin estándar**

```csharp
public void StartWave(int waveNumber)
{
    if (_state != GameState.Playing)
    {
        throw new Exception("error");
    }

    _waveDirector.Start(waveNumber);
}
```

### 8.8 Excepciones propias del proyecto

Los errores propios del dominio del juego derivan de una excepción base del proyecto, llamada GameException, que a su vez deriva de Exception. Esa base permite capturar de una sola vez todos los fallos propios del juego, distinguiéndolos de los del framework. Toda excepción propia termina en Exception conforme a la regla 4.1.

Toda excepción propia declara los tres constructores habituales: sin parámetros, con mensaje, y con mensaje y excepción interna. El tercero es el que permite conservar la causa original al envolver un fallo, conforme a la regla 8.10. El mensaje se escribe en inglés y termina en punto.

**Con estándar**

```csharp
public class GameException : Exception
{
    public GameException()
    {
    }

    public GameException(string message) : base(message)
    {
    }

    public GameException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public sealed class CorruptedSaveFileException : GameException
{
    public CorruptedSaveFileException()
    {
    }

    public CorruptedSaveFileException(string message) : base(message)
    {
    }

    public CorruptedSaveFileException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
```

**Sin estándar**

```csharp
public class SaveError : Exception
{
    public SaveError(string message) : base(message)
    {
    }
}
```

### 8.9 Captura

Solo se captura la excepción que se sabe manejar, y se captura por su tipo más derivado; cuando hay varias cláusulas catch, se ordenan de la más derivada a la menos derivada, porque el motor de ejecución elige la primera que coincide. Se maneja cuando el método puede continuar con una alternativa y se propaga cuando no puede resolver el fallo, conforme a la regla 8.10.

No se admiten bloques catch vacíos ni ningún otro mecanismo que oculte el fallo: una excepción capturada se registra o se propaga, nunca se descarta en silencio. Capturar Exception solo se admite en el punto de entrada de la aplicación, donde se registra el fallo y se termina de forma controlada. El registro se hace una sola vez, conforme a la regla 9.5.

**Con estándar**

```csharp
try
{
    _saveGameRepository.Load(saveStream);
}
catch (CorruptedSaveFileException exception)
{
    _log.Error("Corrupted save file; a new match is started.", exception);
    _session.StartNewGame();
}
```

**Sin estándar**

```csharp
try
{
    _saveGameRepository.Load(saveStream);
}
catch (Exception)
{
}
```

### 8.10 Propagación

Para relanzar la misma excepción dentro del catch que la capturó se escribe throw sin nombrar la variable, porque escribir throw con la variable reinicia la traza y borra la lista de llamadas que llevó hasta el fallo, que es justo lo que se necesita para localizarlo.

Cuando el fallo se sustituye por una excepción del dominio, la original se pasa como excepción interna al constructor de la nueva, conforme a la regla 8.8, de modo que la causa siga disponible aunque el tipo cambie.

**Con estándar**

```csharp
try
{
    _saveGameRepository.Read(saveStream);
}
catch (IOException exception)
{
    throw new CorruptedSaveFileException("The save file could not be read.", exception);
}
```

**Sin estándar**

```csharp
try
{
    _saveGameRepository.Read(saveStream);
}
catch (IOException exception)
{
    throw exception;
}
```

### 8.11 Liberación de recursos con using

Los recursos que implementan IDisposable se liberan con using y no con un bloque finally, porque using libera el recurso aunque el método termine por una excepción y no depende de que nadie escriba la llamada a Dispose. Se usa la declaración using, sin llaves, cuando el recurso vive hasta el final del método, y la sentencia using con bloque cuando debe liberarse antes porque después queda trabajo por hacer.

El bloque finally se reserva para lo que no puede liberarse con using, como devolver un objeto a un almacén propio del juego. Esta regla trata la declaración y la sentencia using que liberan un recurso; la directiva using del encabezado del archivo es otra construcción distinta y se rige por la regla 6.12.

**Con estándar**

```csharp
public SaveGame Load(string saveFilePath)
{
    using FileStream saveStream = File.OpenRead(saveFilePath);

    return Read(saveStream);
}
```

**Sin estándar**

```csharp
public SaveGame Load(string saveFilePath)
{
    FileStream saveStream = File.OpenRead(saveFilePath);

    try
    {
        return Read(saveStream);
    }
    finally
    {
        saveStream.Dispose();
    }
}
```

### 8.12 Lugares donde no se lanzan excepciones

No se lanzan excepciones dentro de un bloque finally, porque esa excepción sustituye a la que estaba propagándose y hace desaparecer el fallo original. Tampoco se lanzan desde Equals, GetHashCode y ToString, que el framework invoca en momentos que el código no controla, ni desde un constructor estático, cuyo fallo deja el tipo inutilizable durante toda la ejecución.

**Con estándar**

```csharp
public override string ToString()
{
    return $"Enemy {Id} ({Kind})";
}
```

**Sin estándar**

```csharp
public override string ToString()
{
    if (Kind == EnemyKind.Unknown)
    {
        throw new InvalidOperationException("The enemy kind is unknown.");
    }

    return $"Enemy {Id} ({Kind})";
}
```

## 9. Registro de eventos

### 9.1 Herramienta de registro

El registro de eventos se realiza mediante log4net, a través de los métodos de la interfaz ILog. Cada clase que registra declara su propio registrador como campo privado, estático y readonly, obtenido con LogManager y el tipo de la clase, de modo que cada mensaje quede asociado a la clase que lo emitió. No se escribe en la consola ni en un archivo por otros medios.

**Con estándar**

```csharp
public sealed class MatchService : IMatchService
{
    private static readonly ILog _log = LogManager.GetLogger(typeof(MatchService));
}
```

**Sin estándar**

```csharp
public sealed class MatchService : IMatchService
{
    public void StartMatch()
    {
        Console.WriteLine("Match started.");
    }
}
```

### 9.2 Niveles de severidad

Un mensaje se escribe solo si su nivel es igual o superior al configurado, así que el nivel decide qué se ve en producción y qué no. Los niveles admitidos, en orden descendente de severidad, son los siguientes.

- FATAL: evento crítico que impide la continuidad de la aplicación o compromete su funcionamiento general. Se registra siempre con la excepción como argumento.
- ERROR: fallo en una operación concreta que no detiene el resto del sistema. Se registra siempre con la excepción como argumento.
- WARN: situación anómala que no hace fallar la operación. Se registra con la excepción cuando proviene de una excepción recuperada, y solo con el mensaje cuando es una alerta preventiva sobre una métrica o un estado.
- INFO: evento relevante del funcionamiento normal, como el inicio o el fin de una partida. Se registra solo con el mensaje.
- DEBUG: información detallada orientada al diagnóstico durante el desarrollo. Se registra solo con el mensaje.
**Con estándar**

```csharp
public void RegisterDefeat(Enemy enemy)
{
    ArgumentNullException.ThrowIfNull(enemy);

    _log.Info($"Enemy {enemy.Id} defeated on wave {_currentWave}.");

    try
    {
        _scoreRepository.Save(enemy.ScoreReward);
    }
    catch (ScoreStorageUnavailableException exception)
    {
        _log.Error($"Could not save the score of enemy {enemy.Id}.", exception);
    }
}
```

**Sin estándar**

```csharp
public void RegisterDefeat(Enemy enemy)
{
    _log.Fatal($"Enemy {enemy.Id} defeated on wave {_currentWave}.");

    try
    {
        _scoreRepository.Save(enemy.ScoreReward);
    }
    catch (ScoreStorageUnavailableException exception)
    {
        _log.Debug(exception.Message);
    }
}
```

### 9.3 Contenido del mensaje

El mensaje indica qué ocurrió y aporta el contexto necesario para localizar el caso: la operación y el identificador de la entidad afectada, como el jugador, el enemigo o la partida. Un mensaje sin ese contexto obliga a reproducir el fallo para saber a qué se refería.

Cuando el evento proviene de una excepción, la excepción se pasa como argumento al método de registro, y no se registra únicamente su mensaje: pasarla conserva su tipo, su traza y su excepción interna, mientras que el texto por sí solo pierde el punto exacto donde ocurrió.

**Con estándar**

```csharp
_log.Error($"Could not save the score of enemy {enemy.Id} on wave {_currentWave}.", exception);
```

**Sin estándar**

```csharp
_log.Error("Error");
_log.Error(exception.Message);
```

### 9.4 Información que no se registra

No se registra información innecesaria ni datos sensibles: contraseñas, tokens de sesión, datos personales del jugador ni el contenido completo de un mensaje recibido. El registro se conserva y se comparte para diagnosticar, así que todo lo que se escribe en él deja de ser privado.

**Con estándar**

```csharp
_log.Info($"Player {player.Id} signed in.");
```

**Sin estándar**

```csharp
_log.Info($"Player {player.Email} signed in with password {player.Password}.");
```

### 9.5 Un solo registro por fallo

Un mismo fallo se registra una sola vez, en el punto donde se decide qué hacer con él. Cuando un método captura una excepción y la propaga, no la registra: la registrará quien la maneje. Cuando la maneja y continúa con una alternativa, la registra allí y no la propaga.

El motivo es que el mismo fallo registrado en cada nivel de la pila produce varias entradas idénticas con trazas distintas, y al leer el registro parecen fallos diferentes. Regla propia del estándar.

**Con estándar**

```csharp
public void LoadMatch(string slotName)
{
    try
    {
        _session.Restore(_saveGameRepository.Load(slotName));
    }
    catch (CorruptedSaveFileException exception)
    {
        _log.Error($"Corrupted save file in slot {slotName}; a new match is started.", exception);
        _session.StartNewGame();
    }
}
```

**Sin estándar**

```csharp
public void LoadMatch(string slotName)
{
    try
    {
        _session.Restore(_saveGameRepository.Load(slotName));
    }
    catch (CorruptedSaveFileException exception)
    {
        _log.Error($"Corrupted save file in slot {slotName}.", exception);
        throw;
    }
}
```

## 10. Pruebas

### 10.1 Nombrado del método de prueba

El nombre de un método de prueba tiene cuatro partes separadas por guion bajo, en este orden: el prefijo Test, el nombre del método probado, el escenario que se prueba y el resultado esperado. Al leer el nombre en el informe de la ejecución se sabe qué falló y en qué caso, sin abrir el código de la prueba.

Este nombre sustituye, dentro del proyecto de pruebas, a la forma de frase verbal que exige la regla 3.11, y es el único lugar del proyecto donde se admite el guion bajo dentro de un identificador; la regla de análisis que lo prohíbe se desactiva en el proyecto de pruebas. El nombre de la clase que agrupa las pruebas se rige por la regla 4.5. Regla propia del estándar.

**Con estándar**

```csharp
public sealed class EnemySpawnerTests
{
    [Fact]
    public void Test_Spawn_WithAvailableCapacity_AddsEnemy()
    {
    }

    [Fact]
    public void Test_Spawn_WithNullSpawnPoint_ThrowsArgumentNullException()
    {
    }
}
```

**Sin estándar**

```csharp
public sealed class EnemySpawnerTests
{
    [Fact]
    public void Test1()
    {
    }

    [Fact]
    public void SpawnTest()
    {
    }
}
```

### 10.2 Estructura de la prueba

Cada método de prueba se organiza en tres bloques separados por una línea en blanco: preparación, ejecución y verificación. La preparación construye el objeto probado y sus datos, la ejecución invoca la acción y la verificación comprueba el resultado. Esa separación permite ver de un vistazo qué se estaba probando cuando la prueba falla.

**Con estándar**

```csharp
[Fact]
public void Test_Spawn_WithAvailableCapacity_AddsEnemy()
{
    var spawner = new EnemySpawner(_enemyFactory);

    spawner.Spawn(_defaultSpawnPoint);

    Assert.Equal(ExpectedEnemyCount, spawner.ActiveEnemyCount);
}
```

**Sin estándar**

```csharp
[Fact]
public void Test_Spawn_WithAvailableCapacity_AddsEnemy()
{
    var spawner = new EnemySpawner(_enemyFactory);
    spawner.Spawn(_defaultSpawnPoint);
    Assert.Equal(ExpectedEnemyCount, spawner.ActiveEnemyCount);
}
```

### 10.3 Una sola ejecución por prueba

Una prueba contiene una sola invocación al método o a la acción que se prueba. Cuando una prueba ejecuta varias acciones y falla, no se sabe cuál de ellas falló ni si las siguientes llegaron a ejecutarse. Si se necesitan varios escenarios, se escriben pruebas separadas o una prueba parametrizada.

**Con estándar**

```csharp
[Theory]
[InlineData(EnemyKind.Grunt, GruntScoreReward)]
[InlineData(EnemyKind.Boss, BossScoreReward)]
public void Test_CalculateScoreReward_WithKnownKind_ReturnsConfiguredReward(EnemyKind kind, int expectedReward)
{
    var scoreBoard = new ScoreBoard();

    int reward = scoreBoard.CalculateScoreReward(kind);

    Assert.Equal(expectedReward, reward);
}
```

**Sin estándar**

```csharp
[Fact]
public void Test_CalculateScoreReward_WithKnownKind_ReturnsConfiguredReward()
{
    var scoreBoard = new ScoreBoard();

    Assert.Equal(GruntScoreReward, scoreBoard.CalculateScoreReward(EnemyKind.Grunt));
    Assert.Equal(BossScoreReward, scoreBoard.CalculateScoreReward(EnemyKind.Boss));
}
```

### 10.4 Sin lógica en la prueba

Una prueba no contiene estructuras de control: ni condiciones, ni bucles, ni transformaciones del resultado esperado. Los datos de entrada y el valor esperado se escriben como valores fijos o como constantes con nombre. Una prueba con lógica puede fallar por su propia lógica y no por el código que pretende verificar, y entonces hay que depurar la prueba.

**Con estándar**

```csharp
[Fact]
public void Test_ApplyDamage_WithArmoredEnemy_ReducesHealthByEffectiveDamage()
{
    var target = new Enemy(InitialHealth, ArmorPoints);

    _damageCalculator.ApplyDamage(target, RawDamage);

    Assert.Equal(ExpectedRemainingHealth, target.Health);
}
```

**Sin estándar**

```csharp
[Fact]
public void Test_ApplyDamage_WithArmoredEnemy_ReducesHealthByEffectiveDamage()
{
    var target = new Enemy(InitialHealth, ArmorPoints);

    _damageCalculator.ApplyDamage(target, RawDamage);

    if (target.Armor > 0)
    {
        Assert.Equal(InitialHealth - (RawDamage - target.Armor), target.Health);
    }
}
```

### 10.5 Aserciones

Se usa la aserción más específica que exista para lo que se comprueba: Assert.Equal para comparar valores, Assert.Null y Assert.NotNull para referencias y Assert.Throws para las excepciones. Assert.True y Assert.False se reservan a los valores que ya son booleanos por sí mismos.

El motivo es el mensaje de fallo: una aserción específica informa qué valor se esperaba y cuál se obtuvo, mientras que Assert.True solo informa que la condición fue falsa y obliga a depurar para saber por qué.

**Con estándar**

```csharp
Assert.Equal(ExpectedEnemyCount, spawner.ActiveEnemyCount);

Assert.Throws<ArgumentNullException>(() => spawner.Spawn(null));
```

**Sin estándar**

```csharp
Assert.True(spawner.ActiveEnemyCount == ExpectedEnemyCount);

Assert.True(spawner.ActiveEnemyCount > 0);
```

## 11. Referencias

Fuentes consultadas entre el 24 y el 30 de agosto de 2026.

1. Microsoft. Identifier names: rules and conventions (C#). Microsoft Learn. https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/identifier-names

2. Microsoft. .NET coding conventions (C#). Microsoft Learn. https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions

3. Microsoft. Capitalization conventions. Framework design guidelines. Microsoft Learn. https://learn.microsoft.com/dotnet/standard/design-guidelines/capitalization-conventions

4. Microsoft. General naming conventions. Framework design guidelines. Microsoft Learn. https://learn.microsoft.com/dotnet/standard/design-guidelines/general-naming-conventions

5. Microsoft. Names of classes, structs, and interfaces. Framework design guidelines. Microsoft Learn. https://learn.microsoft.com/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces

6. Microsoft. Names of type members. Framework design guidelines. Microsoft Learn. https://learn.microsoft.com/dotnet/standard/design-guidelines/names-of-type-members

7. Microsoft. Names of namespaces. Framework design guidelines. Microsoft Learn. https://learn.microsoft.com/dotnet/standard/design-guidelines/names-of-namespaces

8. Microsoft. Field design. Framework design guidelines. Microsoft Learn. https://learn.microsoft.com/dotnet/standard/design-guidelines/field

9. Microsoft. Exceptions and performance: patrones Tester-Doer y Try-Parse. Framework design guidelines. Microsoft Learn. https://learn.microsoft.com/dotnet/standard/design-guidelines/exceptions-and-performance

10. Microsoft. Using standard exception types. Framework design guidelines. Microsoft Learn. https://learn.microsoft.com/dotnet/standard/design-guidelines/using-standard-exception-types

11. Microsoft. Best practices for exceptions. Microsoft Learn. https://learn.microsoft.com/dotnet/standard/exceptions/best-practices-for-exceptions

12. Microsoft. Task-based Asynchronous Pattern (TAP). Microsoft Learn. https://learn.microsoft.com/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap

13. Microsoft. Selection statements: if and switch. C# reference. Microsoft Learn. https://learn.microsoft.com/dotnet/csharp/language-reference/statements/selection-statements

14. Microsoft. Iteration statements: for, foreach, do, while. C# reference. Microsoft Learn. https://learn.microsoft.com/dotnet/csharp/language-reference/statements/iteration-statements

15. Microsoft. Jump statements: break, continue, return, and goto. C# reference. Microsoft Learn. https://learn.microsoft.com/dotnet/csharp/language-reference/statements/jump-statements

16. Microsoft. Lambda expressions. C# reference. Microsoft Learn. https://learn.microsoft.com/dotnet/csharp/language-reference/operators/lambda-expressions

17. Microsoft. using statement and using declaration. C# reference. Microsoft Learn. https://learn.microsoft.com/dotnet/csharp/language-reference/statements/using

18. Microsoft. sealed. C# reference. Microsoft Learn. https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/sealed

19. Microsoft. C# formatting options, opciones de la regla IDE0055. Microsoft Learn. https://learn.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/csharp-formatting-options

20. Microsoft. IDE0007 e IDE0008: Use var o use explicit type. Microsoft Learn. https://learn.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0007-ide0008

21. Microsoft. IDE0011: Add braces. Microsoft Learn. https://learn.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0011

22. Microsoft. IDE0010: Add missing cases to switch statement. Microsoft Learn. https://learn.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0010

23. Microsoft. IDE0066: Use switch expression. Microsoft Learn. https://learn.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0066

24. Microsoft. IDE0046: Use conditional expression for return. Microsoft Learn. https://learn.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0046

25. Microsoft. IDE0031: Use null propagation. Microsoft Learn. https://learn.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0031

26. Microsoft. IDE0005: Remove unnecessary import. Microsoft Learn. https://learn.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0005

27. Microsoft. CA1062: Validate arguments of public methods. Microsoft Learn. https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1062

28. Microsoft. CA1707: Identifiers should not contain underscores. Microsoft Learn. https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1707

29. Microsoft. CA1710: Identifiers should have correct suffix. Microsoft Learn. https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1710

30. Microsoft. CA1711: Identifiers should not have incorrect suffix. Microsoft Learn. https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1711

31. Microsoft. CA1051: Do not declare visible instance fields. Microsoft Learn. https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1051

32. Microsoft. CA1502: Avoid excessive complexity. Microsoft Learn. https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1502

33. Microsoft. ArgumentNullException.ThrowIfNull Method. .NET API browser. https://learn.microsoft.com/dotnet/api/system.argumentnullexception.throwifnull

34. Microsoft. IEnumerator.MoveNext Method. .NET API browser. https://learn.microsoft.com/dotnet/api/system.collections.ienumerator.movenext

35. Microsoft. Recommended XML documentation tags. C# reference. Microsoft Learn. https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags

36. Microsoft. Nullable reference types. C# fundamentals. Microsoft Learn. https://learn.microsoft.com/dotnet/csharp/nullable-references

37. Microsoft. Event design. Framework design guidelines. Microsoft Learn. https://learn.microsoft.com/dotnet/standard/design-guidelines/event

38. Microsoft. .NET formatting options, ordenamiento de directivas using. Microsoft Learn. https://learn.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/dotnet-formatting-options

39. Microsoft. Best practices for writing unit tests. Microsoft Learn. https://learn.microsoft.com/dotnet/core/testing/unit-testing-best-practices

40. .NET Runtime team. C# Coding Style. Repositorio dotnet/runtime. https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md

41. Google. Google C# Style Guide. https://google.github.io/styleguide/csharp-style.html

42. Apache Software Foundation. Apache log4net Manual: Introduction. https://logging.apache.org/log4net/manual/introduction.html
